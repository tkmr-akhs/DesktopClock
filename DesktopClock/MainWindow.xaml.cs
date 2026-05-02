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
        clockWindow.IsTitleBarVisible = false;
        clockWindow.IsShownInSwitchers = false;
        clockWindow.IsAlwaysOnTop = true;
        TryApplyTransparentBackdrop(clockWindow);
        clockWindow.AppWindow.TitleBar.BackgroundColor = transparentColor;
        clockWindow.AppWindow.TitleBar.InactiveBackgroundColor = transparentColor;

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

    private static void ApplyTransparentBackdrop(WindowEx window)
    {
        EnableBlurBehind(window);
        window.SystemBackdrop = new TransparentBackdrop();
    }

    private static void EnableBlurBehind(WindowEx window)
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

            DwmEnableBlurBehindWindow(WinRT.Interop.WindowNative.GetWindowHandle(window), ref blurBehind);
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

    private const int DwmBlurBehindEnable = 0x00000001;

    private const int DwmBlurBehindBlurRegion = 0x00000002;

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

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr objectHandle);
}
