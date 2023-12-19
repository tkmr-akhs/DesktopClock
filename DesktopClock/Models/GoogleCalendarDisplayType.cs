namespace DesktopClock.Models;

/// <summary>
/// Defines the possible display types for a calendar.
/// </summary>
public enum GoogleCalendarDisplayType
{
    /// <summary>
    /// Display days with scheduled events as event days.
    /// </summary>
    Events,

    /// <summary>
    /// Display days as non-working days.
    /// </summary>
    NonWorkingDay,

    /// <summary>
    /// Do not display the calendar.
    /// </summary>
    Hidden
}