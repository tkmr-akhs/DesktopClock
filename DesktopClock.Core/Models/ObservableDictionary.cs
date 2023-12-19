using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DesktopClock.Core.Models;

/// <summary>
/// Extends <see cref="ObservableCollection{T}"/> to provide dictionary-like access.
/// This class is not thread-safe.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
[Serializable]
public class ObservableDictionary<TKey, TValue> : ObservableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, ISerializable
{
    [JsonInclude]
    private readonly Dictionary<TKey, int> _keyToIndex;

    [JsonInclude]
    private readonly ObservableCollection<TValue> _values;

    [NonSerialized]
    private readonly ReadOnlyObservableCollection<TValue> _readOnlyValues;

    //[NonSerialized]
    //private readonly object _syncRoot = new(); // TODO: Implement a more efficient method.

    private enum SyncCollectionState
    {
        None,
        Idle,
        SyncBaseToValues,
        SyncValuesToOthers
    }

    [NonSerialized]
    private static readonly IReadOnlySet<SyncCollectionState> EmptyInvalidStates = new HashSet<SyncCollectionState>();

    [NonSerialized]
    private static readonly IReadOnlySet<SyncCollectionState> InvalidStatesIsSyncBaseToValues = new HashSet<SyncCollectionState>() { SyncCollectionState.SyncBaseToValues };

    [NonSerialized]
    private static readonly IReadOnlySet<SyncCollectionState> InvalidStatesIsSyncValuesToOthers = new HashSet<SyncCollectionState>() { SyncCollectionState.SyncValuesToOthers };

    private class InvalidCollectionStateException : Exception
    {
        internal SyncCollectionState CollectionState
        {
            get;
        }

        internal InvalidCollectionStateException(SyncCollectionState collectionState = SyncCollectionState.None) : base()
        {
            CollectionState = collectionState;
        }

        internal InvalidCollectionStateException(string message, SyncCollectionState collectionState = SyncCollectionState.None) : base(message)
        {
            CollectionState = collectionState;
        }
    }

    [NonSerialized]
    private SyncCollectionState _collectionState;

    public ObservableDictionary()
    {
        _keyToIndex = new Dictionary<TKey, int>();
        _values = new ObservableCollection<TValue>();
        _values.CollectionChanged += _values_CollectionChanged;
        _readOnlyValues = new ReadOnlyObservableCollection<TValue>(_values);
        _collectionState = SyncCollectionState.Idle;
    }


    private void ExecuteWithCollectionStateSet(SyncCollectionState setState, IReadOnlySet<SyncCollectionState> invalidStates, Action action)
    {
        if (invalidStates != null && invalidStates.Contains(_collectionState)) throw new InvalidCollectionStateException(_collectionState);
        if (_collectionState != SyncCollectionState.Idle)
        {
            //System.Diagnostics.Debug.WriteLine($"[State] State is not Idle (state is {_collectionState} now), so execute nothing. The state to be set is {setState}.");
            return;
        }

        //System.Diagnostics.Debug.WriteLine($"[State] State changed: {_collectionState} => {setState}");
        _collectionState = setState;

        try
        {
            action();
        }
        finally
        {
            //System.Diagnostics.Debug.WriteLine($"[State] State changed: {_collectionState} => {SyncCollectionState.Idle}");
            _collectionState = SyncCollectionState.Idle;
        }
    }

    private void _values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // Measures to prevent a loop where this event handler is called due to updates of _values within the InsertItem, RemoveItem, SetItem, MoveItem, ClearItem methods,
        // and then the base collection is updated within this event handler, which in turn calls the aforementioned methods again...
        // Is it properly preventing the loop?
        // Is there a more desirable way?

        //System.Diagnostics.Debug.WriteLine("----------------");
        //System.Diagnostics.Debug.WriteLine($"_values_CollectionChanged: {e.Action}");
        ExecuteWithCollectionStateSet(SyncCollectionState.SyncValuesToOthers, InvalidStatesIsSyncValuesToOthers, () =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                throw new InvalidOperationException("You cannot add a value to a dictionary without specifying a key.");
            }

            //lock (_syncRoot)
            //{
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    // Corresponds to SetItem
                    {
                        if (e.OldStartingIndex != e.NewStartingIndex) throw new NotImplementedException(); // Unexpected case

                        var index = e.NewStartingIndex;
                        foreach (var value in e.NewItems)
                        {
                            base[index] = new KeyValuePair<TKey, TValue>(base[index].Key, (TValue)value);
                            index++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Corresponds to RemoveItem
                    {
                        var index = e.OldStartingIndex;
                        base.RemoveAt(index);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Corresponds to ClearItems
                    {
                        base.Clear();
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Corresponds to MoveItems
                    {
                        var oldIndex = e.OldStartingIndex;
                        var newIndex = e.NewStartingIndex;
                        base.Move(oldIndex, newIndex);
                    }
                    break;
            }
            //}
        });
    }

    /// <summary>
    /// Gets an <see cref="ObservableCollection{T}" /> containing the value in <see cref="ObservableDictionary{TKey, TValue}" />.
    /// Values can be moved, updated, or deleted via this collection. However, additions cannot be made.
    /// </summary>
    public ObservableCollection<TValue> ObservableValues => _values;

    #region ISerializable Implementation

    protected ObservableDictionary(SerializationInfo info, StreamingContext context)
    {
        _keyToIndex = (Dictionary<TKey, int>)info.GetValue("KeyToIndex", typeof(Dictionary<TKey, int>));
        _values = (ObservableCollection<TValue>)info.GetValue("Values", typeof(ObservableCollection<TValue>));
        _values.CollectionChanged += _values_CollectionChanged;
        _collectionState = SyncCollectionState.Idle;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("KeyToIndex", _keyToIndex);
        info.AddValue("Values", _values);
    }

    #endregion

    #region IDictionary Implementation

    ///<inheritdoc/>
    public TValue this[TKey key]
    {
        get
        {
            //lock (_syncRoot)
            //{
            return _values[_keyToIndex[key]];
            //}
        }
        set
        {
            //lock (_syncRoot)
            //{
            if (ContainsKey(key))
            {
                // Update
                var index = _keyToIndex[key];
                this[index] = new KeyValuePair<TKey, TValue>(key, value);
            }
            else
            {
                // Add
                base.Add(new KeyValuePair<TKey, TValue>(key, value));
            }
            //}
        }
    }

    ///<inheritdoc/>
    public ICollection<TKey> Keys => _keyToIndex.Keys;

    ///<inheritdoc/>
    public ICollection<TValue> Values => _readOnlyValues;

    ///<inheritdoc/>
    public void Add(TKey key, TValue value) => Add(new KeyValuePair<TKey, TValue>(key, value));

    ///<inheritdoc/>
    public bool ContainsKey(TKey key)
    {
        //lock (_syncRoot)
        //{
        return _keyToIndex.ContainsKey(key);
        //}
    }

    ///<inheritdoc/>
    public bool Remove(TKey key)
    {
        //lock (_syncRoot)
        //{
        if (_keyToIndex.TryGetValue(key, out var index))
        {
            base.RemoveAt(index);
            return true;
        }

        return false;
        //}
    }

    ///<inheritdoc/>
    public bool TryGetValue(TKey key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TValue value)
    {
        //lock (_syncRoot)
        //{
        if (_keyToIndex.TryGetValue(key, out var index))
        {
            value = _values[index];
            return true;
        }
        else
        {
            value = default;
            return false;
        }
        //}
    }

    #endregion

    #region Base Class Behavior Customization Overrides

    ///<inheritdoc/>
    protected override void InsertItem(int index, KeyValuePair<TKey, TValue> item)
    {
        //System.Diagnostics.Debug.WriteLine("----------------");
        //System.Diagnostics.Debug.WriteLine($"InsertItem: {index}, {item}");
        if (index < 0 || index > Count) throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.");
        if (ContainsKey(item.Key)) throw new ArgumentException($"An item with the key '{item.Key}' has already been added.");

        UpdateKeyToIndexWhenInsert(index);
        _keyToIndex[item.Key] = index;

        ExecuteWithCollectionStateSet(SyncCollectionState.SyncBaseToValues, InvalidStatesIsSyncBaseToValues, () =>
        {
            _values.Insert(index, item.Value);
        });

        base.InsertItem(index, item);
        //DebugWrite();
    }

    private void UpdateKeyToIndexWhenInsert(int index)
    {
        // TODO: Implement a more efficient method.
        foreach (var key in _keyToIndex.Keys)
        {
            if (_keyToIndex[key] >= index) _keyToIndex[key]++;
        }
    }

    ///<inheritdoc/>
    protected override void RemoveItem(int index)
    {
        //System.Diagnostics.Debug.WriteLine("----------------");
        //System.Diagnostics.Debug.WriteLine($"RemoveItem: {index}");
        var keyValuePair = base[index];
        _keyToIndex.Remove(keyValuePair.Key);
        UpdateKeyToIndexWhenRemove(index);
        ExecuteWithCollectionStateSet(SyncCollectionState.SyncBaseToValues, InvalidStatesIsSyncBaseToValues, () =>
        {
            _values.RemoveAt(index);
        });
        base.RemoveItem(index);
        //DebugWrite();
    }

    private void UpdateKeyToIndexWhenRemove(int index)
    {
        // TODO: Implement a more efficient method.
        foreach (var key in _keyToIndex.Keys)
        {
            if (_keyToIndex[key] >= index) _keyToIndex[key]--;
        }
    }

    ///<inheritdoc/>
    protected override void SetItem(int index, KeyValuePair<TKey, TValue> item)
    {
        //System.Diagnostics.Debug.WriteLine("----------------");
        //System.Diagnostics.Debug.WriteLine($"SetItem: {index}, {item}");
        if (!ContainsKey(item.Key)) throw new KeyNotFoundException($"No item with the key '{item.Key}' exists in the dictionary.");
        if (_keyToIndex[item.Key] != index) throw new ArgumentException($"{item.Key} is already set to {_keyToIndex[item.Key]}, but you specified {index}.");

        ExecuteWithCollectionStateSet(SyncCollectionState.SyncBaseToValues, InvalidStatesIsSyncBaseToValues, () =>
        {
            _values[index] = item.Value;
        });

        base.SetItem(index, item);
        //DebugWrite();
    }

    ///<inheritdoc/>
    protected override void MoveItem(int oldIndex, int newIndex)
    {
        UpdateKeyToIndexWhenMove(oldIndex, newIndex);
        ExecuteWithCollectionStateSet(SyncCollectionState.SyncBaseToValues, InvalidStatesIsSyncBaseToValues, () =>
        {
            _values.Move(oldIndex, newIndex);
        });
        base.MoveItem(oldIndex, newIndex);
    }

    private void UpdateKeyToIndexWhenMove(int oldIndex, int newIndex)
    {
        if (oldIndex < newIndex)
        {
            UpdateKeyToIndexWhenMoveBehind(oldIndex, newIndex);
        }
        else
        {
            UpdateKeyToIndexWhenMoveAhead(oldIndex, newIndex);
        }
    }

    private void UpdateKeyToIndexWhenMoveAhead(int oldIndex, int newIndex)
    {
        // TODO: Implement a more efficient method.
        foreach (var item in _keyToIndex)
        {
            if (item.Value > newIndex && item.Value < oldIndex) _keyToIndex[item.Key]++;
            else if (item.Value == oldIndex) _keyToIndex[item.Key] = newIndex;
        }
    }

    private void UpdateKeyToIndexWhenMoveBehind(int oldIndex, int newIndex)
    {
        // TODO: Implement a more efficient method.
        foreach (var item in _keyToIndex)
        {
            if (item.Value > oldIndex && item.Value < newIndex) _keyToIndex[item.Key]--;
            else if (item.Value == oldIndex) _keyToIndex[item.Key] = newIndex;
        }
    }


    ///<inheritdoc/>
    protected override void ClearItems()
    {
        //System.Diagnostics.Debug.WriteLine("----------------");
        //System.Diagnostics.Debug.WriteLine($"ClearItems:");
        _keyToIndex.Clear();
        ExecuteWithCollectionStateSet(SyncCollectionState.SyncBaseToValues, InvalidStatesIsSyncBaseToValues, () =>
        {
            _values.Clear();
        });
        base.ClearItems();
        //DebugWrite();
    }

    private void DebugWrite()
    {
        System.Diagnostics.Debug.WriteLine(String.Empty);
        System.Diagnostics.Debug.WriteLine("base:");
        for (int i = 0; i < Count; i++)
        {
            var item = base[i];
            System.Diagnostics.Debug.WriteLine($"index: {i}, key: {item.Key}, value {item.Value}");
        }

        System.Diagnostics.Debug.WriteLine(String.Empty);
        System.Diagnostics.Debug.WriteLine("_keyToIndex:");
        foreach (var item in _keyToIndex)
        {
            System.Diagnostics.Debug.WriteLine($"key: {item.Key}, index {item.Value}");
        }

        System.Diagnostics.Debug.WriteLine(String.Empty);
        System.Diagnostics.Debug.WriteLine("_values:");
        for (int i = 0; i < _values.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"index: {i}, value {_values[i]}");
        }
    }

    #endregion
    /*
    #region Thread-Safe Write-Access Method Hiding

    public new KeyValuePair<TKey, TValue> this[int index]
    {
        get
        {
            lock (_syncRoot)
            {
                return base[index];
            }
        }
        set
        {
            lock (_syncRoot)
            {
                base[index] = value;
            }
        }
    }

    public new int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return base.Count;
            }
        }
    }

    public bool IsReadOnly => throw new NotImplementedException();

    public new bool Contains(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return base.Contains(item);
        }
    }

    public new void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        lock (_syncRoot)
        {
            base.CopyTo(array, arrayIndex);
        }
    }

    public new void Add(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            base.Add(item);
        }
    }

    public new void Clear()
    {
        lock (_syncRoot)
        {
            base.Clear();
        }
    }

    public new bool Remove(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return base.Remove(item);
        }
    }

    public new void RemoveAt(int index)
    {
        lock (_syncRoot)
        {
            base.RemoveAt(index);
        }
    }

    public new int IndexOf(KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            return base.IndexOf(item);
        }
    }

    public new void Insert(int index, KeyValuePair<TKey, TValue> item)
    {
        lock (_syncRoot)
        {
            base.Insert(index, item);
        }
    }

    public new void Move(int oldIndex, int  newIndex)
    {
        lock (_syncRoot)
        {
            base.Move(oldIndex, newIndex);
        }
    }

    #endregion
    */
}
