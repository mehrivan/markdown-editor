using System.Diagnostics;

using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;

using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using Markdown.UI.Desktop.ViewModels;

using TextMateSharp.Grammars;

namespace Markdown.UI.Desktop.Views;

/// <summary>
/// Editor view hosting the AvaloniaEdit TextEditor control.
/// Handles wiring between the TextEditor and EditorViewModel.
/// </summary>
public partial class EditorView : UserControl
{
    private EditorViewModel? _viewModel;
    private bool _isUpdatingFromViewModel;
    private bool _isEditorEventsSubscribed;
    private TextMate.Installation? _textMateInstallation;
    private RegistryOptions? _registryOptions;

    public EditorView()
    {
        InitializeComponent();

        // Initialize TextMate with appropriate theme based on current app theme
        InitializeTextMate();

        // Subscribe to theme changes to update syntax highlighting theme
        if (Avalonia.Application.Current is not null)
        {
            Avalonia.Application.Current.ActualThemeVariantChanged += OnThemeChanged;
        }

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from previous ViewModel
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.UndoRequested -= OnUndoRequested;
            _viewModel.RedoRequested -= OnRedoRequested;
            _viewModel.CutRequested -= OnCutRequested;
            _viewModel.CopyRequested -= OnCopyRequested;
            _viewModel.PasteRequested -= OnPasteRequested;
        }

        _viewModel = DataContext as EditorViewModel;

        if (_viewModel is not null)
        {
            // Subscribe to ViewModel changes
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.UndoRequested += OnUndoRequested;
            _viewModel.RedoRequested += OnRedoRequested;
            _viewModel.CutRequested += OnCutRequested;
            _viewModel.CopyRequested += OnCopyRequested;
            _viewModel.PasteRequested += OnPasteRequested;

            // Subscribe to editor events only once
            if (!_isEditorEventsSubscribed)
            {
                TextEditor.TextChanged += OnTextEditorTextChanged;
                TextEditor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;
                _isEditorEventsSubscribed = true;
            }

            // Initial sync - capture content now, then apply on UI thread
            var content = _viewModel.Content;
            Debug.WriteLine($"[EditorView] DataContextChanged - Content length: {content?.Length ?? 0}");

            Dispatcher.UIThread.Post(() =>
            {
                Debug.WriteLine($"[EditorView] Applying content to TextEditor, length: {content?.Length ?? 0}");
                ApplyContentToEditor(content ?? string.Empty);
            }, DispatcherPriority.Loaded);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.Content) && !_isUpdatingFromViewModel)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                SyncContentFromViewModel();
            }
            else
            {
                Dispatcher.UIThread.Post(SyncContentFromViewModel);
            }
        }
    }

    private void SyncContentFromViewModel()
    {
        if (_viewModel is null)
        {
            return;
        }

        ApplyContentToEditor(_viewModel.Content);
    }

    private void ApplyContentToEditor(string content)
    {
        _isUpdatingFromViewModel = true;
        try
        {
            // Use Document.Text for more reliable updates
            if (TextEditor.Document.Text != content)
            {
                TextEditor.Document = new TextDocument(content ?? string.Empty);
                TextEditor.ScrollToHome();

                // Re-apply grammar after document replacement
                ApplyMarkdownGrammar();
            }
        }
        finally
        {
            _isUpdatingFromViewModel = false;
        }
    }

    private void OnTextEditorTextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is null || _isUpdatingFromViewModel)
        {
            return;
        }

        // Sync text from editor to ViewModel
        _viewModel.UpdateContent(TextEditor.Document.Text);
    }

    private void OnCaretPositionChanged(object? sender, EventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var caret = TextEditor.TextArea.Caret;
        _viewModel.UpdateCaretPosition(caret.Line, caret.Column);
    }

    private void OnUndoRequested()
    {
        if (TextEditor.Document.UndoStack.CanUndo)
        {
            TextEditor.Document.UndoStack.Undo();
        }
    }

    private void OnRedoRequested()
    {
        if (TextEditor.Document.UndoStack.CanRedo)
        {
            TextEditor.Document.UndoStack.Redo();
        }
    }

    private void OnCutRequested()
    {
        TextEditor.Cut();
    }

    private void OnCopyRequested()
    {
        TextEditor.Copy();
    }

    private void OnPasteRequested()
    {
        TextEditor.Paste();
    }

    private void InitializeTextMate()
    {
        try
        {
            // Determine the theme based on current app theme
            ThemeName themeName = GetThemeNameFromAppTheme();

            // Initialize TextMate registry with the theme
            _registryOptions = new RegistryOptions(themeName);

            // Install TextMate on the TextEditor
            _textMateInstallation = TextEditor.InstallTextMate(_registryOptions);

            // Apply Markdown grammar
            ApplyMarkdownGrammar();

            Debug.WriteLine($"[EditorView] TextMate initialized with theme: {themeName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EditorView] Failed to initialize TextMate: {ex.Message}");
        }
    }

    private void ApplyMarkdownGrammar()
    {
        if (_registryOptions is null || _textMateInstallation is null)
        {
            return;
        }

        try
        {
            // Get Markdown language by file extension
            Language markdownLanguage = _registryOptions.GetLanguageByExtension(".md");
            string scopeName = _registryOptions.GetScopeByLanguageId(markdownLanguage.Id);

            // Apply the grammar
            _textMateInstallation.SetGrammar(scopeName);

            Debug.WriteLine($"[EditorView] Applied Markdown grammar: {scopeName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EditorView] Failed to apply Markdown grammar: {ex.Message}");
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        // Reinitialize TextMate with the new theme
        InitializeTextMate();
    }

    private ThemeName GetThemeNameFromAppTheme()
    {
        if (Avalonia.Application.Current?.ActualThemeVariant == ThemeVariant.Light)
        {
            return ThemeName.LightPlus;
        }

        return ThemeName.DarkPlus;
    }
}
