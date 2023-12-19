using Microsoft.UI;
using Windows.UI;
using DesktopClock.Helpers;

namespace DesktopClock.Services;

internal class CalendarStyleSelectorService : ICalendarStyleSelectorService
{
    private const string ForegroundColorSettingsKey = "CalendarForegroundColor";
    private const string BackgroundColorSettingsKey = "CalendarBackgroundColor";
    private const string ScheduledColorSettingsKey = "CalendarScheduledColor";
    private const string NonWorkingDayColorSettingsKey = "CalendarNonWorkingDayColor";
    private const string SaturdayColorSettingsKey = "CalendarSaturdayColor";
    private const string SundayColorSettingsKey = "CalendarSundayColor";

    private static readonly Color DefaultForegroundColor = Colors.White;
    private static readonly Color DefaultBackgroundColor = Colors.Transparent;
    private static readonly Color DefaultScheduledColor = Colors.PaleGreen;
    private static readonly Color DefaultNonWorkingDayColor = Colors.LightCoral;
    private static readonly Color DefaultSundayColor = Colors.LightCoral;
    private static readonly Color DefaultSaturdayColor = Colors.LightSkyBlue;

    public Color ForegroundColor { get; set;  } = DefaultForegroundColor;

    public Color BackgroundColor { get; set; } = DefaultBackgroundColor;

    public Color ScheduledColor { get; set; } = DefaultScheduledColor;

    public Color NonWorkingDayColor { get; set; } = DefaultNonWorkingDayColor;

    public Color SaturdayColor { get; set; } = DefaultSaturdayColor;

    public Color SundayColor { get; set; } = DefaultSundayColor;

    public event EventHandler? StyleChanged;

    private readonly ILocalSettingsService _localSettingsService;

    public CalendarStyleSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        ForegroundColor = await LoadForegroundColorFromSettingsAsync();
        BackgroundColor = await LoadBackgroundColorFromSettingsAsync();
        ScheduledColor = await LoadScheduledColorFromSettingsAsync();
        NonWorkingDayColor = await LoadNonWorkingDayColorFromSettingsAsync();
        SaturdayColor = await LoadSaturdayColorFromSettingsAsync();
        SundayColor = await LoadSundayColorFromSettingsAsync();

        await Task.CompletedTask;
    }

    private async Task ApplyStyleAsync()
    {
        StyleChanged?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }

    public async Task SetForegroundColorAsync(Color color)
    {
        ForegroundColor = color;
        await ApplyStyleAsync();
        await SaveForegroundColorInSettingsAsync(color);
    }

    private async Task<Color> LoadForegroundColorFromSettingsAsync()
    {
        var colorCode = await _localSettingsService.ReadSettingAsync<string>(ForegroundColorSettingsKey);

        if (colorCode != null && ColorStringifyingHelper.TryParseColorCode(colorCode, out Color? color))
        {
            return color ?? DefaultForegroundColor;
        }

        return DefaultForegroundColor;
    }

    private async Task SaveForegroundColorInSettingsAsync(Color color)
    {
        await _localSettingsService.SaveSettingAsync(ForegroundColorSettingsKey, color);
    }

    public async Task SetBackgroundColorAsync(Color color)
    {
        BackgroundColor = color;
        await ApplyStyleAsync();
        await SaveBackgroundColorInSettingsAsync(color);
    }

    private async Task<Color> LoadBackgroundColorFromSettingsAsync()
    {
        var colorCode = await _localSettingsService.ReadSettingAsync<string>(BackgroundColorSettingsKey);

        if (colorCode != null && ColorStringifyingHelper.TryParseColorCode(colorCode, out Color? color))
        {
            return color ?? DefaultBackgroundColor;
        }

        return DefaultBackgroundColor;
    }

    private async Task SaveBackgroundColorInSettingsAsync(Color color)
    {
        await _localSettingsService.SaveSettingAsync(BackgroundColorSettingsKey, color);
    }

    public async Task SetScheduledColorAsync(Color color)
    {
        ScheduledColor = color;
        await ApplyStyleAsync();
        await SaveScheduledColorInSettingsAsync(color);
    }

    private async Task<Color> LoadScheduledColorFromSettingsAsync()
    {
        var colorCode = await _localSettingsService.ReadSettingAsync<string>(ScheduledColorSettingsKey);

        if (colorCode != null && ColorStringifyingHelper.TryParseColorCode(colorCode, out Color? color))
        {
            return color ?? DefaultScheduledColor;
        }

        return DefaultScheduledColor;
    }

    private async Task SaveScheduledColorInSettingsAsync(Color color)
    {
        await _localSettingsService.SaveSettingAsync(ScheduledColorSettingsKey, color);
    }

    public async Task SetNonWorkingDayColorAsync(Color color)
    {
        NonWorkingDayColor = color;
        await ApplyStyleAsync();
        await SaveNonWorkingDayColorInSettingsAsync(color);
    }

    private async Task<Color> LoadNonWorkingDayColorFromSettingsAsync()
    {
        var colorCode = await _localSettingsService.ReadSettingAsync<string>(NonWorkingDayColorSettingsKey);

        if (colorCode != null && ColorStringifyingHelper.TryParseColorCode(colorCode, out Color? color))
        {
            return color ?? DefaultNonWorkingDayColor;
        }

        return DefaultNonWorkingDayColor;
    }

    private async Task SaveNonWorkingDayColorInSettingsAsync(Color color)
    {
        await _localSettingsService.SaveSettingAsync(NonWorkingDayColorSettingsKey, color);
    }

    public async Task SetSaturdayColorAsync(Color color)
    {
        SaturdayColor = color;
        await ApplyStyleAsync();
        await SaveSaturdayColorInSettingsAsync(color);
    }

    private async Task<Color> LoadSaturdayColorFromSettingsAsync()
    {
        var colorCode = await _localSettingsService.ReadSettingAsync<string>(SaturdayColorSettingsKey);

        if (colorCode != null && ColorStringifyingHelper.TryParseColorCode(colorCode, out Color? color))
        {
            return color ?? DefaultSaturdayColor;
        }

        return DefaultSaturdayColor;
    }

    private async Task SaveSaturdayColorInSettingsAsync(Color color)
    {
        await _localSettingsService.SaveSettingAsync(SaturdayColorSettingsKey, color);
    }

    public async Task SetSundayColorAsync(Color color)
    {
        SundayColor = color;
        await ApplyStyleAsync();
        await SaveSundayColorInSettingsAsync(color);
    }

    private async Task<Color> LoadSundayColorFromSettingsAsync()
    {
        var colorCode = await _localSettingsService.ReadSettingAsync<string>(SundayColorSettingsKey);

        if (colorCode != null && ColorStringifyingHelper.TryParseColorCode(colorCode, out Color? color))
        {
            return color ?? DefaultSundayColor;
        }

        return DefaultSundayColor;
    }

    private async Task SaveSundayColorInSettingsAsync(Color color)
    {
        await _localSettingsService.SaveSettingAsync(SundayColorSettingsKey, color);
    }
}
