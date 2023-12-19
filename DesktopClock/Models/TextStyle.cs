using System.Text.Json.Serialization;
using Windows.UI;
using Windows.UI.Text;

namespace DesktopClock.Models;

/// <summary>
/// Provides default settings for text imaging.
/// </summary>
public static class DefaultTextImagingSetting
{
    public const string FontFamily = "游ゴシック";
    public const double FontHeight = 10;
    public const WindowAlignmentUnit SizeUnit = WindowAlignmentUnit.Percent;
    public const Windows.UI.Text.FontStyle FontStyle = Windows.UI.Text.FontStyle.Normal;
    public static readonly FontWeight FontWeight = Microsoft.UI.Text.FontWeights.Normal;
    public static readonly Color FontColor = Color.FromArgb(0xC0, 0xFF, 0xFF, 0xFF);
    public const double BorderWidth = 0.5;
    public static readonly Color BorderColor = Microsoft.UI.Colors.Black;
}

/// <summary>
/// Represents the style settings for text, including font family, style, weight, color, and border color.
/// </summary>
public class TextStyle
{
    /// <summary>
    /// Gets the font family name.
    /// </summary>
    public string FontFamily { get; private set; }

    /// <summary>
    /// Gets the font style.
    /// </summary>
    public FontStyle FontStyle { get; private set; }

    /// <summary>
    /// Gets the font weight. This property is not included in JSON serialization.
    /// </summary>
    [JsonIgnore]
    public FontWeight FontWeight { get; private set; }

    /// <summary>
    /// Gets or sets the numeric value of the font weight. This property is included in JSON serialization.
    /// </summary>
    [JsonInclude]
    private ushort FontWeightValue
    {
        get => FontWeight.Weight;
        set
        {
            if (value != FontWeight.Weight) FontWeight = new FontWeight(value);
        }
    }

    /// <summary>
    /// Gets the color of the font.
    /// </summary>
    public Color FontColor { get; private set; }

    /// <summary>
    /// Gets the color of the border around the text.
    /// </summary>
    public Color BorderColor { get; private set; }

    /// <summary>
    /// Initializes a new instance of the TextStyle class with optional custom settings or default values.
    /// </summary>
    /// <param name="fontFamily">Font family name.</param>
    /// <param name="fontStyle">Font style.</param>
    /// <param name="fontWeight">Font weight.</param>
    /// <param name="fontColor">Font color.</param>
    /// <param name="borderColor">Border color around the text.</param>
    public TextStyle(
        string fontFamily = DefaultTextImagingSetting.FontFamily,
        FontStyle fontStyle = DefaultTextImagingSetting.FontStyle,
        FontWeight fontWeight = default,
        Color fontColor = default,
        Color borderColor = default)
    {
        FontFamily = String.IsNullOrEmpty(fontFamily) ? DefaultTextImagingSetting.FontFamily : fontFamily ;
        FontStyle = fontStyle;
        FontWeight = fontWeight == default ? DefaultTextImagingSetting.FontWeight :fontWeight;
        FontColor = fontColor == default ? DefaultTextImagingSetting.FontColor : fontColor;
        BorderColor = borderColor == default ? DefaultTextImagingSetting.BorderColor : borderColor;
    }

    /// <summary>
    /// Creates a new TextStyle with modified properties, while keeping other properties unchanged.
    /// </summary>
    /// <param name="fontFamily">Optional font family name to change.</param>
    /// <param name="fontStyle">Optional font style to change.</param>
    /// <param name="fontWeight">Optional font weight to change.</param>
    /// <param name="fontColor">Optional font color to change.</param>
    /// <param name="borderColor">Optional border color to change.</param>
    /// <returns>A new instance of TextStyle with the specified changes.</returns>
    public TextStyle With(string? fontFamily = null, FontStyle? fontStyle = null, FontWeight? fontWeight = null, Color? fontColor = null, Color? borderColor = null)
    {
        var result = new TextStyle(FontFamily, FontStyle, FontWeight, FontColor, BorderColor);

        if (!String.IsNullOrEmpty(fontFamily)) result.FontFamily = fontFamily;
        if (fontStyle != null) result.FontStyle = fontStyle.Value;
        if (fontWeight != null) result.FontWeight = fontWeight.Value;
        if (fontColor != null) result.FontColor = fontColor.Value;
        if (borderColor != null) result.BorderColor = borderColor.Value;

        return result;
    }

    public override string ToString()
    {
        return $"{this.GetType().Name}() {{ FontFamily = {FontFamily}, FontStyle = {FontStyle}, FontWeight = {FontWeight}, FontColor = {FontColor}, BorderColor = {BorderColor} }}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is TextStyle other)
        {
            return FontFamily == other.FontFamily &&
                   FontStyle == other.FontStyle &&
                   FontWeight.Weight == other.FontWeight.Weight &&
                   FontColor.Equals(other.FontColor) &&
                   BorderColor.Equals(other.BorderColor);
        }

        return false;
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            hash = hash * 23 + FontFamily.GetHashCode();
            hash = hash * 23 + FontStyle.GetHashCode();
            hash = hash * 23 + FontWeight.Weight.GetHashCode();
            hash = hash * 23 + FontColor.GetHashCode();
            hash = hash * 23 + BorderColor.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(TextStyle left, TextStyle right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(TextStyle left, TextStyle right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Record representing the size settings for text, including size unit, font height, and border width.
/// </summary>
/// <param name="SizeUnit">The unit of measurement for the text size, such as pixels or percent.</param>
/// <param name="FontHeight">The height of the font, measured in the specified size unit.</param>
/// <param name="BorderWidth">The width of the border around the text, measured in the specified size unit.</param>
public record TextSize(
    WindowAlignmentUnit SizeUnit = DefaultTextImagingSetting.SizeUnit,
    double FontHeight = DefaultTextImagingSetting.FontHeight,
    double BorderWidth = DefaultTextImagingSetting.BorderWidth) { }