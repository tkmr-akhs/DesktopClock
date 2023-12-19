namespace DesktopClock.Contracts.Services;

public interface IAutoStartSelectorService
{
    bool AutoStart { get; }

    Task InitializeAsync();

    Task SetAsync(bool autoStart);
}
