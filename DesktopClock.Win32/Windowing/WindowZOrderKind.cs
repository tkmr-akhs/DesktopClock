namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Defines native z-order behavior.
/// </summary>
public enum WindowZOrderKind
{
    /// <summary>
    /// Leaves the current z-order unchanged.
    /// </summary>
    Unchanged,

    /// <summary>
    /// Moves the window to the bottom of the z-order.
    /// </summary>
    Bottom
}
