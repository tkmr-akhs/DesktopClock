using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;
using FoundationSize = Windows.Foundation.Size;

namespace DesktopClock.Helpers;

/// <summary>
/// Provides helpers for sizing WinUI windows by their client area.
/// </summary>
public static class WindowSizeHelper
{
    /// <summary>
    /// Gets the current client area size in WinUI effective pixels.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <returns>The client area size in effective pixels.</returns>
    public static FoundationSize GetClientSize(WindowEx window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        if (!TryGetClientRect(hwnd, out var clientRect))
        {
            return new FoundationSize(window.Width, window.Height);
        }

        var scale = GetRasterizationScale(window);
        return new FoundationSize(clientRect.Width / scale, clientRect.Height / scale);
    }

    /// <summary>
    /// Resizes a window so its client area matches the requested WinUI effective-pixel size.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="desiredClientSize">The requested client area size in effective pixels.</param>
    public static void ResizeClientArea(WindowEx window, FoundationSize desiredClientSize)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        if (hwnd == IntPtr.Zero
            || !TryGetWindowRect(hwnd, out var windowRect)
            || !TryGetClientRect(hwnd, out var clientRect))
        {
            window.Width = desiredClientSize.Width;
            window.Height = desiredClientSize.Height;
            return;
        }

        var scale = GetRasterizationScale(window);
        var desiredClientWidth = ToPhysicalPixels(desiredClientSize.Width, scale);
        var desiredClientHeight = ToPhysicalPixels(desiredClientSize.Height, scale);
        var frameWidth = Math.Max(0, windowRect.Width - clientRect.Width);
        var frameHeight = Math.Max(0, windowRect.Height - clientRect.Height);
        var desiredWindowSize = new SizeInt32(
            desiredClientWidth + frameWidth,
            desiredClientHeight + frameHeight);

        if (window.AppWindow.Size.Width != desiredWindowSize.Width
            || window.AppWindow.Size.Height != desiredWindowSize.Height)
        {
            window.AppWindow.Resize(desiredWindowSize);
        }
    }

    private static double GetRasterizationScale(WindowEx window)
    {
        if (window.Content is FrameworkElement { XamlRoot: not null } element
            && element.XamlRoot.RasterizationScale > 0)
        {
            return element.XamlRoot.RasterizationScale;
        }

        return 1.0;
    }

    private static int ToPhysicalPixels(double effectivePixels, double scale)
    {
        return Math.Max(1, (int)Math.Ceiling(effectivePixels * scale));
    }

    private static bool TryGetWindowRect(IntPtr hwnd, out NativeRect rect)
    {
        rect = default;
        return hwnd != IntPtr.Zero && GetWindowRect(hwnd, out rect);
    }

    private static bool TryGetClientRect(IntPtr hwnd, out NativeRect rect)
    {
        rect = default;
        return hwnd != IntPtr.Zero && GetClientRect(hwnd, out rect);
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(IntPtr hwnd, out NativeRect rect);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;

        public readonly int Width => Right - Left;

        public readonly int Height => Bottom - Top;
    }
}
