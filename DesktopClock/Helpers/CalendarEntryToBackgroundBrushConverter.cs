using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using DesktopClock.Core.Models;

namespace DesktopClock.Helpers;

internal class CalendarEntryToBackgroundBrushConverter : IValueConverter
{
    private readonly ICalendarStyleSelectorService _calendarStyleSelectorService;

    internal CalendarEntryToBackgroundBrushConverter()
    {
        _calendarStyleSelectorService = App.GetService<ICalendarStyleSelectorService>();
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {

        if (!(value is CalendarEntry)) throw new NotImplementedException();

        var calEntry = (CalendarEntry)value;

        var color = _calendarStyleSelectorService.BackgroundColor;

        if (calEntry.Date == DateOnly.FromDateTime(DateTime.Today))
        {
            if (calEntry.IsScheduledDay)
            {
                color = _calendarStyleSelectorService.ScheduledColor;
            }
            else if (calEntry.IsNonWorkingDay)
            {
                color = _calendarStyleSelectorService.NonWorkingDayColor;
            }
            else if (calEntry.Date.DayOfWeek == DayOfWeek.Sunday)
            {
                color = _calendarStyleSelectorService.SundayColor;
            }
            else if (calEntry.Date.DayOfWeek == DayOfWeek.Saturday)
            {
                color = _calendarStyleSelectorService.SaturdayColor;
            }
            else
            {
                color = _calendarStyleSelectorService.ForegroundColor;
            }
        }

        return new SolidColorBrush(color);
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
