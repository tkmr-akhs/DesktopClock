namespace DesktopClock.Core.Models;

/// <summary>
/// Represents a read-only list where individual items can be replaced.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public interface IReadOnlyReplaceableList<T> : IReadOnlyList<T>
{
    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    new T this[int index] { get; set; }

    /// <summary>
    /// Returns a read-only IList&lt;T&gt; wrapper for the current collection.
    /// </summary>
    /// <returns>An object that acts as a read-only wrapper around the current IReadOnlyReplaceableList&lt;T&gt;.</returns>
    IReadOnlyList<T> AsReadOnly();
}
