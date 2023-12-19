using DesktopClock.Models;

namespace DesktopClock.Services;

internal class MinuteStyleSelectorService : TimeStyleSelectorServiceBase, IMinuteStyleSelectorService
{
    protected override string NumbersSettingsKey { get; } = "TimeStyleNumbers";
    protected override string TextStyleSettingsKey { get; } = "TimeTextStyle";
    protected override string TextSizeSettingsKey { get; } = "TimeTextSize";

    protected override char[] DefaultNumbers { get; } = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    protected override TextStyle DefaultTextStyle { get; } = new();
    protected override TextSize DefaultTextSize { get; } = new(FontHeight: 7, BorderWidth: 0.3, SizeUnit: WindowAlignmentUnit.Percent);

    public MinuteStyleSelectorService(ILocalSettingsService localSettingsService, IWindowAlignmentSelectorService windowAlignmentSelectorService) : base(localSettingsService, windowAlignmentSelectorService) { }
}
