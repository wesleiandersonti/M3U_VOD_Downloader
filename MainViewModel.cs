using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace MeuGestorVODs;

public class MainViewModel : INotifyPropertyChanged
{
    private string _m3uUrl = string.Empty;
    private string _downloadPath = string.Empty;
    private string _filterText = string.Empty;
    private string _statusMessage = "Pronto";
    private string _currentVersionText = "Versao atual: -";
    private string _itemCountText = "Itens: 0";
    private string _groupCountText = "Grupos: 0";
    private string _groupFilterInfoText = string.Empty;
    private Visibility _groupPanelVisibility = Visibility.Collapsed;
    private bool _isLoading;
    private M3UEntry _selectedEntry = new();

    public ObservableCollection<M3UEntry> Entries { get; } = new();
    public ObservableCollection<M3UEntry> FilteredEntries { get; } = new();
    public ObservableCollection<DownloadItem> Downloads { get; } = new();
    public ObservableCollection<GroupCategoryItem> GroupCategories { get; } = new();

    public string M3UUrl { get => _m3uUrl; set => Set(ref _m3uUrl, value, nameof(M3UUrl)); }
    public string DownloadPath { get => _downloadPath; set => Set(ref _downloadPath, value, nameof(DownloadPath)); }
    public string FilterText { get => _filterText; set => Set(ref _filterText, value, nameof(FilterText)); }
    public string StatusMessage { get => _statusMessage; set => Set(ref _statusMessage, value, nameof(StatusMessage)); }
    public string CurrentVersionText { get => _currentVersionText; set => Set(ref _currentVersionText, value, nameof(CurrentVersionText)); }
    public string ItemCountText { get => _itemCountText; set => Set(ref _itemCountText, value, nameof(ItemCountText)); }
    public string GroupCountText { get => _groupCountText; set => Set(ref _groupCountText, value, nameof(GroupCountText)); }
    public string GroupFilterInfoText { get => _groupFilterInfoText; set => Set(ref _groupFilterInfoText, value, nameof(GroupFilterInfoText)); }
    public Visibility GroupPanelVisibility { get => _groupPanelVisibility; set => Set(ref _groupPanelVisibility, value, nameof(GroupPanelVisibility)); }
    public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value, nameof(IsLoading)); }
    public M3UEntry SelectedEntry { get => _selectedEntry; set => Set(ref _selectedEntry, value, nameof(SelectedEntry)); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, string propertyName)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
