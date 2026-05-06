namespace DesktopClock.Win32.Tray;

/// <summary>
/// Describes one notification area context-menu item.
/// </summary>
public sealed class TrayIconMenuItem
{
    /// <summary>
    /// Gets the item kind.
    /// </summary>
    public TrayIconMenuItemKind Kind { get; init; } = TrayIconMenuItemKind.Command;

    /// <summary>
    /// Gets the display text.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Gets the handler invoked when the item is clicked.
    /// </summary>
    public EventHandler? Click { get; init; }

    /// <summary>
    /// Gets a factory for the optional item image stream.
    /// </summary>
    public Func<Stream?>? ImageStreamFactory { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the item is visible.
    /// </summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the item is checked.
    /// </summary>
    public bool IsChecked { get; init; }

    /// <summary>
    /// Gets the child menu items.
    /// </summary>
    public IReadOnlyList<TrayIconMenuItem> Children { get; init; } = Array.Empty<TrayIconMenuItem>();
}
