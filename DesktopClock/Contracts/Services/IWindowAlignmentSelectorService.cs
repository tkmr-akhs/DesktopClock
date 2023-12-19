using DesktopClock.Models;

namespace DesktopClock.Contracts.Services;

public interface IWindowAlignmentSelectorService
{
    WindowAlignmentSetting AlignmentSetting { get; }

    Task InitializeAsync();

    Task SetAlignmentAsync(WindowAlignment? alignment = null, double? verticalMargin = null, double? horizontalMargin = null, WindowAlignmentUnit? unit = null);

    void SetRequestedAlignment();

    void AdjustSize();

    int ConvertToPixel(double percentOrPixel, WindowAlignmentUnit unit, WindowAlignmentOrientation orientation);
}