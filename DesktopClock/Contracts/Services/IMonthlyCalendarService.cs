using DesktopClock.Core.Models;
using DesktopClock.Core.Helpers;

namespace DesktopClock.Contracts.Services;

public interface IMonthlyCalendarService
{
    event EventHandler<EventArgs> ScheduleApplied;

    MonthlyCalendar MonthlyCalendar { get; }

    HolidayDurationCalculator HolidayDurationCalculator { get; }
    
    Task ApplyScheduleAsync();
}
