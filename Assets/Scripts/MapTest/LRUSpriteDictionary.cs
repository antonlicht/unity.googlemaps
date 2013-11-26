using System.Collections;
using System.Collections.Generic;

public class LRUSpriteDictionary : IDictionary<LRUSpriteDictionary.SpriteID, RefCountedSprite>
{
    public struct SpriteID
    {
        public int X;
        public int Y;
        public SpriteID(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    private Dictionary<SpriteID, LinkedListNode<KeyValuePair<SpriteID, RefCountedSprite>>> _dict = new Dictionary<SpriteID, LinkedListNode<KeyValuePair<SpriteID, RefCountedSprite>>>();

    private LinkedList<KeyValuePair<SpriteID, RefCountedSprite>> _list = new LinkedList<KeyValuePair<SpriteID, RefCountedSprite>>();

    public int Max_Size { get; set; }

    public LRUSpriteDictionary(int maxsize)
    {
        Max_Size = maxsize;
    }

    public void Add(SpriteID key, RefCountedSprite value)
    {
        lock (_dict)
        {
            LinkedListNode<KeyValuePair<SpriteID, RefCountedSprite>> node;

            if (_dict.TryGetValue(key, out node))
            {
                _list.Remove(node);
                _list.AddFirst(node);
            }
            else
            {
                node = new LinkedListNode<KeyValuePair<SpriteID, RefCountedSprite>>(
                new KeyValuePair<SpriteID, RefCountedSprite>(key, value));

                _dict.Add(key, node);
                _list.AddFirst(node);
                value.AddRef();

            }

            if (_dict.Count > Max_Size)
            {
                var nodetoremove = _list.Last;
                if (nodetoremove != null)
                    Remove(nodetoremove.Value.Key);
            }
        }

    }

    public bool Remove(SpriteID key)
    {
        lock (_dict)
        {
            LinkedListNode<KeyValuePair<SpriteID, RefCountedSprite>> removednode;
            if (_dict.TryGetValue(key, out removednode))
            {
                _dict.Remove(key);
                _list.Remove(removednode);
                removednode.Value.Value.Release();
                return true;
            }

            else
                return false;
        }
    }

    public bool TryGetValue(SpriteID key, out RefCountedSprite value)
    {
        LinkedListNode<KeyValuePair<SpriteID, RefCountedSprite>> node;

        bool result = false;
        lock (_dict)
            result = _dict.TryGetValue(key, out node);

        if (node != null)
        {
            value = node.Value.Value;
            _list.Remove(node);
            _list.AddFirst(node);
        }
        else
            value = default(RefCountedSprite);

        return result;
    }



    public bool ContainsKey(SpriteID key)
    {
        lock (_dict)
            return _dict.ContainsKey(key);
    }

    public ICollection<SpriteID> Keys
    {
        get { lock (_dict) return _dict.Keys; }
    }

    public ICollection<RefCountedSprite> Values
    {
        get { throw new System.NotImplementedException(); }
    }

    public RefCountedSprite this[SpriteID key]
    {
        get
        {
            RefCountedSprite value;
            if (TryGetValue(key, out value))
                return value;
            return default(RefCountedSprite);
        }
        set
        {
            if (Equals(value, default(RefCountedSprite)))
                Remove(key);
            else
                Add(key, value);
        }
    }

    public void Add(KeyValuePair<SpriteID, RefCountedSprite> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }

    public bool Contains(KeyValuePair<SpriteID, RefCountedSprite> item)
    {
        throw new System.NotImplementedException();
    }

    public void CopyTo(KeyValuePair<SpriteID, RefCountedSprite>[] array, int arrayIndex)
    {
        throw new System.NotImplementedException();
    }

    public int Count
    {
        get { return _dict.Count; }
    }

    public bool IsReadOnly
    {
        get { throw new System.NotImplementedException(); }
    }

    public bool Remove(KeyValuePair<SpriteID, RefCountedSprite> item)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator<KeyValuePair<SpriteID, RefCountedSprite>> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
}
