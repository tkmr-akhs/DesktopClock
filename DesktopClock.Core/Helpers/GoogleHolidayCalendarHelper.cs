namespace DesktopClock.Core.Helpers;

/// <summary>
/// Provides helper methods for Google holiday calendar identifiers and events.
/// </summary>
public static class GoogleHolidayCalendarHelper
{
    private const string JapaneseHolidayCalendarSuffix = ".japanese#holiday@group.v.calendar.google.com";
    private const string JapaneseOfficialHolidayCalendarSuffix = ".japanese.official#holiday@group.v.calendar.google.com";

    /// <summary>
    /// Converts a Google Japanese holiday calendar identifier to the public-holiday-only identifier when possible.
    /// </summary>
    /// <param name="calendarId">The Google Calendar identifier.</param>
    /// <returns>The public-holiday-only calendar identifier if the calendar is a Japanese holiday calendar; otherwise, the original identifier.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="calendarId"/> is null or empty.</exception>
    public static string ToPublicHolidayOnlyCalendarId(string calendarId)
    {
        ArgumentException.ThrowIfNullOrEmpty(calendarId);

        if (calendarId.EndsWith(JapaneseOfficialHolidayCalendarSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return calendarId;
        }

        if (!calendarId.EndsWith(JapaneseHolidayCalendarSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return calendarId;
        }

        var localePrefix = calendarId[..^JapaneseHolidayCalendarSuffix.Length];
        return $"{localePrefix}{JapaneseOfficialHolidayCalendarSuffix}";
    }

    /// <summary>
    /// Determines whether a Google Calendar identifier is for a Japanese holiday calendar.
    /// </summary>
    /// <param name="calendarId">The Google Calendar identifier.</param>
    /// <returns>True if the identifier is for a Japanese holiday calendar; otherwise, false.</returns>
    public static bool IsJapaneseHolidayCalendarId(string calendarId)
    {
        if (String.IsNullOrEmpty(calendarId)) return false;

        return calendarId.EndsWith(JapaneseHolidayCalendarSuffix, StringComparison.OrdinalIgnoreCase)
            || calendarId.EndsWith(JapaneseOfficialHolidayCalendarSuffix, StringComparison.OrdinalIgnoreCase);
    }
}
