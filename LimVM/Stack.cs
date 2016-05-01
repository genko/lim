public class Stack
{
    System.Collections.Generic.List<object> stack = new System.Collections.Generic.List<object>();

    public object Peek()
    {
        return stack[(stack.Count-1)];
    }

    public void Push(object obj)
    {
        stack.Add(obj);
    }

    public object Pop()
    {
        object obj = stack[(stack.Count-1)];
        stack.Remove(obj);
        return obj;
    }
}
