using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MeuGestorVODs.Models;

namespace MeuGestorVODs.Services;

public interface IM3UService
{
    Task<IEnumerable<M3UEntry>> LoadM3UAsync(string url, bool forceRefresh = false, CancellationToken cancellationToken = default);
}

public class M3UService : IM3UService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<M3UService> _logger;
    private readonly AppConfig _config;

    public M3UService(HttpClient httpClient, ILogger<M3UService> logger, AppConfig config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    public async Task<IEnumerable<M3UEntry>> LoadM3UAsync(string url, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid URL format", nameof(url));

        _logger.LogInformation("Downloading M3U from {Url}", url);
        var content = await DownloadM3UAsync(uri, cancellationToken);
        return ParseM3U(content);
    }

    private async Task<string> DownloadM3UAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(uri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to download M3U file");
            throw;
        }
    }

    private IEnumerable<M3UEntry> ParseM3U(string content)
    {
        var entries = new List<M3UEntry>();
        var lines = content.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length - 1; i++)
        {
            var line = lines[i].Trim();
            
            if (!line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
                continue;

            var nextLine = lines[i + 1].Trim();
            if (string.IsNullOrWhiteSpace(nextLine) || nextLine.StartsWith("#"))
                continue;

            var entry = ParseEntry(line, nextLine);
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        _logger.LogInformation("Parsed {Count} VOD entries from M3U", entries.Count);
        return entries;
    }

    private M3UEntry? ParseEntry(string extinfLine, string urlLine)
    {
        try
        {
            var entry = new M3UEntry
            {
                Url = urlLine
            };

            entry.Id = ExtractAttribute(extinfLine, "tvg-id") ?? Guid.NewGuid().ToString("N")[..8];
            entry.Name = ExtractAttribute(extinfLine, "tvg-name") ?? 
                        ExtractNameFromExtinf(extinfLine) ?? 
                        "Unknown";
            entry.GroupTitle = ExtractAttribute(extinfLine, "group-title") ?? "";
            entry.LogoUrl = ExtractAttribute(extinfLine, "tvg-logo") ?? "";

            if (urlLine.Contains("/movie", StringComparison.OrdinalIgnoreCase))
                entry.Type = EntryType.Movie;
            else if (urlLine.Contains("/serie", StringComparison.OrdinalIgnoreCase))
                entry.Type = EntryType.Series;
            else
                entry.Type = EntryType.Unknown;

            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse M3U entry");
            return null;
        }
    }

    private string? ExtractAttribute(string line, string attribute)
    {
        var pattern = $"{attribute}=\"([^\"]*)\"";
        var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private string? ExtractNameFromExtinf(string line)
    {
        var lastComma = line.LastIndexOf(',');
        if (lastComma >= 0 && lastComma < line.Length - 1)
        {
            return line[(lastComma + 1)..].Trim();
        }
        return null;
    }
}
