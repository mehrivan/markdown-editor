using Avalonia.Controls;

using AvaloniaEdit;

using Markdown.UI.Desktop.ViewModels;

namespace Markdown.UI.Desktop.Views;

/// <summary>
/// Editor view hosting the AvaloniaEdit TextEditor control.
/// Handles wiring between the TextEditor and EditorViewModel.
/// </summary>
internal partial class EditorView : UserControl
{
    private EditorViewModel? _viewModel;
    private bool _isUpdatingFromViewModel;

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

            // Initial sync from ViewModel to editor
            SyncContentFromViewModel();

            // Subscribe to editor events
            TextEditor.TextChanged += OnTextEditorTextChanged;
            TextEditor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.Content) && !_isUpdatingFromViewModel)
        {
            SyncContentFromViewModel();
        }
    }

    private void SyncContentFromViewModel()
    {
        if (_viewModel is null)
        {
            return;
        }

        _isUpdatingFromViewModel = true;
        try
        {
            if (TextEditor.Text != _viewModel.Content)
            {
                TextEditor.Text = _viewModel.Content;
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
        _viewModel.Content = TextEditor.Text;
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
