using Microsoft.Win32;

namespace MediaHub.App.Services;

public sealed class WindowsFolderPicker : IFolderPicker
{
    public string? PickFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Mediatarmappa kivalasztasa",
            Multiselect = false,
        };

        return dialog.ShowDialog() is true ? dialog.FolderName : null;
    }
}
