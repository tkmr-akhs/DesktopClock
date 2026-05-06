using DesktopClock.Win32.Native;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;
using WinUIEx;
using FoundationSize = Windows.Foundation.Size;

namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Provides WinUI window sizing operations backed by native window metrics.
/// </summary>
public sealed class WindowSizeService : IWindowSizeService
{
    /// <inheritdoc />
    public FoundationSize GetClientSize(WindowEx window)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        if (!TryGetClientRect(windowHandle, out var clientRect))
        {
            return new FoundationSize(window.Width, window.Height);
        }

        var scale = GetRasterizationScale(window);
        return new FoundationSize(clientRect.Width / scale, clientRect.Height / scale);
    }

    /// <inheritdoc />
    public void ResizeClientArea(WindowEx window, FoundationSize desiredClientSize)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        if (windowHandle == IntPtr.Zero
            || !TryGetWindowRect(windowHandle, out var windowRect)
            || !TryGetClientRect(windowHandle, out var clientRect))
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

    private static bool TryGetWindowRect(IntPtr windowHandle, out NativeRect rect)
    {
        rect = default;
        return windowHandle != IntPtr.Zero && User32.GetWindowRect(windowHandle, out rect);
    }

    private static bool TryGetClientRect(IntPtr windowHandle, out NativeRect rect)
    {
        rect = default;
        return windowHandle != IntPtr.Zero && User32.GetClientRect(windowHandle, out rect);
    }
}
