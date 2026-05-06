using DesktopClock.Win32.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI.ViewManagement;
using WinUIColor = Windows.UI.Color;
using XamlApplication = Microsoft.UI.Xaml.Application;

namespace DesktopClock.Services;

public class ThemeSelectorService : IThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    private readonly ILocalSettingsService _localSettingsService;
    private readonly ITitleBarService _titleBarService;

    public ThemeSelectorService(ILocalSettingsService localSettingsService, ITitleBarService titleBarService)
    {
        _localSettingsService = localSettingsService;
        _titleBarService = titleBarService;
    }

    public async Task InitializeAsync()
    {
        Theme = await LoadThemeFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;

        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

    public async Task SetRequestedThemeAsync()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;
            ApplyTitleBarTheme(Theme);
        }

        await Task.CompletedTask;
    }

    private async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());
    }

    private void ApplyTitleBarTheme(ElementTheme theme)
    {
        if (!App.MainWindow.ExtendsContentIntoTitleBar)
        {
            return;
        }

        var resolvedTheme = ResolveTheme(theme);
        var resources = XamlApplication.Current.Resources;

        resources["WindowCaptionForeground"] = resolvedTheme switch
        {
            ElementTheme.Dark => new SolidColorBrush(Colors.White),
            ElementTheme.Light => new SolidColorBrush(Colors.Black),
            _ => new SolidColorBrush(Colors.Transparent)
        };

        resources["WindowCaptionForegroundDisabled"] = resolvedTheme switch
        {
            ElementTheme.Dark => new SolidColorBrush(WinUIColor.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
            ElementTheme.Light => new SolidColorBrush(WinUIColor.FromArgb(0x66, 0x00, 0x00, 0x00)),
            _ => new SolidColorBrush(Colors.Transparent)
        };

        resources["WindowCaptionButtonBackgroundPointerOver"] = resolvedTheme switch
        {
            ElementTheme.Dark => new SolidColorBrush(WinUIColor.FromArgb(0x33, 0xFF, 0xFF, 0xFF)),
            ElementTheme.Light => new SolidColorBrush(WinUIColor.FromArgb(0x33, 0x00, 0x00, 0x00)),
            _ => new SolidColorBrush(Colors.Transparent)
        };

        resources["WindowCaptionButtonBackgroundPressed"] = resolvedTheme switch
        {
            ElementTheme.Dark => new SolidColorBrush(WinUIColor.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
            ElementTheme.Light => new SolidColorBrush(WinUIColor.FromArgb(0x66, 0x00, 0x00, 0x00)),
            _ => new SolidColorBrush(Colors.Transparent)
        };

        resources["WindowCaptionButtonStrokePointerOver"] = resolvedTheme switch
        {
            ElementTheme.Dark => new SolidColorBrush(Colors.White),
            ElementTheme.Light => new SolidColorBrush(Colors.Black),
            _ => new SolidColorBrush(Colors.Transparent)
        };

        resources["WindowCaptionButtonStrokePressed"] = resolvedTheme switch
        {
            ElementTheme.Dark => new SolidColorBrush(Colors.White),
            ElementTheme.Light => new SolidColorBrush(Colors.Black),
            _ => new SolidColorBrush(Colors.Transparent)
        };

        resources["WindowCaptionBackground"] = new SolidColorBrush(Colors.Transparent);
        resources["WindowCaptionBackgroundDisabled"] = new SolidColorBrush(Colors.Transparent);

        _titleBarService.RefreshCaptionButtons(App.MainWindow);
    }

    private static ElementTheme ResolveTheme(ElementTheme theme)
    {
        if (theme != ElementTheme.Default)
        {
            return theme;
        }

        var uiSettings = new UISettings();
        var background = uiSettings.GetColorValue(UIColorType.Background);
        if (background == Colors.White)
        {
            return ElementTheme.Light;
        }

        if (background == Colors.Black)
        {
            return ElementTheme.Dark;
        }

        return XamlApplication.Current.RequestedTheme == ApplicationTheme.Light
            ? ElementTheme.Light
            : ElementTheme.Dark;
    }
}
