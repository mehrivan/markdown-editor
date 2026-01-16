using System.Globalization;

using Avalonia;
using Avalonia.Data.Converters;

namespace Markdown.UI.Desktop.Converters;

/// <summary>
/// Converts a tree level (int) to a Thickness for indentation.
/// Each level adds 16 pixels of left padding for proper tree hierarchy visualization.
/// </summary>
internal sealed class TreeIndentConverter : IValueConverter
{
    private const double IndentSize = 16.0;

    /// <summary>
    /// Converts a tree level to a Thickness with left padding.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int level)
        {
            // Each level adds IndentSize pixels to the left
            return new Thickness(level * IndentSize, 0, 0, 0);
        }

        return new Thickness(0);
    }

    /// <summary>
    /// Not supported - this is a one-way converter.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("TreeIndentConverter does not support ConvertBack.");
}
