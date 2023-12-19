using System.Runtime.InteropServices;

namespace WinFormsWrapper;

/// <summary>
/// Provides functionality to obtain information about the screen(s) connected to the system.
/// </summary>
public static class ScreenInformation
{
    /// <summary>
    /// Retrieves the bounds of all the screens connected to the system.
    /// </summary>
    /// <returns>A read-only list of <see cref="Rectangle"/> objects representing the bounds of each screen.</returns>
    public static IReadOnlyList<Rectangle> GetScreensBounds()
    {
        IList<Rectangle> screensBounds = new List<Rectangle>();

        Interop.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
        {
            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            Interop.GetMonitorInfo(hMonitor, ref mi);

            Rectangle monitorBounds = new Rectangle(mi.rcMonitor.left, mi.rcMonitor.top,
                                                    mi.rcMonitor.right - mi.rcMonitor.left,
                                                    mi.rcMonitor.bottom - mi.rcMonitor.top);
            screensBounds.Add(monitorBounds);

            return true;
        }, IntPtr.Zero);

        return screensBounds.AsReadOnly();
    }
}
