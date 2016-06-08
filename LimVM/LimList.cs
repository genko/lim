public class LimList : LimObject
{
    public override string getName() { return "List"; }
    public LimObjectArrayList list = new LimObjectArrayList();

    public static LimList createProto(LimState state)
    {
        LimList m = new LimList();
        return m.proto(state) as LimList;
    }

    public static LimList createObject(LimState state)
    {
        LimList m = new LimList();
        return m.clone(state) as LimList;
    }

    public override LimObject proto(LimState state)
    {
        LimList pro = new LimList();
        pro.setState(state);
        pro.createSlots();
        pro.createProtos();
        pro.list = new LimObjectArrayList();
        state.registerProtoWithFunc(pro.getName(), new LimStateProto(pro.getName(), pro, new LimStateProtoFunc(pro.proto)));
        pro.protos.Add(state.protoWithInitFunc("Object"));

        LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("indexOf", new LimMethodFunc(LimList.slotIndexOf)),
                new LimCFunction("capacity", new LimMethodFunc(LimList.slotSize)),
                new LimCFunction("size", new LimMethodFunc(LimList.slotSize)),
                new LimCFunction("removeAll", new LimMethodFunc(LimList.slotRemoveAll)),
                new LimCFunction("append", new LimMethodFunc(LimList.slotAppend)),
                new LimCFunction("appendSeq", new LimMethodFunc(LimList.slotAppendSeq)),
                new LimCFunction("with", new LimMethodFunc(LimList.slotWith)),
                new LimCFunction("prepend", new LimMethodFunc(LimList.slotPrepend)),
                new LimCFunction("push", new LimMethodFunc(LimList.slotAppend)),
                new LimCFunction("at", new LimMethodFunc(LimList.slotAt)),
                new LimCFunction("last", new LimMethodFunc(LimList.slotLast)),
                new LimCFunction("pop", new LimMethodFunc(LimList.slotPop)),
                new LimCFunction("removeAt", new LimMethodFunc(LimList.slotRemoveAt)),
                new LimCFunction("reverseForeach", new LimMethodFunc(LimList.slotReverseForeach)),
            };

        pro.addTaglessMethodTable(state, methodTable);
        return pro;
    }


    public override LimObject clone(LimState state)
    {
        LimObject proto = state.protoWithInitFunc(getName());
        LimList result = new LimList();
        uniqueIdCounter++;
        result.uniqueId = uniqueIdCounter;
        result.list = new LimObjectArrayList();
        result.setState(state);
        result.createProtos();
        result.createSlots();
        result.protos.Add(proto);
        return result;
    }

    // Published Slots
    public static LimObject slotIndexOf(LimObject target, LimObject locals, LimObject m)
    {
        LimList o = target as LimList;
        LimObject value = (m as LimMessage).localsValueArgAt(locals, 1);
        try
        {
            return LimNumber.newWithDouble(target.getState(), o.list.IndexOf(value));
        }
        catch (System.ArgumentOutOfRangeException aoore)
        {
            object ex = aoore;
            return target.getState().LimNil;
        }
    }

    public static LimObject slotRemoveAll(LimObject target, LimObject locals, LimObject m)
    {
        LimList o = target as LimList;
        if (o.list != null)
        {
            o.list.Clear();
        }
        return target;
    }

    public static LimObject slotCapacity(LimObject target, LimObject locals, LimObject m)
    {
        LimList o = target as LimList;
        return LimNumber.newWithDouble(target.getState(), o.list.Capacity());
    }

    public static LimObject slotSize(LimObject target, LimObject locals, LimObject m)
    {
        LimList o = target as LimList;
        return LimNumber.newWithDouble(target.getState(), o.list.Count());
    }

    public void append(LimObject o)
    {
        this.list.Add(o);
    }

    public static LimObject slotAppend(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimList o = target as LimList;

        for (int i = 0; i < m.args.Count(); i++)
        {
            LimObject obj = m.localsValueArgAt(locals, i);
            o.list.Add(obj);
        }
        return o;
    }

    public static LimObject slotAppendSeq(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimList o = target as LimList;

        for (int i = 0; i < m.args.Count(); i++)
        {
            LimList obj = m.localsValueArgAt(locals, i) as LimList;
            for (int j = 0; j < obj.list.Count(); j++)
            {
                LimObject v = obj.list.Get(j) as LimObject;
                o.list.Add(v);
            }
        }
        return o;
    }

    public static LimObject slotWith(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimList o = LimList.createObject(target.getState()) as LimList;

        for (int i = 0; i < m.args.Count(); i++)
        {
            LimObject obj = m.localsValueArgAt(locals, i);
            o.list.Add(obj);
        }
        return o;
    }

    public static LimObject slotPrepend(LimObject target, LimObject locals, LimObject message)
    {
        return target;
    }

    public static LimObject slotAt(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimNumber ind = m.localsNumberArgAt(locals, 0);
        LimList o = target as LimList;
        LimObject v = o.list.Get(ind.asInt()) as LimObject;
        return v == null ? target.getState().LimNil : v;
    }

    public static LimObject slotLast(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimList o = target as LimList;
        if (o.list.Count() > 0)
        {
            LimObject e = o.list.Get(o.list.Count() - 1) as LimObject;
            return e;
        }
        return target.getState().LimNil;
    }

    public static LimObject slotPop(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimList o = target as LimList;
        if (o.list.Count() > 0)
        {
            LimObject e = o.list.Get(o.list.Count() - 1) as LimObject;
            o.list.RemoveAt(o.list.Count() - 1);
            return e;
        }
        else
        {
            return target.getState().LimNil;
        }
    }

    public static LimObject slotContains(LimObject target, LimObject locals, LimObject message)
    {
        return null; // TODO: return IoBool
    }

    public static LimObject slotForeach(LimObject target, LimObject locals, LimObject message)
    {
        return null; // TODO: return IoBool
    }

    public static LimObject slotReverseForeach(LimObject target, LimObject locals, LimObject message)
    {
        return target; // TODO: return IoBool
    }

    public static LimObject slotRemoveAt(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimNumber ind = m.localsNumberArgAt(locals, 0);
        LimList o = target as LimList;
        try
        {
            o.list.RemoveAt(ind.asInt());
            return target;
        }
        catch (System.ArgumentOutOfRangeException aoore)
        {
            object ex = aoore;
            return target.getState().LimNil;
        }
    }

    public override string ToString()
    {
        return uniqueId.ToString();
    }
}