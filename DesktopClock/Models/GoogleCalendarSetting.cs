using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopClock.Models;

/// <summary>
/// Represents the settings for a calendar, including its display type
/// and associated color for events. This class encapsulates the configuration
/// for how a Google Calendar should be displayed within the application.
/// </summary>
public partial class GoogleCalendarSetting : ObservableObject
{
    /// <summary>
    /// The identifire of the calendar.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The name of the calendar.
    /// </summary>
    [ObservableProperty]
    private string _name;

    /// <summary>
    /// Gets the display type for the calendar.
    /// This property defines how the calendar is displayed, whether as events, holidays, or hidden.
    /// </summary>
    [ObservableProperty]
    private GoogleCalendarDisplayType _displayType;

    //public ICommand ChangeDisplayTypeCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleCalendarSetting"/> class with the specified settings.
    /// </summary>
    /// <param name="name">The name Google Calendar.</param>
    /// <param name="displayType">The preferred display type for the calendar.</param>
    public GoogleCalendarSetting(string id, string name, GoogleCalendarDisplayType displayType = GoogleCalendarDisplayType.Events)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id), "Google calendar ID cannot be null.");
        _name = name ?? throw new ArgumentNullException(nameof(name), "Google calendar name  cannot be null.");
        _displayType = displayType;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        GoogleCalendarSetting other = (GoogleCalendarSetting)obj;
        return Name == other.Name && DisplayType == other.DisplayType;
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            hash = hash * 23 + Name.GetHashCode();
            hash = hash * 23 + DisplayType.GetHashCode();
            return hash;
        }
    }
}