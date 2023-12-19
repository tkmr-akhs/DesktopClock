namespace DesktopClock.Models;

/// <summary>
/// Record that represents the alignment settings for a window, including screen ID, alignment, margins, and margin units.
/// </summary>
/// <param name="ScreenId">The ID of the screen where the window is displayed.</param>
/// <param name="Alignment">The alignment setting of the window on the screen.</param>
/// <param name="VerticalMargin">The vertical margin for the window.</param>
/// <param name="HorizontalMargin">The horizontal margin for the window.</param>
/// <param name="MarginUnit">The unit of measurement for the margins.</param>
public record WindowAlignmentSetting(int ScreenId, WindowAlignment Alignment, double VerticalMargin, double HorizontalMargin, WindowAlignmentUnit MarginUnit)
{
    private const double DefaultHorizontalMargin = 5.0;
    private const double DefaultVerticalMargin = 7.0;
    private const WindowAlignment DefaultAlignment = WindowAlignment.TopRight;
    private const WindowAlignmentUnit DefaultMarginUnit = WindowAlignmentUnit.Percent;

    /// <summary>
    /// Gets a value indicating whether the window is aligned to the top.
    /// </summary>
    public bool IsTop =>
        Alignment == WindowAlignment.TopLeft
        || Alignment == WindowAlignment.TopCenter
        || Alignment == WindowAlignment.TopRight;

    /// <summary>
    /// Gets a value indicating whether the window is vertically centered.
    /// </summary>
    public bool IsVerticalCenter =>
        Alignment == WindowAlignment.CenterLeft
        || Alignment == WindowAlignment.CenterCenter
        || Alignment == WindowAlignment.CenterRight;

    /// <summary>
    /// Gets a value indicating whether the window is aligned to the bottom.
    /// </summary>
    public bool IsBottom =>
        Alignment == WindowAlignment.BottomLeft
        || Alignment == WindowAlignment.BottomCenter
        || Alignment == WindowAlignment.BottomRight;

    /// <summary>
    /// Gets a value indicating whether the window is aligned to the left.
    /// </summary>
    public bool IsLeft =>
        Alignment == WindowAlignment.TopLeft
        || Alignment == WindowAlignment.CenterLeft
        || Alignment == WindowAlignment.BottomLeft;

    /// <summary>
    /// Gets a value indicating whether the window is horizontally centered.
    /// </summary>
    public bool IsHorizontalCenter =>
        Alignment == WindowAlignment.TopCenter
        || Alignment == WindowAlignment.CenterCenter
        || Alignment == WindowAlignment.BottomCenter;

    /// <summary>
    /// Gets a value indicating whether the window is aligned to the right.
    /// </summary>
    public bool IsRight =>
        Alignment == WindowAlignment.TopRight
        || Alignment == WindowAlignment.CenterRight
        || Alignment == WindowAlignment.BottomRight;

    /// <summary>
    /// Initializes a new instance of the WindowAlignmentSetting record with default values.
    /// </summary>
    public WindowAlignmentSetting() : this(0, DefaultAlignment, DefaultVerticalMargin, DefaultHorizontalMargin, DefaultMarginUnit) { }
}
