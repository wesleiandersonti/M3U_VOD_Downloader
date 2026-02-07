using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MeuGestorVODs.Services;

public interface IUpdateService
{
    Task CheckForUpdatesAsync(bool silent = false);
}

public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UpdateService> _logger;
    private const string GitHubApiUrl = "https://api.github.com/repos/wesleiandersonti/MEU_GESTOR_DE_VODS/releases/latest";

    public UpdateService(HttpClient httpClient, ILogger<UpdateService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task CheckForUpdatesAsync(bool silent = false)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MeuGestorVODs/1.0");
            var response = await _httpClient.GetAsync(GitHubApiUrl);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Update check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for updates");
        }
    }
}
