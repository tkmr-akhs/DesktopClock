namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Defines the backdrop to apply to a WinUI window.
/// </summary>
public enum WindowBackdropKind
{
    /// <summary>
    /// Leaves the current backdrop unchanged.
    /// </summary>
    Unchanged,

    /// <summary>
    /// Removes the current backdrop.
    /// </summary>
    None,

    /// <summary>
    /// Applies a fully transparent backdrop.
    /// </summary>
    Transparent,

    /// <summary>
    /// Applies a host backdrop blur brush.
    /// </summary>
    HostBlur
}
