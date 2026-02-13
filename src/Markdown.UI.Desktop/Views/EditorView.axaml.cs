using System.Diagnostics;

using Avalonia.Controls;
using Avalonia.Threading;

using AvaloniaEdit.Document;

using Markdown.UI.Desktop.ViewModels;

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

    public EditorView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from previous ViewModel
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = DataContext as EditorViewModel;

        if (_viewModel is not null)
        {
            // Subscribe to ViewModel changes
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

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
        _viewModel.Content = TextEditor.Document.Text;
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
}
