using System.Globalization;

using Avalonia.Data.Converters;

namespace Markdown.UI.Desktop.Converters;

/// <summary>
/// Multi-value converter that appends a modified indicator (dot) to a title when the document is modified.
/// Expects two values: the title (string) and isModified (bool).
/// </summary>
internal sealed class ModifiedIndicatorConverter : IMultiValueConverter
{
    private const string _modifiedSuffix = "\u2022"; // Bullet character (•)

    /// <summary>
    /// Converts a title and modified state to a display title.
    /// </summary>
    /// <param name="values">Array containing [0] = title (string), [1] = isModified (bool).</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>The title with a dot appended if modified, otherwise just the title.</returns>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return null;
        }

        string title = values[0] as string ?? "Untitled";
        bool isModified = values[1] is true;

        return isModified ? $"{title}{_modifiedSuffix}" : title;
    }
}
