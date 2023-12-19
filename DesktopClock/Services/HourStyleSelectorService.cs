using DesktopClock.Models;

namespace DesktopClock.Services;

internal class HourStyleSelectorService : TimeStyleSelectorServiceBase, IHourStyleSelectorService
{
    protected override string NumbersSettingsKey { get; } = "HourStyleNumbers";
    protected override string TextStyleSettingsKey { get; } = "HourTextStyle";
    protected override string TextSizeSettingsKey { get; } = "HourTextSize";

    protected override char[] DefaultNumbers { get; } = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    protected override TextStyle DefaultTextStyle { get; } = new(fontWeight: Microsoft.UI.Text.FontWeights.Bold);
    protected override TextSize DefaultTextSize { get; } = new(FontHeight: 7.5, BorderWidth: 0.3, SizeUnit: WindowAlignmentUnit.Percent);

    public HourStyleSelectorService(ILocalSettingsService localSettingsService, IWindowAlignmentSelectorService windowAlignmentSelectorService) : base(localSettingsService, windowAlignmentSelectorService) { }
}
