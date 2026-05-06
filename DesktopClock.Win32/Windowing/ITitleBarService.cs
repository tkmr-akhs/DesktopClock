using WinUIEx;

namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Refreshes native title bar state for WinUI windows.
/// </summary>
public interface ITitleBarService
{
    /// <summary>
    /// Refreshes the native caption buttons.
    /// </summary>
    /// <param name="window">The target window.</param>
    void RefreshCaptionButtons(WindowEx window);
}
