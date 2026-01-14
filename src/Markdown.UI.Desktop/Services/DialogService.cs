using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Implementation of dialog service using Avalonia's StorageProvider API.
/// </summary>
internal sealed class DialogService : IDialogService
{
    /// <inheritdoc />
    public async Task<string?> ShowOpenFileDialogAsync(Visual visual, string title, string[] filters)
    {
        var topLevel = GetTopLevel(visual);
        if (topLevel is null)
        {
            return null;
        }

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = ParseFilters(filters),
        };

        var result = await topLevel.StorageProvider.OpenFilePickerAsync(options).ConfigureAwait(true);

        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    /// <inheritdoc />
    public async Task<string?> ShowSaveFileDialogAsync(Visual visual, string title, string[] filters, string? defaultFileName)
    {
        var topLevel = GetTopLevel(visual);
        if (topLevel is null)
        {
            return null;
        }

        var options = new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultFileName,
            FileTypeChoices = ParseFilters(filters),
            DefaultExtension = GetDefaultExtension(filters),
        };

        var result = await topLevel.StorageProvider.SaveFilePickerAsync(options).ConfigureAwait(true);

        return result?.Path.LocalPath;
    }

    /// <inheritdoc />
    public async Task<string?> ShowOpenFolderDialogAsync(Visual visual, string title)
    {
        var topLevel = GetTopLevel(visual);
        if (topLevel is null)
        {
            return null;
        }

        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        };

        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(options).ConfigureAwait(true);

        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    /// <inheritdoc />
    public async Task<bool> ShowConfirmationDialogAsync(Visual visual, string title, string message)
    {
        var topLevel = GetTopLevel(visual);
        if (topLevel is not Window window)
        {
            return false;
        }

        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            Content = CreateConfirmationContent(message, out var yesButton, out var noButton),
        };

        var tcs = new TaskCompletionSource<bool>();

        yesButton.Click += (_, _) =>
        {
            tcs.TrySetResult(true);
            dialog.Close();
        };

        noButton.Click += (_, _) =>
        {
            tcs.TrySetResult(false);
            dialog.Close();
        };

        dialog.Closing += (_, _) =>
        {
            tcs.TrySetResult(false);
        };

        await dialog.ShowDialog(window).ConfigureAwait(true);

        return await tcs.Task.ConfigureAwait(true);
    }

    /// <inheritdoc />
    public async Task ShowErrorDialogAsync(Visual visual, string title, string message)
    {
        await ShowMessageDialogAsync(visual, title, message, isError: true).ConfigureAwait(true);
    }

    /// <inheritdoc />
    public async Task ShowInfoDialogAsync(Visual visual, string title, string message)
    {
        await ShowMessageDialogAsync(visual, title, message, isError: false).ConfigureAwait(true);
    }

    /// <inheritdoc />
    public async Task<bool?> ShowThreeButtonConfirmationAsync(
        Visual visual,
        string title,
        string message,
        string saveButton = "Save",
        string dontSaveButton = "Don't Save",
        string cancelButton = "Cancel")
    {
        var topLevel = GetTopLevel(visual);
        if (topLevel is not Window window)
        {
            return null;
        }

        var dialog = new Window
        {
            Title = title,
            Width = 450,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            Content = CreateThreeButtonContent(
                message,
                saveButton,
                dontSaveButton,
                cancelButton,
                out var saveBtn,
                out var dontSaveBtn,
                out var cancelBtn),
        };

        var tcs = new TaskCompletionSource<bool?>();

        saveBtn.Click += (_, _) =>
        {
            tcs.TrySetResult(true);
            dialog.Close();
        };

        dontSaveBtn.Click += (_, _) =>
        {
            tcs.TrySetResult(false);
            dialog.Close();
        };

        cancelBtn.Click += (_, _) =>
        {
            tcs.TrySetResult(null);
            dialog.Close();
        };

        dialog.Closing += (_, _) =>
        {
            tcs.TrySetResult(null);
        };

        await dialog.ShowDialog(window).ConfigureAwait(true);

        return await tcs.Task.ConfigureAwait(true);
    }

    private static async Task ShowMessageDialogAsync(Visual visual, string title, string message, bool isError)
    {
        var topLevel = GetTopLevel(visual);
        if (topLevel is not Window window)
        {
            return;
        }

        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            Content = CreateMessageContent(message, isError, out var okButton),
        };

        okButton.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(window).ConfigureAwait(true);
    }

    private static TopLevel? GetTopLevel(Visual visual)
    {
        return TopLevel.GetTopLevel(visual);
    }

    private static List<FilePickerFileType> ParseFilters(string[] filters)
    {
        var fileTypes = new List<FilePickerFileType>();

        foreach (var filter in filters)
        {
            var parts = filter.Split('|');
            if (parts.Length != 2)
            {
                continue;
            }

            var name = parts[0].Trim();
            var patterns = parts[1]
                .Split(';')
                .Select(p => p.Trim())
                .ToList();

            fileTypes.Add(new FilePickerFileType(name)
            {
                Patterns = patterns,
            });
        }

        return fileTypes;
    }

    private static string? GetDefaultExtension(string[] filters)
    {
        if (filters.Length == 0)
        {
            return null;
        }

        var firstFilter = filters[0];
        var parts = firstFilter.Split('|');
        if (parts.Length != 2)
        {
            return null;
        }

        var firstPattern = parts[1].Split(';').FirstOrDefault()?.Trim();
        if (firstPattern is null || !firstPattern.StartsWith("*.", StringComparison.Ordinal))
        {
            return null;
        }

        return firstPattern[1..]; // Remove the * prefix, keep .ext
    }

    private static StackPanel CreateConfirmationContent(string message, out Button yesButton, out Button noButton)
    {
        yesButton = new Button
        {
            Content = "Yes",
            Width = 80,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };

        noButton = new Button
        {
            Content = "No",
            Width = 80,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10,
            Children = { yesButton, noButton },
        };

        return new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20,
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                },
                buttonPanel,
            },
        };
    }

    private static StackPanel CreateMessageContent(string message, bool isError, out Button okButton)
    {
        okButton = new Button
        {
            Content = "OK",
            Width = 80,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };

        return new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20,
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Foreground = isError
                        ? Avalonia.Media.Brushes.Red
                        : null,
                },
                okButton,
            },
        };
    }

    private static StackPanel CreateThreeButtonContent(
        string message,
        string saveButtonText,
        string dontSaveButtonText,
        string cancelButtonText,
        out Button saveButton,
        out Button dontSaveButton,
        out Button cancelButton)
    {
        saveButton = new Button
        {
            Content = saveButtonText,
            Width = 100,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };

        dontSaveButton = new Button
        {
            Content = dontSaveButtonText,
            Width = 100,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };

        cancelButton = new Button
        {
            Content = cancelButtonText,
            Width = 100,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10,
            Children = { saveButton, dontSaveButton, cancelButton },
        };

        return new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20,
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                },
                buttonPanel,
            },
        };
    }
}
