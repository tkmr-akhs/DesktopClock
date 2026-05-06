using System.Runtime.InteropServices;
using WinRT.Interop;

namespace DesktopClock.Helpers;

internal static class DesktopWindowOwnerHelper
{
    private const int WindowLongIndexOwner = -8;

    private const string ProgmanWindowClass = "Progman";

    internal static bool TrySetDesktopOwner(WindowEx window)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        var desktopHandle = FindDesktopOwnerWindow();

        if (windowHandle == IntPtr.Zero || desktopHandle == IntPtr.Zero)
        {
            return false;
        }

        return TrySetWindowOwner(windowHandle, desktopHandle);
    }

    private static IntPtr FindDesktopOwnerWindow()
    {
        var shellWindowHandle = GetShellWindow();
        return shellWindowHandle != IntPtr.Zero ? shellWindowHandle : FindWindow(ProgmanWindowClass, null);
    }

    private static bool TrySetWindowOwner(IntPtr windowHandle, IntPtr ownerHandle)
    {
        // A shell-owned popup stays in the desktop z-order band and is not swept into Win+D.
        SetLastError(0);
        var previousOwnerHandle = SetWindowLongPtr(windowHandle, WindowLongIndexOwner, ownerHandle);
        return previousOwnerHandle != IntPtr.Zero || Marshal.GetLastWin32Error() == 0;
    }

    private static IntPtr SetWindowLongPtr(IntPtr windowHandle, int index, IntPtr value)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(windowHandle, index, value)
            : new IntPtr(SetWindowLong32(windowHandle, index, value.ToInt32()));
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? className, string? windowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr windowHandle, int index, int value);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr windowHandle, int index, IntPtr value);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern void SetLastError(uint errorCode);
}
