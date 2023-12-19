using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using DesktopClock.Core.Models;

namespace DesktopClock.Helpers;

internal class CalendarEntryToDateTextImageConverter : IValueConverter
{
    private readonly IDateStyleSelectorService _dateStyleSelectorService;
    private readonly string _dateFormat;

    internal CalendarEntryToDateTextImageConverter()
    {
        var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

        _dateFormat = resourceLoader.GetString("DateFormat");
        _dateStyleSelectorService = App.GetService<IDateStyleSelectorService>();
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var bitmaps = new System.Drawing.Bitmap[3];

        if (value is CalendarEntry calEntry)
        {
            bitmaps[0] = _dateStyleSelectorService.GetBitmapAsync(calEntry.Date.ToString(_dateFormat) + " (").GetAwaiter().GetResult();

            bitmaps[1] = _dateStyleSelectorService.GetBitmapAsync(calEntry.Date.ToString("dddd"), asNonWorkingDay: calEntry.IsNonWorkingDay, asSaturday: calEntry.IsSaturday, asSunday: calEntry.IsSunday).GetAwaiter().GetResult();

            bitmaps[2] = _dateStyleSelectorService.GetBitmapAsync(")").GetAwaiter().GetResult();

            var combinedBitmap = ImagingHelper.CombineBitmaps(bitmaps);
            return ImagingHelper.ConvertBitmapToBitmapImage(combinedBitmap);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
