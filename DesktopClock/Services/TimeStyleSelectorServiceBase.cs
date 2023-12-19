using Windows.UI;
using Windows.UI.Text;
using DesktopClock.Models;

namespace DesktopClock.Services;

internal abstract class TimeStyleSelectorServiceBase
{
    protected abstract string NumbersSettingsKey
    {
        get;
    }
    protected abstract string TextStyleSettingsKey
    {
        get;
    }
    protected abstract string TextSizeSettingsKey
    {
        get;
    }

    protected abstract char[] DefaultNumbers
    {
        get;
    }
    protected abstract TextStyle DefaultTextStyle
    {
        get;
    }
    protected abstract TextSize DefaultTextSize
    {
        get;
    }

    public char[] Numbers
    {
        get; set;
    }
    public TextStyle TextStyle
    {
        get; protected set;
    }
    public TextSize TextSize
    {
        get; protected set;
    }

    public event EventHandler? StyleChanged;
    public event EventHandler? Initialized;

    private readonly ILocalSettingsService _localSettingsService;
    private readonly IWindowAlignmentSelectorService _windowAlignmentSelectorService;

    private NumberImageCache _imageCache;

    public TimeStyleSelectorServiceBase(ILocalSettingsService localSettingsService, IWindowAlignmentSelectorService windowAlignmentSelectorService)
    {
        _localSettingsService = localSettingsService;
        _windowAlignmentSelectorService = windowAlignmentSelectorService;

        Numbers = DefaultNumbers;
        TextStyle = DefaultTextStyle;
        TextSize = DefaultTextSize;
    }

    protected void OnStyleChanged()
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

        CalculateTextHeightAndBorderWidth(out var fontHeightPixel, out var borderWidthPixel);

        _imageCache = new NumberImageCache(Numbers, TextStyle, borderWidthPixel, fontHeightPixel);

        OnInitialized();

        await Task.CompletedTask;
    }

    public async Task SetTextStyleAsync(string? fontFamily = null, FontStyle? fontStyle = null, FontWeight? fontWeight = null, Color? fontColor = null, Color? borderColor = null, WindowAlignmentUnit? sizeUnit = null, double? fontHeight = null, double? borderWidth = null)
    {
        var newStyle = TextStyle.With(fontFamily, fontStyle, fontWeight, fontColor, borderColor);

        TextSize newSize = TextSize;
        if (sizeUnit != null) newSize = newSize with { SizeUnit = sizeUnit.Value };
        if (fontHeight != null) newSize = newSize with { FontHeight = fontHeight.Value };
        if (borderWidth != null) newSize = newSize with { BorderWidth = borderWidth.Value };

        if (TextStyle != newStyle || TextSize != newSize)
        {
            TextStyle = newStyle;
            TextSize = newSize;
            await SetRequestedTextStyleAsync();
        }
    }

    public async Task SetRequestedTextStyleAsync()
    {
        CalculateTextHeightAndBorderWidth(out var desiredHeightPixel, out var borderWidthPixel);
        _imageCache = new NumberImageCache(Numbers, TextStyle, borderWidthPixel, desiredHeightPixel);

        OnStyleChanged();
        await SaveTextStyleInSettingsAsync(TextStyle);
        await SaveTextSizeInSettingsAsync(TextSize);
    }

    public async Task<Microsoft.UI.Xaml.Media.Imaging.BitmapImage> GetImageAsync(char num)
    {
        return _imageCache.GetImage(num);
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
}
