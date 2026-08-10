using System.Collections.ObjectModel;
using System.Windows.Input;
using MediaHub.App.Presentation;
using MediaHub.App.Services;
using MediaHub.Application.Library;
using MediaHub.Domain.Library;

namespace MediaHub.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly LibraryRootCatalog _catalog;
    private readonly IFolderPicker _folderPicker;
    private string _currentSection = "Home";
    private string _pageHeading = "Kezd\u0151lap";
    private string _statusMessage = "A helyi m\u00e9diat\u00e1r inicializ\u00e1l\u00e1sa...";
    private bool _isBusy;
    private LibraryRootKindOption _selectedKind;

    public MainWindowViewModel(
        LibraryRootCatalog catalog,
        IFolderPicker folderPicker,
        bool safeMode)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(folderPicker);

        _catalog = catalog;
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
        AddFolderCommand = new AsyncRelayCommand(AddFolderAsync, () => !IsBusy);
    }

    public ObservableCollection<LibraryRootItemViewModel> LibraryRoots { get; } = [];

    public IReadOnlyList<LibraryRootKindOption> KindOptions { get; }

    public ICommand NavigateCommand { get; }

    public AsyncRelayCommand AddFolderCommand { get; }

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

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                AddFolderCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public LibraryRootKindOption SelectedKind
    {
        get => _selectedKind;
        set => SetProperty(ref _selectedKind, value);
    }

    public bool IsHome => _currentSection == "Home";

    public bool IsSettings => _currentSection == "Settings";

    public bool IsPlaceholder => !IsHome && !IsSettings;

    public bool HasLibraryRoots => LibraryRoots.Count > 0;

    public bool IsLibraryEmpty => !HasLibraryRoots;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        try
        {
            var roots = await _catalog.GetAllAsync(cancellationToken);
            LibraryRoots.Clear();
            foreach (var root in roots)
            {
                LibraryRoots.Add(LibraryRootItemViewModel.FromSummary(root));
            }

            NotifyLibraryStateChanged();
            StatusMessage = SafeMode
                ? "Cs\u00f6kkentett m\u00f3d akt\u00edv. A kieg\u00e9sz\u00edt\u0151k nem indulnak el."
                : HasLibraryRoots
                    ? $"{LibraryRoots.Count} m\u00e9diat\u00e1rmappa k\u00e9szen \u00e1ll."
                    : "Adj hozz\u00e1 egy mapp\u00e1t a kezd\u00e9shez.";
        }
        finally
        {
            IsBusy = false;
        }
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

        OnPropertyChanged(nameof(IsHome));
        OnPropertyChanged(nameof(IsSettings));
        OnPropertyChanged(nameof(IsPlaceholder));
    }

    private async Task AddFolderAsync()
    {
        var selectedPath = _folderPicker.PickFolder();
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        IsBusy = true;
        StatusMessage = "A mappa hozz\u00e1f\u00e9r\u00e9s\u00e9nek ellen\u0151rz\u00e9se...";
        try
        {
            var result = await _catalog.AddAsync(selectedPath, SelectedKind.Kind);
            if (result.IsSuccess)
            {
                LibraryRoots.Add(LibraryRootItemViewModel.FromSummary(result.Root!));
                NotifyLibraryStateChanged();
                StatusMessage = "A m\u00e9diat\u00e1rmappa mentve. A vizsg\u00e1lat a k\u00f6vetkez\u0151 l\u00e9p\u00e9sben indul.";
                return;
            }

            StatusMessage = FailureMessage(result.Failure);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            StatusMessage = "A helyi adatb\u00e1zis nem tudta menteni a mapp\u00e1t.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifyLibraryStateChanged()
    {
        OnPropertyChanged(nameof(HasLibraryRoots));
        OnPropertyChanged(nameof(IsLibraryEmpty));
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
}
