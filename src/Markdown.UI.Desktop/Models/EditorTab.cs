namespace Markdown.UI.Desktop.Models;

/// <summary>
/// Immutable record representing the state of an open editor tab.
/// </summary>
/// <param name="Id">Unique identifier for the tab.</param>
/// <param name="FilePath">The file path of the document, or null for new untitled documents.</param>
/// <param name="Title">The display title of the tab.</param>
/// <param name="IsModified">Whether the document has unsaved changes.</param>
internal sealed record EditorTab(
    Guid Id,
    string? FilePath,
    string Title,
    bool IsModified
)
{
    /// <summary>
    /// Indicates whether this tab represents a new document that has never been saved.
    /// </summary>
    public bool IsNewDocument => FilePath is null;

    /// <summary>
    /// Creates a new untitled editor tab.
    /// </summary>
    /// <returns>A new EditorTab with a unique ID and default title.</returns>
    public static EditorTab CreateNew() => new(
        Guid.NewGuid(),
        FilePath: null,
        Title: "Untitled",
        IsModified: false
    );

    /// <summary>
    /// Creates an editor tab for an existing file.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>A new EditorTab for the specified file.</returns>
    public static EditorTab FromFile(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        return new(
            Guid.NewGuid(),
            filePath,
            fileName,
            IsModified: false
        );
    }

    /// <summary>
    /// Returns a new EditorTab with the modified state set.
    /// </summary>
    /// <param name="isModified">The new modified state.</param>
    /// <returns>A new EditorTab with the updated modified state.</returns>
    public EditorTab WithModified(bool isModified) =>
        this with { IsModified = isModified };

    /// <summary>
    /// Returns a new EditorTab with the file path and title updated after saving.
    /// </summary>
    /// <param name="filePath">The new file path.</param>
    /// <returns>A new EditorTab with the updated path and title.</returns>
    public EditorTab WithFilePath(string filePath) =>
        this with
        {
            FilePath = filePath,
            Title = Path.GetFileName(filePath),
            IsModified = false
        };
}
