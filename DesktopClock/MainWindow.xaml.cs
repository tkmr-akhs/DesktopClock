using DesktopClock.Services;
using DesktopClock.Helpers;
using DesktopClock.Views;
using DesktopClock.Win32.Tray;
using DesktopClock.Win32.Windowing;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;

namespace DesktopClock;

public sealed partial class MainWindow : WindowEx
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;
    private readonly UISettings _settings;
    private readonly ILoggingService _loggingService;
    private readonly IWindowRepositoryService _windowRepositoryService;
    private readonly IWindowChromeService _windowChromeService;
    private readonly ITrayIconService _trayIconService;

    private static readonly WindowChromeOptions SettingsWindowChromeOptions = new()
    {
        Width = 730,
        Height = 530,
        IsMaximizable = false,
        IsShownInSwitchers = false,
        MinimizeAfterApplying = true
    };

    private static readonly WindowChromeOptions ClockWindowChromeOptions = new()
    {
        IsResizable = false,
        IsMaximizable = false,
        IsMinimizable = false,
        IsShownInSwitchers = false,
        IsAlwaysOnTop = true,
        BackdropKind = WindowBackdropKind.Transparent,
        UseTransparentTitleBar = true,
        HideSystemFrame = true,
        RemoveStandardWindowFrame = true,
        SuppressWindowShadow = true
    };

    private static readonly WindowChromeOptions CalendarWindowChromeOptions = new()
    {
        IsResizable = false,
        IsMaximizable = false,
        IsMinimizable = false,
        IsTitleBarVisible = false,
        IsShownInSwitchers = false,
        BackdropKind = WindowBackdropKind.HostBlur,
        UseTransparentTitleBar = true,
        OwnerKind = WindowOwnerKind.DesktopShell,
        ZOrderKind = WindowZOrderKind.Bottom
    };

    private static readonly WindowChromeOptions SuppressWindowShadowOptions = new()
    {
        SuppressWindowShadow = true
    };

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        _settings = new UISettings();
        _settings.ColorValuesChanged += Settings_ColorValuesChanged;

        _loggingService = App.GetService<ILoggingService>();
        _windowRepositoryService = App.GetService<IWindowRepositoryService>();
        _windowChromeService = App.GetService<IWindowChromeService>();
        _trayIconService = App.GetService<ITrayIconService>();

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
        _windowChromeService.Apply(this, SettingsWindowChromeOptions);
    }

    private void CreateClockWindow()
    {
        _windowRepositoryService.TryAddWindowOfPage<ClockPage>();
        var clockWindow = _windowRepositoryService.GetWindowOfPage<ClockPage>();

        TryConfigureClockWindow(clockWindow);
        clockWindow.SizeChanged += ClockWindow_SizeChanged;

        clockWindow.Hide();
    }

    private void CreateCalendarWindow()
    {
        _windowRepositoryService.TryAddWindowOfPage<CalendarPage>();
        var calendarWindow = _windowRepositoryService.GetWindowOfPage<CalendarPage>();

        TryConfigureCalendarWindow(calendarWindow);
        calendarWindow.ZOrderChanged += CalendarWindow_ZOrderChanged;

        calendarWindow.Hide();
    }

    private void CreateNotifyIcon()
    {
        var appDisplayName = "AppDisplayName".GetLocalized();
        var exitText = "NotifyIcon_Exit".GetLocalized();
        var settingsText = "NotifyIcon_Settings".GetLocalized();

        using var iconStream = GetAssetStream("NotifyIcon.ico");
        _trayIconService.Initialize(new TrayIconOptions
        {
            IconStream = iconStream,
            TooltipText = appDisplayName,
            MenuItems =
            [
                new TrayIconMenuItem
                {
                    Text = settingsText,
                    Click = SettingsMenuItem_Click
                },
                new TrayIconMenuItem
                {
                    Text = exitText,
                    Click = ExitMenuItem_Click
                }
            ]
        });
    }

    private void CalendarWindow_ZOrderChanged(object? sender, ZOrderInfo e)
    {
        if (sender is WindowEx calendarWindow)
        {
            calendarWindow.AppWindow.MoveInZOrderAtBottom();
        }
    }

    private void ClockWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
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
        _trayIconService.Dispose();
        App.Current.Exit();
    }

    private static Stream GetAssetStream(string assetFileName)
    {
        return File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Assets", assetFileName));
    }

    private void TryConfigureClockWindow(WindowEx window)
    {
        try
        {
            _windowChromeService.Apply(window, ClockWindowChromeOptions);
        }
        catch (Exception exp)
        {
            _loggingService.WriteLog(
                nameof(MainWindow),
                nameof(TryConfigureClockWindow),
                "Clock window customization is not available.",
                LogSeverity.Warning,
                exp);
        }
    }

    private void TryConfigureCalendarWindow(WindowEx window)
    {
        try
        {
            var result = _windowChromeService.Apply(window, CalendarWindowChromeOptions);
            if (!result.OwnerApplied)
            {
                _loggingService.WriteLog(
                    nameof(MainWindow),
                    nameof(TryConfigureCalendarWindow),
                    "Desktop owner window is not available.",
                    LogSeverity.Warning);
            }
        }
        catch (Exception exp)
        {
            _loggingService.WriteLog(
                nameof(MainWindow),
                nameof(TryConfigureCalendarWindow),
                "Calendar window customization is not available.",
                LogSeverity.Warning,
                exp);
        }
    }

    private void TrySuppressWindowShadow(WindowEx window)
    {
        try
        {
            _windowChromeService.Apply(window, SuppressWindowShadowOptions);
        }
        catch (Exception exp)
        {
            _loggingService.WriteLog(
                nameof(MainWindow),
                nameof(TrySuppressWindowShadow),
                "Window shadow customization is not available.",
                LogSeverity.Warning,
                exp);
        }
    }

    // This handles updating the caption button colors correctly when Windows system theme is changed
    // while the app is open.
    private void Settings_ColorValuesChanged(UISettings sender, object args)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            _ = App.GetService<IThemeSelectorService>().SetRequestedThemeAsync();
        });
    }
}
