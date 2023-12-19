using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopClock.Helpers;
using DesktopClock.Models;
using DesktopClock.Services;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Text;

namespace DesktopClock.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly ILoggingService _loggingService;
    private readonly IAutoStartSelectorService _autoStartSelectorService;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IWindowAlignmentSelectorService _windowAlignmentSelectorService;
    //private readonly IScreenChangeDetectionService _screenChangeDetectionService;

    private readonly IHourStyleSelectorService _hourStyleSelectorService;
    private readonly IMinuteStyleSelectorService _minuteStyleSelectorService;
    private readonly IDateStyleSelectorService _dateStyleSelectorService;
    private readonly IGooglePkceService _googlePkceService;
    private readonly IGoogleCalendarService _googleCalendarService;

    [ObservableProperty]
    private WindowAlignmentUnit _clockSizeUnit;

    [ObservableProperty]
    private string _hourFontFamily;

    [ObservableProperty]
    private FontStyle _hourFontStyle;

    [ObservableProperty]
    private FontWeight _hourFontWeight;

    [ObservableProperty]
    private double _hourHeight;

    [ObservableProperty]
    private Color _hourFontColor;

    [ObservableProperty]
    private double _hourBorderWidth;

    [ObservableProperty]
    private Color _hourBorderColor;

    [ObservableProperty]
    private string _minuteFontFamily;

    [ObservableProperty]
    private FontStyle _minuteFontStyle;

    [ObservableProperty]
    private FontWeight _minuteFontWeight;

    [ObservableProperty]
    private double _minuteHeight;

    [ObservableProperty]
    private Color _minuteFontColor;

    [ObservableProperty]
    private double _minuteBorderWidth;

    [ObservableProperty]
    private Color _minuteBorderColor;

    [ObservableProperty]
    private string _dateFontFamily;

    [ObservableProperty]
    private FontStyle _dateFontStyle;

    [ObservableProperty]
    private FontWeight _dateFontWeight;

    [ObservableProperty]
    private double _dateHeight;

    [ObservableProperty]
    private Color _dateFontColor;

    [ObservableProperty]
    private double _dateBorderWidth;

    [ObservableProperty]
    private Color _dateBorderColor;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private WindowAlignmentUnit _marginUnit;

    [ObservableProperty]
    private double _verticalMargin;

    [ObservableProperty]
    private double _horizontalMargin;

    [ObservableProperty]
    private WindowAlignment _alignment;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuthenticateCommand))]
    [NotifyCanExecuteChangedFor(nameof(UnauthenticateCommand))]
    private bool _authenticationStatus;

    [ObservableProperty]
    private bool _autoStart;

    public ObservableCollection<GoogleCalendarSetting> GoogleCalendarSettings => _googleCalendarService.CalendarSettings;

    [ObservableProperty]
    private string _versionDescription;

    public MainViewModel(
        ILoggingService loggingService,
        IAutoStartSelectorService autoStartSelectorService,
        IThemeSelectorService themeSelectorService,
        IWindowAlignmentSelectorService windowAlignmentSelectorService,
        //IScreenChangeDetectionService screenChangeDetectionService,
        IHourStyleSelectorService hourStyleSelectorService,
        IMinuteStyleSelectorService minuteStyleSelectorService,
        IDateStyleSelectorService dateTimeStyleSelectorService,
        IGooglePkceService googlePkceService,
        IGoogleCalendarService googleCalendarService)
    {
        // Set services to fields
        _loggingService = loggingService;
        _autoStartSelectorService = autoStartSelectorService;
        _themeSelectorService = themeSelectorService;
        _windowAlignmentSelectorService = windowAlignmentSelectorService;
        //_screenChangeDetectionService = screenChangeDetectionService;
        _hourStyleSelectorService = hourStyleSelectorService;
        _minuteStyleSelectorService = minuteStyleSelectorService;
        _dateStyleSelectorService = dateTimeStyleSelectorService;
        _googlePkceService = googlePkceService;
        _googleCalendarService = googleCalendarService;

        // Set current settings to properties
        _clockSizeUnit = _hourStyleSelectorService.TextSize.SizeUnit;
        if (_hourStyleSelectorService.TextSize.SizeUnit != _minuteStyleSelectorService.TextSize.SizeUnit) _minuteStyleSelectorService.SetTextStyleAsync(sizeUnit: ClockSizeUnit);
        if (_hourStyleSelectorService.TextSize.SizeUnit != _dateStyleSelectorService.TextSize.SizeUnit) _dateStyleSelectorService.SetTextStyleAsync(sizeUnit: ClockSizeUnit);

        _hourFontFamily = _hourStyleSelectorService.TextStyle.FontFamily;
        _hourFontStyle = _hourStyleSelectorService.TextStyle.FontStyle;
        _hourFontWeight = _hourStyleSelectorService.TextStyle.FontWeight;
        _hourHeight = _hourStyleSelectorService.TextSize.FontHeight;
        _hourFontColor = _hourStyleSelectorService.TextStyle.FontColor;
        _hourBorderWidth = _hourStyleSelectorService.TextSize.BorderWidth;
        _hourBorderColor = _hourStyleSelectorService.TextStyle.BorderColor;

        _minuteFontFamily = _minuteStyleSelectorService.TextStyle.FontFamily;
        _minuteFontStyle = _minuteStyleSelectorService.TextStyle.FontStyle;
        _minuteFontWeight = _minuteStyleSelectorService.TextStyle.FontWeight;
        _minuteHeight = _minuteStyleSelectorService.TextSize.FontHeight;
        _minuteFontColor = _minuteStyleSelectorService.TextStyle.FontColor;
        _minuteBorderWidth = _minuteStyleSelectorService.TextSize.BorderWidth;
        _minuteBorderColor = _minuteStyleSelectorService.TextStyle.BorderColor;

        _dateFontFamily = _dateStyleSelectorService.TextStyle.FontFamily;
        _dateFontStyle = _dateStyleSelectorService.TextStyle.FontStyle;
        _dateFontWeight = _dateStyleSelectorService.TextStyle.FontWeight;
        _dateHeight = _dateStyleSelectorService.TextSize.FontHeight;
        _dateFontColor = _dateStyleSelectorService.TextStyle.FontColor;
        _dateBorderWidth = _dateStyleSelectorService.TextSize.BorderWidth;
        _dateBorderColor = _dateStyleSelectorService.TextStyle.BorderColor;

        _elementTheme = _themeSelectorService.Theme;
        _marginUnit = _windowAlignmentSelectorService.AlignmentSetting.MarginUnit;
        _verticalMargin = _windowAlignmentSelectorService.AlignmentSetting.VerticalMargin;
        _horizontalMargin = _windowAlignmentSelectorService.AlignmentSetting.HorizontalMargin;
        _alignment = _windowAlignmentSelectorService.AlignmentSetting.Alignment;
        _authenticationStatus = _googlePkceService.IsAuthenticationRequired;
        _autoStart = _autoStartSelectorService.AutoStart;

        // Set version information
        _versionDescription = GetVersionDescription();

        // Set event handlers
        PropertyChanged += MainViewModel_PropertyChanged;
        _hourStyleSelectorService.Initialized += _hourStyleSelectorService_Initialized;
        _minuteStyleSelectorService.Initialized += _minuteStyleSelectorService_Initialized;
        _dateStyleSelectorService.Initialized += _dateStyleSelectorService_Initialized;
    }

    #region Commands

    [RelayCommand]
    private async Task SwitchThemeAsync(ElementTheme param)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(SwitchThemeAsync), severity: Services.LogSeverity.Debug);
        if (ElementTheme != param)
        {
            ElementTheme = param;
            await _themeSelectorService.SetThemeAsync(param);
        }
    }

    [RelayCommand]
    private async Task ChangeMarginUnitAsync(int param)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(ChangeMarginUnitAsync), severity: Services.LogSeverity.Debug);
        var unit = (WindowAlignmentUnit)param;

        if (MarginUnit != unit)
        {
            MarginUnit = unit;
            await _windowAlignmentSelectorService.SetAlignmentAsync(unit: unit);
        }
    }

    [RelayCommand]
    private async Task ChangeAlignmentAsync(int param)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(ChangeAlignmentAsync), severity: Services.LogSeverity.Debug);
        var alignment = (WindowAlignment)param;

        if (Alignment != alignment)
        {
            Alignment = alignment;
            await _windowAlignmentSelectorService.SetAlignmentAsync(alignment: alignment);
        }
    }

    [RelayCommand]
    private async Task ChangeClockSizeUnitAsync(int param)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(ChangeClockSizeUnitAsync), severity: Services.LogSeverity.Debug);
        var unit = (WindowAlignmentUnit)param;

        if (ClockSizeUnit != unit)
        {
            ClockSizeUnit = unit;
            await _hourStyleSelectorService.SetTextStyleAsync(sizeUnit: unit);
            await _minuteStyleSelectorService.SetTextStyleAsync(sizeUnit: unit);
            await _dateStyleSelectorService.SetTextStyleAsync(sizeUnit: unit);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteAuthenticate))]
    private async Task AuthenticateAsync(object param)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(AuthenticateAsync), severity: Services.LogSeverity.Debug);
        try
        {
            await _googlePkceService.SetAuthenticationRequiredAsync(true, CancellationToken.None);
            var userCredential = await _googlePkceService.AuthenticateAsync(CancellationToken.None);
            if (userCredential != null)
            {
                await _loggingService.WriteLogAsync(nameof(GoogleCalendarService), nameof(MainViewModel), "Authenticated.");
                AuthenticationStatus = true;
                //System.Diagnostics.Debug.WriteLine(userCredential.Token.AccessToken);
            }
            else
            {
                await _loggingService.WriteLogAsync(nameof(GoogleCalendarService), nameof(MainViewModel), "User credential is null.", severity: LogSeverity.Warning);
            }
        }
        catch (Exception exp)
        {
            await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(AuthenticateAsync), exception: exp);
            AuthenticationStatus = false;
        }
    }

    private bool CanExecuteAuthenticate(object param)
    {
        return !AuthenticationStatus;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteUnauthenticate))]
    private async Task UnauthenticateAsync(object param)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(UnauthenticateAsync), severity: Services.LogSeverity.Debug);
        try
        {
            await _googlePkceService.SetAuthenticationRequiredAsync(false, CancellationToken.None);
            AuthenticationStatus = false;
        }
        catch (Exception exp)
        {
            await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(UnauthenticateAsync), exception: exp);
        }
    }

    private bool CanExecuteUnauthenticate(object param)
    {
        return AuthenticationStatus;
    }


    #endregion

    #region Event Handlers

    private async void _hourStyleSelectorService_Initialized(object? sender, EventArgs e)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(_hourStyleSelectorService_Initialized), severity: Services.LogSeverity.Debug);

        HourFontFamily = _hourStyleSelectorService.TextStyle.FontFamily;
        HourFontStyle = _hourStyleSelectorService.TextStyle.FontStyle;
        HourFontWeight = _hourStyleSelectorService.TextStyle.FontWeight;
        HourHeight = _hourStyleSelectorService.TextSize.FontHeight;
        HourFontColor = _hourStyleSelectorService.TextStyle.FontColor;
        HourBorderWidth = _hourStyleSelectorService.TextSize.BorderWidth;
        HourBorderColor = _hourStyleSelectorService.TextStyle.BorderColor;
    }

    private async void _minuteStyleSelectorService_Initialized(object? sender, EventArgs e)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(_minuteStyleSelectorService_Initialized), severity: Services.LogSeverity.Debug);

        MinuteFontFamily = _minuteStyleSelectorService.TextStyle.FontFamily;
        MinuteFontStyle = _minuteStyleSelectorService.TextStyle.FontStyle;
        MinuteFontWeight = _minuteStyleSelectorService.TextStyle.FontWeight;
        MinuteHeight = _minuteStyleSelectorService.TextSize.FontHeight;
        MinuteFontColor = _minuteStyleSelectorService.TextStyle.FontColor;
        MinuteBorderWidth = _minuteStyleSelectorService.TextSize.BorderWidth;
        MinuteBorderColor = _minuteStyleSelectorService.TextStyle.BorderColor;
    }

    private async void _dateStyleSelectorService_Initialized(object? sender, EventArgs e)
    {
        await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(_dateStyleSelectorService_Initialized), severity: Services.LogSeverity.Debug);

        DateFontFamily = _dateStyleSelectorService.TextStyle.FontFamily;
        DateFontStyle = _dateStyleSelectorService.TextStyle.FontStyle;
        DateFontWeight = _dateStyleSelectorService.TextStyle.FontWeight;
        DateHeight = _dateStyleSelectorService.TextSize.FontHeight;
        DateFontColor = _dateStyleSelectorService.TextStyle.FontColor;
        DateBorderWidth = _dateStyleSelectorService.TextSize.BorderWidth;
        DateBorderColor = _dateStyleSelectorService.TextStyle.BorderColor;
    }

    private async void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(VerticalMargin):
                if (VerticalMargin != _windowAlignmentSelectorService.AlignmentSetting.VerticalMargin)
                {
                    await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(MainViewModel_PropertyChanged), $"{nameof(VerticalMargin)} changed. {_windowAlignmentSelectorService.AlignmentSetting.VerticalMargin} => {VerticalMargin}", severity: Services.LogSeverity.Debug);
                    await _windowAlignmentSelectorService.SetAlignmentAsync(verticalMargin: VerticalMargin);
                }
                break;
            case nameof(HorizontalMargin):
                if (HorizontalMargin != _windowAlignmentSelectorService.AlignmentSetting.HorizontalMargin)
                {
                    await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(MainViewModel_PropertyChanged), $"{nameof(HorizontalMargin)} changed. {_windowAlignmentSelectorService.AlignmentSetting.HorizontalMargin} => {HorizontalMargin}", severity: Services.LogSeverity.Debug);
                    await _windowAlignmentSelectorService.SetAlignmentAsync(horizontalMargin: HorizontalMargin);
                }
                break;
            case nameof(HourHeight):
                if (HourHeight != _hourStyleSelectorService.TextSize.FontHeight)
                {
                    await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(MainViewModel_PropertyChanged), $"{nameof(HourHeight)} changed. {_hourStyleSelectorService.TextSize.FontHeight} => {HourHeight}", severity: Services.LogSeverity.Debug);
                    await _hourStyleSelectorService.SetTextStyleAsync(fontHeight: HourHeight);
                    _windowAlignmentSelectorService.AdjustSize();
                    _windowAlignmentSelectorService.SetRequestedAlignment();
                }
                break;
            case nameof(MinuteHeight):
                if (MinuteHeight != _minuteStyleSelectorService.TextSize.FontHeight)
                {
                    await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(MainViewModel_PropertyChanged), $"{nameof(MinuteHeight)} changed. {_minuteStyleSelectorService.TextSize.FontHeight} => {MinuteHeight}", severity: Services.LogSeverity.Debug);
                    await _minuteStyleSelectorService.SetTextStyleAsync(fontHeight: MinuteHeight);
                    _windowAlignmentSelectorService.AdjustSize();
                    _windowAlignmentSelectorService.SetRequestedAlignment();
                }
                break;
            case nameof(DateHeight):
                if (DateHeight != _dateStyleSelectorService.TextSize.FontHeight)
                {
                    await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(MainViewModel_PropertyChanged), $"{nameof(DateHeight)} changed. {_dateStyleSelectorService.TextSize.FontHeight} => {DateHeight}", severity: Services.LogSeverity.Debug);
                    await _dateStyleSelectorService.SetTextStyleAsync(fontHeight: DateHeight);
                    _windowAlignmentSelectorService.AdjustSize();
                    _windowAlignmentSelectorService.SetRequestedAlignment();
                }
                break;
            case nameof(AutoStart):
                if (AutoStart != _autoStartSelectorService.AutoStart)
                {
                    await _loggingService.WriteLogAsync(nameof(MainViewModel), nameof(MainViewModel_PropertyChanged), $"{nameof(AutoStart)} changed. {_autoStartSelectorService.AutoStart} => {AutoStart}", severity: Services.LogSeverity.Debug);
                    await _autoStartSelectorService.SetAsync(AutoStart);
                }
                break;
        }
    }

    #endregion

    #region Helper Methods

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    #endregion
}
