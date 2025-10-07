using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    // Save the dictionary content to the lists for serialization
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // Load the dictionary content from the lists after deserialization
    public void OnAfterDeserialize()
    {
        this.Clear();
        if (keys.Count != values.Count)
        {
            Debug.LogError("Deserialized dictionary data is inconsistent.");
            return;
        }
        for (int i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }
}