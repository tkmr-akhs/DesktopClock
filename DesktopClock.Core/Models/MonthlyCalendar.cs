using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace DesktopClock.Core.Models;

/// <summary>
/// Represents a monthly calendar, providing access to weekly calendar entries.
/// </summary>
public class MonthlyCalendar : IReadOnlyList<WeeklyCalendar>, IReadOnlyDictionary<DateOnly, CalendarEntry>, INotifyPropertyChanging, INotifyPropertyChanged, INotifyCollectionChanged
{
    private const int IndexPadding = 4;
    private static readonly List<string> _allProperties = new()
    {
        nameof(Year),
        nameof(Month),
        nameof(MinDate),
        nameof(StartDate),
        nameof(FirstDay),
        nameof(LastDay),
        nameof(FinishDate),
        nameof(MaxDate),
    };

    /// <summary>
    /// Gets the year of the monthly calendar.
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Gets the month of the monthly calendar.
    /// </summary>
    public int Month { get; private set; }

    /// <summary>
    /// Represents the minimum date covered by the monthly calendar. This encompasses dates internally prior to the StartDate.
    /// </summary>
    public DateOnly MinDate { get; private set; }

    /// <summary>
    /// Represents the maximum date covered by the monthly calendar. This encompasses the last date internally after the FinishDate.
    /// </summary>
    public DateOnly MaxDate { get; private set; }

    /// <summary>
    /// Represents the first day of the month in the monthly calendar.
    /// </summary>
    public DateOnly FirstDay { get; private set; }

    /// <summary>
    /// Represents the last day of the month in the monthly calendar.
    /// </summary>
    public DateOnly LastDay { get; private set; }

    /// <summary>
    /// Represents the start date of the monthly calendar display. This may include days from the previous month to fill the first week.
    /// </summary>
    public DateOnly StartDate { get; private set; }

    /// <summary>
    /// Represents the finish date of the monthly calendar display. This may include days from the next month to complete the last week.
    /// </summary>
    public DateOnly FinishDate { get; private set; }

    private readonly ObservableCollection<WeeklyCalendar> _weeks;

    /// <inheritdoc/>
    public int Count => _weeks.Count - IndexPadding * 2;

    /// <inheritdoc/>
    public IEnumerable<DateOnly> Keys
    {
        get
        {
            for (int weekIndex = 0; weekIndex < _weeks.Count; weekIndex++)
            {
                for (int dayIndex = 0; dayIndex < _weeks[weekIndex].Count; dayIndex++)
                {
                    yield return _weeks[weekIndex][dayIndex].Date;
                }
            }

        }
    }

    /// <inheritdoc/>
    public IEnumerable<CalendarEntry> Values
    {
        get
        {
            for (int weekIndex = 0; weekIndex < _weeks.Count; weekIndex++)
            {
                for (int dayIndex = 0; dayIndex < _weeks[weekIndex].Count; dayIndex++)
                {
                    yield return _weeks[weekIndex][dayIndex];
                }
            }

        }
    }

    public CalendarEntry this[DateOnly key]
    {
        get
        {
            if (!ContainsKey(key)) throw new KeyNotFoundException();

            var index = GetWeekIndex(key);
            return _weeks[index][key];
        }
    }

    public WeeklyCalendar this[int index] => _weeks[index + IndexPadding];

    /// <summary>
    /// Occurs when the calendar is generated.
    /// </summary>
    public event EventHandler CalendarGenerated;

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler PropertyChanging;

    /// <summary>
    /// Initializes a new instance of the MonthlyCalendar class with the current year and month.
    /// </summary>
    public MonthlyCalendar() : this(DateTime.Today.Year, DateTime.Today.Month) { }

    /// <summary>
    /// Initializes a new instance of the MonthlyCalendar class for a specific year and month.
    /// </summary>
    /// <param name="year">The year of the calendar.</param>
    /// <param name="month">The month of the calendar.</param>
    public MonthlyCalendar(int year, int month)
    {
        Year = year;
        Month = month;

        _weeks = new ObservableCollection<WeeklyCalendar>();

        _weeks.CollectionChanged += _weeks_CollectionChanged;

        for (int i = 0; i < 6 + IndexPadding * 2; i++)
        {
            var daysInWeek = new WeeklyCalendar();
            _weeks.Add(daysInWeek);
        }

        GenerateCalendar();
    }

    /// <summary>
    /// Raises the PropertyChanged event for a specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the PropertyChanged event for a list of properties.
    /// </summary>
    /// <param name="propertyNames">A list of property names that have changed.</param>
    protected void OnPropertyChanged(IReadOnlyList<string> propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Raises the PropertyChanging event for a specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    protected void OnPropertyChanging(string propertyName)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the PropertyChanging event for the Year and Month properties, and determines if they have changed. 
    /// Also populates a list with the names of all properties that are changing as a result of the year and month changes.
    /// </summary>
    /// <param name="year">The new year value to check against the current year.</param>
    /// <param name="month">The new month value to check against the current month.</param>
    /// <param name="changingProperties">An output list of property names that are changing.</param>
    /// <returns>True if either the Year or Month property is changing; otherwise, false.</returns>
    protected bool OnPropertyChanging(int year, int month, out IList<string> changingProperties)
    {
        changingProperties = new List<string>();

        var changed = false;

        if (Year != year)
        {
            changingProperties.Add(nameof(Year));
            changed = true;
        }

        if (Month != month)
        {
            changingProperties.Add(nameof(Month));
            changed = true;
        }

        if (changed)
        {
            changingProperties.Add(nameof(MinDate));
            changingProperties.Add(nameof(StartDate));
            changingProperties.Add(nameof(FirstDay));
            changingProperties.Add(nameof(LastDay));
            changingProperties.Add(nameof(FinishDate));
            changingProperties.Add(nameof(MaxDate));
        }

        return changed;
    }

    private void _weeks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move
            || e.Action == NotifyCollectionChangedAction.Replace
            || e.Action == NotifyCollectionChangedAction.Remove)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    CollectionChanged?.Invoke(this, new(e.Action, e.OldItems, e.NewStartingIndex - IndexPadding, e.OldStartingIndex - IndexPadding));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    CollectionChanged?.Invoke(this, new(e.Action, e.NewItems, e.OldItems, e.OldStartingIndex - IndexPadding));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    CollectionChanged?.Invoke(this, new(e.Action, e.OldItems, e.OldStartingIndex - IndexPadding));
                    break;
                default:
                    CollectionChanged?.Invoke(this, e);
                    break;
            }
        }
    }

    /// <summary>
    /// Jumps to the next month in the calendar.
    /// </summary>
    public void Next()
    {
        var year = Year;
        var month = Month;

        if (Month == 12)
        {
            year++;
            month = 1;
        }
        else
        {
            month++;
        }

        JumpTo(year, month);
    }

    /// <summary>
    /// Jumps to the previous month in the calendar.
    /// </summary>
    public void Previous()
    {
        var year = Year;
        var month = Month;

        if (Month == 1)
        {
            year--;
            month = 12;
        }
        else
        {
            month--;
        }

        JumpTo(year, month);
    }

    /// <summary>
    /// Jumps to a specific year and month in the calendar.
    /// </summary>
    /// <param name="year">The year to jump to.</param>
    /// <param name="month">The month to jump to.</param>
    /// <param name="force">Optional. Indicates whether to refresh the calendar even if the specified year and month are the same as the current. Default is false, meaning no refresh if the same. Set to true to force a refresh.</param>
    public void JumpTo(int year, int month, bool force = false)
    {
        var needToGenerate = false;
        IList<string> changingProperties;
        if (force)
        {
            changingProperties = _allProperties;
            needToGenerate = true;
        }
        else
        {
            needToGenerate = OnPropertyChanging(year, month, out changingProperties);
        }

        if (needToGenerate)
        {
            Year = year;
            Month = month;
            GenerateCalendar();
            OnPropertyChanged(changingProperties.AsReadOnly());
        }
    }

    private int GetWeekIndex(DateOnly date)
    {
        var dateSpan = date.ToDateTime(TimeOnly.MinValue) - MinDate.ToDateTime(TimeOnly.MinValue);

        return dateSpan.Days / 7;
    }

    /// <summary>
    /// Adds information to a calendar entry for a specific date.
    /// </summary>
    /// <param name="date">The date of the calendar entry to modify.</param>
    /// <param name="information">The information to add to the calendar entry.</param>
    /// <exception cref="ArgumentException">Thrown if the date is not within the current week of the calendar.</exception>
    public void AddEntryInformation(DateOnly date, string information)
    {
        var weekInMonthIndex = GetWeekIndex(date);
        _weeks[weekInMonthIndex].AddEntryInformation(date, information);
    }

    /// <summary>
    /// Adds information to a calendar entry at a specific index.
    /// </summary>
    /// <param name="weekInMonthIndex">The zero-based index of the week within the month.</param>
    /// <param name="dayInWeekIndex">The zero-based index of the day within the specified week.</param>
    /// <param name="information">The information to add to the calendar entry.</param>
    public void AddEntryInformation(int weekInMonthIndex, int dayInWeekIndex, string information)
    {
        _weeks[weekInMonthIndex].AddEntryInformation(dayInWeekIndex, information);
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
        var weekInMonthIndex = GetWeekIndex(date);

        _weeks[weekInMonthIndex].MarkEntry(date, isOutsideMonth, isNonWorkingDay, isScheduledDay);
    }

    /// <summary>
    /// Marks a calendar entry with specified attributes at a specific index.
    /// </summary>
    /// <param name="weekInMonthIndex">The zero-based index of the week within the month.</param>
    /// <param name="dayInWeekIndex">The zero-based index of the day within the specified week.</param>
    /// <param name="isOutsideMonth">Optional. Specifies if the day is outside the current month.</param>
    /// <param name="isNonWorkingDay">Optional. Specifies if the day is a non-working day.</param>
    /// <param name="isScheduledDay">Optional. Specifies if the day has scheduled events.</param>
    public void MarkEntry(int weekInMonthIndex, int dayInWeekIndex, bool? isOutsideMonth = null, bool? isNonWorkingDay = null, bool? isScheduledDay = null)
    {
        _weeks[weekInMonthIndex].MarkEntry(dayInWeekIndex, isOutsideMonth, isNonWorkingDay, isScheduledDay);
    }

    /// <summary>
    /// Retrieves the calendar entry for a specific date. If the date is not found within the calendar, a new entry is created with the provided date.
    /// </summary>
    /// <param name="date">The date for which to retrieve the calendar entry.</param>
    /// <returns>The CalendarEntry associated with the specified date. If no entry exists, a new entry with the specified date is returned.</returns>
    public CalendarEntry GetEntry(DateOnly date)
    {
        if (ContainsKey(date)) return this[date];
        else return new CalendarEntry(date);
    }

    private void GenerateCalendar()
    {
        FirstDay = new DateOnly(Year, Month, 1);
        
        if (Month == 12)
        {
            LastDay = new DateOnly(Year + 1, 1, 1).AddDays(-1);
        }
        else
        {
            LastDay = new DateOnly(Year, Month + 1, 1).AddDays(-1);
        }

        DateOnly startDate;

        if (FirstDay.DayOfWeek == DayOfWeek.Sunday)
        {
            startDate = FirstDay.AddDays(-7);
        }
        else
        {
            startDate = FirstDay.AddDays(-(int)FirstDay.DayOfWeek);
        }

        StartDate = startDate;
        FinishDate = startDate.AddDays(7 * Count);

        MinDate = StartDate.AddDays(7 * -IndexPadding);
        MaxDate = FinishDate.AddDays(7 * IndexPadding);

        var day = MinDate;

        for (int weekIndex = 0; weekIndex < _weeks.Count; weekIndex++)
        {
            var week = _weeks[weekIndex];
            week.GenerateCalendar(day);

            if (week.Sunday.Date < FirstDay || LastDay < week.Saturday.Date)
            {
                for (int dayIndex = 0; dayIndex < week.Count; dayIndex++)
                {
                    if (day < FirstDay || LastDay < day)
                    {
                        week.MarkEntry(dayIndex, isOutsideMonth: true);
                    }
                    day = day.AddDays(1);
                }
            }
            else
            {
                day = day.AddDays(7);
            }
        }

        CalendarGenerated?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        foreach(var week in _weeks)
        {
            for(int i = 0; i < week.Count; i++)
            {
                week.Clear();
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerator<WeeklyCalendar> GetEnumerator()
    {
        for (int i = IndexPadding; i < _weeks.Count - IndexPadding; i++)
        {
            yield return _weeks[i];
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public bool ContainsKey(DateOnly key)
    {
        return MinDate <= key && key <= MaxDate;
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
