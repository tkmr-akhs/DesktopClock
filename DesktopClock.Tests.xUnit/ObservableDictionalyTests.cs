using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopClock.Core.Models;

namespace DesktopClock.Tests.xUnit;

public class ObservableDictionalyTests
{
    #region Properties
    #region ObservableValues
    #region Add

    [Fact]
    public void ObservableValues_GetAccess_ReturnsCollectionReflectingAdditionsFromDictionary()
    {
        // arrange
        var keyObject = new object();
        var valueObject = new object();

        var dict = new ObservableDictionary<object, object>();
        NotifyCollectionChangedAction? dictEvent = null;
        dict.CollectionChanged += (sender, e) => { dictEvent = e.Action; };

        var collection = dict.ObservableValues;
        NotifyCollectionChangedAction? collectionEvent = null;
        collection.CollectionChanged += (sender, e) => { collectionEvent = e.Action; };

        // act
        dict.Add(keyObject, valueObject);
        
        // assert
        Assert.Equal(valueObject, dict[keyObject]);
        Assert.Equal(valueObject, collection[0]);
        Assert.Equal(NotifyCollectionChangedAction.Add, dictEvent);
        Assert.Equal(NotifyCollectionChangedAction.Add, collectionEvent);
    }

    [Fact]
    public void ObservableValues_GetAccess_ThrowsExceptionOnDirectAdd()
    {
        // arrange
        var keyObject = new object();
        var valueObject = new object();

        var dict = new ObservableDictionary<object, object>();
        NotifyCollectionChangedAction? dictEvent = null;
        dict.CollectionChanged += (sender, e) => { dictEvent = e.Action; };

        var collection = dict.ObservableValues;
        NotifyCollectionChangedAction? collectionEvent = null;
        collection.CollectionChanged += (sender, e) => { collectionEvent = e.Action; };

        // act
        // assert
        Assert.Throws<InvalidOperationException>(() => collection.Add(valueObject));
        Assert.Equal(default, dictEvent);
        Assert.Equal(default, collectionEvent);
    }

    #endregion


    #region Insert

    [Fact]
    public void ObservableValues_GetAccess_ReturnsCollectionReflectingInsertionsFromDictionary()
    {
        // arrange
        var keyObject1 = new object();
        var valueObject1 = new object();
        var keyObject2 = new object();
        var valueObject2 = new object();

        var dict = new ObservableDictionary<object, object>();
        var collection = dict.ObservableValues;
        dict.Add(keyObject1, valueObject1);

        NotifyCollectionChangedAction? dictEvent = null;
        dict.CollectionChanged += (sender, e) => { dictEvent = e.Action; };

        NotifyCollectionChangedAction? collectionEvent = null;
        collection.CollectionChanged += (sender, e) => { collectionEvent = e.Action; };

        // act
        dict.Insert(0, new KeyValuePair<object, object>(keyObject2, valueObject2));

        // assert
        Assert.Equal(valueObject2, dict[keyObject2]);
        Assert.Equal(valueObject2, collection[0]);
        Assert.Equal(NotifyCollectionChangedAction.Add, dictEvent);
        Assert.Equal(NotifyCollectionChangedAction.Add, collectionEvent);
    }

    [Fact]
    public void ObservableValues_GetAccess_ThrowsExceptionOnDirectInsert()
    {
        // arrange
        var keyObject1 = new object();
        var valueObject1 = new object();
        var keyObject2 = new object();
        var valueObject2 = new object();

        var dict = new ObservableDictionary<object, object>();
        var collection = dict.ObservableValues;
        dict.Add(keyObject1, valueObject1);

        NotifyCollectionChangedAction? dictEvent = null;
        dict.CollectionChanged += (sender, e) => { dictEvent = e.Action; };

        NotifyCollectionChangedAction? collectionEvent = null;
        collection.CollectionChanged += (sender, e) => { collectionEvent = e.Action; };

        // act
        // assert
        Assert.Throws<InvalidOperationException>(() => collection.Insert(0, valueObject2));
        Assert.Equal(default, dictEvent);
        Assert.Equal(default, collectionEvent);
    }

    #endregion

    #region Delete

    [Fact]
    public void ObservableValues_GetAccess_ReturnsCollectionReflectingDeletionsFromDictionary()
    {
        // arrange
        var keyObject1 = new object();
        var valueObject1 = new object();
        var keyObject2 = new object();
        var valueObject2 = new object();

        var dict = new ObservableDictionary<object, object>();
        var collection = dict.ObservableValues;
        dict.Add(keyObject1, valueObject1);
        dict.Add(keyObject2, valueObject2);

        NotifyCollectionChangedAction? dictEvent = null;
        dict.CollectionChanged += (sender, e) => { dictEvent = e.Action; };

        NotifyCollectionChangedAction? collectionEvent = null;
        collection.CollectionChanged += (sender, e) => { collectionEvent = e.Action; };

        // act
        dict.Remove(keyObject1);

        // assert
        Assert.Equal(valueObject2, dict[keyObject2]);
        Assert.Equal(valueObject2, collection[0]);
        Assert.Equal(NotifyCollectionChangedAction.Remove, dictEvent);
        Assert.Equal(NotifyCollectionChangedAction.Remove, collectionEvent);
    }

    [Fact]
    public void ObservableValues_GetAccess_ReturnsCollectionReflectingDeletionsToDictionary()
    {
        // arrange
        var keyObject1 = new object();
        var valueObject1 = new object();
        var keyObject2 = new object();
        var valueObject2 = new object();

        var dict = new ObservableDictionary<object, object>();
        var collection = dict.ObservableValues;
        dict.Add(keyObject1, valueObject1);
        dict.Add(keyObject2, valueObject2);

        NotifyCollectionChangedAction? dictEvent = null;
        dict.CollectionChanged += (sender, e) => { dictEvent = e.Action; };

        NotifyCollectionChangedAction? collectionEvent = null;
        collection.CollectionChanged += (sender, e) => { collectionEvent = e.Action; };

        // act
        collection.Remove(valueObject1);

        // assert
        Assert.Equal(valueObject2, dict[keyObject2]);
        Assert.Equal(valueObject2, collection[0]);
        Assert.Equal(NotifyCollectionChangedAction.Remove, dictEvent);
        Assert.Equal(NotifyCollectionChangedAction.Remove, collectionEvent);
    }

    #endregion





    #endregion
    #endregion




    /*
    【プロパティ】
    Keys: 辞書内のキーのコレクションを取得します。
    Values: 辞書内の値のコレクションを取得します。
    Count: コレクション内の要素数を取得します。
    IsReadOnly: コレクションが読み取り専用かどうかを示します。

    【メソッド】
    Add(TKey key, TValue value): 指定されたキーと値のペアを辞書に追加します。
    ContainsKey(TKey key): 特定のキーが辞書に含まれているかどうかを判断します。
    Remove(TKey key): 指定されたキーに関連付けられた要素を削除します。
    TryGetValue(TKey key, out TValue value): 指定されたキーに関連付けられた値を取得しようとします。
    GetObjectData(SerializationInfo info, StreamingContext context): シリアライズプロセスにデータを提供します。
    Add(KeyValuePair<TKey, TValue> item): キーと値のペアをコレクションに追加します。
    Contains(KeyValuePair<TKey, TValue> item): 特定の値がコレクションに含まれているかどうかを判断します。
    CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex): 特定の配列インデックスから始まる配列に要素をコピーします。
    Remove(KeyValuePair<TKey, TValue> item): 特定のオブジェクトをコレクションから削除します。
    Clear(): コレクションからすべての要素を削除します。
    IndexOf(KeyValuePair<TKey, TValue> item): 指定した要素のインデックスを検索します。
    Insert(int index, KeyValuePair<TKey, TValue> item): 指定したインデックスの位置に要素を挿入します。
    RemoveAt(int index): 指定したインデックス位置にある要素を削除します。

    【インデクサ】
    Item[int index]: 指定されたインデックスにある要素を取得または設定します。
    Item[TKey key]: 指定されたキーに関連付けられた値を取得または設定します。
    */
}
