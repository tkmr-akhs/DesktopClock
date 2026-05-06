namespace DesktopClock.Win32.Tray;

/// <summary>
/// Describes a notification area icon and its context menu.
/// </summary>
public sealed class TrayIconOptions
{
    /// <summary>
    /// Gets the icon stream used for the notification area icon.
    /// </summary>
    public required Stream IconStream { get; init; }

    /// <summary>
    /// Gets the notification area icon tooltip text.
    /// </summary>
    public required string TooltipText { get; init; }

    /// <summary>
    /// Gets the root menu items shown from the notification area icon.
    /// </summary>
    public IReadOnlyList<TrayIconMenuItem> MenuItems { get; init; } = Array.Empty<TrayIconMenuItem>();
}
