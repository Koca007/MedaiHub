using MediaHub.Application.Library;
using MediaHub.Domain.Library;

namespace MediaHub.App.ViewModels;

public sealed record LibraryRootItemViewModel(
    Guid Id,
    string Name,
    string Path,
    string KindLabel,
    string AvailabilityLabel,
    bool IsEnabled)
{
    public static LibraryRootItemViewModel FromSummary(LibraryRootSummary root)
    {
        var trimmedPath = System.IO.Path.TrimEndingDirectorySeparator(root.Path);
        var name = System.IO.Path.GetFileName(trimmedPath);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = root.Path;
        }

        return new LibraryRootItemViewModel(
            root.Id,
            name,
            root.Path,
            KindLabelFor(root.Kind),
            root.Availability == LibraryRootAvailability.Available
                ? "El\u00e9rhet\u0151"
                : "Nem el\u00e9rhet\u0151",
            root.IsEnabled);
    }

    private static string KindLabelFor(LibraryRootKind kind) =>
        kind switch
        {
            LibraryRootKind.Movies => "Filmek",
            LibraryRootKind.Series => "Sorozatok",
            LibraryRootKind.Mixed => "Vegyes",
            _ => "Automatikus felismer\u00e9s",
        };
}
