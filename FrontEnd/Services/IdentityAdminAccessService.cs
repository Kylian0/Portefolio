using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace FrontEnd.Services;

public sealed class IdentityAdminAccessService(HttpClient httpClient, IJSRuntime js) : IAdminAccessService
{
    private const string TokenKey = "portfolio.admin.accessToken";
    private const string ExpiryKey = "portfolio.admin.expiresAt";
    public bool IsAuthenticated { get; private set; }

    public async Task InitializeAsync()
    {
        var token = await js.InvokeAsync<string?>("sessionStorage.getItem", TokenKey);
        var expiryValue = await js.InvokeAsync<string?>("sessionStorage.getItem", ExpiryKey);
        if (string.IsNullOrWhiteSpace(token) || !DateTimeOffset.TryParse(expiryValue, out var expiry) || expiry <= DateTimeOffset.UtcNow) { await ClearAsync(); return; }
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try { using var response = await httpClient.GetAsync("api/auth/me"); IsAuthenticated = response.IsSuccessStatusCode; if (!IsAuthenticated) await ClearAsync(); }
        catch { await ClearAsync(); }
    }

    public async Task<bool> SignInAsync(string username, string password)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/login", new { username, password });
        if (!response.IsSuccessStatusCode) { await ClearAsync(); return false; }
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken)) return false;
        var expiry = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
        await js.InvokeVoidAsync("sessionStorage.setItem", TokenKey, token.AccessToken);
        await js.InvokeVoidAsync("sessionStorage.setItem", ExpiryKey, expiry.ToString("O"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        IsAuthenticated = true;
        return true;
    }

    public Task SignOutAsync() => ClearAsync().AsTask();

    private async ValueTask ClearAsync()
    {
        IsAuthenticated = false; httpClient.DefaultRequestHeaders.Authorization = null;
        await js.InvokeVoidAsync("sessionStorage.removeItem", TokenKey); await js.InvokeVoidAsync("sessionStorage.removeItem", ExpiryKey);
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey); await js.InvokeVoidAsync("localStorage.removeItem", ExpiryKey);
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("accessToken")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expiresIn")] public long ExpiresIn { get; set; }
    }
}
