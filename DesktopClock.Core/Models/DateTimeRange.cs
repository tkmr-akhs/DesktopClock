namespace DesktopClock.Core.Models;

/// <summary>
/// Represents a range of dates and times. This range can be defined as inclusive or exclusive of its start and finish dates/times.
/// The range can be empty, fully closed, open, or half-open based on its start and finish inclusivity.
/// </summary>
public struct DateTimeRange
{
    public static readonly DateTimeRange Empty = new();

    private DateTime _start;
    /// <summary>
    /// Gets or sets the start date and time of the range.
    /// Setting a start date later than or equal to the finish date (depending on inclusivity) will throw an exception.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the set value is later than the finish date/time or equal when the range is half-open.</exception>
    public DateTime Start
    {
        get => _start;
        private set
        {
            if (value > _finish || value == _finish && IsHalfOpen) throw new ArgumentException(nameof(Start), "The start date/time must be earlier than or equal to the finish date/time.");
            _start = value;
        }
    }

    private bool _includesStart;
    /// <summary>
    /// Gets or sets whether the start date and time are included in the range.
    /// Throws an exception if setting a state that is inconsistent with the finish date/time inclusivity when start and finish are the same.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when attempting to set an inconsistent state with the finish date/time.</exception>
    public bool IncludesStart
    {
        get => _includesStart;
        private set
        {
            if (Start == Finish && value != IncludesFinish) throw new ArgumentException(nameof(IncludesStart), "Cannot set IncludesStart to a different value than IncludesFinish when Start and Finish are the same.");
            _includesStart = value;
        }
    }

    private DateTime _finish;
    /// <summary>
    /// Gets or sets the finish date and time of the range.
    /// Setting a finish date earlier than or equal to the start date (depending on inclusivity) will throw an exception.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the set value is earlier than the start date/time or equal when the range is half-open.</exception>
    public DateTime Finish
    {
        get => _finish;
        private set
        {
            if (value < _start || value == _start && IsHalfOpen) throw new ArgumentException(nameof(Finish), "The finish date/time must be later than or equal to the start date/time.");
            _finish = value;
        }
    }

    private bool _includesFinish;
    /// <summary>
    /// Gets or sets whether the finish date and time are included in the range.
    /// Throws an exception if setting a state that is inconsistent with the start date/time inclusivity when start and finish are the same.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when attempting to set an inconsistent state with the start date/time.</exception>
    public bool IncludesFinish
    {
        get => _includesFinish;
        private set
        {
            if (Start == Finish && value != IncludesStart) throw new ArgumentException(nameof(IncludesFinish), "Cannot set IncludesFinish to a different value than IncludesFinish when Start and Finish are the same.");
            _includesFinish = value;
        }
    }

    /// <summary>
    /// Gets the duration of the date and time range.
    /// Duration is calculated as the difference between the finish and start date/time.
    /// </summary>
    public TimeSpan Offset => Finish - Start;

    /// <summary>
    /// Determines whether the date and time range is empty. 
    /// A range is considered empty if the start and finish are the same and neither is included in the range.
    /// </summary>
    public bool IsEmpty => Start == Finish && IsOpen;

    /// <summary>
    /// Initializes a new instance of the DateTimeRange struct with the minimum value for both start and finish dates.
    /// This constructor creates an empty range.
    /// </summary>
    public DateTimeRange() : this(DateTime.MinValue, DateTime.MinValue, includesStart: false) { }

    /// <summary>
    /// Initializes a new instance of the DateTimeRange struct with specified start and finish dates.
    /// IncludesStart and IncludesFinish determine if the start and finish dates are inclusive in the range.
    /// </summary>
    /// <param name="start">The start date and time of the range.</param>
    /// <param name="finish">The finish date and time of the range.</param>
    /// <param name="includesStart">Whether the start date/time is included in the range.</param>
    /// <param name="includesFinish">Whether the finish date/time is included in the range.</param>
    /// <exception cref="ArgumentException">Thrown when the finish date is earlier than the start date or when start and finish are the same but includesStart does not equal includesFinish.</exception>
    public DateTimeRange(DateTime start, DateTime finish, bool includesStart = true, bool includesFinish = false)
    {
        if (finish < start) throw new ArgumentException("End date/time must be after the begin date/time.");
        if (finish == start && includesStart != includesFinish) throw new ArgumentException("When start and finish are the same, includesStart must equal includesFinish.");

        _start = start;
        _finish = finish;
        _includesStart = includesStart;
        _includesFinish = includesFinish;
    }

    private bool IsFullyClosed => IncludesStart && IncludesFinish;
    private bool IsOpen => !IncludesStart && !IncludesFinish;
    private bool IsHalfOpen => !IsFullyClosed && !IsOpen;

    /// <summary>
    /// Determines if the specified DateTimeRange is completely included within this range.
    /// Considers the inclusivity of start and finish dates/times for both ranges.
    /// </summary>
    /// <param name="range">The DateTimeRange to check for inclusion.</param>
    /// <returns>True if the specified range is entirely included within this range, false otherwise.</returns>
    public bool Includes(DateTimeRange range)
    {
        if (range.Start < Start || Finish < range.Finish) return false; // これから、引数の範囲がはみ出しているか、触れていない。
        if (Start < range.Start && range.Finish < Finish) return true; // これの中に、引数の範囲が含まれる。

        // ここまでで、境界がかかわらない判断は終わっている。
        return IncludesBorder(range);
    }

    private bool IncludesBorder(DateTimeRange range)
    {
        if (range.Start == Finish)
        {
            if (range.Start == range.Finish)
            {
                return range.IncludesStart && IncludesFinish;
            }
        }
        else if (range.Finish == Start)
        {
            if (range.Start == range.Finish)
            {
                return range.IncludesFinish && IncludesStart;
            }
        }
        else if (range.Start == Start)
        {
            if (!IncludesStart && range.IncludesStart) return false;

            if (range.Finish < Finish) return true;

            if (range.Finish == Finish)
            {
                return IncludesFinish || !range.IncludesFinish;
            }
        }
        else if (range.Finish == Finish)
        {
            if (!IncludesFinish && range.IncludesFinish) return false;

            return IncludesFinish || !range.IncludesFinish;
        }

        return false;
    }

    /// <summary>
    /// Determines if the specified DateTime is included within this range.
    /// Considers the inclusivity of the start and finish dates/times.
    /// </summary>
    /// <param name="dateTime">The DateTime to check for inclusion in the range.</param>
    /// <returns>True if the dateTime is included in the range, false otherwise.</returns>
    public bool Includes(DateTime dateTime)
    {
        if (Start < dateTime && dateTime < Finish) return true;

        if (dateTime == Start) return IncludesStart;
        if (dateTime == Finish) return IncludesFinish;

        return false;
    }

    /// <summary>
    /// Determines if the specified date is included within this range.
    /// Allows specifying custom inclusivity for start and finish dates.
    /// </summary>
    /// <param name="date">The date to check for inclusion.</param>
    /// <param name="includesStartDate">Whether to include the start date in the range.</param>
    /// <param name="includesFinishDate">Whether to include the finish date in the range.</param>
    /// <returns>True if the date is included, false otherwise.</returns>
    public bool Includes(DateOnly date, bool includesStartDate = true, bool includesFinishDate = true)
    {
        var startTimeOnly = TimeOnly.FromDateTime(Start);
        var finishTimeOnly = TimeOnly.FromDateTime(Finish);

        var startDate = includesStartDate || (startTimeOnly == TimeOnly.MinValue && IncludesStart) ? DateOnly.FromDateTime(Start) : DateOnly.FromDateTime(Start).AddDays(1);
        var finishDate = includesFinishDate || (finishTimeOnly == TimeOnly.MinValue && IncludesFinish) ? DateOnly.FromDateTime(Finish) : DateOnly.FromDateTime(Finish).AddDays(-1);

        return startDate <= date && date <= finishDate;
    }

    /// <summary>
    /// Enumerates all dates included in this range.
    /// Uses an iterator to efficiently yield each date, especially useful for large date ranges.
    /// </summary>
    /// <param name="includesStartDate">If true, includes the start date in the enumeration.</param>
    /// <param name="includesFinishDate">If true, includes the finish date in the enumeration.</param>
    /// <returns>An IEnumerable<DateOnly> that can be iterated over to access each date within the range.</returns>
    public IEnumerable<DateOnly> GetAllDatesInRange(bool includesStartDate = true, bool includesFinishDate = true)
    {
        var startTimeOnly = TimeOnly.FromDateTime(Start);
        var finishTimeOnly = TimeOnly.FromDateTime(Finish);

        var startDate = includesStartDate || (startTimeOnly == TimeOnly.MinValue && IncludesStart) ? DateOnly.FromDateTime(Start) : DateOnly.FromDateTime(Start).AddDays(1);
        var finishDate = includesFinishDate || (finishTimeOnly == TimeOnly.MinValue && IncludesFinish) ? DateOnly.FromDateTime(Finish) : DateOnly.FromDateTime(Finish).AddDays(-1);

        for (var date = startDate; date <= finishDate; date = date.AddDays(1))
        {
            yield return date;
        }
    }

    /// <summary>
    /// Determines if this range overlaps with another specified range.
    /// Considers the inclusivity of start and finish dates/times for both ranges.
    /// </summary>
    /// <param name="range">The DateTimeRange to check for overlap.</param>
    /// <returns>True if the ranges overlap, false otherwise.</returns>
    public bool Overlaps(DateTimeRange range)
    {
        if (Start == Finish && !IncludesStart && !IncludesFinish) return false;
        if (range.Start == range.Finish && !range.IncludesStart && !range.IncludesFinish) return false;

        if (range.Finish < Start || Finish < range.Start) return false; // これの右か左に、引数の範囲が外れている
        if (range.Start < Start && Start < range.Finish) return true; // これの左に、引数の範囲の左がかかっている。
        if (range.Start < Finish && Finish < range.Finish) return true; // これの右に、引数の範囲の右がかかっている。
        if (Start < range.Start && range.Finish < Finish) return true; // これの中に、引数の範囲が含まれる。

        // ここまでで、境界がかかわらない判断は終わっている。
        return OverlapsBorder(range);
    }

    private bool OverlapsBorder(DateTimeRange range)
    {
        if (range.Start == Finish)
        {
            return IncludesFinish && range.IncludesStart;
        }
        else if (range.Finish == Start)
        {
            return IncludesStart && range.IncludesFinish;
        }
        else if (range.Start == Start)
        {
            if (range.Start == range.Finish && (!IncludesStart || !range.IncludesStart)) return false;

            return true;
        }
        else if (range.Finish == Finish)
        {
            if (range.Start == range.Finish && (!IncludesFinish || !range.IncludesFinish)) return false;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the intersection between this range and another specified range.
    /// Considers the inclusivity of start and finish dates/times for both ranges.
    /// Returns an empty range if they do not intersect.
    /// </summary>
    /// <param name="range">The DateTimeRange to intersect with.</param>
    /// <returns>A DateTimeRange representing the intersection of the two ranges, or an empty range if they do not intersect.</returns>
    public DateTimeRange GetIntersection(DateTimeRange range)
    {
        if (range.Start < Start)
        {
            // この中に、引数の範囲の始端がない場合
            if (range.Finish < Start)
            {
                // これから、左側に外れている場合
                return new DateTimeRange(Start, Start, false, false);
            }
            else if (range.Finish == Start)
            {
                // これの始端と、引数の範囲の終端が触れている場合
                var i = IncludesStart && range.IncludesFinish;
                return new DateTimeRange(Start, Start, i, i);
            }
            else if (Start < range.Finish && range.Finish < Finish)
            {
                // これの中に、引数の範囲の終端がある場合
                return new DateTimeRange(Start, range.Finish, IncludesStart, range._includesFinish);
            }
            else if (range.Finish == Finish)
            {
                // これの終端と、引数の範囲の終端が一致する場合
                return new DateTimeRange(Start, Finish, IncludesStart, IncludesFinish && range.IncludesFinish);
            }
            else
            {
                // これが、引数の範囲に含まれる場合
                return this;
            }
        }
        else if (range.Start == Start)
        {
            var margedIncludesStart = IncludesStart && range.IncludesStart;
            // これの始端と、引数の範囲の始端が一致する場合
            if (range.Finish == Start)
            {
                // これの始端と、引数の範囲の終端が一致する場合
                return new DateTimeRange(Start, Start, margedIncludesStart, margedIncludesStart);
            }
            else if (Start < range.Finish && range.Finish < Finish)
            {
                // これの中に、引数の範囲の終端がある場合
                return new DateTimeRange(Start, range.Finish, margedIncludesStart, range.IncludesFinish);
            }
            else if (range.Finish == Finish)
            {
                // これの終端と、引数の範囲の終端が一致する場合
                return new DateTimeRange(Start, Finish, margedIncludesStart, IncludesFinish && range.IncludesFinish);
            }
            else
            {
                // (始端を除き)これが、引数の範囲に含まれる場合
                if (IncludesStart == range.IncludesStart || range.IncludesStart)
                {
                    return this;
                }
                else
                {
                    if (Start == Finish)
                    {
                        return new DateTimeRange(Start, Finish, margedIncludesStart, margedIncludesStart);
                    }
                    else
                    {
                        return new DateTimeRange(Start, Finish, margedIncludesStart, IncludesFinish);
                    }
                }
            }
        }
        else if (range.Start < Finish)
        {
            // この中に、引数の範囲の始端がある場合
            if (range.Finish < Finish)
            {
                // この中に、引数の範囲の始端と終端がある場合
                return range;
            }
            else if (range.Finish == Finish)
            {
                // この中に、引数の範囲の始端と終端があり、終端が一致する場合
                return new DateTimeRange(range.Start, Finish, range.IncludesStart, IncludesFinish && range.IncludesFinish);
            }
            else
            {
                // 終端がはみ出る場合
                return new DateTimeRange(range.Start, Finish, range.IncludesStart, IncludesFinish);
            }
        }
        else if (range.Start == Finish)
        {
            // これの終端と、引数の範囲の始端が一致する場合
            if (range.Finish == Finish)
            {
                // これの終端と、引数の範囲の終端が一致する場合
                var i = IncludesFinish && range.IncludesStart;
                return new DateTimeRange(Finish, Finish, i, i);
            }
            else
            {
                // これの終端と、引数の範囲の始端が触れている場合
                var i = IncludesFinish && range.IncludesStart;
                return new DateTimeRange(Finish, Finish, i, i);
            }
        }
        else
        {
            // これから、右側に外れている場合
            return new DateTimeRange(Finish, Finish, false, false);
        }
    }

    /// <summary>
    /// Gets the union of this range with another specified range.
    /// Considers the inclusivity of start and finish dates/times for both ranges.
    /// The result may consist of one or two ranges depending on the overlap.
    /// </summary>
    /// <param name="range">The DateTimeRange to form the union with.</param>
    /// <returns>A list of DateTimeRange instances representing the union of the two ranges.</returns>
    public IList<DateTimeRange> GetUnion(DateTimeRange range)
    {
        var result = new List<DateTimeRange>();

        if (range.Start < Start)
        {
            // この中に、引数の範囲の始端がない場合
            if (range.Finish < Start)
            {
                // これから、左側に外れている場合
                result.Add(range);
                result.Add(this);
            }
            else if (range.Finish == Start)
            {
                // これの始端と、引数の範囲の終端が触れている場合
                if (!IncludesStart && !range.IncludesFinish)
                {
                    result.Add(range);
                    result.Add(this);
                }
                else
                {
                    if (Start == Finish)
                    {
                        result.Add(new(range.Start, Finish, range.IncludesStart, IncludesFinish || range.IncludesFinish));
                    }
                    else {
                        result.Add(new(range.Start, Finish, range.IncludesStart, IncludesFinish));
                    }
                }
            }
            else if (Start < range.Finish && range.Finish < Finish)
            {
                // これの中に、引数の範囲の終端がある場合
                result.Add(new(range.Start, Finish, range.IncludesStart, IncludesFinish));
            }
            else if (range.Finish == Finish)
            {
                // これの終端と、引数の範囲の終端が一致する場合
                result.Add(new(range.Start, Finish, range.IncludesStart, IncludesFinish || range.IncludesFinish));
            }
            else
            {
                // これが、引数の範囲に含まれる場合
                result.Add(range);
            }
        }
        else if (range.Start == Start)
        {
            // これの始端と、引数の範囲の始端が一致する場合
            var margedIncludesStart = IncludesStart || range.IncludesStart;
            if (range.Finish == Start)
            {
                // これの始端と、引数の範囲の終端が一致する場合
                if (Start == Finish)
                {
                    result.Add(new(Start, Finish, margedIncludesStart, margedIncludesStart));
                }
                else if (!IncludesStart && !range.IncludesFinish)
                {
                    result.Add(range);
                    result.Add(this);
                }
                else
                {
                    result.Add(new(Start, Finish, margedIncludesStart, IncludesFinish));
                }
            }
            else if (Start < range.Finish && range.Finish < Finish)
            {
                // これの中に、引数の範囲の終端がある場合
                result.Add(new(Start, Finish, margedIncludesStart, IncludesFinish));
            }
            else if (range.Finish == Finish)
            {
                // これの終端と、引数の範囲の終端が一致する場合
                result.Add(new(Start, Finish, margedIncludesStart, IncludesFinish || range.IncludesFinish));
            }
            else
            {
                // (始端を除き)これが、引数の範囲に含まれる場合
                result.Add(new(Start, range.Finish, margedIncludesStart, range.IncludesFinish));
            }
        }
        else if (range.Start < Finish)
        {
            // この中に、引数の範囲の始端がある場合
            if (range.Finish < Finish)
            {
                // この中に、引数の範囲の始端と終端がある場合
                result.Add(this);
            }
            else if (range.Finish == Finish)
            {
                // この中に、引数の範囲の始端と終端があり、終端が一致する場合
                result.Add(new(Start, Finish, IncludesStart, IncludesFinish || range.IncludesFinish));
            }
            else
            {
                // 終端がはみ出る場合
                result.Add(new(Start, range.Finish, IncludesStart, range.IncludesFinish));
            }
        }
        else if (range.Start == Finish)
        {
            // これの終端と、引数の範囲の始端が一致する場合
            if (range.Finish == Finish)
            {
                // これの終端と、引数の範囲の終端が一致する場合
                if (!IncludesFinish && !range.IncludesStart)
                {
                    result.Add(this);
                    result.Add(range);
                }
                else
                {
                    result.Add(new(Start, range.Finish, IncludesStart, IncludesFinish || range.IncludesFinish));
                }
            }
            else
            {
                // これの終端と、引数の範囲の始端が触れている場合
                if (!IncludesFinish && !range.IncludesStart)
                {
                    result.Add(this);
                    result.Add(range);
                }
                else
                {
                    result.Add(new(Start, range.Finish, IncludesStart, range.IncludesFinish));
                }
            }
        }
        else
        {
            // これから、右側に外れている場合
            result.Add(this);
            result.Add(range);
        }

        return ExcludeEmpty(result);
    }

    /// <summary>
    /// Excludes the overlapping portion of the specified DateTimeRange from this range.
    /// Results in one or two new ranges that represent the non-overlapping parts.
    /// </summary>
    /// <param name="range">The DateTimeRange to compare and exclude the overlap with.</param>
    /// <returns>A collection of DateTimeRange instances representing the non-overlapping parts of this range.</returns>
    public IList<DateTimeRange> ExcludeOverlap(DateTimeRange range)
    {
        var result = new List<DateTimeRange>();
        if (range.IsEmpty)
        {
            result.Add(this);
        }
        else if (IsEmpty)
        {
            if (Start == range.Start)
            {
                if (IncludesStart && !range.IncludesStart) result.Add(new(Start, Start, true, true));
                else if (IncludesStart == range.IncludesStart) result.Add(new(Start, Start, false, false));
            }
            else if (Finish == range.Finish)
            {
                if (IncludesFinish && !range.IncludesFinish) result.Add(new(Finish, Finish, true, true));
                else if (IncludesFinish == range.IncludesFinish) result.Add(new(Finish, Finish, false, false));
            }
            else if (Finish < range.Start && range.Finish < Start)
            {
                result.Add(this);
            }
        }
        else
        {
            if (range.Start < Start)
            {
                // この中に、引数の範囲の始端がない場合
                if (range.Finish < Start)
                {
                    // これから、左側に外れている場合
                    result.Add(this);
                }
                else if (range.Finish == Start)
                {
                    // これの始端と、引数の範囲の終端が触れている場合
                    if (Start == Finish) result.Add(new(Start, Finish, IncludesFinish && !range.IncludesFinish, IncludesFinish && !range.IncludesFinish));
                    else if (IncludesStart && range.IncludesFinish) result.Add(new(Start, Finish, false, IncludesFinish));
                    else result.Add(this);
                }
                else if (Start < range.Finish && range.Finish < Finish)
                {
                    // これの中に、引数の範囲の終端がある場合
                    result.Add(new(range.Finish, Finish, !range.IncludesFinish, IncludesFinish));
                }
                else if (range.Finish == Finish)
                {
                    // これの終端と、引数の範囲の終端が一致する場合
                    if (IncludesFinish && !range.IncludesFinish) result.Add(new(Finish, Finish, true, true));
                    else if (IncludesFinish == range.IncludesFinish) result.Add(new(Finish, Finish, false, false));
                }
                else
                {
                    // これが、引数の範囲に含まれる場合
                    return new DateTimeRange[0];
                }
            }
            else if (range.Start == Start)
            {
                // これの始端と、引数の範囲の始端が一致する場合
                if (IncludesStart && !range.IncludesStart) result.Add(new(Start, Start, true, true)); // 常に行う始端の処理

                if (range.Finish == Start)
                {
                    // これの始端と、引数の範囲の終端が一致する場合 (引数の範囲の長さがなく、始端に位置する場合)
                    if (Start != Finish) result.Add(new(range.Finish, Finish, false, IncludesFinish));
                }
                else if (Start < range.Finish && range.Finish < Finish)
                {
                    // これの中に、引数の範囲の終端がある場合
                    if (Start != Finish) result.Add(new(range.Finish, Finish, !range.IncludesFinish, IncludesFinish));
                }
                else if (range.Finish == Finish)
                {
                    // これの終端と、引数の範囲の終端が一致する場合
                    if (Start != Finish)
                    {
                        if (IncludesFinish && !range.IncludesFinish) result.Add(new(Finish, Finish, true, true));
                        else if (IncludesFinish == range.IncludesFinish) result.Add(new(Finish, Finish, false, false));
                    }
                }
                else
                {
                    // (始端を除き)これが、引数の範囲に含まれる場合
                    // なにもしない。
                }
            }
            else if (range.Start < Finish)
            {
                // この中に、引数の範囲の始端がある場合
                result.Add(new(Start, range.Start, IncludesStart, !range.IncludesStart)); // 常に行う前半の処理

                if (range.Finish < Finish)
                {
                    // この中に、引数の範囲の始端と終端がある場合
                    result.Add(new(range.Finish, Finish, !range.IncludesFinish, IncludesFinish));
                }
                else if (range.Finish == Finish)
                {
                    // この中に、引数の範囲の始端と終端があり、終端が一致する場合
                    if (Start != Finish)
                    {
                        if (IncludesFinish && !range.IncludesFinish) result.Add(new(Finish, Finish, true, true));
                        else if (IncludesFinish == range.IncludesFinish) result.Add(new(Finish, Finish, false, false));
                    }
                }
                else
                {
                    // 終端がはみ出る場合
                    // なにもしない。
                }
            }
            else if (range.Start == Finish)
            {
                // これの終端と、引数の範囲の始端が一致する場合
                if (range.Finish == Finish)
                {
                    // これの終端と、引数の範囲の終端が一致する場合
                    if (!range.IncludesStart || IncludesFinish == !range.IncludesStart) result.Add(this);
                    else result.Add(new(Start, range.Start, IncludesStart, false));

                    if (IncludesFinish && !range.IncludesFinish) result.Add(new(Finish, Finish, true, true));
                }
                else
                {
                    // これの終端と、引数の範囲の終端が一致する場合
                    if (!range.IncludesStart || IncludesFinish == !range.IncludesStart) result.Add(this);
                    else result.Add(new(Start, range.Start, IncludesStart, false));
                }
            }
            else
            {
                // これから、右側に外れている場合
                result.Add(this);
            }
        }

        return ExcludeEmpty(result);
    }

    private IList<DateTimeRange> ExcludeEmpty(IList<DateTimeRange> list)
    {
        var result = new List<DateTimeRange>();
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].IsEmpty) result.Add(list[i]);
        }

        return result;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{typeof(DateTimeRange)}() {{ Start = {Start}, Finish = {Finish}, IncludesStart = {IncludesStart}, IncludesFinish = {IncludesFinish} }}";
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is DateTimeRange other)
        {
            if (other.IsEmpty)
            {
                return IsEmpty;
            }
            else {
                return Start == other.Start && Finish == other.Finish && IncludesStart == other.IncludesStart && IncludesFinish == other.IncludesFinish;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (IsEmpty) {
            return HashCode.Combine(Empty.Start, Empty.Finish, Empty.IncludesStart, Empty.IncludesFinish);
        }
        else {
            return HashCode.Combine(Start, Finish, IncludesStart, IncludesFinish);
        }
    }

    public static bool operator ==(DateTimeRange left, DateTimeRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DateTimeRange left, DateTimeRange right)
    {
        return !(left == right);
    }
}
