namespace DesktopClock.Models;

/// <summary>
/// Enumerates the orientations for calculating pixels from percentages, indicating whether to base the calculation on the screen's horizontal or vertical dimension.
/// </summary>
public enum WindowAlignmentOrientation
{
    /// <summary>
    /// Indicates that the calculation is based on the screen's horizontal dimension.
    /// </summary>
    Horizontally,

    /// <summary>
    /// Indicates that the calculation is based on the screen's vertical dimension.
    /// </summary>
    Vertically
}
