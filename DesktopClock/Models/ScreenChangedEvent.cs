using System.Drawing;
using System.Text;

namespace DesktopClock.Models;

/// <summary>
/// Represents the arguments for a screen change event.
/// </summary>
public class ScreenChangedEventArgs : EventArgs
{

    /// <summary>
    /// Gets the screen identifier.
    /// </summary>
    public int ScreenId
    {
        get;
    }

    /// <summary>
    /// Gets the bounds of the screen before the change.
    /// </summary>
    public Rectangle OldBounds
    {
        get;
    }

    /// <summary>
    /// Gets the bounds of the screen after the change.
    /// </summary>
    public Rectangle NewBounds
    {
        get;
    }

    /// <summary>
    /// Gets the type of size change occurred on the screen.
    /// </summary>
    public ScreenChangedSize ChangedSize
    {
        get;
    }

    /// <summary>
    /// Gets the type of screen change.
    /// </summary>
    public ScreenChangeType ChangeType
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenChangedEventArgs"/> class with default values.
    /// </summary>
    public ScreenChangedEventArgs() : this(0, Rectangle.Empty, Rectangle.Empty, ScreenChangedSize.None, ScreenChangeType.NotChanged) { }

    /// <summary>
    /// Initializes a new instance of the ScreenChangedEventArgs class using the specified screen ID, old and new bounds, size change type, and change type.
    /// </summary>
    /// <param name="screenId">The screen identifier.</param>
    /// <param name="oldBounds">The old screen bounds.</param>
    /// <param name="newBounds">The new screen bounds.</param>
    /// <param name="changed">The type of size change.</param>
    /// <param name="changeType">The type of screen change.</param>
    public ScreenChangedEventArgs(int screenId, Rectangle oldBounds, Rectangle newBounds, ScreenChangedSize changed, ScreenChangeType changeType)
    {
        ScreenId = screenId;
        OldBounds = oldBounds;
        NewBounds = newBounds;
        ChangedSize = changed;
        ChangeType = changeType;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{GetType().Name}(")
                     .AppendLine($"    screenId: {ScreenId}")
                     .AppendLine($"    oldBounds: X = {OldBounds.X}, Y = {OldBounds.Y}, Width = {OldBounds.Width}, Height = {OldBounds.Height}")
                     .AppendLine($"    newBounds: X = {NewBounds.X}, Y = {NewBounds.Y}, Width = {NewBounds.Width}, Height = {NewBounds.Height}")
                     .AppendLine($"    changedSize: {StringifyChangedSizeFlags(ChangedSize)}")
                     .AppendLine($"    changeType: {ChangeType}")
                     .AppendLine("    );");

        return stringBuilder.ToString();
    }

    private string StringifyChangedSizeFlags(ScreenChangedSize changedSize)
    {
        if (changedSize == ScreenChangedSize.None || changedSize == ScreenChangedSize.All)
            return changedSize.ToString();

        var flags = Enum.GetValues(typeof(ScreenChangedSize)).Cast<ScreenChangedSize>();
        var selectedFlags = flags.Where(flag => flag != ScreenChangedSize.None && changedSize.HasFlag(flag));

        return string.Join(" | ", selectedFlags);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ScreenChangedEventArgs other &&
               ScreenId == other.ScreenId &&
               OldBounds.Equals(other.OldBounds) &&
               NewBounds.Equals(other.NewBounds) &&
               ChangedSize == other.ChangedSize &&
               ChangeType == other.ChangeType;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ScreenId);
        hash.Add(OldBounds);
        hash.Add(NewBounds);
        hash.Add(ChangedSize);
        hash.Add(ChangeType);
        return hash.ToHashCode();
    }

    public static bool operator ==(ScreenChangedEventArgs left, ScreenChangedEventArgs right)
    {
        if (ReferenceEquals(left, right)) return true;

        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(ScreenChangedEventArgs left, ScreenChangedEventArgs right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Specifies constants that define the type of size change for a screen.
/// </summary>
[Flags]
public enum ScreenChangedSize
{
    /// <summary>
    /// No size change.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// The X position of the screen has changed.
    /// </summary>
    X = 0x1,

    /// <summary>
    /// The Y position of the screen has changed.
    /// </summary>
    Y = 0x2,

    /// <summary>
    /// The width of the screen has changed.
    /// </summary>
    Width = 0x4,

    /// <summary>
    /// The height of the screen has changed.
    /// </summary>
    Height = 0x8,

    /// <summary>
    /// All properties (X, Y, Width, Height) of the screen have changed.
    /// </summary>
    All = X | Y | Width | Height
}

/// <summary>
/// Specifies constants that define the type of change for a screen.
/// </summary>
public enum ScreenChangeType
{
    /// <summary>
    /// The screen has not changed.
    /// </summary>
    NotChanged,

    /// <summary>
    /// The size of the screen has changed.
    /// </summary>
    ScreenSizeChanged,

    /// <summary>
    /// A new screen has been added.
    /// </summary>
    ScreenAdded,

    /// <summary>
    /// A screen has been removed.
    /// </summary>
    ScreenRemoved,
}

/// <summary>
/// Represents the method that will handle a screen changed event.
/// </summary>
/// <param name="sender">The source of the event; typically the object that raised the event.</param>
/// <param name="e">A <see cref="ScreenChangedEventArgs"/> object that contains the event data.</param>
public delegate void ScreenChangedEventHandler(object? sender, ScreenChangedEventArgs e);
