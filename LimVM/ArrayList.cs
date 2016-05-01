﻿public class ArrayList
{
    System.Collections.Generic.List<object> list = new System.Collections.Generic.List<object>();

    public void Add(object obj)
    {
        list.Add(obj);
    }

    public int Count()
    {
        return list.Count;
    }

    public object Get(int index)
    {
        return list[index];
    }

    public int IndexOf(object obj)
    {
        return list.IndexOf(obj);
    }

    public void Clear()
    {
        list.Clear();
    }

    public int Capacity()
    {
        return list.Capacity;
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
    }

    public System.Collections.Generic.List<object>  getIter()
    {
        return list;
    }

}
