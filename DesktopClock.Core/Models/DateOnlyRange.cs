namespace DesktopClock.Core.Models;

/// <summary>
/// Represents a range of dates and times. This range can be defined as inclusive or exclusive of its start and finish dates/times.
/// The range can be empty, fully closed, open, or half-open based on its start and finish inclusivity.
/// </summary>
public struct DateOnlyRange
{
    public static readonly DateOnlyRange Empty = new();

    /// <summary>
    /// Gets or sets the start date of the range.
    /// Setting a start date later than or equal to the finish date (depending on inclusivity) will throw an exception.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the set value is later than the finish date or equal when the range is half-open.</exception>
    public DateOnly Start => ToDateOnly(_dateTimeRange.Start);


    /// <summary>
    /// Gets or sets whether the start date are included in the range.
    /// Throws an exception if setting a state that is inconsistent with the finish date inclusivity when start and finish are the same.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when attempting to set an inconsistent state with the finish date.</exception>
    public bool IncludesStart => _dateTimeRange.IncludesStart;

    /// <summary>
    /// Gets or sets the finish date of the range.
    /// Setting a finish date earlier than or equal to the start date (depending on inclusivity) will throw an exception.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the set value is earlier than the start date or equal when the range is half-open.</exception>
    public DateOnly Finish => ToDateOnly(_dateTimeRange.Finish);

    /// <summary>
    /// Gets or sets whether the finish date are included in the range.
    /// Throws an exception if setting a state that is inconsistent with the start date inclusivity when start and finish are the same.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when attempting to set an inconsistent state with the start date.</exception>
    public bool IncludesFinish => _dateTimeRange.IncludesFinish;

    /// <summary>
    /// Gets the duration of the date range.
    /// Duration is calculated as the difference between the finish and start date.
    /// </summary>
    public TimeSpan Offset => _dateTimeRange.Finish - _dateTimeRange.Start;

    /// <summary>
    /// Determines whether the date range is empty. 
    /// A range is considered empty if the start and finish are the same and neither is included in the range.
    /// </summary>
    public bool IsEmpty => _dateTimeRange.IsEmpty;

    private readonly DateTimeRange _dateTimeRange;

    /// <summary>
    /// Initializes a new instance of the DateOnlyRange struct with the minimum value for both start and finish dates.
    /// This constructor creates an empty range.
    /// </summary>
    public DateOnlyRange() : this(DateOnly.MinValue, DateOnly.MinValue, includesStart: false) { }

    /// <summary>
    /// Initializes a new instance of the DateOnlyRange struct with specified start and finish dates.
    /// IncludesStart and IncludesFinish determine if the start and finish dates are inclusive in the range.
    /// </summary>
    /// <param name="start">The start date of the range.</param>
    /// <param name="finish">The finish date of the range.</param>
    /// <param name="includesStart">Whether the start date is included in the range.</param>
    /// <param name="includesFinish">Whether the finish date is included in the range.</param>
    /// <exception cref="ArgumentException">Thrown when the finish date is earlier than the start date or when start and finish are the same but includesStart does not equal includesFinish.</exception>
    public DateOnlyRange(DateOnly start, DateOnly finish, bool includesStart = true, bool includesFinish = false)
    {
        _dateTimeRange = new(ToDateTime(start),ToDateTime(finish), includesStart, includesFinish);
    }

    private DateOnlyRange(DateTimeRange dateTimeRange)
    {
        _dateTimeRange = dateTimeRange;
    }

    private static DateTime ToDateTime(DateOnly dateOnly)
    {
        return dateOnly.ToDateTime(TimeOnly.MinValue);
    }

    private static DateOnly ToDateOnly(DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }

    /// <summary>
    /// Determines if the specified DateOnlyRange is completely included within this range.
    /// Considers the inclusivity of start and finish dates/times for both ranges.
    /// </summary>
    /// <param name="range">The DateOnlyRange to check for inclusion.</param>
    /// <returns>True if the specified range is entirely included within this range, false otherwise.</returns>
    public bool Includes(DateOnlyRange range)
    {
        return _dateTimeRange.Includes(range._dateTimeRange);
    }

    /// <summary>
    /// Determines if the specified DateTime is included within this range.
    /// Considers the inclusivity of the start and finish dates/times.
    /// </summary>
    /// <param name="dateTime">The DateTime to check for inclusion in the range.</param>
    /// <returns>True if the dateTime is included in the range, false otherwise.</returns>
    public bool Includes(DateTime dateTime)
    {
        var dateOnly = ToDateOnly(dateTime);
        return Includes(dateOnly);
    }

    /// <summary>
    /// Determines if the specified date is included within this range.
    /// Allows specifying custom inclusivity for start and finish dates.
    /// </summary>
    /// <param name="date">The date to check for inclusion.</param>
    /// <returns>True if the date is included, false otherwise.</returns>
    public bool Includes(DateOnly date)
    {
        var dateTime = ToDateTime(date);
        return _dateTimeRange.Includes(dateTime);
    }

    /// <summary>
    /// Enumerates all dates included in this range.
    /// Uses an iterator to efficiently yield each date, especially useful for large date ranges.
    /// </summary>
    /// <returns>An IEnumerable<DateOnly> that can be iterated over to access each date within the range.</returns>
    public IEnumerable<DateOnly> GetAllDatesInRange()
    {
        return _dateTimeRange.GetAllDatesInRange(_dateTimeRange.IncludesStart, _dateTimeRange.IncludesFinish);
    }

    /// <summary>
    /// Determines if this range overlaps with another specified range.
    /// Considers the inclusivity of start and finish dates/times for both ranges.
    /// </summary>
    /// <param name="range">The DateOnlyRange to check for overlap.</param>
    /// <returns>True if the ranges overlap, false otherwise.</returns>
    public bool Overlaps(DateOnlyRange range)
    {
        return _dateTimeRange.Overlaps(range._dateTimeRange);
    }

    /// <summary>
    /// Gets the intersection between this range and another specified range.
    /// Considers the inclusivity of start and finish dates/times for both ranges.
    /// Returns an empty range if they do not intersect.
    /// </summary>
    /// <param name="range">The DateOnlyRange to intersect with.</param>
    /// <returns>A DateOnlyRange representing the intersection of the two ranges, or an empty range if they do not intersect.</returns>
    public DateOnlyRange GetIntersection(DateOnlyRange range)
    {
        var intersection = _dateTimeRange.GetIntersection(range._dateTimeRange);
        var result = new DateOnlyRange(intersection);
        return result;
    }

    /// <summary>
    /// Gets the union of this range with another specified range.
    /// Considers the inclusivity of start and finish dates/times for both ranges.
    /// The result may consist of one or two ranges depending on the overlap.
    /// </summary>
    /// <param name="range">The DateOnlyRange to form the union with.</param>
    /// <returns>A list of DateOnlyRange instances representing the union of the two ranges.</returns>
    public IList<DateOnlyRange> GetUnion(DateOnlyRange range)
    {
        var union = _dateTimeRange.GetUnion(range._dateTimeRange);

        var result = new List<DateOnlyRange>();

        foreach (var item in union)
        {
            var resultRange = new DateOnlyRange(item);
            result.Add(resultRange);
        }
        
        return result;
    }

    /// <summary>
    /// Excludes the overlapping portion of the specified DateOnlyRange from this range.
    /// Results in one or two new ranges that represent the non-overlapping parts.
    /// </summary>
    /// <param name="range">The DateOnlyRange to compare and exclude the overlap with.</param>
    /// <returns>A collection of DateOnlyRange instances representing the non-overlapping parts of this range.</returns>
    public IList<DateOnlyRange> ExcludeOverlap(DateOnlyRange range)
    {
        var excludeOverlap = _dateTimeRange.ExcludeOverlap(range._dateTimeRange);

        var result = new List<DateOnlyRange>();

        foreach (var item in excludeOverlap)
        {
            var resultRange = new DateOnlyRange(item);
            result.Add(resultRange);
        }

        return result;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{typeof(DateOnlyRange)}() {{ Start = {Start}, Finish = {Finish}, IncludesStart = {IncludesStart}, IncludesFinish = {IncludesFinish} }}";
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is DateOnlyRange other)
        {
            if (other.IsEmpty)
            {
                return IsEmpty;
            }
            else
            {
                return Start == other.Start && Finish == other.Finish && IncludesStart == other.IncludesStart && IncludesFinish == other.IncludesFinish;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (IsEmpty)
        {
            return HashCode.Combine(Empty.Start, Empty.Finish, Empty.IncludesStart, Empty.IncludesFinish);
        }
        else
        {
            return HashCode.Combine(Start, Finish, IncludesStart, IncludesFinish);
        }
    }

    public static bool operator ==(DateOnlyRange left, DateOnlyRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DateOnlyRange left, DateOnlyRange right)
    {
        return !(left == right);
    }
}
