using Microsoft.AspNetCore.Components.Authorization;

namespace OkyanusServis.Web.Services;

/// <summary>Blazor'a "kim giriş yapmış" bilgisini verir. AuthorizeView / [Authorize] bunu kullanır.</summary>
public class ApiAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthState _state;
    public ApiAuthStateProvider(AuthState state) => _state = state;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_state.User));

    // Giriş/çıkış olunca UI'ı tazelemek için
    public void Notify() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
