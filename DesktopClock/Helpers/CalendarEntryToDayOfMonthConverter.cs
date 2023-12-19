using Microsoft.UI.Xaml.Data;
using DesktopClock.Core.Models;

namespace DesktopClock.Helpers;

internal class CalendarEntryToDayOfMonthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (!(value is CalendarEntry)) throw new NotImplementedException();

        var calEntry = (CalendarEntry)value;

        return calEntry.Date.Day;
        //return calEntry.Date.Day >= 10 ? calEntry.Date.Day.ToString() : " " + calEntry.Date.Day;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
