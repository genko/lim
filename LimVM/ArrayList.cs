public class ArrayList
{
    System.Collections.Generic.List<object> list = new System.Collections.Generic.List<object>();

    public void Add(object obj)
    {
        list.Add(obj);
    }

    public int Count
    {
        get { return list.Count; }
    }

    public object Get(int index)
    {
        try
        {
            return list[index];
        }
        catch (System.Exception)
        {
            return null;
        }
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

    public void Remove(object item)
    {
        list.Remove(item);
    }

    public System.Collections.Generic.List<object>  getIter()
    {
        return list;
    }

}
