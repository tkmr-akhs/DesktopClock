using System.Drawing;

namespace DesktopClock.Win32.Input;

/// <summary>
/// Provides cursor information from the Windows desktop environment.
/// </summary>
public interface ICursorService
{
    /// <summary>
    /// Gets the current cursor position in physical screen pixels.
    /// </summary>
    /// <returns>The current cursor position.</returns>
    Point GetCursorPosition();
}
