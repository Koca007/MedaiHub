using System.Collections.ObjectModel;
using System.Windows.Input;
using MediaHub.App.Presentation;
using MediaHub.App.Services;
using MediaHub.Application.Library;
using MediaHub.Application.Scanning;
using MediaHub.Domain.Library;
using MediaHub.Scanner.Pipeline;

namespace MediaHub.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly LibraryRootCatalog _catalog;
    private readonly IMediaFileRepository _mediaRepository;
    private readonly LibraryScanner _scanner;
    private readonly IFolderPicker _folderPicker;
    private readonly List<MediaFileItemViewModel> _allMedia = [];
    private readonly ScanControl _scanControl = new();
    private CancellationTokenSource? _scanCancellation;
    private string _currentSection = "Home";
    private string _pageHeading = "Kezd\u0151lap";
    private string _statusMessage = "A helyi m\u00e9diat\u00e1r inicializ\u00e1l\u00e1sa...";
    private string _scanStatusText = "Nincs akt\u00edv vizsg\u00e1lat.";
    private string? _currentScanPath;
    private bool _isBusy;
    private bool _isScanning;
    private bool _isScanPaused;
    private int _discoveredFiles;
    private int _indexedFiles;
    private int _skippedFiles;
    private LibraryRootKindOption _selectedKind;

    public MainWindowViewModel(
        LibraryRootCatalog catalog,
        IMediaFileRepository mediaRepository,
        LibraryScanner scanner,
        IFolderPicker folderPicker,
        bool safeMode)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(mediaRepository);
        ArgumentNullException.ThrowIfNull(scanner);
        ArgumentNullException.ThrowIfNull(folderPicker);

        _catalog = catalog;
        _mediaRepository = mediaRepository;
        _scanner = scanner;
        _folderPicker = folderPicker;
        SafeMode = safeMode;

        KindOptions =
        [
            new(LibraryRootKind.AutoDetect, "Automatikus felismer\u00e9s"),
            new(LibraryRootKind.Movies, "Filmek"),
            new(LibraryRootKind.Series, "Sorozatok"),
            new(LibraryRootKind.Mixed, "Vegyes"),
        ];
        _selectedKind = KindOptions[0];

        NavigateCommand = new RelayCommand(Navigate);
        AddFolderCommand = new AsyncRelayCommand(AddFolderAsync, () => !IsBusy && !IsScanning);
        ScanAllCommand = new AsyncRelayCommand(ScanAllAsync, () => HasLibraryRoots && !IsScanning);
        PauseScanCommand = new RelayCommand(
            _ => PauseScan(),
            _ => IsScanning && !IsScanPaused);
        ResumeScanCommand = new RelayCommand(
            _ => ResumeScan(),
            _ => IsScanning && IsScanPaused);
        CancelScanCommand = new RelayCommand(
            _ => _scanCancellation?.Cancel(),
            _ => IsScanning);
    }

    public ObservableCollection<LibraryRootItemViewModel> LibraryRoots { get; } = [];

    public ObservableCollection<MediaFileItemViewModel> VisibleMedia { get; } = [];

    public IReadOnlyList<LibraryRootKindOption> KindOptions { get; }

    public ICommand NavigateCommand { get; }

    public AsyncRelayCommand AddFolderCommand { get; }

    public AsyncRelayCommand ScanAllCommand { get; }

    public RelayCommand PauseScanCommand { get; }

    public RelayCommand ResumeScanCommand { get; }

    public RelayCommand CancelScanCommand { get; }

    public bool SafeMode { get; }

    public string PageHeading
    {
        get => _pageHeading;
        private set => SetProperty(ref _pageHeading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ScanStatusText
    {
        get => _scanStatusText;
        private set => SetProperty(ref _scanStatusText, value);
    }

    public string? CurrentScanPath
    {
        get => _currentScanPath;
        private set => SetProperty(ref _currentScanPath, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                NotifyCommandStates();
            }
        }
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (SetProperty(ref _isScanning, value))
            {
                NotifyCommandStates();
            }
        }
    }

    public bool IsScanPaused
    {
        get => _isScanPaused;
        private set
        {
            if (SetProperty(ref _isScanPaused, value))
            {
                NotifyCommandStates();
            }
        }
    }

    public int DiscoveredFiles
    {
        get => _discoveredFiles;
        private set => SetProperty(ref _discoveredFiles, value);
    }

    public int IndexedFiles
    {
        get => _indexedFiles;
        private set => SetProperty(ref _indexedFiles, value);
    }

    public int SkippedFiles
    {
        get => _skippedFiles;
        private set => SetProperty(ref _skippedFiles, value);
    }

    public LibraryRootKindOption SelectedKind
    {
        get => _selectedKind;
        set => SetProperty(ref _selectedKind, value);
    }

    public bool IsHome => _currentSection == "Home";

    public bool IsSettings => _currentSection == "Settings";

    public bool IsBrowse => _currentSection is "Movies" or "Series";

    public bool IsPlaceholder => !IsHome && !IsSettings && !IsBrowse;

    public bool HasLibraryRoots => LibraryRoots.Count > 0;

    public bool IsLibraryEmpty => !HasLibraryRoots;

    public bool IsBrowseEmpty => VisibleMedia.Count == 0;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        IReadOnlyList<LibraryRootSummary> roots;
        try
        {
            roots = await _catalog.GetAllAsync(cancellationToken);
            LibraryRoots.Clear();
            foreach (var root in roots)
            {
                LibraryRoots.Add(LibraryRootItemViewModel.FromSummary(root));
            }

            await LoadRecentMediaAsync(cancellationToken);
            NotifyLibraryStateChanged();
            StatusMessage = SafeMode
                ? "Cs\u00f6kkentett m\u00f3d akt\u00edv. Az automatikus vizsg\u00e1lat nem indul el."
                : HasLibraryRoots
                    ? $"{LibraryRoots.Count} m\u00e9diat\u00e1rmappa k\u00e9szen \u00e1ll."
                    : "Adj hozz\u00e1 egy mapp\u00e1t a kezd\u00e9shez.";
        }
        finally
        {
            IsBusy = false;
        }

        if (!SafeMode && roots.Count > 0)
        {
            await RunScansAsync(roots);
        }
    }

    public void Dispose()
    {
        _scanCancellation?.Cancel();
        GC.SuppressFinalize(this);
    }

    private void Navigate(object? parameter)
    {
        _currentSection = parameter as string ?? "Home";
        PageHeading = _currentSection switch
        {
            "Movies" => "Filmek",
            "Series" => "Sorozatok",
            "Collections" => "Gy\u0171jtem\u00e9nyek",
            "Playlists" => "Lej\u00e1tsz\u00e1si list\u00e1k",
            "Settings" => "Be\u00e1ll\u00edt\u00e1sok",
            _ => "Kezd\u0151lap",
        };

        ApplyBrowseFilter();
        OnPropertyChanged(nameof(IsHome));
        OnPropertyChanged(nameof(IsSettings));
        OnPropertyChanged(nameof(IsBrowse));
        OnPropertyChanged(nameof(IsPlaceholder));
    }

    private async Task AddFolderAsync()
    {
        var selectedPath = _folderPicker.PickFolder();
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        LibraryRootSummary? addedRoot = null;
        IsBusy = true;
        StatusMessage = "A mappa hozz\u00e1f\u00e9r\u00e9s\u00e9nek ellen\u0151rz\u00e9se...";
        try
        {
            var result = await _catalog.AddAsync(selectedPath, SelectedKind.Kind);
            if (result.IsSuccess)
            {
                addedRoot = result.Root!;
                LibraryRoots.Add(LibraryRootItemViewModel.FromSummary(addedRoot));
                NotifyLibraryStateChanged();
                StatusMessage = "A m\u00e9diat\u00e1rmappa mentve. A vizsg\u00e1lat indul.";
            }
            else
            {
                StatusMessage = FailureMessage(result.Failure);
            }
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            StatusMessage = "A helyi adatb\u00e1zis nem tudta menteni a mapp\u00e1t.";
        }
        finally
        {
            IsBusy = false;
        }

        if (addedRoot is not null)
        {
            await RunScansAsync([addedRoot]);
        }
    }

    private async Task ScanAllAsync()
    {
        var roots = await _catalog.GetAllAsync();
        await RunScansAsync(roots);
    }

    private async Task RunScansAsync(IReadOnlyList<LibraryRootSummary> roots)
    {
        if (IsScanning || roots.Count == 0)
        {
            return;
        }

        var scanCancellation = new CancellationTokenSource();
        _scanCancellation = scanCancellation;
        IsScanning = true;
        IsScanPaused = false;
        DiscoveredFiles = 0;
        IndexedFiles = 0;
        SkippedFiles = 0;
        var totalDiscovered = 0;
        var totalProcessed = 0;
        var totalIndexed = 0;
        var totalSkipped = 0;

        try
        {
            foreach (var root in roots.Where(root => root.IsEnabled))
            {
                var discoveredBeforeRoot = totalDiscovered;
                var processedBeforeRoot = totalProcessed;
                var indexedBeforeRoot = totalIndexed;
                var skippedBeforeRoot = totalSkipped;
                var progress = new Progress<ScanProgress>(
                    update => HandleScanProgress(
                        update with
                        {
                            DiscoveredFiles = discoveredBeforeRoot + update.DiscoveredFiles,
                            ProcessedFiles = processedBeforeRoot + update.ProcessedFiles,
                            IndexedFiles = indexedBeforeRoot + update.IndexedFiles,
                            SkippedFiles = skippedBeforeRoot + update.SkippedFiles,
                        }));
                var outcome = await _scanner.ScanAsync(
                    root,
                    _scanControl,
                    progress,
                    scanCancellation.Token);
                totalDiscovered += outcome.DiscoveredFiles;
                totalProcessed += outcome.ProcessedFiles;
                totalIndexed += outcome.IndexedFiles;
                totalSkipped += outcome.SkippedFiles;
                if (outcome.Phase == ScanPhase.Cancelled)
                {
                    break;
                }

                if (outcome.Phase == ScanPhase.Failed)
                {
                    ScanStatusText = ScanFailureMessage(outcome.ErrorCode);
                }
            }

            DiscoveredFiles = totalDiscovered;
            IndexedFiles = totalIndexed;
            SkippedFiles = totalSkipped;
            await LoadRecentMediaAsync(CancellationToken.None);
            StatusMessage = totalIndexed > 0
                ? $"{totalIndexed} m\u00e9diaf\u00e1jl indexelve."
                : StatusMessage;
        }
        finally
        {
            _scanControl.Resume();
            scanCancellation.Dispose();
            if (ReferenceEquals(_scanCancellation, scanCancellation))
            {
                _scanCancellation = null;
            }
            IsScanPaused = false;
            IsScanning = false;
            CurrentScanPath = null;
        }
    }

    private async Task LoadRecentMediaAsync(CancellationToken cancellationToken)
    {
        var mediaFiles = await _mediaRepository.GetRecentAsync(500, cancellationToken);
        _allMedia.Clear();
        _allMedia.AddRange(mediaFiles.Select(MediaFileItemViewModel.FromSummary));
        ApplyBrowseFilter();
    }

    private void HandleScanProgress(ScanProgress progress)
    {
        DiscoveredFiles = progress.DiscoveredFiles;
        IndexedFiles = progress.IndexedFiles;
        SkippedFiles = progress.SkippedFiles;
        CurrentScanPath = progress.CurrentPath;
        IsScanPaused = progress.Phase == ScanPhase.Paused;
        ScanStatusText = progress.Phase switch
        {
            ScanPhase.Enumerating => "Mapp\u00e1k bej\u00e1r\u00e1sa...",
            ScanPhase.Processing => "M\u00e9diaf\u00e1jlok feldolgoz\u00e1sa...",
            ScanPhase.Paused => "A vizsg\u00e1lat sz\u00fcnetel.",
            ScanPhase.Completed => "A vizsg\u00e1lat befejez\u0151d\u00f6tt.",
            ScanPhase.Cancelled => "A vizsg\u00e1lat megszak\u00edtva. A mentett elemek konzisztensek.",
            ScanPhase.Failed => ScanFailureMessage(progress.ErrorCode),
            _ => ScanStatusText,
        };
    }

    private void PauseScan()
    {
        _scanControl.Pause();
        IsScanPaused = true;
        ScanStatusText = "A vizsg\u00e1lat sz\u00fcnetel.";
    }

    private void ResumeScan()
    {
        _scanControl.Resume();
        IsScanPaused = false;
        ScanStatusText = "A vizsg\u00e1lat folytat\u00f3dik...";
    }

    private void ApplyBrowseFilter()
    {
        VisibleMedia.Clear();
        var expectedKind = _currentSection == "Series"
            ? MediaCandidateKind.Episode
            : MediaCandidateKind.Movie;
        if (_currentSection is "Movies" or "Series")
        {
            foreach (var mediaFile in _allMedia.Where(item => item.CandidateKind == expectedKind))
            {
                VisibleMedia.Add(mediaFile);
            }
        }

        OnPropertyChanged(nameof(IsBrowseEmpty));
    }

    private void NotifyLibraryStateChanged()
    {
        OnPropertyChanged(nameof(HasLibraryRoots));
        OnPropertyChanged(nameof(IsLibraryEmpty));
        NotifyCommandStates();
    }

    private void NotifyCommandStates()
    {
        AddFolderCommand.NotifyCanExecuteChanged();
        ScanAllCommand.NotifyCanExecuteChanged();
        PauseScanCommand.NotifyCanExecuteChanged();
        ResumeScanCommand.NotifyCanExecuteChanged();
        CancelScanCommand.NotifyCanExecuteChanged();
    }

    private static string FailureMessage(AddLibraryRootFailure failure) =>
        failure switch
        {
            AddLibraryRootFailure.NotFound => "A kiv\u00e1lasztott mappa nem tal\u00e1lhat\u00f3.",
            AddLibraryRootFailure.AccessDenied => "A MediaHub nem tudja olvasni ezt a mapp\u00e1t.",
            AddLibraryRootFailure.Duplicate => "Ez a mappa m\u00e1r szerepel a m\u00e9diat\u00e1rban.",
            AddLibraryRootFailure.InputOutputError => "A meghajt\u00f3 jelenleg nem \u00e9rhet\u0151 el.",
            _ => "A kiv\u00e1lasztott mappa el\u00e9r\u00e9si \u00fatja \u00e9rv\u00e9nytelen.",
        };

    private static string ScanFailureMessage(string? errorCode) =>
        errorCode switch
        {
            "root_access_denied" => "A gy\u00f6k\u00e9rmappa nem olvashat\u00f3.",
            "root_unavailable" => "A gy\u00f6k\u00e9rmappa jelenleg nem \u00e9rhet\u0151 el. Semmi nem lett t\u00f6r\u00f6lve.",
            "file_access_denied" => "Egy m\u00e9diaf\u00e1jl nem olvashat\u00f3.",
            _ => "A f\u00e1jlrendszer vizsg\u00e1lata nem fejez\u0151d\u00f6tt be.",
        };
}
