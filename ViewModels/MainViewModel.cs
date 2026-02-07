using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuGestorVODs.Models;
using MeuGestorVODs.Services;

namespace MeuGestorVODs.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IM3UService _m3uService;
    private readonly IDownloadService _downloadService;
    private readonly IPlayerService _playerService;
    private readonly AppConfig _config;

    [ObservableProperty]
    private string _m3uUrl = string.Empty;

    [ObservableProperty]
    private string _downloadPath = string.Empty;

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private int _maxParallelDownloads = 3;

    public ObservableCollection<M3UEntry> Entries { get; } = new();
    public ObservableCollection<DownloadTask> DownloadTasks { get; } = new();

    private List<M3UEntry> _allEntries = new();
    private CancellationTokenSource? _downloadCts;

    public MainViewModel()
    {
        _config = AppConfig.Load();
        _m3uUrl = _config.M3UUrl;
        _downloadPath = _config.DownloadPath;
        _maxParallelDownloads = _config.MaxParallelDownloads;
        
        _m3uService = null!;
        _downloadService = null!;
        _playerService = null!;
    }

    partial void OnFilterTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Entries.Clear();
        
        var filtered = string.IsNullOrWhiteSpace(FilterText)
            ? _allEntries
            : _allEntries.Where(e => e.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        foreach (var entry in filtered)
        {
            Entries.Add(entry);
        }
    }

    [RelayCommand]
    private async Task LoadM3UAsync()
    {
        StatusMessage = "Loading...";
        await Task.Delay(100);
        StatusMessage = "Loaded";
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var entry in Entries)
        {
            entry.IsSelected = true;
        }
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var entry in Entries)
        {
            entry.IsSelected = false;
        }
    }

    [RelayCommand]
    private void OpenGitHub()
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://github.com/wesleiandersonti/MEU_GESTOR_DE_VODS",
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);
    }
}
