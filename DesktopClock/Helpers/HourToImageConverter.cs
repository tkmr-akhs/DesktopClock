using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DesktopClock.Helpers;

internal class HourToImageConverter : IValueConverter
{
    private readonly IHourStyleSelectorService _timeStyleSelectorService;
    
    internal HourToImageConverter()
    {
        _timeStyleSelectorService = App.GetService<IHourStyleSelectorService>();
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is char number)
        {
            return _timeStyleSelectorService.GetImageAsync(number.ToString()[0]).GetAwaiter().GetResult();
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
