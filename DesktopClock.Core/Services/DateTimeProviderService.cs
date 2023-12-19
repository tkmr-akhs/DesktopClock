using System.ComponentModel;


namespace DesktopClock.Core.Services;

/// <summary>
/// Implementation of <see cref="IDateTimeEventSource"/>.
/// The <see cref="DateTimeProviderService.PropertyChanged"/> event occurs at the moment when all properties change simultaneously (e.g., at the start of a new year) in the following order:
/// "Year → Month → Day → Today → Hour → Minute → Second → Now".
/// The order of occurrence for "HolidayName → IsHoliday" and "Day" is not guaranteed (updated sometime after Month and before Today).
/// Additionally, events for properties without changes will not be triggered.
/// For example, at the moment of a day change, only events for Day and below will be triggered, while Year and Month will not.
/// </summary>
public class DateTimeProviderService : IDateTimeProviderService
{
    private const string ALREADY_SET_MESSAGE = "{0} is already set.";
    private const string TOO_SMALL_MESSAGE = "{0} is too small. (< {1})";
    private const string ALREADY_STARTED_MESSAGE = "Already started.";
    private const string NOT_RUNNING_MESSAGE = "This is not running.";
    private const int INITIAL_YEAR = -1;
    private const int INITIAL_MONTH = 99;
    private const int INITIAL_DAY = 99;
    private const int INITIAL_HOUR = 99;
    private const int INITIAL_MINUTE = 99;
    private const int INITIAL_SECOND = 99;

    /// <summary>
    /// Minimum interval for time checking.
    /// </summary>
    public const int MinimumInterval = 200;

    private bool _IsRunning;
    /// <summary>
    /// Indicates whether the service is currently running.
    /// </summary>
    public bool IsRunning
    {
        get { return _IsRunning; }
        private set
        {
            if (value != _IsRunning)
            {
                _IsRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
            }
        }
    }

    private int _Year;
    /// <inheritdoc/>
    public int Year
    {
        get { return _Year; }
        private set
        {
            if (value != _Year)
            {
                _Year = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Year)));
            }
        }
    }

    private int _Month;
    /// <inheritdoc/>
    public int Month
    {
        get { return _Month; }
        private set
        {
            if (value != _Month)
            {
                _Month = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Month)));
            }
        }
    }

    private int _Day;
    /// <inheritdoc/>
    public int Day
    {
        get { return _Day; }
        private set
        {
            if (value != _Day)
            {
                _Day = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Day)));
            }
        }
    }

    private int _Hour;
    /// <inheritdoc/>
    public int Hour
    {
        get { return _Hour; }
        private set
        {
            if (value != _Hour)
            {
                _Hour = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hour)));
            }
        }
    }

    private int _Minute;
    /// <inheritdoc/>
    public int Minute
    {
        get { return _Minute; }
        private set
        {
            if (value != _Minute)
            {
                _Minute = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Minute)));
            }
        }
    }

    private int _Second;
    /// <inheritdoc/>
    public int Second
    {
        get { return _Second; }
        private set
        {
            if (value != _Second)
            {
                _Second = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Second)));
            }
        }
    }

    private DateTime _Today;
    /// <inheritdoc/>
    public DateTime Today
    {
        get { return _Today; }
        private set
        {
            if (value != _Today) {
                _Today = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Today)));
            }
        }
    }

    private DateTime _Now;
    /// <inheritdoc/>
    public DateTime Now
    {
        get { return _Now; }
        private set
        {
            if (value != _Now)
            {
                _Now = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Now)));
            }
        }
    }

    private int _MillisecondsInterval;
    /// <summary>
    /// The interval for time checking, with the minimum being <see cref="MinimumInterval"/>.
    /// </summary>
    public int MillisecondsInterval
    {
        get { return _MillisecondsInterval; }
        set
        {
            if (MillisecondsInterval < MinimumInterval) throw new ArgumentException(String.Format(TOO_SMALL_MESSAGE, nameof(MillisecondsInterval), MinimumInterval));
            _MillisecondsInterval = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MillisecondsInterval)));
        }
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    private CancellationTokenSource tokenSource;

    /// <summary>
    /// Initializes the DateTimeProviderService with a specified interval for time checking.
    /// </summary>
    /// <param name="millisecondsInterval">Time interval for checking the current time, in milliseconds.</param>
    public DateTimeProviderService(int millisecondsInterval = MinimumInterval)
    {
        _MillisecondsInterval = millisecondsInterval < MinimumInterval ? MinimumInterval : millisecondsInterval;
        InitializeDateTime();
        Start();
    }

    private void InitializeDateTime()
    {
        _Year = INITIAL_YEAR;
        _Month = INITIAL_MONTH;
        _Day = INITIAL_DAY;
        _Hour = INITIAL_HOUR;
        _Minute = INITIAL_MINUTE;
        _Second = INITIAL_SECOND;
    }

    /// <summary>
    /// Starts the time checking process. When a change in time is detected, the <see cref="PropertyChanged"/> event is triggered.
    /// </summary>
    public void Start()
    {
        if (IsRunning) throw new InvalidOperationException(ALREADY_STARTED_MESSAGE);
        tokenSource = new CancellationTokenSource();
        IsRunning = true;
        CheckUpdate(tokenSource.Token);
    }

    private async void CheckUpdate(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            UpdateDateTime();
            await Task.Delay(MillisecondsInterval, token);
        }
    }

    private void UpdateDateTime()
    {
        var timeStamp = DateTime.Now;
        timeStamp = new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day, timeStamp.Hour, timeStamp.Minute, timeStamp.Second);
        var today = new DateTime(timeStamp.Year, timeStamp.Month, timeStamp.Day);
        Year = timeStamp.Year;
        Month = timeStamp.Month;
        Day = timeStamp.Day;
        Today = today;
        Hour = timeStamp.Hour;
        Minute = timeStamp.Minute;
        Second = timeStamp.Second;
        Now = timeStamp;
    }

    /// <summary>
    /// Stops the time checking process. All properties are reset to their initial state.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning) throw new InvalidOperationException(NOT_RUNNING_MESSAGE);
        tokenSource.Cancel();
        tokenSource = null;
        IsRunning = false;
        InitializeDateTime();
    }
}
