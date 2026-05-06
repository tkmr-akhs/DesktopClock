namespace DesktopClock.Win32.Tray;

/// <summary>
/// Provides a Windows notification area icon.
/// </summary>
public interface ITrayIconService : IDisposable
{
    /// <summary>
    /// Initializes the notification area icon and its menu.
    /// </summary>
    /// <param name="options">The notification area icon options.</param>
    void Initialize(TrayIconOptions options);
}
