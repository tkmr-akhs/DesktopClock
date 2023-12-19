using DesktopClock.Core.Models;

namespace DesktopClock.Core.Helpers;

/// <summary>
/// A helper class for calculating holidays' duration
/// </summary>
public class HolidayDurationCalculator
{
    private readonly MonthlyCalendar _calendar;

    /// <summary>
    /// Initializes a new instance of the HolidayDurationCalculator class.
    /// </summary>
    /// <param name="calendar">A MonthlyCalendar object providing monthly calendar data.</param>
    public HolidayDurationCalculator(MonthlyCalendar calendar)
    {
        _calendar = calendar;
    }

    /// <summary>
    /// Calculates the duration of holidays related to the specified date. Ignores 2-day weekends consisting only of Saturday and Sunday.
    /// </summary>
    /// <param name="today">The date to be checked. Defaults to today's date.</param>
    /// <param name="searchDepth">The depth of days to check.</param>
    /// <returns>A HolidayDurationInfo object containing information about the holiday duration.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if searchDepth is less than 1, or if today is not included in the calendar.</exception>
    public HolidayDurationInfo CalculateHolidayDuration(DateOnly today = default, int searchDepth = 3)
    {
        if (searchDepth < 1) throw new ArgumentOutOfRangeException(nameof(searchDepth), $"The search depth must be 1 or greater. Provided search depth: {searchDepth}.");

        today = today == default ? DateOnly.FromDateTime(DateTime.Today) : today;
        var tomorrow = today.AddDays(1);

        if (!_calendar.ContainsKey(today)) throw new ArgumentOutOfRangeException(nameof(today), $"Today isn't contained in the calendar. Provided date as today: {today}");

        HolidayDurationInfo result = default;

        if (IsNonWorkingDayTotally(tomorrow))
        {
            // 今日をチェックする
            CalculateHolidayDurationContainingToday(today, out var totalDaysToday, out var startInDaysToday);

            // 今日が連休の一部なら終了
            if (totalDaysToday != 0)
            {
                result = new(totalDaysToday, startInDaysToday);
            }
        }

        if (result == default) {
            // 明日以降をチェックする
            var checkDay = today.AddDays(1);
            var startInDays = 1;
            for (int i = 1; i < searchDepth; i++)
            {
                CalculateHolidayDurationAfterTomorrow(checkDay, out var totalDays);
                if (totalDays != 0)
                {
                    result = new(totalDays, startInDays);
                    break;
                }
                checkDay = checkDay.AddDays(1);
                startInDays++;
            }
        }

        // 連休が見つからなかったら空の情報を返す
        result = result == default ? HolidayDurationInfo.Empty : result;

        return result;
    }

    /// <summary>
    /// Checks if today is a holiday and calculates the consecutive number of holiday days including today. If the previous day(s) were also holidays, it processes retrospectively. Ignores 2-day weekends consisting only of Saturday and Sunday.
    /// </summary>
    /// <param name="today">The date to be checked.</param>
    /// <param name="totalDays">Total number of consecutive holidays starting from the check date.</param>
    /// <param name="startInDays">Number of days until the holiday starts from the check date. Negative if the previous day(s) were also holidays.</param>
    private void CalculateHolidayDurationContainingToday(DateOnly today, out int totalDays, out int startInDays)
    {
        // 今日が休日でなければ 0 日間で返す。
        if (!IsNonWorkingDayTotally(today))
        {
            totalDays = 0;
            startInDays = 0;
            return;
        }

        startInDays = 0;
        var checkDay = today.AddDays(-1);
        while (_calendar.ContainsKey(checkDay) && IsNonWorkingDayTotally(checkDay))
        {
            startInDays--;
            checkDay = checkDay.AddDays(-1);
        }

        CheckHolidayCore(today, out var totalDaysFromToday);

        totalDays = -startInDays + totalDaysFromToday;

        if (totalDays == 2)
        {
            if (IsWeekend(today, totalDays, startInDays))
            {
                totalDays = 0;
                startInDays = 0;
            }
        }

        if (totalDays == 1)
        {
            if (IsSaturdayOrSunday(today))
            {
                totalDays = 0;
                startInDays = 0;
            }
        }
    }

    /// <summary>
    /// Checks for holidays on dates after tomorrow and calculates the consecutive holiday duration starting from tomorrow. Ignores 2-day weekends consisting only of Saturday and Sunday.
    /// </summary>
    /// <param name="checkDay">The date to be checked.</param>
    /// <param name="totalDays">Total number of consecutive holidays starting from the check date.</param>
    private void CalculateHolidayDurationAfterTomorrow(DateOnly checkDay, out int totalDays)
    {
        CheckHolidayCore(checkDay, out totalDays);

        if (totalDays == 2)
        {
            if (IsWeekend(checkDay, totalDays, 0))
            {
                totalDays = 0;
            }
        }

        if (totalDays == 1)
        {
            if (IsSaturdayOrSunday(checkDay))
            {
                totalDays = 0;
            }
        }
    }

    /// <summary>
    /// The core function for checking holidays for a specific date and calculating the duration.
    /// </summary>
    /// <param name="checkDay">The date to be checked.</param>
    /// <param name="totalDays">Total number of consecutive holidays starting from the check date.</param>
    private void CheckHolidayCore(DateOnly checkDay, out int totalDays)
    {
        // チェック開始日が休日でなければ 0 日間で返す。
        if (!IsNonWorkingDayTotally(checkDay))
        {
            totalDays = 0;
            return;
        }

        totalDays = 1;
        var day = checkDay.AddDays(1);
        while (_calendar.ContainsKey(checkDay) && IsNonWorkingDayTotally(day)) {
            totalDays++;
            day = day.AddDays(1);
        }
    }
    /// <summary>
    /// Determines whether the specified date is a 2-day weekend consisting only of Saturday and Sunday.
    /// </summary>
    /// <param name="checkDay">The date to be checked.</param>
    /// <param name="totalDays">Total number of consecutive holiday days.</param>
    /// <param name="startInDays">Number of days until the holiday starts from the check date.</param>
    /// <returns>True if it is a 2-day weekend consisting only of Saturday and Sunday, otherwise false.</returns>
    private bool IsWeekend(DateOnly checkDay, int totalDays, int startInDays)
    {
        if (totalDays != 2) return false;

        var day = checkDay.AddDays(startInDays);
        return _calendar[day].IsSaturday;
    }

    /// <summary>
    /// Determines whether the specified date is a non-working day (holiday, Saturday, or Sunday).
    /// </summary>
    /// <param name="checkDay">The date to be checked.</param>
    /// <returns>True if it is a non-working day, otherwise false.</returns>
    private bool IsNonWorkingDayTotally(DateOnly checkDay)
    {
        return _calendar[checkDay].IsNonWorkingDay || _calendar[checkDay].IsSaturday || _calendar[checkDay].IsSunday;
    }

    private bool IsSaturdayOrSunday(DateOnly checkDay)
    {
        return _calendar[checkDay].IsSaturday || _calendar[checkDay].IsSunday;
    }
}

/// <summary>
/// A record class providing information about holiday duration.
/// </summary>
/// <param name="TotalDays">The number of holiday days.</param>
/// <param name="StartInDays">Number of days from today until the holiday starts.</param>
public record HolidayDurationInfo(int TotalDays, int StartInDays)
{
    /// <summary>
    /// Empty information.
    /// </summary>
    public static readonly HolidayDurationInfo Empty = new(0, 0);

    /// <summary>
    /// Returns true if the holiday lasts more than one day.
    /// </summary>
    public bool IsNDayHoliday => TotalDays > 1;
}