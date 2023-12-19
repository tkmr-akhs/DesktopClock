namespace DesktopClock.Core.Models;

/// <summary>
/// Represents an entry in a calendar, encapsulating date and related information.
/// </summary>
/// <param name="Date">The date of the calendar entry.</param>
/// <param name="Information">Additional information associated with the date.</param>
/// <param name="IsOutsideMonth">Indicates whether the date is outside the current month's scope.</param>
/// <param name="IsNonWorkingDay">Indicates whether the date is a non-working day.</param>
/// <param name="IsScheduledDay">Indicates whether any schedules are associated with the date.</param>
public record CalendarEntry(DateOnly Date, string Information, bool IsOutsideMonth, bool IsNonWorkingDay, bool IsScheduledDay)
{
    /// <summary>
    /// Initializes a new instance of the CalendarEntry record with default values.
    /// </summary>
    public CalendarEntry() : this(DateOnly.FromDateTime(DateTime.MinValue), String.Empty, false, false, false) {}

    /// <summary>
    /// Initializes a new instance of the CalendarEntry record with a specified date and default values for other properties.
    /// </summary>
    /// <param name="date">The date of the calendar entry.</param>
    public CalendarEntry(DateOnly date) : this(date, String.Empty, false, false, false) {}

    /// <summary>
    /// Initializes a new instance of the CalendarEntry record with a specified date and outside month status, and default values for other properties.
    /// </summary>
    /// <param name="date">The date of the calendar entry.</param>
    /// <param name="isOutsideMonth">Indicates whether the date is outside the current month's scope.</param>
    public CalendarEntry(DateOnly date, bool isOutsideMonth) : this(date, String.Empty, isOutsideMonth, false, false) {}

    /// <summary>
    /// Indicates whether the date is a Saturday.
    /// </summary>
    public bool IsSaturday => Date.DayOfWeek == DayOfWeek.Saturday;

    /// <summary>
    /// Indicates whether the date is a Sunday.
    /// </summary>
    public bool IsSunday => Date.DayOfWeek == DayOfWeek.Sunday;

    /// <summary>
    /// Represents an empty calendar entry.
    /// </summary>
    public static readonly CalendarEntry Empty = new();
}
