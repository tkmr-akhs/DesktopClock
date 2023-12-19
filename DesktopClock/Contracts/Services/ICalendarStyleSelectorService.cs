using Windows.UI;

namespace DesktopClock.Contracts.Services;

public interface ICalendarStyleSelectorService
{
    Color ForegroundColor { get; }

    Color BackgroundColor { get; }
    
    Color ScheduledColor { get; }

    Color NonWorkingDayColor { get; }

    Color SaturdayColor { get; }

    Color SundayColor { get; }

    event EventHandler? StyleChanged;

    Task InitializeAsync();

    Task SetForegroundColorAsync(Color color);

    Task SetBackgroundColorAsync(Color color);

    Task SetScheduledColorAsync(Color color);

    Task SetNonWorkingDayColorAsync(Color color);

    Task SetSaturdayColorAsync(Color color);

    Task SetSundayColorAsync(Color color);
}
