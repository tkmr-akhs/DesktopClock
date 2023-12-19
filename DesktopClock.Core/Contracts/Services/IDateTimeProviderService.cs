using System.ComponentModel;

namespace DesktopClock.Core.Contracts.Services;

public interface IDateTimeProviderService : INotifyPropertyChanged
{
    /// <summary>
    /// The year.
    /// </summary>
    public int Year
    {
        get;
    }

    /// <summary>
    /// The month.
    /// </summary>
    public int Month
    {
        get;
    }

    /// <summary>
    /// The day.
    /// </summary>
    public int Day
    {
        get;
    }

    /// <summary>
    /// The hour.
    /// </summary>
    public int Hour
    {
        get;
    }

    /// <summary>
    /// The minute.
    /// </summary>
    public int Minute
    {
        get;
    }

    /// <summary>
    /// The second.
    /// </summary>
    public int Second
    {
        get;
    }

    /// <summary>
    /// Represents today as a <see cref="DateTime" />. Used when a consistent date is required in multithreading contexts.
    /// </summary>
    public DateTime Today
    {
        get;
    }

    /// <summary>
    /// Represents the current moment as a <see cref="DateTime" />. Used when consistent hour, minute, and second values are required in multithreading contexts.
    /// Values below seconds are truncated.
    /// </summary>
    public DateTime Now
    {
        get;
    }
}