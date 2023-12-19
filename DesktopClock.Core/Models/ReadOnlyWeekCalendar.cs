using System.Collections;
using System.ComponentModel;

namespace DesktopClock.Core.Models;

/// <summary>
/// Represents a read-only view of a weekly calendar.
/// </summary>
public class ReadOnlyWeekCalendar : IReadOnlyCollection<CalendarEntry>, INotifyPropertyChanged
{
    private readonly WeeklyCalendar _week;

    /// <summary>
    /// Initializes a new instance of the ReadOnlyWeekCalendar class with a specified WeeklyCalendar.
    /// </summary>
    /// <param name="week">The WeeklyCalendar to create a read-only view for.</param>
    public ReadOnlyWeekCalendar(WeeklyCalendar week)
    {
        _week = week;
    }

    /// <inheritdoc/>
    public int Count => ((IReadOnlyCollection<CalendarEntry>)_week).Count;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged
    {
        add => ((INotifyPropertyChanged)_week).PropertyChanged += value;
        remove => ((INotifyPropertyChanged)_week).PropertyChanged -= value;
    }

    /// <inheritdoc/>
    public IEnumerator<CalendarEntry> GetEnumerator() => ((IEnumerable<CalendarEntry>)_week).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_week).GetEnumerator();
}