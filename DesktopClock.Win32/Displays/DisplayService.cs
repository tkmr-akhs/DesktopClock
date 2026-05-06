using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using DesktopClock.Win32.Native;

namespace DesktopClock.Win32.Displays;

/// <summary>
/// Provides monitor bounds by using Win32 monitor enumeration.
/// </summary>
public sealed class DisplayService : IDisplayService
{
    /// <inheritdoc />
    public IReadOnlyList<Rectangle> GetScreenBounds()
    {
        var screenBounds = new List<Rectangle>();

        var result = User32.EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            (IntPtr monitorHandle, IntPtr monitorDeviceContext, ref NativeRect monitorBounds, IntPtr data) =>
            {
                var monitorInfo = new MonitorInfo
                {
                    Size = Marshal.SizeOf<MonitorInfo>()
                };

                if (!User32.GetMonitorInfo(monitorHandle, ref monitorInfo))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                screenBounds.Add(new Rectangle(
                    monitorInfo.Monitor.Left,
                    monitorInfo.Monitor.Top,
                    monitorInfo.Monitor.Width,
                    monitorInfo.Monitor.Height));

                return true;
            },
            IntPtr.Zero);

        if (!result)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return screenBounds.AsReadOnly();
    }
}
