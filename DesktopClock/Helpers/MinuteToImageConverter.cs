using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DesktopClock.Helpers;

internal class MinuteToImageConverter : IValueConverter
{
    private readonly IMinuteStyleSelectorService _timeStyleSelectorService;
    
    internal MinuteToImageConverter()
    {
        _timeStyleSelectorService = App.GetService<IMinuteStyleSelectorService>();
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is char number)
        {
            //return Task<Microsoft.UI.Xaml.Media.Imaging.BitmapImage>.Run<Microsoft.UI.Xaml.Media.Imaging.BitmapImage>(() => _timeStyleSelectorService.GetImageAsync(number.ToString()[0])).GetAwaiter().GetResult();
            return _timeStyleSelectorService.GetImageAsync(number.ToString()[0]).GetAwaiter().GetResult();
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
