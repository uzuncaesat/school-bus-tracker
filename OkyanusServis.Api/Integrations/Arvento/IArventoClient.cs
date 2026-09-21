namespace OkyanusServis.Api.Integrations.Arvento;

/// <summary>
/// Araç konumlarını sağlayan kaynak. Gerçek Arvento web servisi ya da mock (sahte)
/// implementasyonu bu arayüzü uygular. Yaklaşım motoru sadece bu arayüzü tanır,
/// böylece gerçek/mock geçişi tek yerden yapılır (ServiceConfig.UseMock).
/// </summary>
public interface IArventoClient
{
    /// <summary>Takip edilen tüm araçların son konumlarını döner.</summary>
    Task<IReadOnlyList<VehiclePosition>> GetPositionsAsync(CancellationToken ct = default);

    /// <summary>Gerçek Arvento bağlantısını test eder — UseMock'tan bağımsız, tanı için ham yanıt döner.</summary>
    Task<string> ProbeAsync(CancellationToken ct = default);

    /// <summary>Arvento'daki cihaz↔plaka listesini döner (GetNodes).</summary>
    Task<IReadOnlyList<(string Node, string Plate)>> GetNodesAsync(CancellationToken ct = default);
}
