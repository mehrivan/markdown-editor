namespace Markdown.Application.ReadModels;

public sealed record WorkspaceEntry(
    string Path,
    FileEntryType Type,
    string Name,
    long Size,
    DateTime ModifiedAt,
    bool IsMarkdownFile
)
{
    public static WorkspaceEntry FromFileInfo(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        bool isMarkdown = fileInfo.Extension.Equals(".md", StringComparison.OrdinalIgnoreCase)
            || fileInfo.Extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase);

        return new WorkspaceEntry(
            Path: fileInfo.FullName,
            Type: FileEntryType.File,
            Name: fileInfo.Name,
            Size: fileInfo.Length,
            ModifiedAt: fileInfo.LastWriteTimeUtc,
            IsMarkdownFile: isMarkdown
        );
    }

    public static WorkspaceEntry FromDirectoryInfo(DirectoryInfo dirInfo)
    {
        ArgumentNullException.ThrowIfNull(dirInfo);

        return new WorkspaceEntry(
            Path: dirInfo.FullName,
            Type: FileEntryType.Folder,
            Name: dirInfo.Name,
            Size: 0,
            ModifiedAt: dirInfo.LastWriteTimeUtc,
            IsMarkdownFile: false
        );
    }
}

public enum FileEntryType
{
    File,
    Folder
}
