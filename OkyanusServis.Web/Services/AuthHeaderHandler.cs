using System.Net.Http.Headers;

namespace OkyanusServis.Web.Services;

/// <summary>Her giden isteğe "Authorization: Bearer {token}" başlığını ekler.</summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly AuthState _state;
    public AuthHeaderHandler(AuthState state) => _state = state;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_state.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _state.Token);

        return base.SendAsync(request, cancellationToken);
    }
}
