using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Lumen.Features.Updates;

/// <summary>Leest de nieuwste release van de GitHub-repository van Lumen.</summary>
public sealed class GitHubReleaseSource : IReleaseSource
{
    private const string LatestReleaseUrl = "https://api.github.com/repos/kyell182/lumen-player/releases/latest";

    private readonly HttpClient _http;

    public GitHubReleaseSource(HttpClient http) => _http = http;

    public async Task<ReleaseInfo?> GetLatestAsync(CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(LatestReleaseUrl, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var release = await response.Content.ReadFromJsonAsync<GitHubRelease>(cancellationToken);
        if (release is null || !AppVersion.TryParseTag(release.TagName, out var version)) return null;

        var assets = release.Assets.Select(a => new ReleaseAsset(a.Name, a.DownloadUrl)).ToList();
        return new ReleaseInfo(version, release.HtmlUrl, assets);
    }

    private sealed record GitHubRelease(
        [property: JsonPropertyName("tag_name")] string TagName,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("assets")] List<GitHubAsset> Assets);

    private sealed record GitHubAsset(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("browser_download_url")] string DownloadUrl);
}
