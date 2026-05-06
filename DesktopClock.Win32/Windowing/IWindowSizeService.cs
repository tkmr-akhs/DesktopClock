using WinUIEx;
using FoundationSize = Windows.Foundation.Size;

namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Provides WinUI window sizing operations backed by native window metrics.
/// </summary>
public interface IWindowSizeService
{
    /// <summary>
    /// Gets the current client area size in WinUI effective pixels.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <returns>The client area size.</returns>
    FoundationSize GetClientSize(WindowEx window);

    /// <summary>
    /// Resizes a window so its client area matches the requested WinUI effective-pixel size.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="desiredClientSize">The requested client area size.</param>
    void ResizeClientArea(WindowEx window, FoundationSize desiredClientSize);
}
