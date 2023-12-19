using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DesktopClock.Helpers;

internal abstract class DateInformationToImageConverterBase : IValueConverter
{
    protected IDateStyleSelectorService _dateStyleSelectorService { get; private set; }

    protected string Prefix { get; set; }
    protected string Suffix { get; set; }

    protected bool AsNonWorkingDay { get; set; }
    protected bool AsScheduledDay { get; set; }
    protected bool AsSaturday { get; set; }
    protected bool AsSunday { get; set; }

    internal DateInformationToImageConverterBase()
    {
        _dateStyleSelectorService = App.GetService<IDateStyleSelectorService>();
        Prefix = String.Empty;
        Suffix = String.Empty;
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var bitmaps = new System.Drawing.Bitmap[3];

        if (value is string dateInfo)
        {
            bitmaps[0] = _dateStyleSelectorService.GetBitmapAsync(Prefix).GetAwaiter().GetResult();

            bitmaps[1] = _dateStyleSelectorService.GetBitmapAsync(dateInfo, asScheduledDay: AsScheduledDay, asNonWorkingDay: AsNonWorkingDay, asSaturday: AsSaturday, asSunday: AsSunday).GetAwaiter().GetResult();

            bitmaps[2] = _dateStyleSelectorService.GetBitmapAsync(Suffix).GetAwaiter().GetResult();

            var combinedBitmap = ImagingHelper.CombineBitmaps(bitmaps);
            return ImagingHelper.ConvertBitmapToBitmapImage(combinedBitmap);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

internal class TodayHolidayInformationToImageConverter : DateInformationToImageConverterBase
{
    internal TodayHolidayInformationToImageConverter() : base()
    {
        var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
        Prefix = resourceLoader.GetString("ClockPage_TodayInfoPrefix");
        Suffix = resourceLoader.GetString("ClockPage_TodayInfoSuffix");
        AsNonWorkingDay = true;
    }
}

internal class TodayScheduleInformationToImageConverter : DateInformationToImageConverterBase
{
    internal TodayScheduleInformationToImageConverter() : base()
    {
        var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
        Prefix = resourceLoader.GetString("ClockPage_TodayInfoPrefix");
        Suffix = resourceLoader.GetString("ClockPage_TodayInfoSuffix");
        AsScheduledDay = true;
    }
}

internal class FutureScheduleInformationToImageConverter : DateInformationToImageConverterBase
{
    internal FutureScheduleInformationToImageConverter() : base()
    {
        AsScheduledDay = true;
    }
}

internal class FutureHolidayInformationToImageConverter : DateInformationToImageConverterBase
{
    internal FutureHolidayInformationToImageConverter() : base()
    {
        AsNonWorkingDay = true;
    }
}