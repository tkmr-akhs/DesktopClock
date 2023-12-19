using Windows.UI;
using Windows.UI.Text;
using DesktopClock.Models;

namespace DesktopClock.Contracts.Services;

public interface IDateStyleSelectorService
{
    TextStyle TextStyle { get; }

    TextSize TextSize { get; }

    event EventHandler? StyleChanged;

    event EventHandler? Initialized;

    Task InitializeAsync();

    Task<Microsoft.UI.Xaml.Media.Imaging.BitmapImage> GetImageAsync(string text, bool asScheduledDay = false, bool asNonWorkingDay = false, bool asSaturday = false, bool asSunday = false);

    Task<System.Drawing.Bitmap> GetBitmapAsync(string text, bool asScheduledDay = false, bool asNonWorkingDay = false, bool asSaturday = false, bool asSunday = false);

    Task SetTextStyleAsync(string? fontFamily = null, FontStyle? fontStyle = null, FontWeight? fontWeight = null, Color? fontColor = null, Color? borderColor = null, WindowAlignmentUnit? sizeUnit = null, double? fontHeight = null, double? borderWidth = null);
    
    Task SetRequestedTextStyleAsync();
}

