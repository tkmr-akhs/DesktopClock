namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Defines native owner-window behavior.
/// </summary>
public enum WindowOwnerKind
{
    /// <summary>
    /// Leaves the current owner unchanged.
    /// </summary>
    Unchanged,

    /// <summary>
    /// Sets the shell desktop window as owner.
    /// </summary>
    DesktopShell
}
