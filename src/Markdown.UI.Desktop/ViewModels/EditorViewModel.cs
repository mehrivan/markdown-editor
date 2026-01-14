namespace Markdown.UI.Desktop.ViewModels;

/// <summary>
/// ViewModel for the text editor, managing content and caret position.
/// </summary>
public sealed partial class EditorViewModel : ViewModelBase
{
    private bool _isLoadingContent;

    /// <summary>
    /// The text content of the editor.
    /// </summary>
    [ObservableProperty]
    private string _content = string.Empty;

    /// <summary>
    /// Current caret line position (1-based).
    /// </summary>
    [ObservableProperty]
    private int _caretLine = 1;

    /// <summary>
    /// Current caret column position (1-based).
    /// </summary>
    [ObservableProperty]
    private int _caretColumn = 1;

    /// <summary>
    /// Indicates whether the editor is in read-only mode.
    /// </summary>
    [ObservableProperty]
    private bool _isReadOnly;

    /// <summary>
    /// Total number of lines in the content.
    /// </summary>
    public int TotalLines => string.IsNullOrEmpty(Content)
        ? 1
        : Content.Split('\n').Length;

    /// <summary>
    /// Raised when the content changes (for auto-save integration).
    /// </summary>
    public event EventHandler<string>? ContentChanged;

    /// <summary>
    /// Raised when undo is requested.
    /// </summary>
    public event Action? UndoRequested;

    /// <summary>
    /// Raised when redo is requested.
    /// </summary>
    public event Action? RedoRequested;

    /// <summary>
    /// Raised when cut is requested.
    /// </summary>
    public event Action? CutRequested;

    /// <summary>
    /// Raised when copy is requested.
    /// </summary>
    public event Action? CopyRequested;

    /// <summary>
    /// Raised when paste is requested.
    /// </summary>
    public event Action? PasteRequested;

    /// <summary>
    /// Called when the Content property changes.
    /// Fires the ContentChanged event unless content is being loaded.
    /// </summary>
    /// <param name="value">The new content value.</param>
    partial void OnContentChanged(string value)
    {
        OnPropertyChanged(nameof(TotalLines));

        if (!_isLoadingContent)
        {
            ContentChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// Loads content into the editor without triggering the ContentChanged event.
    /// Use this when loading a file to avoid marking the document as modified.
    /// </summary>
    /// <param name="content">The content to load.</param>
    public void LoadContent(string content)
    {
        _isLoadingContent = true;
        try
        {
            Content = content;
        }
        finally
        {
            _isLoadingContent = false;
        }
    }

    /// <summary>
    /// Updates the caret position.
    /// </summary>
    /// <param name="line">Line number (1-based).</param>
    /// <param name="column">Column number (1-based).</param>
    public void UpdateCaretPosition(int line, int column)
    {
        CaretLine = line;
        CaretColumn = column;
    }

    /// <summary>
    /// Updates the content from the editor.
    /// </summary>
    /// <param name="content">The updated content.</param>
    public void UpdateContent(string content)
    {
        Content = content;
    }

    /// <summary>
    /// Requests undo operation on the editor.
    /// </summary>
    public void Undo()
    {
        UndoRequested?.Invoke();
    }

    /// <summary>
    /// Requests redo operation on the editor.
    /// </summary>
    public void Redo()
    {
        RedoRequested?.Invoke();
    }

    /// <summary>
    /// Requests cut operation on the editor.
    /// </summary>
    public void Cut()
    {
        CutRequested?.Invoke();
    }

    /// <summary>
    /// Requests copy operation on the editor.
    /// </summary>
    public void Copy()
    {
        CopyRequested?.Invoke();
    }

    /// <summary>
    /// Requests paste operation on the editor.
    /// </summary>
    public void Paste()
    {
        PasteRequested?.Invoke();
    }
}
