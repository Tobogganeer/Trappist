using System.Collections.Generic;

[System.Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [UnityEngine.SerializeField]
    List<SerializablePair<TKey, TValue>> values;
    Dictionary<TKey, TValue> _dict;
    public Dictionary<TKey, TValue> dictionary
    {
        get
        {
            if (_dict == null)
            {
                _dict = new Dictionary<TKey, TValue>();
                foreach (SerializablePair<TKey, TValue> pair in values)
                {
                    _dict.Add(pair.key, pair.value);
                }
            }

            return _dict;
        }
    }

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set => dictionary[key] = value;
    }

    public void Add(TKey key, TValue value) => dictionary.Add(key, value);

    public void Clear() => dictionary.Clear();

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public bool ContainsValue(TValue value) => dictionary.ContainsValue(value);

    public bool Remove(TKey key) => dictionary.Remove(key);

    public bool Remove(TKey key, out TValue value) => dictionary.Remove(key, out value);

    public bool TryAdd(TKey key, TValue value) => dictionary.TryAdd(key, value);

    public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);


    [System.Serializable]
    class SerializablePair<T1, T2>
    {
        public T1 key;
        public T2 value;
    }
}