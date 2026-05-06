using DesktopClock.Helpers;
using Windows.UI.ViewManagement;
using DesktopClock.Services;
using WinFormsWrapper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using DesktopClock.Views;
using System.Runtime.InteropServices;

namespace DesktopClock;

public sealed partial class MainWindow : WindowEx
{
    private Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue;

    private UISettings settings;

    internal NotifyIcon DesktopClockNotifyIcon;

    private readonly IWindowRepositoryService _windowRepositoryService;

    private readonly Windows.UI.Color transparentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);

    public MainWindow()
    {
        InitializeComponent();
        
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        settings = new UISettings();
        settings.ColorValuesChanged += Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

        _windowRepositoryService = App.GetService<IWindowRepositoryService>();

        Activated += MainWindow_Activated_FirstTime;

        AppWindow.Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true;
        this.Minimize();
        this.IsShownInSwitchers = false;
    }

    private async void MainWindow_Activated_FirstTime(object sender, WindowActivatedEventArgs args)
    {
        Activated -= MainWindow_Activated_FirstTime;

        // Ideally, we want to initialize using ActivationService.InitializeAsync(),
        // but since Bitmap cannot be drawn on a thread other than the MainWindow thread,
        // we initialize here.
        await App.GetService<IHourStyleSelectorService>().InitializeAsync();
        await App.GetService<IMinuteStyleSelectorService>().InitializeAsync();
        await App.GetService<IDateStyleSelectorService>().InitializeAsync();
        await App.GetService<IDispatcherQueueService>().InitializeAsync();

        CustomizeMainWindow();

        CreateClockWindow();

        CreateCalendarWindow();

        CreateNotifyIcon();
    }

    private void CustomizeMainWindow()
    {
        this.Width = 730;
        this.Height = 530;
        this.IsMaximizable = false;
        this.IsShownInSwitchers = false;
        this.Minimize();
    }

    private void CreateClockWindow()
    {
        _windowRepositoryService.TryAddWindowOfPage<ClockPage>();
        var clockWindow = _windowRepositoryService.GetWindowOfPage<ClockPage>();

        clockWindow.IsResizable = false;
        clockWindow.IsMaximizable = false;
        clockWindow.IsMinimizable = false;
        clockWindow.IsShownInSwitchers = false;
        clockWindow.IsAlwaysOnTop = true;
        TryApplyTransparentBackdrop(clockWindow);
        clockWindow.AppWindow.TitleBar.BackgroundColor = transparentColor;
        clockWindow.AppWindow.TitleBar.InactiveBackgroundColor = transparentColor;
        TryHideSystemFrame(clockWindow, removeStandardWindowFrame: true, suppressWindowShadow: true);
        clockWindow.SizeChanged += ClockWindow_SizeChanged;

        clockWindow.Hide();
    }

    private void CreateCalendarWindow()
    {
        _windowRepositoryService.TryAddWindowOfPage<CalendarPage>();
        var calendarWindow = _windowRepositoryService.GetWindowOfPage<CalendarPage>();

        calendarWindow.IsResizable = false;
        calendarWindow.IsMaximizable = false;
        calendarWindow.IsMinimizable = false;
        calendarWindow.IsTitleBarVisible = false;
        calendarWindow.IsShownInSwitchers = false;
        TryApplyBlurredBackdrop(calendarWindow);
        calendarWindow.AppWindow.TitleBar.BackgroundColor = transparentColor;
        calendarWindow.AppWindow.TitleBar.InactiveBackgroundColor = transparentColor;
        TrySetDesktopOwner(calendarWindow);
        calendarWindow.AppWindow.MoveInZOrderAtBottom();
        calendarWindow.ZOrderChanged += CalendarWindow_ZOrderChanged;

        calendarWindow.Hide();
    }

    private void CreateNotifyIcon()
    {
        var appDisplayName = "AppDisplayName".GetLocalized();
        var exitText = "NotifyIcon_Exit".GetLocalized();
        var settingsText = "NotifyIcon_Settings".GetLocalized();

        using (var s = GetAssetStream("NotifyIcon.ico"))
        {
            DesktopClockNotifyIcon = new NotifyIcon(s, appDisplayName);
        }
        DesktopClockNotifyIcon.AddMenuItem(new NotifyIconMenuItem(settingsText, SettingsMenuItem_Click));
        DesktopClockNotifyIcon.AddMenuItem(new NotifyIconMenuItem(exitText, ExitMenuItem_Click));
    }

    private void CalendarWindow_ZOrderChanged(object? sender, ZOrderInfo e)
    {
        ((WindowEx)sender).AppWindow.MoveInZOrderAtBottom();
    }

    private static void ClockWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (sender is WindowEx clockWindow)
        {
            TrySuppressWindowShadow(clockWindow);
        }
    }

    private void SettingsMenuItem_Click(object? sender, EventArgs e)
    {
        this.IsShownInSwitchers = true;
        this.Show();
        this.Activate();
    }

    private void ExitMenuItem_Click(object? sender, EventArgs e)
    {
        var clockWindow = _windowRepositoryService.GetWindowOfPage<ClockPage>();
        var calendarWindow = _windowRepositoryService.GetWindowOfPage<CalendarPage>();

        clockWindow.Close();
        calendarWindow.Close();
        DesktopClockNotifyIcon.Dispose();
        App.Current.Exit();
    }

    private static Stream GetAssetStream(string assetFileName)
    {
        return File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Assets", assetFileName));
    }

    private static void TryApplyTransparentBackdrop(WindowEx window)
    {
        try
        {
            ApplyTransparentBackdrop(window);
        }
        catch (Exception exp)
        {
            App.GetService<ILoggingService>().WriteLog(nameof(MainWindow), nameof(TryApplyTransparentBackdrop), "Transparent backdrop is not available.", LogSeverity.Warning, exp);
        }
    }

    private static void TryApplyBlurredBackdrop(WindowEx window)
    {
        try
        {
            window.SystemBackdrop = new BlurredBackdrop();
        }
        catch (Exception exp)
        {
            App.GetService<ILoggingService>().WriteLog(nameof(MainWindow), nameof(TryApplyBlurredBackdrop), "Blurred backdrop is not available.", LogSeverity.Warning, exp);
        }
    }

    private static void TrySetDesktopOwner(WindowEx window)
    {
        try
        {
            if (!DesktopWindowOwnerHelper.TrySetDesktopOwner(window))
            {
                App.GetService<ILoggingService>().WriteLog(nameof(MainWindow), nameof(TrySetDesktopOwner), "Desktop owner window is not available.", LogSeverity.Warning);
            }
        }
        catch (Exception exp)
        {
            App.GetService<ILoggingService>().WriteLog(nameof(MainWindow), nameof(TrySetDesktopOwner), "Desktop owner customization is not available.", LogSeverity.Warning, exp);
        }
    }

    private static void ApplyTransparentBackdrop(WindowEx window)
    {
        window.SystemBackdrop = new TransparentBackdrop();
        EnableTransparentWindow(window);
    }

    private static void EnableTransparentWindow(WindowEx window)
    {
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        var backdropType = DwmSystemBackdropTypeNone;
        _ = DwmSetWindowAttribute(windowHandle, DwmWindowAttributeSystemBackdropType, ref backdropType, sizeof(int));

        if (!TryEnableRedirectionBitmapAlpha(windowHandle))
        {
            EnableBlurBehind(windowHandle);
        }
    }

    private static bool TryEnableRedirectionBitmapAlpha(IntPtr windowHandle)
    {
        var enableAlpha = NativeBooleanTrue;
        return DwmSetWindowAttribute(windowHandle, DwmWindowAttributeRedirectionBitmapAlpha, ref enableAlpha, sizeof(int)) >= 0;
    }

    private static void TryHideSystemFrame(WindowEx window, bool removeStandardWindowFrame, bool suppressWindowShadow)
    {
        try
        {
            HideSystemFrame(window, removeStandardWindowFrame, suppressWindowShadow);
        }
        catch (Exception exp)
        {
            App.GetService<ILoggingService>().WriteLog(nameof(MainWindow), nameof(TryHideSystemFrame), "System frame customization is not available.", LogSeverity.Warning, exp);
        }
    }

    private static void HideSystemFrame(WindowEx window, bool removeStandardWindowFrame, bool suppressWindowShadow)
    {
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
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

    private static void TrySuppressWindowShadow(WindowEx window)
    {
        try
        {
            SuppressWindowShadow(WinRT.Interop.WindowNative.GetWindowHandle(window));
        }
        catch (Exception exp)
        {
            App.GetService<ILoggingService>().WriteLog(nameof(MainWindow), nameof(TrySuppressWindowShadow), "Window shadow customization is not available.", LogSeverity.Warning, exp);
        }
    }

    private static void RemoveStandardWindowFrame(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        var style = GetWindowLongPtr(windowHandle, WindowLongIndexStyle);
        var newStyle = new IntPtr(style.ToInt64() & ~NativeWindowFrameStyles);
        _ = SetWindowLongPtr(windowHandle, WindowLongIndexStyle, newStyle);
        _ = SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, SetWindowPositionFrameChangedFlags);
    }

    private static void SuppressAccentColorWindowBorder(IntPtr windowHandle)
    {
        var borderColor = DwmColorNone;
        _ = DwmSetWindowAttribute(windowHandle, DwmWindowAttributeBorderColor, ref borderColor, sizeof(uint));
    }

    private static void SuppressWindowShadow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        var renderingPolicy = DwmNonClientRenderingPolicyDisabled;
        var cornerPreference = DwmWindowCornerPreferenceDoNotRound;
        _ = DwmSetWindowAttribute(windowHandle, DwmWindowAttributeNonClientRenderingPolicy, ref renderingPolicy, sizeof(int));
        _ = DwmSetWindowAttribute(windowHandle, DwmWindowAttributeWindowCornerPreference, ref cornerPreference, sizeof(int));
        _ = TryEnableRedirectionBitmapAlpha(windowHandle);
        RemoveShadowExtendedWindowStyles(windowHandle);
        _ = SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, SetWindowPositionFrameChangedFlags);
    }

    private static void RemoveShadowExtendedWindowStyles(IntPtr windowHandle)
    {
        var extendedStyle = GetWindowLongPtr(windowHandle, WindowLongIndexExtendedStyle);
        var newExtendedStyle = new IntPtr(extendedStyle.ToInt64() & ~NativeWindowShadowExtendedStyles);
        _ = SetWindowLongPtr(windowHandle, WindowLongIndexExtendedStyle, newExtendedStyle);
    }

    private static IntPtr GetWindowLongPtr(IntPtr windowHandle, int index)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(windowHandle, index)
            : new IntPtr(GetWindowLong32(windowHandle, index));
    }

    private static IntPtr SetWindowLongPtr(IntPtr windowHandle, int index, IntPtr value)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(windowHandle, index, value)
            : new IntPtr(SetWindowLong32(windowHandle, index, value.ToInt32()));
    }

    private static void EnableBlurBehind(IntPtr windowHandle)
    {
        var blurRegion = CreateRectRgn(-2, -2, -1, -1);
        try
        {
            var blurBehind = new DwmBlurBehind
            {
                Flags = DwmBlurBehindEnable | DwmBlurBehindBlurRegion,
                Enable = true,
                BlurRegion = blurRegion
            };

            _ = DwmEnableBlurBehindWindow(windowHandle, ref blurBehind);
        }
        finally
        {
            if (blurRegion != IntPtr.Zero)
            {
                DeleteObject(blurRegion);
            }
        }
    }

    // this handles updating the caption button colors correctly when windows system theme is changed
    // while the app is open
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        // This calls comes off-thread, hence we will need to dispatch it to current app's thread
        dispatcherQueue.TryEnqueue(() =>
        {
            TitleBarHelper.ApplySystemThemeToCaptionButtons();
        });
    }
    
    private class BlurredBackdrop : CompositionBrushBackdrop
    {
        protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
            => compositor.CreateHostBackdropBrush();
    }

    private class TransparentBackdrop : CompositionBrushBackdrop
    {
        protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
            => compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255));
    }

    private const int NativeBooleanTrue = 1;

    private const int DwmBlurBehindEnable = 0x00000001;

    private const int DwmBlurBehindBlurRegion = 0x00000002;

    private const int DwmWindowAttributeNonClientRenderingPolicy = 2;

    private const int DwmWindowAttributeWindowCornerPreference = 33;

    private const int DwmWindowAttributeBorderColor = 34;

    private const int DwmWindowAttributeSystemBackdropType = 38;

    private const int DwmWindowAttributeRedirectionBitmapAlpha = 39;

    private const int DwmNonClientRenderingPolicyDisabled = 1;

    private const int DwmWindowCornerPreferenceDoNotRound = 1;

    private const int DwmSystemBackdropTypeNone = 1;

    private const uint DwmColorNone = 0xFFFFFFFE;

    private const int WindowLongIndexStyle = -16;

    private const int WindowLongIndexExtendedStyle = -20;

    private const long NativeWindowStyleBorder = 0x00800000L;

    private const long NativeWindowStyleCaption = 0x00C00000L;

    private const long NativeWindowStyleDialogFrame = 0x00400000L;

    private const long NativeWindowStyleThickFrame = 0x00040000L;

    private const long NativeWindowFrameStyles = NativeWindowStyleBorder | NativeWindowStyleCaption | NativeWindowStyleDialogFrame | NativeWindowStyleThickFrame;

    private const long ExtendedWindowStyleDialogModalFrame = 0x00000001L;

    private const long ExtendedWindowStyleWindowEdge = 0x00000100L;

    private const long ExtendedWindowStyleClientEdge = 0x00000200L;

    private const long ExtendedWindowStyleStaticEdge = 0x00020000L;

    private const long NativeWindowShadowExtendedStyles =
        ExtendedWindowStyleDialogModalFrame |
        ExtendedWindowStyleWindowEdge |
        ExtendedWindowStyleClientEdge |
        ExtendedWindowStyleStaticEdge;

    private const uint SetWindowPositionNoSize = 0x0001;

    private const uint SetWindowPositionNoMove = 0x0002;

    private const uint SetWindowPositionNoZOrder = 0x0004;

    private const uint SetWindowPositionNoActivate = 0x0010;

    private const uint SetWindowPositionFrameChanged = 0x0020;

    private const uint SetWindowPositionNoOwnerZOrder = 0x0200;

    private const uint SetWindowPositionFrameChangedFlags =
        SetWindowPositionNoSize |
        SetWindowPositionNoMove |
        SetWindowPositionNoZOrder |
        SetWindowPositionNoActivate |
        SetWindowPositionFrameChanged |
        SetWindowPositionNoOwnerZOrder;

    [StructLayout(LayoutKind.Sequential)]
    private struct DwmBlurBehind
    {
        public int Flags;

        [MarshalAs(UnmanagedType.Bool)]
        public bool Enable;

        public IntPtr BlurRegion;

        [MarshalAs(UnmanagedType.Bool)]
        public bool TransitionOnMaximized;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmEnableBlurBehindWindow(IntPtr windowHandle, ref DwmBlurBehind blurBehind);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref uint attributeValue, int attributeSize);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int attributeValue, int attributeSize);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr windowHandle, int index);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr windowHandle, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr windowHandle, int index, int value);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr windowHandle, int index, IntPtr value);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr windowHandle, IntPtr windowHandleInsertAfter, int x, int y, int width, int height, uint flags);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr objectHandle);
}
