namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Contains the result of applying window chrome options.
/// </summary>
public sealed class WindowChromeResult
{
    /// <summary>
    /// Gets whether the requested owner window was applied.
    /// </summary>
    public bool OwnerApplied { get; init; }
}
