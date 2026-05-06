using DesktopClock.Win32.Native;
using WinRT.Interop;
using WinUIEx;

namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Refreshes native title bar state for WinUI windows.
/// </summary>
public sealed class TitleBarService : ITitleBarService
{
    /// <inheritdoc />
    public void RefreshCaptionButtons(WindowEx window)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        if (windowHandle == User32.GetActiveWindow())
        {
            User32.SendMessage(windowHandle, NativeConstants.WindowMessageActivate, NativeConstants.WindowActivateInactive, IntPtr.Zero);
            User32.SendMessage(windowHandle, NativeConstants.WindowMessageActivate, NativeConstants.WindowActivateActive, IntPtr.Zero);
        }
        else
        {
            User32.SendMessage(windowHandle, NativeConstants.WindowMessageActivate, NativeConstants.WindowActivateActive, IntPtr.Zero);
            User32.SendMessage(windowHandle, NativeConstants.WindowMessageActivate, NativeConstants.WindowActivateInactive, IntPtr.Zero);
        }
    }
}
