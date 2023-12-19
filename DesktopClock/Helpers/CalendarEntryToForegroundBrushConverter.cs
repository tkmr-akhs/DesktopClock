using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using DesktopClock.Core.Models;

namespace DesktopClock.Helpers;
internal class CalendarEntryToForegroundBrushConverter : IValueConverter
{
    private readonly ICalendarStyleSelectorService _calendarStyleSelectorService;

    internal CalendarEntryToForegroundBrushConverter()
    {
        _calendarStyleSelectorService = App.GetService<ICalendarStyleSelectorService>();
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {

        if (!(value is CalendarEntry)) throw new NotImplementedException();

        var calEntry = (CalendarEntry)value;

        var color = _calendarStyleSelectorService.ForegroundColor;

        if (calEntry.Date == DateOnly.FromDateTime(DateTime.Today))
        {
            color = GetNegativeColor(_calendarStyleSelectorService.ForegroundColor);
        }
        else if (calEntry.IsScheduledDay)
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

        return new SolidColorBrush(color);
    }

    private static Color GetNegativeColor(Color color)
    {
        return Color.FromArgb(color.A, (byte)(Byte.MaxValue - color.R), (byte)(Byte.MaxValue - color.G), (byte)(Byte.MaxValue - color.B));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
