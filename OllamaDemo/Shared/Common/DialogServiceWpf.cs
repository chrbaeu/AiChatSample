using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace OllamaDemo.Shared.Common;

internal sealed class DialogServiceWpf : IDialogService
{
    public SynchronizationContext? SynchronizationContext { get; } = SynchronizationContext.Current;

    public DialogResult ShowMessageBox(object? owner, string title, string text, DialogButton buttons = DialogButton.OK)
    {
        owner = GetParentWindow(owner);
        MessageBoxResult result = MessageBox.Show(owner as Window ?? Application.Current.MainWindow, text, title, (MessageBoxButton)buttons);
        return (DialogResult)result;
    }

    public string ShowOpenFileDialog(object? owner, string? extensions = null, string? initialDirectory = null, string? fileName = null, string? title = null)
    {
        OpenFileDialog openFileDialog = new();
        if (!string.IsNullOrEmpty(fileName)) { openFileDialog.FileName = fileName; }
        if (!string.IsNullOrEmpty(extensions)) { openFileDialog.Filter = extensions; }
        if (!string.IsNullOrEmpty(title)) { openFileDialog.Title = title; }
        if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)) { openFileDialog.InitialDirectory = initialDirectory; }
        if (openFileDialog.ShowDialog(GetParentWindow(owner)) == true) { return openFileDialog.FileName ?? string.Empty; }
        return "";
    }

    public string[] ShowOpenFileMultiselectDialog(object? owner, string? extensions = null, string? initialDirectory = null, string? fileName = null, string? title = null)
    {
        OpenFileDialog openFileDialog = new() { Multiselect = true };
        if (!string.IsNullOrEmpty(fileName)) { openFileDialog.FileName = fileName; }
        if (!string.IsNullOrEmpty(extensions)) { openFileDialog.Filter = extensions; }
        if (!string.IsNullOrEmpty(title)) { openFileDialog.Title = title; }
        if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)) { openFileDialog.InitialDirectory = initialDirectory; }
        if (openFileDialog.ShowDialog(GetParentWindow(owner)) == true) { return openFileDialog.FileNames; }
        return [];
    }

    public string ShowSaveFileDialog(object? owner, string? extensions = null, string? initialDirectory = null, string? fileName = null, string? title = null)
    {
        SaveFileDialog saveFileDialog = new();
        if (!string.IsNullOrEmpty(fileName)) { saveFileDialog.FileName = fileName; }
        if (!string.IsNullOrEmpty(extensions)) { saveFileDialog.Filter = extensions; }
        if (!string.IsNullOrEmpty(title)) { saveFileDialog.Title = title; }
        if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)) { saveFileDialog.InitialDirectory = initialDirectory; }
        if (saveFileDialog.ShowDialog(GetParentWindow(owner)) == true) { return saveFileDialog.FileName ?? ""; }
        return "";
    }

    public string ShowOpenFolderDialog(object? owner, string? initialDirectory = null, string? title = null)
    {
        OpenFolderDialog openFolderDialog = new();
        if (!string.IsNullOrEmpty(title)) { openFolderDialog.Title = title; }
        if (!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)) { openFolderDialog.InitialDirectory = initialDirectory; }
        if (openFolderDialog.ShowDialog() == true) { return openFolderDialog.FolderName ?? ""; }
        return "";
    }

    public Window GetParentWindow(object? instance)
    {
        return FindParentWindow(instance as DependencyObject);
    }

    public static Window FindParentWindow(DependencyObject? view)
    {
        while (view is not null and not Window)
        {
            view = VisualTreeHelper.GetParent(view);
        }
        return (view as Window) ?? Application.Current.MainWindow;
    }

}
