using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DesktopClock.Core.Models;

/// <summary>
/// Represents a weekly calendar, providing access to calendar entries for each day of the week.
/// </summary>
public class WeeklyCalendar : IReadOnlyList<CalendarEntry>, IReadOnlyDictionary<DateOnly, CalendarEntry>, INotifyPropertyChanged
{
    private static readonly string[] DayOfWeekName = new string[]
    {
        nameof(Sunday), nameof(Monday),  nameof(Tuesday), nameof(Wednesday), nameof(Thursday), nameof(Friday), nameof(Saturday)
    };

    private static readonly Dictionary<DayOfWeek, int> DayOfWeekIndex = new()
    {
        { DayOfWeek.Sunday, 0 },
        { DayOfWeek.Monday, 1 },
        { DayOfWeek.Tuesday, 2 },
        { DayOfWeek.Wednesday, 3 },
        { DayOfWeek.Thursday, 4 },
        { DayOfWeek.Friday, 5 },
        { DayOfWeek.Saturday, 6 },
    };

    /// <summary>
    /// Gets the CalendarEntry for Sunday.
    /// </summary>
    public CalendarEntry Sunday => _weekCalendar[0];

    /// <summary>
    /// Gets the CalendarEntry for Monday.
    /// </summary>
    public CalendarEntry Monday => _weekCalendar[1];

    /// <summary>
    /// Gets the CalendarEntry for Tuesday.
    /// </summary>
    public CalendarEntry Tuesday => _weekCalendar[2];

    /// <summary>
    /// Gets the CalendarEntry for Wednesday.
    /// </summary>
    public CalendarEntry Wednesday => _weekCalendar[3];

    /// <summary>
    /// Gets the CalendarEntry for Thursday.
    /// </summary>
    public CalendarEntry Thursday => _weekCalendar[4];

    /// <summary>
    /// Gets the CalendarEntry for Friday.
    /// </summary>
    public CalendarEntry Friday => _weekCalendar[5];

    /// <summary>
    /// Gets the CalendarEntry for Saturday.
    /// </summary>
    public CalendarEntry Saturday => _weekCalendar[6];
    
    /// <inheritdoc/>
    public int Count => ((ICollection<CalendarEntry>)_weekCalendar).Count;

    /// <inheritdoc/>
    public IEnumerable<DateOnly> Keys
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _weekCalendar[i].Date;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<CalendarEntry> Values
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _weekCalendar[i];
            }
        }
    }

    public CalendarEntry this[DateOnly key]
    {
        get
        {
            if (!ContainsKey(key))
            {
                throw new KeyNotFoundException();
            }

            return _weekCalendar[DayOfWeekIndex[key.DayOfWeek]];
        }
    }

    public CalendarEntry this[int index]
    {
        get => ((IList<CalendarEntry>)_weekCalendar)[index];
    }

    private readonly CalendarEntry[] _weekCalendar;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(int index)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(DayOfWeekName[index]));
    }

    private void OnAllPropertyChanged()
    {
        for (int i = 0; i < _weekCalendar.Length; i++)
        {
            OnPropertyChanged(i);
        }
    }

    /// <summary>
    /// Initializes a new instance of the WeeklyCalendar class.
    /// </summary>
    public WeeklyCalendar()
    {
        _weekCalendar = new CalendarEntry[7];
        for (int i = 0; i < _weekCalendar.Length; i++)
        {
            _weekCalendar[i] = CalendarEntry.Empty;
        }
    }

    /// <summary>
    /// Initializes a new instance of the WeeklyCalendar class starting from a specific date.
    /// </summary>
    /// <param name="year">Year of the start date.</param>
    /// <param name="month">Month of the start date.</param>
    /// <param name="day">Day of the start date.</param>
    public WeeklyCalendar(int year, int month, int day) : this(new DateOnly(year, month, day)) { }

    /// <summary>
    /// Initializes a new instance of the WeeklyCalendar class starting from a specific DateOnly instance.
    /// </summary>
    /// <param name="date">The start date for the calendar week.</param>
    public WeeklyCalendar(DateOnly date)
    {
        _weekCalendar = new CalendarEntry[7];
        var firstDay = date.AddDays(-DayOfWeekIndex[date.DayOfWeek]);
        GenerateCalendar(firstDay);
    }

    internal void GenerateCalendar(DateOnly firstDay)
    {
        if (firstDay.DayOfWeek != DayOfWeek.Sunday) throw new ArgumentException();

        var date = firstDay;
        for (int i = 0; i < _weekCalendar.Length; i++)
        {
            _weekCalendar[i] = new CalendarEntry(date);
            date = date.AddDays(1);
        }
        OnAllPropertyChanged();

    }

    /// <summary>
    /// Clears all information from the calendar entries, resetting them to their default state.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _weekCalendar.Length; i++)
        {
            var calendarEntry = _weekCalendar[i];
            _weekCalendar[i] = calendarEntry with { IsNonWorkingDay = false, IsScheduledDay = false, Information = String.Empty };
            OnPropertyChanged(i);
        }
    }

    /// <summary>
    /// Adds information to a calendar entry for a specified day of the week.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week to add information to.</param>
    /// <param name="information">The information to add to the calendar entry.</param>
    public void AddEntryInformation(DayOfWeek dayOfWeek, string information)
    {
        var index = DayOfWeekIndex[dayOfWeek];
        AddEntryInformation(index, information);
    }

    /// <summary>
    /// Adds information to a calendar entry for a specific date.
    /// </summary>
    /// <param name="date">The date of the calendar entry to modify.</param>
    /// <param name="information">The information to add to the calendar entry.</param>
    /// <exception cref="ArgumentException">Thrown if the date is not within the current week of the calendar.</exception>
    public void AddEntryInformation(DateOnly date, string information)
    {
        if (date < Sunday.Date || Saturday.Date < date) throw new ArgumentException();

        var index = DayOfWeekIndex[date.DayOfWeek];
        AddEntryInformation(index, information);
    }

    /// <summary>
    /// Adds information to a calendar entry at a specific index.
    /// </summary>
    /// <param name="index">The index of the calendar entry to modify.</param>
    /// <param name="information">The information to add to the calendar entry.</param>
    public void AddEntryInformation(int index, string information)
    {
        var calendarEntry = _weekCalendar[index];
        _weekCalendar[index] = calendarEntry with { Information = information };
        OnPropertyChanged(index);
    }

    /// <summary>
    /// Marks a calendar entry with specified attributes for a given day of the week.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week to mark.</param>
    /// <param name="isOutsideMonth">Optional. Specifies if the day is outside the current month.</param>
    /// <param name="isNonWorkingDay">Optional. Specifies if the day is a non-working day.</param>
    /// <param name="isScheduledDay">Optional. Specifies if the day has scheduled events.</param>
    public void MarkEntry(DayOfWeek dayOfWeek, bool? isOutsideMonth = null, bool? isNonWorkingDay = null, bool? isScheduledDay = null)
    {
        var index = DayOfWeekIndex[dayOfWeek];
        MarkEntry(index, isOutsideMonth, isNonWorkingDay, isScheduledDay);
    }

    /// <summary>
    /// Marks a calendar entry with specified attributes for a specific date.
    /// </summary>
    /// <param name="date">The date to mark.</param>
    /// <param name="isOutsideMonth">Optional. Specifies if the day is outside the current month.</param>
    /// <param name="isNonWorkingDay">Optional. Specifies if the day is a non-working day.</param>
    /// <param name="isScheduledDay">Optional. Specifies if the day has scheduled events.</param>
    /// <exception cref="ArgumentException">Thrown if the date is not within the current week of the calendar.</exception>
    public void MarkEntry(DateOnly date, bool? isOutsideMonth = null, bool? isNonWorkingDay = null, bool? isScheduledDay = null)
    {
        if (date < Sunday.Date || Saturday.Date < date) throw new ArgumentException();

        var index = DayOfWeekIndex[date.DayOfWeek];
        MarkEntry(index, isOutsideMonth, isNonWorkingDay, isScheduledDay);
    }

    /// <summary>
    /// Marks a calendar entry with specified attributes at a specific index.
    /// </summary>
    /// <param name="index">The index of the calendar entry to mark.</param>
    /// <param name="isOutsideMonth">Optional. Specifies if the day is outside the current month.</param>
    /// <param name="isNonWorkingDay">Optional. Specifies if the day is a non-working day.</param>
    /// <param name="isScheduledDay">Optional. Specifies if the day has scheduled events.</param>
    public void MarkEntry(int index, bool? isOutsideMonth = null, bool? isNonWorkingDay = null, bool? isScheduledDay = null)
    {
        var calendarEntry = _weekCalendar[index];


        if (isOutsideMonth == null)
        {
            if (isNonWorkingDay == null)
            {
                if (isScheduledDay == null) return;
                _weekCalendar[index] = calendarEntry with { IsScheduledDay = isScheduledDay ?? default };
            }
            else
            {

                if (isScheduledDay == null)
                {
                    _weekCalendar[index] = calendarEntry with { IsNonWorkingDay = isNonWorkingDay ?? default };
                }
                else
                {
                    _weekCalendar[index] = calendarEntry with
                    {
                        IsNonWorkingDay = isNonWorkingDay ?? default,
                        IsScheduledDay = isScheduledDay ?? default
                    };
                }
            }
        }
        else
        {
            if (isNonWorkingDay == null)
            {
                if (isScheduledDay == null)
                {
                    _weekCalendar[index] = calendarEntry with
                    {
                        IsOutsideMonth = isOutsideMonth ?? default
                    };
                }
                else
                {
                    _weekCalendar[index] = calendarEntry with
                    {
                        IsOutsideMonth = isOutsideMonth ?? default,
                        IsScheduledDay = isScheduledDay ?? default
                    };
                }
            }
            else
            {

                if (isScheduledDay == null)
                {
                    _weekCalendar[index] = calendarEntry with
                    {
                        IsOutsideMonth = isOutsideMonth ?? default,
                        IsNonWorkingDay = isNonWorkingDay ?? default
                    };
                }
                else
                {
                    _weekCalendar[index] = calendarEntry with
                    {
                        IsOutsideMonth = isOutsideMonth ?? default,
                        IsNonWorkingDay = isNonWorkingDay ?? default,
                        IsScheduledDay = isScheduledDay ?? default
                    };
                }
            }
        }

        OnPropertyChanged(index);
    }

    /// <summary>
    /// Provides a read-only view of the WeeklyCalendar.
    /// </summary>
    /// <returns>A read-only collection of CalendarEntry objects.</returns>
    public IReadOnlyCollection<CalendarEntry> AsReadOnly()
    {
        return new ReadOnlyWeekCalendar(this);
    }

    /// <inheritdoc/>
    public IEnumerator<CalendarEntry> GetEnumerator() => ((IEnumerable<CalendarEntry>)_weekCalendar).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_weekCalendar).GetEnumerator();

    /// <inheritdoc/>
    public bool ContainsKey(DateOnly key)
    {
        return Sunday.Date <= key && key <= Saturday.Date;
    }

    /// <inheritdoc/>
    public bool TryGetValue(DateOnly key, [MaybeNullWhen(false)] out CalendarEntry value)
    {
        if (!ContainsKey(key))
        {
            value = null;
            return false;
        }

        value = this[key];
        return true;
    }

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<DateOnly, CalendarEntry>> IEnumerable<KeyValuePair<DateOnly, CalendarEntry>>.GetEnumerator()
    {
        foreach (var value in Values)
        {
            yield return new KeyValuePair<DateOnly, CalendarEntry>(value.Date, value);
        }
    }
}