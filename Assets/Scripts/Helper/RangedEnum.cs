using System;

[Serializable]
public struct MinMaxEnum<T> where T : Enum
{
    public T start;
    public T end;
}