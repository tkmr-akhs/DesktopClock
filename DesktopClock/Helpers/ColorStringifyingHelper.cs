using Windows.UI;

namespace DesktopClock.Helpers;

/// <summary>
/// Provides helper methods to convert between color objects and their string representations.
/// </summary>
public static class ColorStringifyingHelper
{
    /// <summary>
    /// Tries to parse a hexadecimal color code into a Color object.
    /// </summary>
    /// <param name="colorCode">The color code to parse. Expected format: #AARRGGBB.</param>
    /// <param name="color">The resulting Color object, or null if the parsing fails or the string is empty.</param>
    /// <returns>True if the parsing is successful, otherwise false.</returns>
    public static bool TryParseColorCode(string colorCode, out Color? color)
    {
        if (colorCode == string.Empty)
        {
            color = null;
            return true;
        }

        if (string.IsNullOrEmpty(colorCode) || colorCode.Length != 9 || colorCode[0] != '#')
        {
            color = default(Color);
            return false;
        }

        try
        {
            byte a = Convert.ToByte(colorCode.Substring(1, 2), 16);
            byte r = Convert.ToByte(colorCode.Substring(3, 2), 16);
            byte g = Convert.ToByte(colorCode.Substring(5, 2), 16);
            byte b = Convert.ToByte(colorCode.Substring(7, 2), 16);
            color = Color.FromArgb(a, r, g, b);
            return true;
        }
        catch (ArgumentException)
        {
            color = default(Color);
            return false;
        }
        catch (FormatException)
        {
            color = default(Color);
            return false;
        }
        catch (OverflowException)
        {
            color = default(Color);
            return false;
        }
    }

    /// <summary>
    /// Converts a Color object to its hexadecimal color code representation.
    /// </summary>
    /// <param name="color">The Color object to convert. If null, an empty string is returned.</param>
    /// <returns>A hexadecimal color code string in the format #AARRGGBB, or an empty string if the color is null.</returns>
    public static string ConvertToColorCode(Color? color)
    {
        if (color == null) return string.Empty;

        var nonNullableColor = (Color)color;

        return $"#{nonNullableColor.A:X2}{nonNullableColor.R:X2}{nonNullableColor.G:X2}{nonNullableColor.B:X2}";
    }
}
