using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.JSInterop;

namespace OkyanusServis.Web.Services;

/// <summary>Giriş / çıkış işlemleri ve oturumun geri yüklenmesi.</summary>
public class AuthService
{
    private const string TokenKey = "ok_token";
    private readonly HttpClient _http;
    private readonly AuthState _state;
    private readonly IJSRuntime _js;
    private readonly ApiAuthStateProvider _provider;

    public AuthService(HttpClient http, AuthState state, IJSRuntime js, ApiAuthStateProvider provider)
    {
        _http = http; _state = state; _js = js; _provider = provider;
    }

    // Giriş yapan kullanıcının rolü var mı?
    public bool IsInRole(string role) => _state.User.IsInRole(role);

    // Uygulama açılışında: localStorage'da token varsa oturumu geri yükle
    public async Task InitializeAsync()
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        if (!string.IsNullOrEmpty(token))
        {
            _state.Token = token;
            if (!await LoadUserAsync())
                await LogoutAsync();   // token geçersiz/süresi dolmuşsa temizle
        }
    }

    // Giriş. Başarılıysa null, hata varsa mesaj döndürür.
    public async Task<string?> LoginAsync(string email, string password)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
        if (!resp.IsSuccessStatusCode)
            return "E-posta veya şifre hatalı.";

        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        if (body is null || string.IsNullOrEmpty(body.AccessToken))
            return "Giriş başarısız.";

        _state.Token = body.AccessToken;
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, body.AccessToken);
        await LoadUserAsync();
        _provider.Notify();
        return null;
    }

    public async Task LogoutAsync()
    {
        _state.Token = null;
        _state.User = new ClaimsPrincipal(new ClaimsIdentity());
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        _provider.Notify();
    }

    // /api/auth/me'den email + rolleri çekip ClaimsPrincipal oluştur
    private async Task<bool> LoadUserAsync()
    {
        try
        {
            var me = await _http.GetFromJsonAsync<MeResponse>("api/auth/me");
            if (me is null) return false;

            var claims = new List<Claim> { new(ClaimTypes.Name, me.Email ?? "") };
            foreach (var r in me.Roles ?? Array.Empty<string>())
                claims.Add(new Claim(ClaimTypes.Role, r));

            _state.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "apiauth"));
            _provider.Notify();
            return true;
        }
        catch { return false; }
    }

    private class LoginResponse { public string? AccessToken { get; set; } }
    private class MeResponse { public string? Email { get; set; } public string[]? Roles { get; set; } }
}
