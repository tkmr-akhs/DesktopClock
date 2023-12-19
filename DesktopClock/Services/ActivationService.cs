using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DesktopClock.Activation;
using DesktopClock.Core.Contracts.Services;

namespace DesktopClock.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IAutoStartSelectorService _autoStartSelectorService;
    private readonly IDateTimeProviderService _dateTimeProviderService;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IHourStyleSelectorService _hourStyleSelectorService;
    private readonly IMinuteStyleSelectorService _minuteStyleSelectorService;
    private readonly IDateStyleSelectorService _dateStyleSelectorService;
    private readonly ICalendarStyleSelectorService _calendarStyleSelectorService;
    private readonly IWindowRepositoryService _windowRepositoryService;
    private readonly IWindowAlignmentSelectorService _windowAlignmentSelectorService;
    private readonly IScreenChangeDetectionService _screenChangedDetectionService;
    private readonly IGooglePkceService _googlePkceService;
    private readonly IGoogleCalendarService _googleCalendarService;
    private UIElement? _shell = null;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
        IEnumerable<IActivationHandler> activationHandlers,
        IAutoStartSelectorService autoStartSelectorService,
        IDateTimeProviderService dateTimeProviderService,
        IThemeSelectorService themeSelectorService,
        IHourStyleSelectorService hourStyleSelectorService,
        IMinuteStyleSelectorService minuteStyleSelectorService,
        IDateStyleSelectorService dateStyleSelectorService,
        ICalendarStyleSelectorService calendarStyleSelectorService,
        IWindowRepositoryService windowRepositoryService,
        IWindowAlignmentSelectorService windowAlignmentSelectorService,
        IScreenChangeDetectionService screenChangedDetectionService,
        IGooglePkceService googlePkceService,
        IGoogleCalendarService googleCalendarService)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _autoStartSelectorService = autoStartSelectorService;
        _dateTimeProviderService = dateTimeProviderService;
        _themeSelectorService = themeSelectorService;
        _hourStyleSelectorService = hourStyleSelectorService;
        _minuteStyleSelectorService = minuteStyleSelectorService;
        _dateStyleSelectorService = dateStyleSelectorService;
        _calendarStyleSelectorService = calendarStyleSelectorService;
        _windowRepositoryService = windowRepositoryService;
        _windowAlignmentSelectorService = windowAlignmentSelectorService;
        _screenChangedDetectionService = screenChangedDetectionService;
        _googlePkceService = googlePkceService;
        _googleCalendarService = googleCalendarService;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            App.MainWindow.Content = _shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        await _autoStartSelectorService.InitializeAsync().ConfigureAwait(false);
        await _windowRepositoryService.InitializeAsync().ConfigureAwait(false);
        await _windowAlignmentSelectorService.InitializeAsync().ConfigureAwait(false);
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
        //await _hourStyleSelectorService.InitializeAsync().ConfigureAwait(false);
        //await _minuteStyleSelectorService.InitializeAsync().ConfigureAwait(false);
        //await _dateStyleSelectorService.InitializeAsync().ConfigureAwait(false);
        await _calendarStyleSelectorService.InitializeAsync().ConfigureAwait(false);
        await _screenChangedDetectionService.InitializeAsync().ConfigureAwait(false);
        await _googlePkceService.InitializeAsync().ConfigureAwait(false);
        await _googleCalendarService.InitializeAsync().ConfigureAwait(false);
        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        await _themeSelectorService.SetRequestedThemeAsync();
        await Task.CompletedTask;
    }
}
