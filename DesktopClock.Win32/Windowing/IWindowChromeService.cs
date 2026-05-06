using WinUIEx;

namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Applies Windows-specific chrome customization to WinUI windows.
/// </summary>
public interface IWindowChromeService
{
    /// <summary>
    /// Applies window chrome options.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="options">The chrome options.</param>
    /// <returns>The result of applying the chrome options.</returns>
    WindowChromeResult Apply(WindowEx window, WindowChromeOptions options);
}
