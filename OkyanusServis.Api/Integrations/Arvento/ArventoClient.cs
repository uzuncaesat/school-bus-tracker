using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OkyanusServis.Api.Data;

namespace OkyanusServis.Api.Integrations.Arvento;

/// <summary>
/// Konum kaynağı. ServiceConfig.UseMock = true iken sahte (yaklaşan servis)
/// konumları üretir; false iken gerçek Arvento web servisine (GetVehicleStatusJSON) bağlanır.
/// Böylece Arvento bilgileri gelene kadar tüm sistemi mock ile test edebiliriz.
/// </summary>
public class ArventoClient : IArventoClient
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<ArventoClient> _log;

    public ArventoClient(AppDbContext db, IHttpClientFactory httpFactory, ILogger<ArventoClient> log)
    {
        _db = db;
        _httpFactory = httpFactory;
        _log = log;
    }

    public async Task<IReadOnlyList<VehiclePosition>> GetPositionsAsync(CancellationToken ct = default)
    {
        var cfg = await _db.ServiceConfigs.AsNoTracking().FirstOrDefaultAsync(ct)
                  ?? new Entities.ServiceConfig();

        return cfg.UseMock
            ? await BuildMockPositionsAsync(ct)
            : await FetchRealPositionsAsync(cfg, ct);
    }

    // ---------------------------------------------------------------------
    // MOCK: Her aracı (ArventoNode dolu) ilk öğrencisinin evine doğru
    // "yaklaşıyormuş" gibi konumlandırır. Mesafe 240 sn'lik bir döngüde
    // ~1500 m'den ~150 m'ye iner; böylece her döngüde eşiği (800 m) geçer
    // ve yaklaşım motoru gerçek bir bildirim üretir.
    // ---------------------------------------------------------------------
    private async Task<IReadOnlyList<VehiclePosition>> BuildMockPositionsAsync(CancellationToken ct)
    {
        var vehicles = await _db.Vehicles
            .Where(v => v.ArventoNode != null && v.ArventoNode != "")
            .Select(v => new
            {
                v.ArventoNode,
                Home = v.Registrations
                    .Where(r => r.Latitude != null && r.Longitude != null)
                    .OrderBy(r => r.Id)
                    .Select(r => new { r.Latitude, r.Longitude })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        int secInCycle = (int)(now.Ticks / TimeSpan.TicksPerSecond % 240); // 0..239
        double dist = 1500 - (secInCycle / 240.0) * 1350;                   // 1500 -> 150 m

        var list = new List<VehiclePosition>();
        foreach (var v in vehicles)
        {
            if (v.Home?.Latitude is null || v.Home.Longitude is null) continue;
            // Evin kuzeyinden (0°) 'dist' metre uzağa yerleştir.
            var (lat, lon) = GeoUtils.Offset(v.Home.Latitude.Value, v.Home.Longitude.Value, dist, 0);
            list.Add(new VehiclePosition(v.ArventoNode!, lat, lon, Speed: 30, now));
        }

        _log.LogDebug("Mock konum üretildi: {Count} araç, mesafe ~{Dist:0} m", list.Count, dist);
        return list;
    }

    // ---------------------------------------------------------------------
    // GERÇEK: Arvento GetVehicleStatusJSON çağrısı. Arvento erişimi gelince
    // devreye girer. Alan adları gerçek yanıt görüldüğünde teyit edilecek.
    // ---------------------------------------------------------------------
    private async Task<IReadOnlyList<VehiclePosition>> FetchRealPositionsAsync(Entities.ServiceConfig cfg, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cfg.ArventoUsername)
            && string.IsNullOrWhiteSpace(cfg.ArventoPin1)
            && string.IsNullOrWhiteSpace(cfg.ArventoPin2))
        {
            _log.LogWarning("Arvento kimlik bilgileri boş — konum çekilemedi.");
            return Array.Empty<VehiclePosition>();
        }

        var http = _httpFactory.CreateClient("arvento");
        var q = $"Username={Uri.EscapeDataString(cfg.ArventoUsername ?? "")}"
              + $"&PIN1={Uri.EscapeDataString(cfg.ArventoPin1 ?? "")}"
              + $"&PIN2={Uri.EscapeDataString(cfg.ArventoPin2 ?? "")}&Language=0";
        var url = cfg.ArventoBaseUrl.TrimEnd('/') + "/GetVehicleStatus?" + q;

        try
        {
            var resp = await http.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync(ct);
            return ParseXmlPositions(body);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Arvento konum çağrısı başarısız.");
            return Array.Empty<VehiclePosition>();
        }
    }

    /// <summary>GetNodes: Arvento cihaz↔plaka listesi.</summary>
    public async Task<IReadOnlyList<(string Node, string Plate)>> GetNodesAsync(CancellationToken ct = default)
    {
        var cfg = await _db.ServiceConfigs.AsNoTracking().FirstOrDefaultAsync(ct) ?? new Entities.ServiceConfig();
        var http = _httpFactory.CreateClient("arvento");
        var q = $"Username={Uri.EscapeDataString(cfg.ArventoUsername ?? "")}"
              + $"&PIN1={Uri.EscapeDataString(cfg.ArventoPin1 ?? "")}"
              + $"&PIN2={Uri.EscapeDataString(cfg.ArventoPin2 ?? "")}&Group=&Language=0";
        var url = cfg.ArventoBaseUrl.TrimEnd('/') + "/GetNodes?" + q;

        var list = new List<(string, string)>();
        try
        {
            var resp = await http.GetAsync(url, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (body.Contains("Access denied") || body.Contains("yetkiniz yok")) return list;

            var doc = System.Xml.Linq.XDocument.Parse(body);
            foreach (var row in doc.Descendants())
            {
                var kids = row.Elements().ToList();
                var nodeEl = kids.FirstOrDefault(e => e.Name.LocalName.Equals("Node", StringComparison.OrdinalIgnoreCase));
                if (nodeEl is null || string.IsNullOrWhiteSpace(nodeEl.Value)) continue;
                var plate = kids.FirstOrDefault(e =>
                    e.Name.LocalName.IndexOf("LicensePlate", StringComparison.OrdinalIgnoreCase) >= 0
                    || e.Name.LocalName.IndexOf("Plaka", StringComparison.OrdinalIgnoreCase) >= 0)?.Value ?? "";
                list.Add((nodeEl.Value.Trim(), plate.Trim()));
            }
        }
        catch (Exception ex) { _log.LogError(ex, "GetNodes ayrıştırılamadı."); }
        return list;
    }

    /// <summary>GetVehicleStatus DataSet XML'ini VehiclePosition listesine çevirir (savunmacı, alan adı esnek).</summary>
    private IReadOnlyList<VehiclePosition> ParseXmlPositions(string body)
    {
        var result = new List<VehiclePosition>();
        if (string.IsNullOrWhiteSpace(body) || body.Contains("Access denied")) return result;
        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(body);
            // Satır elemanları: içinde "Cihaz"/"Device"/"Node" içeren bir alt eleman olanlar
            foreach (var row in doc.Descendants())
            {
                var kids = row.Elements().ToList();
                if (kids.Count == 0) continue;

                string? Val(params string[] keys) => kids.FirstOrDefault(e =>
                    keys.Any(k => e.Name.LocalName.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))?.Value;

                var node = Val("Cihaz", "Device_No", "DeviceNo", "Node");
                if (string.IsNullOrWhiteSpace(node)) continue;

                var latS = Val("Enlem", "Latitude", "Lat");
                var lonS = Val("Boylam", "Longitude", "Longidtude", "Lon");
                var spdS = Val("Hız", "Hiz", "Speed");
                if (latS is null || lonS is null) continue;

                var ci = System.Globalization.CultureInfo.InvariantCulture;
                double.TryParse(latS.Replace(',', '.'), System.Globalization.NumberStyles.Any, ci, out var lat);
                double.TryParse(lonS.Replace(',', '.'), System.Globalization.NumberStyles.Any, ci, out var lon);
                double.TryParse((spdS ?? "0").Replace(',', '.'), System.Globalization.NumberStyles.Any, ci, out var spd);

                if (lat != 0 || lon != 0)
                    result.Add(new VehiclePosition(node.Trim(), lat, lon, spd, DateTime.UtcNow));
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Arvento XML ayrıştırılamadı.");
        }
        return result;
    }

    /// <summary>Bağlantı testi: gerçek Arvento çağrısı yapar, tanı metni döner (Admin panelinde gösterilir).</summary>
    public async Task<string> ProbeAsync(CancellationToken ct = default)
    {
        var cfg = await _db.ServiceConfigs.AsNoTracking().FirstOrDefaultAsync(ct) ?? new Entities.ServiceConfig();
        if (string.IsNullOrWhiteSpace(cfg.ArventoUsername)
            && string.IsNullOrWhiteSpace(cfg.ArventoPin1)
            && string.IsNullOrWhiteSpace(cfg.ArventoPin2))
            return "Kullanıcı adı / PIN girilmemiş. Önce kaydet, sonra test et.";

        var http = _httpFactory.CreateClient("arvento");
        var baseUrl = cfg.ArventoBaseUrl.TrimEnd('/');
        var u = cfg.ArventoUsername ?? "";
        var p1 = cfg.ArventoPin1 ?? "";
        var p2 = cfg.ArventoPin2 ?? "";
        var q = $"Username={Uri.EscapeDataString(u)}&PIN1={Uri.EscapeDataString(p1)}&PIN2={Uri.EscapeDataString(p2)}&callback=";
        var jsonBody = JsonSerializer.Serialize(new { Username = u, PIN1 = p1, PIN2 = p2, callback = "" });

        var sb = new System.Text.StringBuilder();

        async Task Try(string label, Func<Task<HttpResponseMessage>> call)
        {
            try
            {
                var resp = await call();
                var body = await resp.Content.ReadAsStringAsync(ct);
                int parsed = 0;
                try { parsed = ParseJson(body).Count; } catch { }
                if (parsed == 0) { try { parsed = ParseXmlPositions(body).Count; } catch { } }
                var head = body.Length > 2500 ? body.Substring(0, 2500) + "…" : body;
                sb.AppendLine($"[{label}] HTTP {(int)resp.StatusCode} · uzunluk {body.Length} · araç {parsed}");
                // Arvento hata/mesaj düğümlerini yakala (<Error>...</Error>)
                var msgs = System.Text.RegularExpressions.Regex.Matches(body, "<Error>([^<]+)</Error>")
                    .Select(m => m.Groups[1].Value.Trim())
                    .Where(v => v.Length > 0).Distinct().ToList();
                if (msgs.Count > 0) sb.AppendLine("   >> SUNUCU MESAJI: " + string.Join(" | ", msgs));
                sb.AppendLine("   " + (string.IsNullOrWhiteSpace(head) ? "(boş)" : head.Replace("\n", " ")));
                sb.AppendLine();
            }
            catch (Exception ex) { sb.AppendLine($"[{label}] HATA: {ex.Message}"); sb.AppendLine(); }
        }

        // A) POST application/json
        await Try("POST-json", () => http.PostAsync(baseUrl + "/GetVehicleStatusJSON",
            new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json"), ct));
        // B) GET ?query
        await Try("GET", () => http.GetAsync(baseUrl + "/GetVehicleStatusJSON?" + q, ct));
        // C) POST form-urlencoded
        await Try("POST-form", () => http.PostAsync(baseUrl + "/GetVehicleStatusJSON",
            new FormUrlEncodedContent(new Dictionary<string, string>
            { ["Username"] = u, ["PIN1"] = p1, ["PIN2"] = p2, ["callback"] = "" }), ct));
        // D) Alternatif metot: GetVehicleStatus (XML)
        await Try("GET-XML(GetVehicleStatus)", () => http.GetAsync(
            baseUrl + "/GetVehicleStatus?" + $"Username={Uri.EscapeDataString(u)}&PIN1={Uri.EscapeDataString(p1)}&PIN2={Uri.EscapeDataString(p2)}&Language=0", ct));
        // E) TEŞHİS: en basit metot — çalışırsa kimlik DOĞRU, sorun metot yetkisi
        await Try("GetLocalDateTime", () => http.GetAsync(
            baseUrl + "/GetLocalDateTime?" + q, ct));
        // F) TEŞHİS: cihaz listesi
        await Try("GetNodes", () => http.GetAsync(
            baseUrl + "/GetNodes?" + q + "&Group=", ct));

        return sb.ToString();
    }

    /// <summary>GetVehicleStatusJSON yanıtını VehiclePosition listesine çevirir (savunmacı).</summary>
    private IReadOnlyList<VehiclePosition> ParseJson(string body)
    {
        var result = new List<VehiclePosition>();
        try
        {
            // asmx bazen JSON'u bir string içinde (kaçışlı) döndürür; ikisini de dene.
            var text = body.Trim();
            if (text.StartsWith("\"") && text.EndsWith("\""))
                text = JsonSerializer.Deserialize<string>(text) ?? text;

            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            var arr = root.ValueKind == JsonValueKind.Array
                ? root
                : root.TryGetProperty("d", out var d) ? d : root; // asmx "d" sarmalı

            foreach (var el in arr.EnumerateArray())
            {
                string node = GetStr(el, "Node", "Cihaz No", "Device_No", "DeviceNo");
                double lat = GetNum(el, "Latitude", "Enlem", "Lat");
                double lon = GetNum(el, "Longitude", "Boylam", "Lon", "Longidtude");
                double spd = GetNum(el, "Speed", "Hız", "Hiz");
                if (!string.IsNullOrEmpty(node))
                    result.Add(new VehiclePosition(node, lat, lon, spd, DateTime.UtcNow));
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Arvento JSON ayrıştırılamadı. Ham yanıt: {Body}", body);
        }
        return result;
    }

    private static string GetStr(JsonElement el, params string[] names)
    {
        foreach (var n in names)
            if (el.TryGetProperty(n, out var p) && p.ValueKind == JsonValueKind.String)
                return p.GetString() ?? "";
        return "";
    }

    private static double GetNum(JsonElement el, params string[] names)
    {
        foreach (var n in names)
            if (el.TryGetProperty(n, out var p))
            {
                if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var d)) return d;
                if (p.ValueKind == JsonValueKind.String && double.TryParse(p.GetString(),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var ds)) return ds;
            }
        return 0;
    }
}
