using System.Collections.ObjectModel;

using Avalonia;

using Markdown.Application.Services;
using Markdown.Desktop.Services;
using Markdown.Domain.Entities;
using Markdown.Domain.ValueObjects;
using Markdown.UI.Desktop.Models;
using Markdown.UI.Desktop.Services;

namespace Markdown.UI.Desktop.ViewModels;

/// <summary>
/// Main ViewModel orchestrating the application's primary window.
/// </summary>
internal sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IDialogService _dialogService;
    private readonly IAutoSaveService _autoSaveService;
    private readonly IThemeService _themeService;
    private readonly ISettingsService _settingsService;

    private Visual? _visual;
    private TabViewModel? _previousActiveTab;

    /// <summary>
    /// Indicates whether the sidebar is visible.
    /// </summary>
    [ObservableProperty]
    private bool _isSidebarVisible = true;

    /// <summary>
    /// Collection of open document tabs.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TabViewModel> _tabs = [];

    /// <summary>
    /// The currently active tab.
    /// </summary>
    [ObservableProperty]
    private TabViewModel? _activeTab;

    /// <summary>
    /// The file explorer ViewModel.
    /// </summary>
    public FileExplorerViewModel FileExplorer { get; }

    /// <summary>
    /// The status bar ViewModel.
    /// </summary>
    public StatusBarViewModel StatusBar { get; }

    /// <summary>
    /// Markdown file filter for file dialogs.
    /// </summary>
    private static readonly string[] MarkdownFilters = ["Markdown Files|*.md;*.markdown", "All Files|*.*"];

    /// <summary>
    /// Creates a new MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel(
        IDocumentService documentService,
        IDialogService dialogService,
        IAutoSaveService autoSaveService,
        IThemeService themeService,
        ISettingsService settingsService,
        IWorkspaceExplorer workspaceExplorer,
        IFileWatcherService fileWatcherService)
    {
        _documentService = documentService;
        _dialogService = dialogService;
        _autoSaveService = autoSaveService;
        _themeService = themeService;
        _settingsService = settingsService;

        // Initialize child ViewModels
        FileExplorer = new FileExplorerViewModel(workspaceExplorer, fileWatcherService);
        StatusBar = new StatusBarViewModel();

        // Wire up file explorer events
        FileExplorer.FileOpenRequested += OnFileOpenRequested;

        // Wire up auto-save events
        _autoSaveService.SaveCompleted += OnAutoSaveCompleted;
        _autoSaveService.SaveFailed += OnAutoSaveFailed;

        // Initialize settings
        var settings = _settingsService.Load();
        IsSidebarVisible = settings.IsSidebarVisible;
        StatusBar.UpdateAutoSaveStatus(settings.AutoSaveEnabled);
        _autoSaveService.IsEnabled = settings.AutoSaveEnabled;
        _autoSaveService.DelayMilliseconds = settings.AutoSaveDelayMs;

        // Initialize commands
        NewFileCommand = new AsyncRelayCommand(NewFileAsync);
        OpenFileCommand = new AsyncRelayCommand(OpenFileAsync);
        OpenFolderCommand = new AsyncRelayCommand(OpenFolderAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        SaveAsCommand = new AsyncRelayCommand(SaveAsAsync, CanSave);
        CloseTabCommand = new AsyncRelayCommand<TabViewModel>(CloseTabAsync);
        NextTabCommand = new RelayCommand(NextTab, () => Tabs.Count > 1);
        PreviousTabCommand = new RelayCommand(PreviousTab, () => Tabs.Count > 1);
        ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
    }

    #region Commands

    /// <summary>
    /// Command to create a new untitled document.
    /// </summary>
    public IAsyncRelayCommand NewFileCommand { get; }

    /// <summary>
    /// Command to open a file via dialog.
    /// </summary>
    public IAsyncRelayCommand OpenFileCommand { get; }

    /// <summary>
    /// Command to open a workspace folder.
    /// </summary>
    public IAsyncRelayCommand OpenFolderCommand { get; }

    /// <summary>
    /// Command to save the current document.
    /// </summary>
    public IAsyncRelayCommand SaveCommand { get; }

    /// <summary>
    /// Command to save the current document with a new name.
    /// </summary>
    public IAsyncRelayCommand SaveAsCommand { get; }

    /// <summary>
    /// Command to close a tab.
    /// </summary>
    public IAsyncRelayCommand<TabViewModel> CloseTabCommand { get; }

    /// <summary>
    /// Command to switch to the next tab.
    /// </summary>
    public IRelayCommand NextTabCommand { get; }

    /// <summary>
    /// Command to switch to the previous tab.
    /// </summary>
    public IRelayCommand PreviousTabCommand { get; }

    /// <summary>
    /// Command to toggle sidebar visibility.
    /// </summary>
    public IRelayCommand ToggleSidebarCommand { get; }

    /// <summary>
    /// Command to toggle between light and dark themes.
    /// </summary>
    public IRelayCommand ToggleThemeCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the visual context for dialog operations.
    /// Call this from the View's code-behind after initialization.
    /// </summary>
    /// <param name="visual">The window or visual to use as dialog parent.</param>
    public void SetVisualContext(Visual visual)
    {
        _visual = visual;
    }

    /// <summary>
    /// Loads the last workspace if available.
    /// </summary>
    public async Task InitializeAsync()
    {
        var settings = _settingsService.Load();
        if (!string.IsNullOrEmpty(settings.LastWorkspacePath) && Directory.Exists(settings.LastWorkspacePath))
        {
            await FileExplorer.LoadWorkspaceAsync(settings.LastWorkspacePath);
        }
    }

    #endregion

    #region Property Change Handlers

    /// <summary>
    /// Called when the active tab changes.
    /// </summary>
    partial void OnActiveTabChanged(TabViewModel? value)
    {
        // Unsubscribe from previous tab
        if (_previousActiveTab is not null)
        {
            _previousActiveTab.Editor.ContentChanged -= OnActiveEditorContentChanged;
            _previousActiveTab.PropertyChanged -= OnActiveTabPropertyChanged;
        }

        // Subscribe to new tab
        if (value is not null)
        {
            value.Editor.ContentChanged += OnActiveEditorContentChanged;
            value.PropertyChanged += OnActiveTabPropertyChanged;

            // Update status bar
            StatusBar.UpdateFromEditor(
                value.Editor.CaretLine,
                value.Editor.CaretColumn,
                value.Editor.TotalLines);
            StatusBar.UpdateModifiedState(value.IsModified);
        }
        else
        {
            StatusBar.Reset();
        }

        _previousActiveTab = value;

        // Notify commands that depend on active tab
        SaveCommand.NotifyCanExecuteChanged();
        SaveAsCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Command Implementations

    private Task NewFileAsync()
    {
        var tab = TabViewModel.CreateNew();
        Tabs.Add(tab);
        ActiveTab = tab;

        NotifyTabCommands();
        return Task.CompletedTask;
    }

    private async Task OpenFileAsync()
    {
        if (_visual is null)
        {
            return;
        }

        var path = await _dialogService.ShowOpenFileDialogAsync(
            _visual,
            "Open Markdown File",
            MarkdownFilters);

        if (!string.IsNullOrEmpty(path))
        {
            await OpenFileByPathAsync(path);
        }
    }

    private async Task OpenFolderAsync()
    {
        if (_visual is null)
        {
            return;
        }

        var path = await _dialogService.ShowOpenFolderDialogAsync(_visual, "Open Folder");

        if (!string.IsNullOrEmpty(path))
        {
            await FileExplorer.LoadWorkspaceAsync(path);

            // Save as last workspace
            var settings = _settingsService.Load();
            settings.LastWorkspacePath = path;
            await _settingsService.SaveAsync(settings);
        }
    }

    private async Task SaveAsync()
    {
        if (ActiveTab is null)
        {
            return;
        }

        if (ActiveTab.IsNewDocument)
        {
            await SaveAsAsync();
            return;
        }

        await SaveDocumentAsync(ActiveTab);
    }

    private async Task SaveAsAsync()
    {
        if (ActiveTab is null || _visual is null)
        {
            return;
        }

        var defaultName = ActiveTab.IsNewDocument ? "Untitled.md" : Path.GetFileName(ActiveTab.FilePath);

        var path = await _dialogService.ShowSaveFileDialogAsync(
            _visual,
            "Save Markdown File",
            MarkdownFilters,
            defaultName);

        if (!string.IsNullOrEmpty(path))
        {
            ActiveTab.SetFilePath(path);
            await SaveDocumentAsync(ActiveTab);
        }
    }

    private bool CanSave() => ActiveTab is not null;

    private async Task CloseTabAsync(TabViewModel? tab)
    {
        tab ??= ActiveTab;
        if (tab is null)
        {
            return;
        }

        // Prompt to save if modified
        if (tab.IsModified && _visual is not null)
        {
            var save = await _dialogService.ShowConfirmationDialogAsync(
                _visual,
                "Unsaved Changes",
                $"Do you want to save changes to {tab.Title}?");

            if (save)
            {
                if (tab.IsNewDocument)
                {
                    // Need to do SaveAs for new documents
                    ActiveTab = tab;
                    await SaveAsAsync();

                    // If still modified (user cancelled), don't close
                    if (tab.IsModified)
                    {
                        return;
                    }
                }
                else
                {
                    await SaveDocumentAsync(tab);
                }
            }
        }

        // Cancel any pending auto-save
        _autoSaveService.CancelPendingSave(tab.Id);

        // Remove the tab
        var index = Tabs.IndexOf(tab);
        Tabs.Remove(tab);

        // Select adjacent tab if this was the active tab
        if (ActiveTab == tab)
        {
            if (Tabs.Count > 0)
            {
                ActiveTab = Tabs[Math.Min(index, Tabs.Count - 1)];
            }
            else
            {
                ActiveTab = null;
            }
        }

        NotifyTabCommands();
    }

    private void NextTab()
    {
        if (Tabs.Count < 2 || ActiveTab is null)
        {
            return;
        }

        var index = Tabs.IndexOf(ActiveTab);
        ActiveTab = Tabs[(index + 1) % Tabs.Count];
    }

    private void PreviousTab()
    {
        if (Tabs.Count < 2 || ActiveTab is null)
        {
            return;
        }

        var index = Tabs.IndexOf(ActiveTab);
        ActiveTab = Tabs[(index - 1 + Tabs.Count) % Tabs.Count];
    }

    private void ToggleSidebar()
    {
        IsSidebarVisible = !IsSidebarVisible;

        // Persist setting
        var settings = _settingsService.Load();
        settings.IsSidebarVisible = IsSidebarVisible;
        _ = _settingsService.SaveAsync(settings);
    }

    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
    }

    #endregion

    #region Helper Methods

    private async Task OpenFileByPathAsync(string path)
    {
        // Check if already open
        var existingTab = Tabs.FirstOrDefault(t =>
            string.Equals(t.FilePath, path, StringComparison.OrdinalIgnoreCase));

        if (existingTab is not null)
        {
            ActiveTab = existingTab;
            return;
        }

        // Open the document
        SetBusy();
        try
        {
            var result = await _documentService.OpenAsync(path);

            if (result.IsSuccess)
            {
                var tab = TabViewModel.FromDocument(result.Value);
                Tabs.Add(tab);
                ActiveTab = tab;
                NotifyTabCommands();
            }
            else if (_visual is not null)
            {
                await _dialogService.ShowErrorDialogAsync(
                    _visual,
                    "Failed to Open File",
                    result.Error ?? "An unknown error occurred.");
            }
        }
        finally
        {
            ClearBusy();
        }
    }

    private async Task SaveDocumentAsync(TabViewModel tab)
    {
        if (tab.FilePath is null)
        {
            return;
        }

        SetBusy();
        try
        {
            // Create or update the document
            Document document;
            if (tab.Document is not null)
            {
                // Update existing document
                var content = new MarkdownContent(tab.Editor.Content);
                _ = tab.Document.UpdateContent(content, DateTime.UtcNow);
                document = tab.Document;
            }
            else
            {
                // Create new document
                document = new Document(
                    DocumentId.New(),
                    new FilePath(tab.FilePath),
                    new MarkdownContent(tab.Editor.Content),
                    DateTime.UtcNow);
                tab.SetDocument(document);
            }

            var result = await _documentService.SaveAsync(document);

            if (result.IsSuccess)
            {
                tab.MarkSaved();
            }
            else if (_visual is not null)
            {
                await _dialogService.ShowErrorDialogAsync(
                    _visual,
                    "Failed to Save File",
                    result.Error ?? "An unknown error occurred.");
            }
        }
        finally
        {
            ClearBusy();
        }
    }

    private void NotifyTabCommands()
    {
        NextTabCommand.NotifyCanExecuteChanged();
        PreviousTabCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Event Handlers

    private async void OnFileOpenRequested(object? sender, string path)
    {
        await OpenFileByPathAsync(path);
    }

    private void OnActiveEditorContentChanged(object? sender, string content)
    {
        if (ActiveTab is null)
        {
            return;
        }

        // Update status bar
        StatusBar.UpdateModifiedState(ActiveTab.IsModified);

        // Schedule auto-save if enabled and document has a path
        if (_autoSaveService.IsEnabled && !ActiveTab.IsNewDocument)
        {
            _autoSaveService.ScheduleSave(
                ActiveTab.Id,
                ActiveTab.FilePath,
                () => SaveTabAsync(ActiveTab));
        }
    }

    private async Task<Domain.Primitives.Result> SaveTabAsync(TabViewModel tab)
    {
        if (tab.FilePath is null)
        {
            return Domain.Primitives.Results.Failure("No file path specified.");
        }

        // Create or update the document
        Document document;
        if (tab.Document is not null)
        {
            var content = new MarkdownContent(tab.Editor.Content);
            _ = tab.Document.UpdateContent(content, DateTime.UtcNow);
            document = tab.Document;
        }
        else
        {
            document = new Document(
                DocumentId.New(),
                new FilePath(tab.FilePath),
                new MarkdownContent(tab.Editor.Content),
                DateTime.UtcNow);
            tab.SetDocument(document);
        }

        return await _documentService.SaveAsync(document);
    }

    private void OnActiveTabPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TabViewModel.IsModified) && ActiveTab is not null)
        {
            StatusBar.UpdateModifiedState(ActiveTab.IsModified);
        }
    }

    private void OnAutoSaveCompleted(object? sender, AutoSaveCompletedEventArgs e)
    {
        // Find the tab and mark it as saved
        var tab = Tabs.FirstOrDefault(t => t.Id == e.TabId);
        tab?.MarkSaved();
    }

    private async void OnAutoSaveFailed(object? sender, AutoSaveFailedEventArgs e)
    {
        // Show non-intrusive notification via status bar or dialog
        if (_visual is not null)
        {
            await _dialogService.ShowErrorDialogAsync(
                _visual,
                "Auto-save Failed",
                e.Error);
        }
    }

    #endregion
}
