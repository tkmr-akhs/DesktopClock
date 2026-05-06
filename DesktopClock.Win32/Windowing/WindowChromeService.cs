using System.Runtime.InteropServices;
using DesktopClock.Win32.Native;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using WinUIEx;
using WinUIColor = Windows.UI.Color;

namespace DesktopClock.Win32.Windowing;

/// <summary>
/// Applies Windows-specific chrome customization to WinUI windows.
/// </summary>
public sealed class WindowChromeService : IWindowChromeService
{
    private const string ProgmanWindowClass = "Progman";

    private static readonly WinUIColor TransparentColor = WinUIColor.FromArgb(0, 0, 0, 0);

    /// <inheritdoc />
    public WindowChromeResult Apply(WindowEx window, WindowChromeOptions options)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(options);

        ApplyWindowProperties(window, options);
        ApplyBackdrop(window, options.BackdropKind);

        if (options.UseTransparentTitleBar)
        {
            ApplyTransparentTitleBar(window);
        }

        ApplyNativeFrameOptions(window, options);
        var ownerApplied = ApplyOwner(window, options.OwnerKind);
        ApplyZOrder(window, options.ZOrderKind);

        if (options.MinimizeAfterApplying)
        {
            window.Minimize();
        }

        return new WindowChromeResult
        {
            OwnerApplied = ownerApplied
        };
    }

    private static void ApplyTransparentBackdrop(WindowEx window)
    {
        window.SystemBackdrop = new TransparentBackdrop();
        EnableTransparentWindow(window);
    }

    private static void ApplyTransparentTitleBar(WindowEx window)
    {
        window.AppWindow.TitleBar.BackgroundColor = TransparentColor;
        window.AppWindow.TitleBar.InactiveBackgroundColor = TransparentColor;
    }

    private static void EnableTransparentWindow(WindowEx window)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        var backdropType = NativeConstants.DwmSystemBackdropTypeNone;
        _ = DwmApi.DwmSetWindowAttribute(
            windowHandle,
            NativeConstants.DwmWindowAttributeSystemBackdropType,
            ref backdropType,
            sizeof(int));

        if (!TryEnableRedirectionBitmapAlpha(windowHandle))
        {
            EnableBlurBehind(windowHandle);
        }
    }

    private static bool TryEnableRedirectionBitmapAlpha(IntPtr windowHandle)
    {
        var enableAlpha = NativeConstants.NativeBooleanTrue;
        return DwmApi.DwmSetWindowAttribute(
            windowHandle,
            NativeConstants.DwmWindowAttributeRedirectionBitmapAlpha,
            ref enableAlpha,
            sizeof(int)) >= 0;
    }

    private static void HideSystemFrame(WindowEx window, bool removeStandardWindowFrame, bool suppressWindowShadow)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        if (window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
        }

        if (removeStandardWindowFrame)
        {
            RemoveStandardWindowFrame(windowHandle);
        }

        if (suppressWindowShadow)
        {
            SuppressWindowShadow(windowHandle);
        }

        SuppressAccentColorWindowBorder(windowHandle);
    }

    private static void ApplyWindowProperties(WindowEx window, WindowChromeOptions options)
    {
        if (options.Width.HasValue)
        {
            window.Width = options.Width.Value;
        }

        if (options.Height.HasValue)
        {
            window.Height = options.Height.Value;
        }

        if (options.IsResizable.HasValue)
        {
            window.IsResizable = options.IsResizable.Value;
        }

        if (options.IsMaximizable.HasValue)
        {
            window.IsMaximizable = options.IsMaximizable.Value;
        }

        if (options.IsMinimizable.HasValue)
        {
            window.IsMinimizable = options.IsMinimizable.Value;
        }

        if (options.IsShownInSwitchers.HasValue)
        {
            window.IsShownInSwitchers = options.IsShownInSwitchers.Value;
        }

        if (options.IsAlwaysOnTop.HasValue)
        {
            window.IsAlwaysOnTop = options.IsAlwaysOnTop.Value;
        }

        if (options.IsTitleBarVisible.HasValue)
        {
            window.IsTitleBarVisible = options.IsTitleBarVisible.Value;
        }
    }

    private static void ApplyBackdrop(WindowEx window, WindowBackdropKind backdropKind)
    {
        switch (backdropKind)
        {
            case WindowBackdropKind.Unchanged:
                break;
            case WindowBackdropKind.None:
                window.SystemBackdrop = null;
                break;
            case WindowBackdropKind.Transparent:
                ApplyTransparentBackdrop(window);
                break;
            case WindowBackdropKind.HostBlur:
                window.SystemBackdrop = new BlurredBackdrop();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backdropKind), backdropKind, null);
        }
    }

    private static void ApplyNativeFrameOptions(WindowEx window, WindowChromeOptions options)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        if (options.HideSystemFrame)
        {
            HideSystemFrame(window, options.RemoveStandardWindowFrame, options.SuppressWindowShadow);
            return;
        }

        if (options.RemoveStandardWindowFrame)
        {
            RemoveStandardWindowFrame(windowHandle);
        }

        if (options.SuppressWindowShadow)
        {
            SuppressWindowShadow(windowHandle);
        }

        if (options.SuppressAccentColorWindowBorder)
        {
            SuppressAccentColorWindowBorder(windowHandle);
        }
    }

    private static bool ApplyOwner(WindowEx window, WindowOwnerKind ownerKind)
    {
        return ownerKind switch
        {
            WindowOwnerKind.Unchanged => false,
            WindowOwnerKind.DesktopShell => TrySetDesktopOwner(window),
            _ => throw new ArgumentOutOfRangeException(nameof(ownerKind), ownerKind, null)
        };
    }

    private static void ApplyZOrder(WindowEx window, WindowZOrderKind zOrderKind)
    {
        switch (zOrderKind)
        {
            case WindowZOrderKind.Unchanged:
                break;
            case WindowZOrderKind.Bottom:
                window.AppWindow.MoveInZOrderAtBottom();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(zOrderKind), zOrderKind, null);
        }
    }

    private static void RemoveStandardWindowFrame(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        var style = User32.GetWindowLongPtr(windowHandle, NativeConstants.WindowLongIndexStyle);
        var newStyle = new IntPtr(style.ToInt64() & ~NativeConstants.NativeWindowFrameStyles);
        _ = User32.SetWindowLongPtr(windowHandle, NativeConstants.WindowLongIndexStyle, newStyle);
        _ = User32.SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, NativeConstants.SetWindowPositionFrameChangedFlags);
    }

    private static void SuppressAccentColorWindowBorder(IntPtr windowHandle)
    {
        var borderColor = NativeConstants.DwmColorNone;
        _ = DwmApi.DwmSetWindowAttribute(
            windowHandle,
            NativeConstants.DwmWindowAttributeBorderColor,
            ref borderColor,
            sizeof(uint));
    }

    private static void SuppressWindowShadow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        var renderingPolicy = NativeConstants.DwmNonClientRenderingPolicyDisabled;
        var cornerPreference = NativeConstants.DwmWindowCornerPreferenceDoNotRound;
        _ = DwmApi.DwmSetWindowAttribute(
            windowHandle,
            NativeConstants.DwmWindowAttributeNonClientRenderingPolicy,
            ref renderingPolicy,
            sizeof(int));
        _ = DwmApi.DwmSetWindowAttribute(
            windowHandle,
            NativeConstants.DwmWindowAttributeWindowCornerPreference,
            ref cornerPreference,
            sizeof(int));
        _ = TryEnableRedirectionBitmapAlpha(windowHandle);
        RemoveShadowExtendedWindowStyles(windowHandle);
        _ = User32.SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, NativeConstants.SetWindowPositionFrameChangedFlags);
    }

    private static void RemoveShadowExtendedWindowStyles(IntPtr windowHandle)
    {
        var extendedStyle = User32.GetWindowLongPtr(windowHandle, NativeConstants.WindowLongIndexExtendedStyle);
        var newExtendedStyle = new IntPtr(extendedStyle.ToInt64() & ~NativeConstants.NativeWindowShadowExtendedStyles);
        _ = User32.SetWindowLongPtr(windowHandle, NativeConstants.WindowLongIndexExtendedStyle, newExtendedStyle);
    }

    private static void EnableBlurBehind(IntPtr windowHandle)
    {
        var blurRegion = Gdi32.CreateRectRgn(-2, -2, -1, -1);
        try
        {
            var blurBehind = new DwmBlurBehind
            {
                Flags = NativeConstants.DwmBlurBehindEnable | NativeConstants.DwmBlurBehindBlurRegion,
                Enable = true,
                BlurRegion = blurRegion
            };

            _ = DwmApi.DwmEnableBlurBehindWindow(windowHandle, ref blurBehind);
        }
        finally
        {
            if (blurRegion != IntPtr.Zero)
            {
                _ = Gdi32.DeleteObject(blurRegion);
            }
        }
    }

    private static bool TrySetDesktopOwner(WindowEx window)
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
        var shellWindowHandle = User32.GetShellWindow();
        return shellWindowHandle != IntPtr.Zero
            ? shellWindowHandle
            : User32.FindWindow(ProgmanWindowClass, null);
    }

    private static bool TrySetWindowOwner(IntPtr windowHandle, IntPtr ownerHandle)
    {
        Kernel32.SetLastError(0);
        var previousOwnerHandle = User32.SetWindowLongPtr(windowHandle, NativeConstants.WindowLongIndexOwner, ownerHandle);
        return previousOwnerHandle != IntPtr.Zero || Marshal.GetLastWin32Error() == 0;
    }

    private sealed class BlurredBackdrop : CompositionBrushBackdrop
    {
        protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
            => compositor.CreateHostBackdropBrush();
    }

    private sealed class TransparentBackdrop : CompositionBrushBackdrop
    {
        protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
            => compositor.CreateColorBrush(WinUIColor.FromArgb(0, 255, 255, 255));
    }
}
