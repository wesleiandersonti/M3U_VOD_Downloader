using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using MeuGestorVODs;

namespace MeuGestorVODs.ViewModels
{
    /// <summary>
    /// ViewModel principal da janela: estado e coleções para binding (MVVM).
    /// Reduz a lógica na MainWindow e centraliza propriedades observáveis.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _m3uUrl = "";
        private string _downloadPath = "";
        private string _filterText = "";
        private string _localFilePath = "";
        private string _statusMessage = "Pronto";
        private string _currentVersionText = "Versao atual: -";
        private bool _isUpdateAvailable = false;
        private string _itemCountText = "Itens: 0";
        private string _groupCountText = "Grupos: 0";
        private string _groupFilterInfoText = "";
        private string _analysisProgressText = "Pronto para analisar";
        private string _analysisSummaryText = "Analisados: 0 | ONLINE: 0 | OFFLINE: 0 | Duplicados: 0";
        private string _selectedAnalysisFilter = "Todos";
        private string _themeButtonText = "Tema: Sistema";
        private string _downloadActionButtonText = "Baixar Selecionados";
        private Visibility _groupPanelVisibility = Visibility.Collapsed;
        private bool _isLoading = false;
        private bool _isAnalyzingLinks;
        private bool _isLocalFileDragOver;
        private double _analysisProgressValue;
        private M3UEntry _selectedEntry = new M3UEntry();

        /// <summary>Delegate para obter a versão atual (evita acoplamento com MainWindow).</summary>
        public Func<string>? GetVersion { get; set; }

        public ObservableCollection<M3UEntry> Entries { get; set; } = new ObservableCollection<M3UEntry>();
        public ObservableCollection<M3UEntry> FilteredEntries { get; set; } = new ObservableCollection<M3UEntry>();
        public ObservableCollection<DownloadItem> Downloads { get; set; } = new ObservableCollection<DownloadItem>();
        public ObservableCollection<string> LocalFileHistory { get; } = new ObservableCollection<string>();
        public ObservableCollection<GroupCategoryItem> GroupCategories { get; set; } = new ObservableCollection<GroupCategoryItem>();
        public ObservableCollection<ServerScoreResult> ServerScores { get; set; } = new ObservableCollection<ServerScoreResult>();
        public ObservableCollection<string> AnalysisFilterOptions { get; } = new ObservableCollection<string>
        {
            "Todos",
            "ONLINE",
            "OFFLINE",
            "Duplicados"
        };

        public string M3UUrl
        {
            get => _m3uUrl;
            set { _m3uUrl = value; OnPropertyChanged(nameof(M3UUrl)); }
        }

        public string DownloadPath
        {
            get => _downloadPath;
            set { _downloadPath = value; OnPropertyChanged(nameof(DownloadPath)); }
        }

        public string LocalFilePath
        {
            get => _localFilePath;
            set { _localFilePath = value; OnPropertyChanged(nameof(LocalFilePath)); }
        }

        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; OnPropertyChanged(nameof(FilterText)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public string CurrentVersionText
        {
            get => _currentVersionText;
            set { _currentVersionText = value; OnPropertyChanged(nameof(CurrentVersionText)); }
        }

        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set { _isUpdateAvailable = value; OnPropertyChanged(nameof(IsUpdateAvailable)); }
        }

        public string WindowTitle => $"MEU GESTOR DE VODS v{GetVersion?.Invoke() ?? "?"}";

        public string ItemCountText
        {
            get => _itemCountText;
            set { _itemCountText = value; OnPropertyChanged(nameof(ItemCountText)); }
        }

        public string GroupCountText
        {
            get => _groupCountText;
            set { _groupCountText = value; OnPropertyChanged(nameof(GroupCountText)); }
        }

        public string GroupFilterInfoText
        {
            get => _groupFilterInfoText;
            set { _groupFilterInfoText = value; OnPropertyChanged(nameof(GroupFilterInfoText)); }
        }

        public string AnalysisProgressText
        {
            get => _analysisProgressText;
            set { _analysisProgressText = value; OnPropertyChanged(nameof(AnalysisProgressText)); }
        }

        public string AnalysisSummaryText
        {
            get => _analysisSummaryText;
            set { _analysisSummaryText = value; OnPropertyChanged(nameof(AnalysisSummaryText)); }
        }

        public string SelectedAnalysisFilter
        {
            get => _selectedAnalysisFilter;
            set { _selectedAnalysisFilter = value; OnPropertyChanged(nameof(SelectedAnalysisFilter)); }
        }

        public string ThemeButtonText
        {
            get => _themeButtonText;
            set { _themeButtonText = value; OnPropertyChanged(nameof(ThemeButtonText)); }
        }

        public string DownloadActionButtonText
        {
            get => _downloadActionButtonText;
            set { _downloadActionButtonText = value; OnPropertyChanged(nameof(DownloadActionButtonText)); }
        }

        public double AnalysisProgressValue
        {
            get => _analysisProgressValue;
            set { _analysisProgressValue = value; OnPropertyChanged(nameof(AnalysisProgressValue)); }
        }

        public bool IsAnalyzingLinks
        {
            get => _isAnalyzingLinks;
            set { _isAnalyzingLinks = value; OnPropertyChanged(nameof(IsAnalyzingLinks)); }
        }

        public bool IsLocalFileDragOver
        {
            get => _isLocalFileDragOver;
            set { _isLocalFileDragOver = value; OnPropertyChanged(nameof(IsLocalFileDragOver)); }
        }

        public Visibility GroupPanelVisibility
        {
            get => _groupPanelVisibility;
            set { _groupPanelVisibility = value; OnPropertyChanged(nameof(GroupPanelVisibility)); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public M3UEntry SelectedEntry
        {
            get => _selectedEntry;
            set { _selectedEntry = value; OnPropertyChanged(nameof(SelectedEntry)); }
        }

        /// <summary>Notifica que o título da janela deve ser reavaliado (ex.: após atualização).</summary>
        public void RefreshWindowTitle()
        {
            OnPropertyChanged(nameof(WindowTitle));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
