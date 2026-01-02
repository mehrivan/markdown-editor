using System.Globalization;

using Avalonia.Data.Converters;

namespace Markdown.UI.Desktop.Converters;

/// <summary>
/// Converts a boolean value to a visibility value.
/// Can be inverted to hide when true and show when false.
/// </summary>
internal sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// When true, inverts the visibility logic (hidden when true, visible when false).
    /// </summary>
    public bool Invert { get; set; }

    /// <summary>
    /// Converts a boolean to a visibility boolean.
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isVisible = value is true;
        if (Invert)
        {
            isVisible = !isVisible;
        }
        return isVisible;
    }

    /// <summary>
    /// Not supported - this is a one-way converter.
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("BoolToVisibilityConverter does not support ConvertBack.");
}
