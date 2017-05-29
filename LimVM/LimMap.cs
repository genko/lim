using System.Collections;

public class LimMap : LimObject
{
    public override string getName() { return "Map"; }
    public System.Collections.Generic.Dictionary<object, object> map = new System.Collections.Generic.Dictionary<object, object>();

    public static LimMap createProto(LimState state)
    {
        LimMap m = new LimMap();
        return m.proto(state) as LimMap;
    }

    public override LimObject proto(LimState state)
    {
        LimMap pro = new LimMap();
        pro.setState(state);
        pro.createSlots();
        pro.createProtos();
        pro.map = new System.Collections.Generic.Dictionary<object, object>();
        state.registerProtoWithFunc(pro.getName(), new LimStateProto(pro.getName(), pro, new LimStateProtoFunc(pro.proto)));
        pro.protos.Add(state.protoWithInitFunc("Object"));

        LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("at", new LimMethodFunc(LimMap.slotAt)),
                new LimCFunction("atPut", new LimMethodFunc(LimMap.slotAtPut)),
                new LimCFunction("atIfAbsentPut", new LimMethodFunc(LimMap.slotAtIfAbsentPut)),
                new LimCFunction("empty", new LimMethodFunc(LimMap.slotEmpty)),
                new LimCFunction("size", new LimMethodFunc(LimMap.slotSize)),
                new LimCFunction("removeAt", new LimMethodFunc(LimMap.slotRemoveAt)),
                new LimCFunction("hasKey", new LimMethodFunc(LimMap.slotHasKey)),
                new LimCFunction("hasValue", new LimMethodFunc(LimMap.slotHasValue)),
            };

        pro.addTaglessMethodTable(state, methodTable);
        return pro;
    }

    public override void cloneSpecific(LimObject from, LimObject to)
    {
        (to as LimMap).map = new System.Collections.Generic.Dictionary<object, object>((from as LimMap).map);
    }

    public static LimObject slotEmpty(LimObject target, LimObject locals, LimObject m)
    {
        LimMap dict = target as LimMap;
        if (dict.map != null) dict.map.Clear();
        return target;
    }

    public static LimObject slotSize(LimObject target, LimObject locals, LimObject m)
    {
        LimMap dict = target as LimMap;
        if (dict.map != null) return LimNumber.newWithDouble(dict.getState(), dict.map.Count);
        return dict.getState().LimNil;
    }

    public object lookupMap(object k)
    {
        foreach (object key in map.Keys)
            if (key.ToString().Equals(k.ToString())) return map[key];
        return null;
    }

    public object lookupMapValues(object v)
    {
        foreach (object val in map.Values)
            if (val.Equals(v)) return val;
        return null;
    }

    public static LimObject slotAt(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject result = null;
        LimObject symbol = m.localsValueArgAt(locals, 0);
        LimMap dict = target as LimMap;
        result = dict.lookupMap(symbol) as LimObject;
        if (result == null && m.args.Count > 1)
        {
            result = m.localsValueArgAt(locals, 1);
        }
        return result == null ? dict.getState().LimNil : result;
    }

    public static LimObject slotAtPut(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject key = m.localsValueArgAt(locals, 0);
        LimObject value = m.localsValueArgAt(locals, 1);
        LimMap dict = target as LimMap;
        dict.map[key.ToString()] = value;
        return target;
    }

    public static LimObject slotAtIfAbsentPut(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject key = m.localsValueArgAt(locals, 0);
        LimObject value = m.localsValueArgAt(locals, 1);
        LimMap dict = target as LimMap;
        if (dict.lookupMap(key) == null)
            dict.map[key.ToString()] = value;
        return target;
    }

    public static LimObject slotRemoveAt(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject key = m.localsSymbolArgAt(locals, 0);
        LimMap dict = target as LimMap;
        dict.map[key.ToString()] = null;
        return target;
    }

    public static LimObject slotHasKey(LimObject target, LimObject locals, LimObject message)
    {
        LimMap dict = target as LimMap;
        LimMessage m = message as LimMessage;
        LimObject key = m.localsValueArgAt(locals, 0);
        if (dict.lookupMap(key) == null)
        {
            return dict.getState().LimFalse;
        }
        return dict.getState().LimTrue;
    }

    public static LimObject slotHasValue(LimObject target, LimObject locals, LimObject message)
    {
        LimMap dict = target as LimMap;
        LimMessage m = message as LimMessage;
        LimObject val = m.localsValueArgAt(locals, 0);
        if (dict.lookupMapValues(val) == null)
        {
            return dict.getState().LimFalse;
        }
        return dict.getState().LimTrue;
    }

}