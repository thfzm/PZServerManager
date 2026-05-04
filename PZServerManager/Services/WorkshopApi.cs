using System.Net.Http;
using System.Text.Json;
using PZServerManager.Models;

namespace PZServerManager.Services;

public sealed class WorkshopApi
{
    private const string Endpoint = "https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/";
    private const int PzAppId = 108600;

    private readonly HttpClient _http;

    public WorkshopApi(HttpClient? http = null)
    {
        _http = http ?? new HttpClient();
    }

    public sealed class SearchResult
    {
        public List<WorkshopMod> Mods { get; init; } = new();
        public string? NextCursor { get; init; }
        public int Total { get; init; }
    }

    /// <param name="queryType">21 = ranked by text search; 9 = most subscriptions; 0 = ranked by vote.</param>
    public async Task<SearchResult> SearchAsync(
        string apiKey,
        string? searchText = null,
        string cursor = "*",
        int pageSize = 30,
        int queryType = 21,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Steam Web API key is not set.");

        // For browse mode (no search text) prefer "by subscriptions".
        if (string.IsNullOrWhiteSpace(searchText) && queryType == 21) queryType = 9;

        var url = new UriBuilder(Endpoint);
        var q = new List<string>
        {
            $"key={Uri.EscapeDataString(apiKey)}",
            $"appid={PzAppId}",
            $"query_type={queryType}",
            $"numperpage={pageSize}",
            $"cursor={Uri.EscapeDataString(cursor)}",
            "return_metadata=true",
            "return_short_description=true",
            "return_tags=true",
            "return_vote_data=true",
            "return_previews=true",
        };
        if (!string.IsNullOrWhiteSpace(searchText))
            q.Add($"search_text={Uri.EscapeDataString(searchText)}");
        url.Query = string.Join("&", q);

        using var resp = await _http.GetAsync(url.Uri, ct);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var root = doc.RootElement.GetProperty("response");
        var total = root.TryGetProperty("total", out var t) ? t.GetInt32() : 0;
        string? next = root.TryGetProperty("next_cursor", out var nc) ? nc.GetString() : null;

        var mods = new List<WorkshopMod>();
        if (root.TryGetProperty("publishedfiledetails", out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
                mods.Add(ParseMod(item));
        }

        return new SearchResult { Mods = mods, NextCursor = next, Total = total };
    }

    public async Task<WorkshopMod?> GetByIdAsync(string apiKey, long id, CancellationToken ct = default)
    {
        var url = new UriBuilder("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/");
        using var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("itemcount", "1"),
            new KeyValuePair<string, string>("publishedfileids[0]", id.ToString()),
        });
        using var resp = await _http.PostAsync(url.Uri, content, ct);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var arr = doc.RootElement.GetProperty("response").GetProperty("publishedfiledetails");
        if (arr.GetArrayLength() == 0) return null;
        return ParseMod(arr[0]);
    }

    private static WorkshopMod ParseMod(JsonElement item)
    {
        var mod = new WorkshopMod();
        if (item.TryGetProperty("publishedfileid", out var pid))
        {
            var s = pid.ValueKind == JsonValueKind.String ? pid.GetString() : pid.ToString();
            if (long.TryParse(s, out var id)) mod.PublishedFileId = id;
        }
        if (item.TryGetProperty("title", out var title)) mod.Title = title.GetString() ?? "";
        if (item.TryGetProperty("short_description", out var desc)) mod.Description = desc.GetString() ?? "";
        if (item.TryGetProperty("file_description", out var fd) && string.IsNullOrEmpty(mod.Description))
            mod.Description = fd.GetString() ?? "";
        if (item.TryGetProperty("preview_url", out var prev)) mod.PreviewUrl = prev.GetString() ?? "";
        if (item.TryGetProperty("creator", out var creator)) mod.Creator = creator.GetString() ?? "";

        if (item.TryGetProperty("subscriptions", out var subs) && subs.ValueKind == JsonValueKind.Number)
            mod.Subscriptions = subs.GetInt64();
        else if (item.TryGetProperty("lifetime_subscriptions", out var lsubs) && lsubs.ValueKind == JsonValueKind.Number)
            mod.Subscriptions = lsubs.GetInt64();

        if (item.TryGetProperty("views", out var views) && views.ValueKind == JsonValueKind.Number)
            mod.Views = views.GetInt64();

        if (item.TryGetProperty("vote_data", out var vd) && vd.TryGetProperty("score", out var score))
            mod.VoteScore = score.GetDouble();

        if (item.TryGetProperty("time_updated", out var tu) && tu.ValueKind == JsonValueKind.Number)
            mod.LastUpdated = DateTimeOffset.FromUnixTimeSeconds(tu.GetInt64()).UtcDateTime;

        if (item.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Array)
        {
            foreach (var tag in tags.EnumerateArray())
                if (tag.TryGetProperty("tag", out var tagVal))
                    mod.Tags.Add(tagVal.GetString() ?? "");
        }

        return mod;
    }
}
