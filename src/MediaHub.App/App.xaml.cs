using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Threading;
using MediaHub.App.Services;
using MediaHub.App.ViewModels;
using MediaHub.Application.Library;
using MediaHub.Infrastructure.Local;
using MediaHub.Infrastructure.Local.Library;
using MediaHub.Infrastructure.Local.Persistence;
using Microsoft.Data.Sqlite;

namespace MediaHub.App;

public partial class App : System.Windows.Application, IDisposable
{
    private Mutex? _singleInstanceMutex;
    private LocalDatabase? _localDatabase;
    private bool _ownsSingleInstanceMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(true, @"Local\MediaHub.Desktop", out var isFirstInstance);
        _ownsSingleInstanceMutex = isFirstInstance;
        if (!isFirstInstance)
        {
            MessageBox.Show(
                "A MediaHub m\u00e1r fut.",
                "MediaHub",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += HandleUnhandledException;

        try
        {
            var paths = LocalDataPaths.CreateDefault();
            var database = new LocalDatabase(paths);
            _localDatabase = database;
            await database.InitializeAsync();

            var repository = new SqliteLibraryRootRepository(database);
            var inspector = new FileSystemLibraryPathInspector();
            var catalog = new LibraryRootCatalog(repository, inspector, TimeProvider.System);
            var safeMode = e.Args.Any(
                argument => string.Equals(argument, "--safe-mode", StringComparison.OrdinalIgnoreCase));
            var viewModel = new MainWindowViewModel(
                catalog,
                new WindowsFolderPicker(),
                safeMode);

            var window = new MainWindow(viewModel);
            MainWindow = window;
            window.Show();
            await viewModel.InitializeAsync();
        }
        catch (SqliteException)
        {
            FailStartup("A helyi adatb\u00e1zis nem nyithat\u00f3 meg.");
        }
        catch (UnauthorizedAccessException)
        {
            FailStartup("A MediaHub nem f\u00e9r hozz\u00e1 a helyi adatmapp\u00e1hoz.");
        }
        catch (SecurityException)
        {
            FailStartup("A MediaHub nem f\u00e9r hozz\u00e1 a helyi adatmapp\u00e1hoz.");
        }
        catch (IOException)
        {
            FailStartup("A helyi adatok inicializ\u00e1l\u00e1sa sikertelen.");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Dispose();
        base.OnExit(e);
    }

    public void Dispose()
    {
        _localDatabase?.Dispose();
        _localDatabase = null;

        if (_singleInstanceMutex is not null)
        {
            if (_ownsSingleInstanceMutex)
            {
                _singleInstanceMutex.ReleaseMutex();
            }

            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
            _ownsSingleInstanceMutex = false;
        }

        GC.SuppressFinalize(this);
    }

    private static void HandleUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        _ = sender;
        MessageBox.Show(
            "V\u00e1ratlan hiba t\u00f6rt\u00e9nt. Az adataid a helyi adatb\u00e1zisban megmaradtak.",
            "MediaHub",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void FailStartup(string message)
    {
        MessageBox.Show(message, "MediaHub", MessageBoxButton.OK, MessageBoxImage.Error);
        Shutdown(-1);
    }
}
