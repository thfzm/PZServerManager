using System.Net.Http;

namespace PZServerManager.Services;

public static class PublicIp
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public static async Task<string> FetchAsync(CancellationToken ct = default)
    {
        var text = await Http.GetStringAsync("https://api.ipify.org", ct);
        return text.Trim();
    }
}
