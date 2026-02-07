using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MeuGestorVODs.Models;

namespace MeuGestorVODs.Services;

public interface IPlayerService
{
    Task PlayAsync(M3UEntry entry);
    bool IsPlayerAvailable();
}

public class PlayerService : IPlayerService
{
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(ILogger<PlayerService> logger)
    {
        _logger = logger;
    }

    public Task PlayAsync(M3UEntry entry)
    {
        var playerPath = FindVlcPath();
        if (string.IsNullOrEmpty(playerPath))
        {
            throw new InvalidOperationException("VLC não encontrado.");
        }

        var psi = new ProcessStartInfo
        {
            FileName = playerPath,
            Arguments = $"\"{entry.Url}\"",
            UseShellExecute = true
        };

        Process.Start(psi);
        return Task.CompletedTask;
    }

    public bool IsPlayerAvailable()
    {
        return !string.IsNullOrEmpty(FindVlcPath());
    }

    private string? FindVlcPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var possiblePaths = new[]
            {
                @"C:\Program Files\VideoLAN\VLC\vlc.exe",
                @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }
        }
        
        return null;
    }
}
