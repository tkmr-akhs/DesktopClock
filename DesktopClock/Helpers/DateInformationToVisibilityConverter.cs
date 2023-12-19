using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace DesktopClock.Helpers;

internal class DateInformationToVisibilityConverter : IValueConverter
{
    internal DateInformationToVisibilityConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string text)
        {
            return String.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
