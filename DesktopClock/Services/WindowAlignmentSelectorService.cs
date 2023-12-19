using Windows.Graphics;
using DesktopClock.Models;
using DesktopClock.Views;

namespace DesktopClock.Services;

internal class WindowAlignmentSelectorService : IWindowAlignmentSelectorService
{
    private const string SettingsKey = "WindowAlignment";

    private const int DefaultBetweenWindowsMargin = 10;

    public WindowAlignmentSetting AlignmentSetting { get; private set; }

    private readonly IWindowRepositoryService _windowRepositoryService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IScreenChangeDetectionService _screenChangedDetectionService;

    public WindowAlignmentSelectorService(ILocalSettingsService localSettingsService, IWindowRepositoryService windowRepositoryService, IScreenChangeDetectionService screenChangedDetectionService)
    {
        _localSettingsService = localSettingsService;
        _windowRepositoryService = windowRepositoryService;
        _screenChangedDetectionService = screenChangedDetectionService;
    }

    public async Task InitializeAsync()
    {
        var setting = await LoadAlignmentFromSettingsAsync();
        AlignmentSetting = setting;
        await Task.CompletedTask;
    }

    public async Task SetAlignmentAsync(WindowAlignment? alignment = null, double? verticalMargin = null, double? horizontalMargin = null, WindowAlignmentUnit? unit = null)
    {
        var setting = AlignmentSetting;

        if (alignment != null) setting = setting with { Alignment = alignment.Value };
        if (verticalMargin != null) setting = setting with { VerticalMargin = verticalMargin.Value };
        if (horizontalMargin != null) setting = setting with { HorizontalMargin = horizontalMargin.Value };
        if (unit != null) setting = setting with { MarginUnit = unit.Value };

        AlignmentSetting = setting;

        SetRequestedAlignment();
        await SaveAlignmentInSettingsAsync(setting);
    }

    public void SetRequestedAlignment()
    {
        if (_screenChangedDetectionService.ScreenBounds.Count > 0)
        {
            GetTargetWindows(out var clockWindow, out var calendarWindow);
            ArrangePosition(clockWindow, calendarWindow);
        }
    }

    public void AdjustSize()
    {
        GetTargetWindows(out var clockWindow, out var calendarWindow);
        clockWindow.Hide();
        calendarWindow.Hide();

        var clockPage = (ClockPage)clockWindow.Content;
        var calendarPage = (CalendarPage)calendarWindow.Content;

        var clockPageSize = clockPage.GetActualSize();
        var clockWidth = clockPage.GetClockWidth();
        var calendarPageSize = calendarPage.GetActualSize();

        if (clockWindow.Width != clockPageSize.Width)
        {
            clockWindow.Width = clockPageSize.Width;
        }

        if (clockWindow.Height != clockPageSize.Height)
        {
            clockWindow.Height = clockPageSize.Height;
        }

        if (calendarWindow.Width != clockWidth)
        {
            calendarWindow.Width = clockWidth;
        }

        if (calendarWindow.Height != calendarPageSize.Height)
        {
            calendarWindow.Height = calendarPageSize.Height;
        }

        clockWindow.Show();
        calendarWindow.Show();
    }

    private void GetTargetWindows(out WindowEx clockWindow, out WindowEx calendarWindow)
    {
        _windowRepositoryService.TryGetWindowOfPage<ClockPage>(out var clockWindowOrNull);
        clockWindow = clockWindowOrNull ?? throw new InvalidOperationException();

        _windowRepositoryService.TryGetWindowOfPage<CalendarPage>(out var calendarWindowOrNull);
        calendarWindow = calendarWindowOrNull ?? throw new InvalidOperationException();

    }

    private void ArrangePosition(WindowEx clockWindow, WindowEx calendarWindow)
    {
        clockWindow.Hide();
        calendarWindow.Hide();

        var clockWindowPosition = CalculateClockWindowPosition(clockWindow);
        if (clockWindow.AppWindow.Position != clockWindowPosition)
        {
            clockWindow.AppWindow.Move(clockWindowPosition);
        }

        var calendarWindowPosition = CalculateCalendarWindowPosition(clockWindow, calendarWindow);
        if (calendarWindow.AppWindow.Position != calendarWindowPosition)
        {
            calendarWindow.AppWindow.Move(calendarWindowPosition);
        }

        clockWindow.Show();
        calendarWindow.Show();
    }

    private PointInt32 CalculateClockWindowPosition(WindowEx clockWindow)
    {
        var screenBounds = GetDisplayBounds();

        var marginX = ConvertToPixel(AlignmentSetting.HorizontalMargin, AlignmentSetting.MarginUnit, WindowAlignmentOrientation.Horizontally);
        var marginY = ConvertToPixel(AlignmentSetting.VerticalMargin, AlignmentSetting.MarginUnit, WindowAlignmentOrientation.Vertically);

        int resultX;
        if (AlignmentSetting.IsLeft)
        {
            resultX = screenBounds.X + marginX;
        }
        else if (AlignmentSetting.IsHorizontalCenter)
        {
            resultX = (int)Math.Round(screenBounds.X + (screenBounds.Width - clockWindow.Width) / 2);
        }
        else
        {
            resultX = (int)Math.Round(screenBounds.X + screenBounds.Width - marginX - clockWindow.Width);
        }


        int resultY;
        if (AlignmentSetting.IsTop)
        {
            resultY = screenBounds.Y + marginY;
        }
        else if (AlignmentSetting.IsVerticalCenter)
        {
            resultY = (int)Math.Round(screenBounds.Y + (screenBounds.Height - clockWindow.Height) / 2);
        }
        else
        {
            resultY = (int)Math.Round(screenBounds.Y + screenBounds.Height - marginY - clockWindow.Height);
        }

        return new PointInt32(resultX, resultY);
    }

    private PointInt32 CalculateCalendarWindowPosition(WindowEx clockWindow, WindowEx calendarWindow)
    {
        var clockWindowPosition = clockWindow.AppWindow.Position;
        var clockWindowWidth = clockWindow.AppWindow.Size.Width;
        var calendarWindowWidth = calendarWindow.AppWindow.Size.Width;

        var calenarWindowX = clockWindowPosition.X + (clockWindowWidth - calendarWindowWidth) / 2;

        int calendarWindowY;
        if (AlignmentSetting.IsBottom)
        {
            calendarWindowY = clockWindowPosition.Y - (int)calendarWindow.Height - DefaultBetweenWindowsMargin;
        }
        else
        {

            calendarWindowY = clockWindowPosition.Y + (int)clockWindow.Height + DefaultBetweenWindowsMargin;
        }

        return new PointInt32(calenarWindowX, calendarWindowY);
    }


    private System.Drawing.Rectangle GetDisplayBounds()
    {
        if (_screenChangedDetectionService.ScreenBounds.Count <= AlignmentSetting.ScreenId)
        {
            return _screenChangedDetectionService.ScreenBounds[_screenChangedDetectionService.ScreenBounds.Count - 1];
        }
        else if (AlignmentSetting.ScreenId < _screenChangedDetectionService.ScreenBounds.Count)
        {
            return _screenChangedDetectionService.ScreenBounds[0];
        }
        else
        {
            return _screenChangedDetectionService.ScreenBounds[AlignmentSetting.ScreenId];
        }
    }

    public int ConvertToPixel(double percentOrPixel, WindowAlignmentUnit unit, WindowAlignmentOrientation orientation)
    {
        if (unit == WindowAlignmentUnit.Percent)
        {
            var screenSize =
                orientation == WindowAlignmentOrientation.Horizontally ?
                _screenChangedDetectionService.ScreenBounds[AlignmentSetting.ScreenId].Width :
                _screenChangedDetectionService.ScreenBounds[AlignmentSetting.ScreenId].Height;
            return (int)Math.Round(percentOrPixel * screenSize / 100.0);
        }
        else
        {
            return (int)Math.Round(percentOrPixel);
        }
    }

    private async Task<WindowAlignmentSetting> LoadAlignmentFromSettingsAsync()
    {
        var setting = await _localSettingsService.ReadSettingAsync<WindowAlignmentSetting>(SettingsKey);

        return setting ?? new WindowAlignmentSetting();
    }

    private async Task SaveAlignmentInSettingsAsync(WindowAlignmentSetting setting)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, setting);
    }
}
