using System.Collections.ObjectModel;
using DesktopClock.Models;
using DesktopClock.Core.Models;

namespace DesktopClock.Contracts.Services;

public interface IGoogleCalendarService
{
    ObservableCollection<GoogleCalendarSetting> CalendarSettings { get; }
    Task InitializeAsync();
    Task ApplyScheduleToMonthlyCalendar(MonthlyCalendar calendarOfMonth, CancellationToken cancellationToken);
}
