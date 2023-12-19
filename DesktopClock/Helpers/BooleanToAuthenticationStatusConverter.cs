using Microsoft.UI.Xaml.Data;

namespace DesktopClock.Helpers;

internal class BooleanToAuthenticationStatusConverter : IValueConverter
{
    private const string unknownStatus = "(Unknown)";

    public BooleanToAuthenticationStatusConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var resultMessage = unknownStatus;
        var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

        if (value is bool isAuthorized) {
            if (isAuthorized) resultMessage = resourceLoader.GetString("Authenticated");
            else resultMessage = resourceLoader.GetString("Unauthenticated");
        }
        return resultMessage;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
