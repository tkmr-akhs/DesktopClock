using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using DesktopClock.Win32.Native;

namespace DesktopClock.Win32.Input;

/// <summary>
/// Provides cursor information by using Win32 APIs.
/// </summary>
public sealed class CursorService : ICursorService
{
    /// <inheritdoc />
    public Point GetCursorPosition()
    {
        if (!User32.GetCursorPos(out var point))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return new Point(point.X, point.Y);
    }
}
