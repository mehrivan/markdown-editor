using System.Globalization;

using Avalonia.Data.Converters;

using Markdown.UI.Desktop.Models;

namespace Markdown.UI.Desktop.Converters;

/// <summary>
/// Converts a FileTreeNode to an icon path based on its type.
/// Returns different icons for folders, markdown files, and other files.
/// </summary>
internal sealed class FileTypeToIconConverter : IValueConverter
{
    private const string _folderIcon = "avares://Markdown.UI.Desktop/Assets/folder.svg";
    private const string _markdownIcon = "avares://Markdown.UI.Desktop/Assets/markdown.svg";
    private const string _fileIcon = "avares://Markdown.UI.Desktop/Assets/file.svg";

    /// <summary>
    /// Converts a FileTreeNode to an icon path.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            FileTreeNode { IsFolder: true } => _folderIcon,
            FileTreeNode { IsMarkdownFile: true } => _markdownIcon,
            FileTreeNode => _fileIcon,
            _ => null
        };
    }

    /// <summary>
    /// Not supported - this is a one-way converter.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("FileTypeToIconConverter does not support ConvertBack.");
}
