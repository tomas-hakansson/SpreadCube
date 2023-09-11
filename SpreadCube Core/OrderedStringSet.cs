using System.Collections;

namespace SpreadCube_Core;

public class OrderedStringSet : IList, IList<NonEmptyString>
{
    LinkedList<NonEmptyString> _list;

    public OrderedStringSet()
    {
        _list = new LinkedList<NonEmptyString>();
    }

    public NonEmptyString this[int index]
    {
        get => GetByIndex(index);
        set => throw new NotImplementedException();
    }

    NonEmptyString GetByIndex(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index starts at 0 (zero).");
        if (index >= _list.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must not exceed collection.Count - 1.");

        LinkedListNode<NonEmptyString> currentNode = new("placeholder");

        for (int i = 0; i <= index; i++)
        {
            if (i == 0)
            {
                if (_list.First == null)
                    throw new NullReferenceException("First value of collection is null.");

                currentNode = _list.First;
            }
            else
            {
                if (currentNode.Next == null)
                    throw new NullReferenceException("Next value of collection is null.");

                currentNode = currentNode.Next;
            }
        }

        return currentNode.Value;
    }

    public int Count => _list.Count;

    public void Add(NonEmptyString item)
    {
        if (!Contains(item))
            _list.AddLast(item);
    }

    public bool Contains(NonEmptyString item) =>
        _list.Contains(item);

    IEnumerator IEnumerable.GetEnumerator() =>
        _list.GetEnumerator();


    //Inherited methods, unimplemented because they're unused:

    object? IList.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    bool IList.IsReadOnly => throw new NotImplementedException();
    bool ICollection<NonEmptyString>.IsReadOnly => throw new NotImplementedException();
    bool IList.IsFixedSize => throw new NotImplementedException();
    bool ICollection.IsSynchronized => throw new NotImplementedException();
    object ICollection.SyncRoot => throw new NotImplementedException();
    public int Add(object? value) => throw new NotImplementedException();
    public void Clear() => throw new NotImplementedException();
    public bool Contains(object? value) => throw new NotImplementedException();
    public void CopyTo(NonEmptyString[] array, int arrayIndex) => throw new NotImplementedException();
    public void CopyTo(Array array, int index) => throw new NotImplementedException();
    public IEnumerator<NonEmptyString> GetEnumerator() => throw new NotImplementedException();
    public int IndexOf(NonEmptyString item) => throw new NotImplementedException();
    public int IndexOf(object? value) => throw new NotImplementedException();
    public void Insert(int index, NonEmptyString item) => throw new NotImplementedException();
    public void Insert(int index, object? value) => throw new NotImplementedException();
    public bool Remove(NonEmptyString item) => throw new NotImplementedException();
    public void Remove(object? value) => throw new NotImplementedException();
    public void RemoveAt(int index) => throw new NotImplementedException();
}
