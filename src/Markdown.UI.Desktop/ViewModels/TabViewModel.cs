using Markdown.Domain.Entities;

namespace Markdown.UI.Desktop.ViewModels;

/// <summary>
/// ViewModel representing an open document tab.
/// </summary>
public sealed partial class TabViewModel : ViewModelBase
{
    /// <summary>
    /// Unique identifier for this tab.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The display title of the tab.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    private string _title = "Untitled";

    /// <summary>
    /// The file path of the document, or null for new unsaved documents.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNewDocument))]
    private string? _filePath;

    /// <summary>
    /// Indicates whether the document has unsaved changes.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    private bool _isModified;

    /// <summary>
    /// The associated domain Document entity, or null for new documents.
    /// </summary>
    [ObservableProperty]
    private Document? _document;

    /// <summary>
    /// The editor ViewModel for this tab's content.
    /// </summary>
    public EditorViewModel Editor { get; }

    /// <summary>
    /// Indicates whether this is a new document that hasn't been saved yet.
    /// </summary>
    public bool IsNewDocument => FilePath is null;

    /// <summary>
    /// The display title with a modified indicator (bullet) if the document has unsaved changes.
    /// </summary>
    public string DisplayTitle => IsModified ? $"{Title} \u2022" : Title;

    /// <summary>
    /// Creates a new TabViewModel with a fresh EditorViewModel.
    /// </summary>
    public TabViewModel() : this(new EditorViewModel())
    {
    }

    /// <summary>
    /// Creates a new TabViewModel with the specified EditorViewModel.
    /// </summary>
    /// <param name="editor">The editor ViewModel to use.</param>
    public TabViewModel(EditorViewModel editor)
    {
        Id = Guid.NewGuid();
        Editor = editor;

        // Subscribe to content changes to track modified state
        Editor.ContentChanged += OnEditorContentChanged;
    }

    /// <summary>
    /// Handles content changes from the editor to mark the document as modified.
    /// </summary>
    private void OnEditorContentChanged(object? sender, string content)
    {
        IsModified = true;
    }

    /// <summary>
    /// Marks the document as saved, clearing the modified flag.
    /// </summary>
    public void MarkSaved()
    {
        IsModified = false;
    }

    /// <summary>
    /// Sets the file path and updates the title from the filename.
    /// </summary>
    /// <param name="path">The file path to set.</param>
    public void SetFilePath(string path)
    {
        FilePath = path;
        Title = Path.GetFileName(path);
    }

    /// <summary>
    /// Associates a domain Document entity with this tab.
    /// </summary>
    /// <param name="document">The document entity.</param>
    public void SetDocument(Document document)
    {
        Document = document;
        SetFilePath(document.Path.Value);
        Editor.LoadContent(document.Content.Value);
    }

    /// <summary>
    /// Creates a new tab for a new untitled document.
    /// </summary>
    /// <returns>A new TabViewModel for an untitled document.</returns>
    public static TabViewModel CreateNew()
    {
        return new TabViewModel
        {
            Title = "Untitled"
        };
    }

    /// <summary>
    /// Creates a new tab from an existing document.
    /// </summary>
    /// <param name="document">The document to open.</param>
    /// <returns>A new TabViewModel for the document.</returns>
    public static TabViewModel FromDocument(Document document)
    {
        var tab = new TabViewModel();
        tab.SetDocument(document);
        return tab;
    }
}
