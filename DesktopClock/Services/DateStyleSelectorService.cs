using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.Text;
using DesktopClock.Helpers;
using DesktopClock.Models;

namespace DesktopClock.Services;

internal class DateStyleSelectorService : IDateStyleSelectorService
{
    private const string TextStyleSettingsKey = "DateTextStyle";
    private const string TextSizeSettingsKey = "DateTextSize";

    private static readonly TextStyle DefaultTextStyle = new();
    private static readonly TextSize DefaultTextSize = new(FontHeight: 2, BorderWidth: 0.3, SizeUnit: WindowAlignmentUnit.Percent);

    public TextStyle TextStyle { get; private set; } = DefaultTextStyle;
    public TextSize TextSize { get; private set; } = DefaultTextSize;

    public event EventHandler? StyleChanged;
    public event EventHandler? Initialized;

    private int desiredHeightPixel;
    private int borderWidthPixel;

    private readonly ILocalSettingsService _localSettingsService;
    private readonly IWindowAlignmentSelectorService _windowAlignmentSelectorService;
    private readonly ICalendarStyleSelectorService _calendarStyleSelectorService;

    public DateStyleSelectorService(ILocalSettingsService localSettingsService, IWindowAlignmentSelectorService windowAlignmentSelectorService, ICalendarStyleSelectorService calendarStyleSelectorService)
    {
        _localSettingsService = localSettingsService;
        _windowAlignmentSelectorService = windowAlignmentSelectorService;
        _calendarStyleSelectorService = calendarStyleSelectorService;
    }

    private void OnStyleChanged()
    {
        StyleChanged?.Invoke(this, EventArgs.Empty);
    }

    protected void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    public async Task InitializeAsync()
    {
        TextStyle = await LoadTextStyleFromSettingsAsync();
        TextSize = await LoadTextSizeFromSettingsAsync();

        CalculateTextHeightAndBorderWidth(out desiredHeightPixel, out borderWidthPixel);

        OnInitialized();

        await Task.CompletedTask;
    }

    public async Task SetTextStyleAsync(string? fontFamily = null, FontStyle? fontStyle = null, FontWeight? fontWeight = null, Color? fontColor = null, Color? borderColor = null, WindowAlignmentUnit? sizeUnit = null, double? fontHeight = null, double? borderWidth = null)
    {

        var newStyle = TextStyle.With(fontFamily, fontStyle, fontWeight, fontColor, borderColor);

        var newSize = TextSize;
        if (sizeUnit != null) newSize = newSize with { SizeUnit = sizeUnit.Value };
        if (fontHeight != null) newSize = newSize with { FontHeight = fontHeight.Value };
        if (borderWidth != null) newSize = newSize with { BorderWidth = borderWidth.Value };

        if (TextStyle != newStyle || TextSize != newSize) {
            TextStyle = newStyle;
            TextSize = newSize;
            await SetRequestedTextStyleAsync();
        }
    }

    public async Task SetRequestedTextStyleAsync()
    {
        CalculateTextHeightAndBorderWidth(out desiredHeightPixel, out borderWidthPixel);
        OnStyleChanged();
        await SaveTextStyleInSettingsAsync(TextStyle);
        await SaveTextSizeInSettingsAsync(TextSize);
    }

    public async Task<BitmapImage> GetImageAsync(string text, bool asScheduledDay = false, bool asNonWorkingDay = false, bool asSaturday = false, bool asSunday = false)
    {
        var textStyle = GetTextStyleAs(asScheduledDay, asNonWorkingDay, asSaturday, asSunday);
        var metrics = TextImagingHelper.GetMaxTextBounds(text, textStyle, borderWidthPixel, desiredHeightPixel);
        var image = TextImagingHelper.GenerateStringBitmapImage(text, textStyle, metrics);
        return image;
    }

    public async Task<System.Drawing.Bitmap> GetBitmapAsync(string text, bool asScheduledDay = false, bool asNonWorkingDay = false, bool asSaturday = false, bool asSunday = false)
    {
        var textStyle = GetTextStyleAs(asScheduledDay, asNonWorkingDay, asSaturday, asSunday);
        var metrics = TextImagingHelper.GetMaxTextBounds(text, textStyle, borderWidthPixel, desiredHeightPixel);
        var bitmap = TextImagingHelper.GenerateStringBitmap(text, textStyle, metrics);
        return bitmap;
    }

    private TextStyle GetTextStyleAs(bool asScheduledDay, bool asNonWorkingDay, bool asSaturday, bool asSunday)
    {
        if (asScheduledDay) return TextStyle.With(fontColor: _calendarStyleSelectorService.ScheduledColor);
        else if (asNonWorkingDay) return TextStyle.With(fontColor: _calendarStyleSelectorService.NonWorkingDayColor);
        else if (asSaturday) return TextStyle.With(fontColor: _calendarStyleSelectorService.SaturdayColor);
        else if (asSunday) return TextStyle.With(fontColor: _calendarStyleSelectorService.SundayColor);
        else return TextStyle;
    }

    private async Task<TextStyle> LoadTextStyleFromSettingsAsync()
    {
        var textStyle = await _localSettingsService.ReadSettingAsync<TextStyle>(TextStyleSettingsKey);

        if (textStyle != null)
        {
            return textStyle;
        }

        return DefaultTextStyle;
    }

    private async Task<TextSize> LoadTextSizeFromSettingsAsync()
    {
        var textSize = await _localSettingsService.ReadSettingAsync<TextSize>(TextSizeSettingsKey);

        if (textSize != null)
        {
            return textSize;
        }

        return DefaultTextSize;
    }

    private async Task SaveTextStyleInSettingsAsync(TextStyle textStyle)
    {
        await _localSettingsService.SaveSettingAsync(TextStyleSettingsKey, textStyle);
    }

    private async Task SaveTextSizeInSettingsAsync(TextSize textSyze)
    {
        await _localSettingsService.SaveSettingAsync(TextSizeSettingsKey, textSyze);
    }

    private void CalculateTextHeightAndBorderWidth(out int fontHeightPixel, out int borderWidthPixel)
    {
        fontHeightPixel = _windowAlignmentSelectorService.ConvertToPixel(TextSize.FontHeight, TextSize.SizeUnit, WindowAlignmentOrientation.Vertically);
        borderWidthPixel = _windowAlignmentSelectorService.ConvertToPixel(TextSize.BorderWidth, TextSize.SizeUnit, WindowAlignmentOrientation.Vertically);
    }

    /*
    private static bool TryParseColorCode(string colorCode, out Color color)
    {
        if (string.IsNullOrEmpty(colorCode) || colorCode.Length != 9 || colorCode[0] != '#')
        {
            color = default(Color);
            return false;
        }

        try
        {
            byte a = Convert.ToByte(colorCode.Substring(1, 2), 16);
            byte r = Convert.ToByte(colorCode.Substring(3, 2), 16);
            byte g = Convert.ToByte(colorCode.Substring(5, 2), 16);
            byte b = Convert.ToByte(colorCode.Substring(7, 2), 16);
            color = Color.FromArgb(a, r, g, b);
            return true;
        }
        catch (ArgumentException)
        {
            color = default(Color);
            return false;
        }
        catch (FormatException)
        {
            color = default(Color);
            return false;
        }
        catch (OverflowException)
        {
            color = default(Color);
            return false;
        }
    }

    private static string ConvertToColorCode(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }
    */
}
