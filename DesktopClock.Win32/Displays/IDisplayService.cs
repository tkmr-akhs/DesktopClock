using System.Drawing;

namespace DesktopClock.Win32.Displays;

/// <summary>
/// Provides display information from the Windows desktop environment.
/// </summary>
public interface IDisplayService
{
    /// <summary>
    /// Gets the bounds of all connected displays in physical pixels.
    /// </summary>
    /// <returns>The display bounds.</returns>
    IReadOnlyList<Rectangle> GetScreenBounds();
}
