using Microsoft.UI.Xaml.Data;
using DesktopClock.Core.Models;

namespace DesktopClock.Helpers;

internal class CalendarEntryToOpacityConverter : IValueConverter
{
    private const double OutsideColorAlpha = 0.50;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (!(value is CalendarEntry)) throw new NotImplementedException();

        var calEntry = (CalendarEntry)value;

        if (calEntry.IsOutsideMonth)
        {
            return OutsideColorAlpha;
        }
        else
        {
            return 1.0;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
