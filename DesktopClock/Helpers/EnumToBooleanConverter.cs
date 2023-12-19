using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using DesktopClock.Models;

namespace DesktopClock.Helpers;

public class EnumToBooleanConverter<T> : IValueConverter
{
    public EnumToBooleanConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
            }

            var enumValue = Enum.Parse(typeof(T), enumString);

            return enumValue.Equals(value);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse(typeof(T), enumString);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }
}

public class ElementThemeToBooleanConverter : EnumToBooleanConverter<ElementTheme>
{
    public ElementThemeToBooleanConverter() { }
}

public class GoogleCalendarDisplayTypeToBooleanConverter : EnumToBooleanConverter<GoogleCalendarDisplayType>
{
    public GoogleCalendarDisplayTypeToBooleanConverter()
    {
    }
}


public class WindowAlignmentToBooleanConverter : EnumToBooleanConverter<WindowAlignment>
{
    public WindowAlignmentToBooleanConverter()
    {
    }
}

public class WindowAlignmentUnitToBooleanConverter : EnumToBooleanConverter<WindowAlignmentUnit>
{
    public WindowAlignmentUnitToBooleanConverter()
    {
    }
}