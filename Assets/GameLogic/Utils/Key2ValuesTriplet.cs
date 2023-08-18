using System;

[Serializable]
public class Key2ValuesTriplet<K, V1, V2>
{
    public K Key;
    public V1 Value1;
    public V2 Value2;

    public Key2ValuesTriplet(K key, V1 value1, V2 value2)
    {
        Key = key;
        Value1 = value1;
        Value2 = value2;
    }
}
