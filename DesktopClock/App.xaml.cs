using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using DesktopClock.Activation;
using DesktopClock.Core.Contracts.Services;
using DesktopClock.Core.Services;
using DesktopClock.Models;
using DesktopClock.Services;
using DesktopClock.ViewModels;
using DesktopClock.Views;

namespace DesktopClock;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IAutoStartSelectorService, AutoStartSelectorService>();
            services.AddSingleton<ILocalSettingsDataStoreService, LocalSettingsDataStoreService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDateTimeProviderService, DateTimeProviderService>();
            services.AddSingleton<IHourStyleSelectorService, HourStyleSelectorService>();
            services.AddSingleton<IMinuteStyleSelectorService, MinuteStyleSelectorService>();
            services.AddSingleton<IDateStyleSelectorService, DateStyleSelectorService>();
            services.AddSingleton<ICalendarStyleSelectorService, CalendarStyleSelectorService>();
            services.AddSingleton<IWindowRepositoryService, WindowRepositoryService>();
            services.AddSingleton<IWindowAlignmentSelectorService, WindowAlignmentSelectorService>();
            services.AddSingleton<IScreenChangeDetectionService, ScreenChangedDetectionService>();
            services.AddSingleton<IMonthlyCalendarService, MonthlyCalendarService>();
            services.AddSingleton<IGooglePkceService, GooglePkceService>();
            services.AddSingleton<IGoogleCalendarService, GoogleCalendarService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IDispatcherQueueService, DispatcherQueueService>();

            // Views and ViewModels
            services.AddTransient<CalendarViewModel>();
            services.AddTransient<CalendarPage>();
            services.AddTransient<ClockViewModel>();
            services.AddTransient<ClockPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
