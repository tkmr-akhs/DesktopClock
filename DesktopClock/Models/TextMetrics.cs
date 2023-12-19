using System.Drawing;

namespace DesktopClock.Models;

/// <summary>
/// Structure that holds image size information.
/// </summary>
public struct TextMetrics
{
    /// <summary>
    /// Gets the actual height of the font.
    /// </summary>
    public int ActualFontHeight { get; set; }

    /// <summary>
    /// Gets or sets the width of the border around the text.
    /// </summary>
    public int BorderWidth { get; set; }

    /// <summary>
    /// Gets the boundaries of the text.
    /// </summary>
    public RectangleF Bounds { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the text is centered.
    /// </summary>
    public bool IsCenter { get; set; }

    /// <summary>
    /// Initializes a new instance of the TextMetrics structure.
    /// </summary>
    /// <param name="fontHeight">The height of the font.</param>
    /// <param name="borderWidth">The width of the border around the text.</param>
    /// <param name="bounds">The boundaries of the text.</param>
    /// <param name="isCenter">Indicates whether the text is centered.</param>
    public TextMetrics(int fontHeight, int borderWidth, RectangleF bounds, bool isCenter)
    {
        this.ActualFontHeight = fontHeight;
        this.BorderWidth = borderWidth;
        this.Bounds = bounds;
        this.IsCenter = isCenter;
    }

    public override string? ToString()
    {
        return $"{GetType().Name}() {{ ActualFontHeight = {ActualFontHeight}, BorderWidth = {BorderWidth}, {Bounds.GetType().Name}(){{ X = {Bounds.X}, Y = {Bounds.Y}, Width = {Bounds.Width}, Height = {Bounds.Height} }} IsCenter = {IsCenter} }}";
    }
}
