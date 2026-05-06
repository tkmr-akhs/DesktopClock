namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Describes configurable window chrome behavior.
/// </summary>
public sealed class WindowChromeOptions
{
    /// <summary>
    /// Gets the desired window width in effective pixels.
    /// </summary>
    public double? Width { get; init; }

    /// <summary>
    /// Gets the desired window height in effective pixels.
    /// </summary>
    public double? Height { get; init; }

    /// <summary>
    /// Gets whether the window can be resized.
    /// </summary>
    public bool? IsResizable { get; init; }

    /// <summary>
    /// Gets whether the window can be maximized.
    /// </summary>
    public bool? IsMaximizable { get; init; }

    /// <summary>
    /// Gets whether the window can be minimized.
    /// </summary>
    public bool? IsMinimizable { get; init; }

    /// <summary>
    /// Gets whether the window is shown in system switchers.
    /// </summary>
    public bool? IsShownInSwitchers { get; init; }

    /// <summary>
    /// Gets whether the window stays above other windows.
    /// </summary>
    public bool? IsAlwaysOnTop { get; init; }

    /// <summary>
    /// Gets whether the title bar is visible.
    /// </summary>
    public bool? IsTitleBarVisible { get; init; }

    /// <summary>
    /// Gets whether the window should be minimized after options are applied.
    /// </summary>
    public bool MinimizeAfterApplying { get; init; }

    /// <summary>
    /// Gets the requested backdrop.
    /// </summary>
    public WindowBackdropKind BackdropKind { get; init; } = WindowBackdropKind.Unchanged;

    /// <summary>
    /// Gets whether the title bar should use transparent background colors.
    /// </summary>
    public bool UseTransparentTitleBar { get; init; }

    /// <summary>
    /// Gets whether the WinUI overlapped presenter border and title bar should be hidden.
    /// </summary>
    public bool HideSystemFrame { get; init; }

    /// <summary>
    /// Gets whether standard native frame styles should be removed.
    /// </summary>
    public bool RemoveStandardWindowFrame { get; init; }

    /// <summary>
    /// Gets whether the native window shadow should be suppressed.
    /// </summary>
    public bool SuppressWindowShadow { get; init; }

    /// <summary>
    /// Gets whether the accent-colored native window border should be suppressed.
    /// </summary>
    public bool SuppressAccentColorWindowBorder { get; init; }

    /// <summary>
    /// Gets the requested owner window behavior.
    /// </summary>
    public WindowOwnerKind OwnerKind { get; init; } = WindowOwnerKind.Unchanged;

    /// <summary>
    /// Gets the requested z-order behavior.
    /// </summary>
    public WindowZOrderKind ZOrderKind { get; init; } = WindowZOrderKind.Unchanged;
}
