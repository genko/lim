using System.Collections;

public class LimObjectArrayList : ArrayList
{
    public override string ToString()
    {
        string s = " (";
        for (int i = 0; i < Count; i++)
        {
            s += base[i].ToString();
            if (i != Count - 1)
                s += ",";
        }
        s += ")";
        return s;
    }
}

// SYMBOL HANDLING HELPER

public class LimSeqObjectHashtable : Hashtable
{
    public LimState state = null;
    public LimSeqObjectHashtable(LimState s) { state = s; }
    public override object this[object key]
    {
        get
        {
            return base[state.IOSYMBOL(key.ToString())];
        }
        set
        {
            base[state.IOSYMBOL(key.ToString())] = value;
        }
    }
}

public class LimStateProto
{
    public string name;
    public LimStateProtoFunc func;
    public LimObject proto;
    public LimStateProto(string name, LimObject proto, LimStateProtoFunc func)
    {
        this.name = name;
        this.func = func;
        this.proto = proto;
    }
}

public delegate LimObject LimStateProtoFunc(LimState state);

public class LimObject
{
    public LimState _state = null;

    public LimState getState()
    {
        return _state;
    }

    public void setState(LimState value)
    {
        _state = value;
        if (slots != null) slots.state = value;
    }

    public static long uniqueIdCounter = 0;
    public long uniqueId = 0;
    public virtual string getName() { return "Object"; }
    public LimSeqObjectHashtable slots;
    public LimObjectArrayList listeners;
    public LimObjectArrayList protos;
    public bool hasDoneLookup;
    public bool isActivatable;
    public bool isLocals;

    public static LimObject createProto(LimState state)
    {
        LimObject pro = new LimObject();
        return pro.proto(state);
    }

    public static LimObject createObject(LimState state)
    {
        LimObject pro = new LimObject();
        return pro.clone(state);
    }

    public virtual LimObject proto(LimState state)
    {
        LimObject pro = new LimObject();
        pro.setState(state);
        pro.createSlots();
        pro.createProtos();
        pro.uniqueId = 0;
        state.registerProtoWithFunc(getName(), new LimStateProto(pro.getName(), pro, new LimStateProtoFunc(pro.proto)));
        return pro;
    }

    public virtual LimObject clone(LimState state)
    {
        LimObject proto = state.protoWithInitFunc(getName());
        LimObject o = System.Activator.CreateInstance(this.GetType()) as LimObject;//typeof(this)new LimObject();
        uniqueIdCounter++;
        o.uniqueId = uniqueIdCounter;
        o.setState(proto.getState());
        o.createSlots();
        o.createProtos();
        o.protos.Add(proto);
        cloneSpecific(this, o);
        return o;
    }

    public virtual void cloneSpecific(LimObject from, LimObject to)
    {
    }

    // proto finish must be called only before first Sequence proto created

    public LimObject protoFinish(LimState state)
    {
        LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("compare", new LimMethodFunc(LimObject.slotCompare)),
                new LimCFunction("==", new LimMethodFunc(LimObject.slotEquals)),
                new LimCFunction("!=", new LimMethodFunc(LimObject.slotNotEquals)),
                new LimCFunction(">=", new LimMethodFunc(LimObject.slotGreaterThanOrEqual)),
                new LimCFunction("<=", new LimMethodFunc(LimObject.slotLessThanOrEqual)),
                new LimCFunction(">", new LimMethodFunc(LimObject.slotGreaterThan)),
                new LimCFunction("<", new LimMethodFunc(LimObject.slotLessThan)),
                new LimCFunction("-", new LimMethodFunc(LimObject.slotSubstract)),
                new LimCFunction("", new LimMethodFunc(LimObject.slotEevalArg)),
                new LimCFunction("self", new LimMethodFunc(LimObject.slotSelf)),
                new LimCFunction("clone", new LimMethodFunc(LimObject.slotClone)),
                new LimCFunction("return", new LimMethodFunc(LimObject.slotReturn)),
                new LimCFunction("cloneWithoutInit", new LimMethodFunc(LimObject.slotCloneWithoutInit)),
                new LimCFunction("doMessage", new LimMethodFunc(LimObject.slotDoMessage)),
                new LimCFunction("print", new LimMethodFunc(LimObject.slotPrint)),
                new LimCFunction("println", new LimMethodFunc(LimObject.slotPrintln)),
                new LimCFunction("slotNames", new LimMethodFunc(LimObject.slotSlotNames)),
                new LimCFunction("type", new LimMethodFunc(LimObject.slotType)),
                new LimCFunction("evalArg", new LimMethodFunc(LimObject.slotEevalArg)),
                new LimCFunction("evalArgAndReturnSelf", new LimMethodFunc(LimObject.slotEevalArgAndReturnSelf)),
                new LimCFunction("do", new LimMethodFunc(LimObject.slotDo)),
                new LimCFunction("getSlot", new LimMethodFunc(LimObject.slotGetSlot)),
                new LimCFunction("updateSlot", new LimMethodFunc(LimObject.slotUpdateSlot)),
                new LimCFunction("setSlot", new LimMethodFunc(LimObject.slotSetSlot)),
                new LimCFunction("setSlotWithType", new LimMethodFunc(LimObject.slotSetSlotWithType)),
                new LimCFunction("message", new LimMethodFunc(LimObject.slotMessage)),
                new LimCFunction("method", new LimMethodFunc(LimObject.slotMethod)),
                new LimCFunction("block", new LimMethodFunc(LimObject.slotBlock)),
                new LimCFunction("init", new LimMethodFunc(LimObject.slotSelf)),
                new LimCFunction("thisContext", new LimMethodFunc(LimObject.slotSelf)),
                new LimCFunction("thisMessage", new LimMethodFunc(LimObject.slotThisMessage)),
                new LimCFunction("thisLocals", new LimMethodFunc(LimObject.slotThisLocals)),
                new LimCFunction("init", new LimMethodFunc(LimObject.slotSelf)),
                new LimCFunction("if", new LimMethodFunc(LimObject.slotIf)),
                new LimCFunction("yield", new LimMethodFunc(LimObject.slotYield)),
                new LimCFunction("yieldingCoros", new LimMethodFunc(LimObject.slotYieldingCoros)),
                new LimCFunction("while", new LimMethodFunc(LimObject.slotWhile))
            };
        LimObject o = state.protoWithInitFunc(getName());
        o.addTaglessMethodTable(state, methodTable);
        return o;
    }

    // Published Slots

    public static LimObject slotCompare(LimObject self, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject o = m.localsValueArgAt(locals, 0);
        return LimNumber.newWithDouble(self.getState(), System.Convert.ToDouble(self.compare(o)));
    }

    public virtual int compare(LimObject v)
    {
        return uniqueId.CompareTo(v.uniqueId);
    }

    public static LimObject slotEquals(LimObject self, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject o = m.localsValueArgAt(locals, 0);
        return self.compare(o) == 0 ? self.getState().LimTrue : self.getState().LimFalse;
    }

    public static LimObject slotNotEquals(LimObject self, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject o = m.localsValueArgAt(locals, 0);
        return self.compare(o) != 0 ? self.getState().LimTrue : self.getState().LimFalse;
    }

    public static LimObject slotGreaterThanOrEqual(LimObject self, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject o = m.localsValueArgAt(locals, 0);
        return self.compare(o) >= 0 ? self.getState().LimTrue : self.getState().LimFalse;
    }

    public static LimObject slotLessThanOrEqual(LimObject self, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject o = m.localsValueArgAt(locals, 0);
        return self.compare(o) <= 0 ? self.getState().LimTrue : self.getState().LimFalse;
    }

    public static LimObject slotLessThan(LimObject self, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject o = m.localsValueArgAt(locals, 0);
        return self.compare(o) < 0 ? self.getState().LimTrue : self.getState().LimFalse;
    }

    public static LimObject slotSubstract(LimObject self, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimNumber o = m.localsNumberArgAt(locals, 0);
        return LimNumber.newWithDouble(self.getState(), -o.asDouble());
    }

    public static LimObject slotGreaterThan(LimObject self, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject o = m.localsValueArgAt(locals, 0);
        return self.compare(o) > 0 ? self.getState().LimTrue : self.getState().LimFalse;
    }

    public static LimObject slotSelf(LimObject self, LimObject locals, LimObject m)
    {
        return self;
    }

    public static LimObject slotThisMessage(LimObject self, LimObject locals, LimObject m)
    {
        return m;
    }

    public static LimObject slotThisLocals(LimObject self, LimObject locals, LimObject m)
    {
        return locals;
    }


    public static LimObject slotClone(LimObject target, LimObject locals, LimObject m)
    {
        //LimObject newObject = target.tag.cloneFunc(target.state);
        LimObject newObject = target.clone(target.getState());
        //newObject.protos.Clear();
        newObject.protos.Add(target);
        return target.initClone(target, locals, m as LimMessage, newObject);
    }

    public static LimObject slotReturn(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject v = m.localsValueArgAt(locals, 0);
        target.getState().Return(v);
        return target;
    }

    public static LimObject slotCloneWithoutInit(LimObject target, LimObject locals, LimObject m)
    {
        return target.clone(target.getState());
    }

    public static LimObject slotDoMessage(LimObject self, LimObject locals, LimObject m)
    {
        LimMessage msg = m as LimMessage;
        LimMessage aMessage = msg.localsMessageArgAt(locals, 0) as LimMessage;
        LimObject context = self;
        if (msg.args.Count >= 2)
        {
            context = msg.localsValueArgAt(locals, 1);
        }
        return aMessage.localsPerformOn(context, self);
    }

    public static LimObject slotPrint(LimObject target, LimObject locals, LimObject m)
    {
        target.print();
        return target;
    }

    public static LimObject slotPrintln(LimObject target, LimObject locals, LimObject m)
    {
        target.print();
        System.Console.WriteLine();
        return target;
    }

    public static LimObject slotSlotNames(LimObject target, LimObject locals, LimObject message)
    {
        if (target.slots == null || target.slots.Count == 0) return target;
        foreach (object key in target.slots.Keys)
        {
            System.Console.Write(key.ToString() + " ");
        }
        System.Console.WriteLine();
        return target;
    }

    public static LimObject slotType(LimObject target, LimObject locals, LimObject message)
    {
        return LimSeq.createObject(target.getState(), target.getName());
    }

    public static LimObject slotEevalArg(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        return m.localsValueArgAt(locals, 0);
    }

    public static LimObject slotEevalArgAndReturnSelf(LimObject target, LimObject locals, LimObject message)
    {
        LimObject.slotEevalArg(target, locals, message);
        return target;
    }

    public static LimObject slotDo(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        if (m.args.Count != 0)
        {
            LimMessage argMessage = m.rawArgAt(0);
            argMessage.localsPerformOn(target, target);
        }
        return target;
    }

    public static LimObject slotGetSlot(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimSeq slotName = m.localsSymbolArgAt(locals, 0);
        LimObject slot = target.rawGetSlot(slotName);
        return slot == null ? target.getState().LimNil : slot;
    }

    public static LimObject slotSetSlot(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimSeq slotName = m.localsSymbolArgAt(locals, 0);
        LimObject slotValue = m.localsValueArgAt(locals, 1);
        if (slotName == null) return target;
        target.slots[slotName] = slotValue;
        return slotValue;
    }

    public static LimObject localsUpdateSlot(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimSeq slotName = m.localsSymbolArgAt(locals, 0);
        if (slotName == null) return target;
        LimObject obj = target.rawGetSlot(slotName);
        if (obj != null)
        {
            LimObject slotValue = m.localsValueArgAt(locals, 1);
            target.slots[slotName] = slotValue;
            return slotValue;
        }
        else
        {
            LimObject theSelf = target.rawGetSlot(target.getState().selfMessage.messageName);
            if (theSelf != null)
            {
                return theSelf.perform(theSelf, locals, m);
            }
        }
        return target.getState().LimNil;
    }

    public static LimObject slotUpdateSlot(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimSeq slotName = m.localsSymbolArgAt(locals, 0);
        LimObject slotValue = m.localsValueArgAt(locals, 1);
        if (slotName == null) return target;

        if (target.rawGetSlot(slotName) != null)
        {
            target.slots[slotName] = slotValue;
        }
        else
        {
            System.Console.WriteLine("Slot {0} not found. Must define slot using := operator before updating.", slotName.value);
        }

        return slotValue;
    }

    public static LimObject slotSetSlotWithType(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimSeq slotName = m.localsSymbolArgAt(locals, 0);
        LimObject slotValue = m.localsValueArgAt(locals, 1);
        target.slots[slotName] = slotValue;
        if (slotValue.slots[target.getState().typeSymbol] == null)
        {
            slotValue.slots[target.getState().typeSymbol] = slotName;
        }
        return slotValue;
    }

    public static LimObject slotMessage(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        return m.args.Count > 0 ? m.rawArgAt(0) : target.getState().LimNil;
    }

    public static LimObject slotMethod(LimObject target, LimObject locals, LimObject message)
    {
        return LimBlock.slotMethod(target, locals, message);
    }

    public static LimObject slotBlock(LimObject target, LimObject locals, LimObject message)
    {
        return LimBlock.slotBlock(target, locals, message);
    }

    public static LimObject slotLocalsForward(LimObject target, LimObject locals, LimObject message)
    {
        LimObject o = target.slots[target.getState().selfSymbol] as LimObject;
        if (o != null && o != target)
            return target.perform(o, locals, message);
        return target.getState().LimNil;
    }

    public static LimObject slotIf(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject r = m.localsValueArgAt(locals, 0);
        bool condition = r != target.getState().LimNil && r != target.getState().LimFalse;
        int index = condition ? 1 : 2;
        if (index < m.args.Count)
            return m.localsValueArgAt(locals, index);
        return condition ? target.getState().LimTrue : target.getState().LimFalse;
    }

    public static LimObject slotYieldingCoros(LimObject target, LimObject locals, LimObject message)
    {
        return LimNumber.newWithDouble(target.getState(), target.getState().contextList.Count);
    }

    public static LimObject slotYield(LimObject target, LimObject locals, LimObject message)
    {
        LimState state = target.getState();
        ArrayList toDeleteThread = new ArrayList();
        for (int i = 0; i < state.contextList.Count; i++)
        {
            IEnumerator e = state.contextList[i] as IEnumerator;
            bool end = e.MoveNext();
            if (!end) toDeleteThread.Add(e);
        }
        foreach (object e in toDeleteThread)
            state.contextList.Remove(e);
        return LimNumber.newWithDouble(state, state.contextList.Count);
    }

    public class EvaluateArgsEventArgs : System.EventArgs
    {
        public int Position = 0;
        public EvaluateArgsEventArgs(int pos) { Position = pos; }
    }

    public delegate void EvaluateArgsEventHandler(LimMessage msg, EvaluateArgsEventArgs e, out LimObject res);

    public static LimObject slotWhile(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;

        LimObject result = target.getState().LimNil;
        while (true)
        {
            LimObject cond = m.localsValueArgAt(locals, 0);
            if (cond == target.getState().LimFalse || cond == target.getState().LimNil)
            {
                break;
            }
            result = m.localsValueArgAt(locals, 1);
            if (target.getState().handleStatus() != 0)
            {
                goto done;
            }

        }
    done:
        return result;
    }

    // Object Public Raw Methods
    public LimObject initClone(LimObject target, LimObject locals, LimMessage m, LimObject newObject)
    {
        LimObject context = null;
        LimObject initSlot = target.rawGetSlotContext(target.getState().initMessage.messageName, out context);
        if (initSlot != null)
            initSlot.activate(initSlot, newObject, locals, target.getState().initMessage, context);
        return newObject;
    }

    public void addTaglessMethodTable(LimState state, LimCFunction[] table)
    {
        foreach (LimCFunction entry in table)
        {
            entry.setState(state);
            slots[entry.funcName] = entry;
        }
    }

    public LimObject forward(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = message as LimMessage;
        LimObject context = null;
        LimObject forwardSlot = target.rawGetSlotContext(target.getState().forwardMessage.messageName, out context);

        System.Console.WriteLine("'{0}' does not respond to message '{1}'",
            target.getName(), m.messageName.ToString());
        return target;
    }

    public LimObject perform(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage msg = message as LimMessage;
        LimObject context = null;
        LimObject slotValue = target.rawGetSlotContext(msg.messageName, out context);

        if (slotValue != null)
        {
            return slotValue.activate(slotValue, target, locals, msg, context);
        }
        if (target.isLocals)
        {
            return LimObject.slotLocalsForward(target, locals, message);
        }
        return target.forward(target, locals, message);
    }

    public LimObject localsProto(LimState state)
    {
        LimObject obj = LimObject.createObject(state);
        LimObject firstProto = obj.protos[0] as LimObject;
        foreach (object key in firstProto.slots.Keys)
            obj.slots[key] = firstProto.slots[key];
        firstProto.protos.Clear();
        obj.slots["setSlot"] = new LimCFunction(state, "setSlot", new LimMethodFunc(LimObject.slotSetSlot));
        obj.slots["setSlotWithType"] = new LimCFunction(state, "setSlotWithType", new LimMethodFunc(LimObject.slotSetSlotWithType));
        obj.slots["updateSlot"] = new LimCFunction(state, "updateSlot", new LimMethodFunc(LimObject.localsUpdateSlot));
        obj.slots["thisLocalContext"] = new LimCFunction(state, "thisLocalContext", new LimMethodFunc(LimObject.slotThisLocals));
        obj.slots["forward"] = new LimCFunction(state, "forward", new LimMethodFunc(LimObject.slotLocalsForward));
        return obj;
    }

    public virtual LimObject activate(LimObject self, LimObject target, LimObject locals, LimMessage m, LimObject slotContext)
    {
        return self.isActivatable ? self.activate(self, target, locals, m) : self;
    }

    public LimObject activate(LimObject self, LimObject target, LimObject locals, LimMessage m)
    {
        if (self.isActivatable)
        {
            LimObject context = null;
            LimObject slotValue = self.rawGetSlotContext(self.getState().activateMessage.messageName, out context);
            if (slotValue != null)
            {
                return activate(slotValue, target, locals, m, context);
            }
            return getState().LimNil;
        }
        else return self;
    }

    public void createSlots()
    {
        if (slots == null)
            slots = new LimSeqObjectHashtable(getState());
    }

    public void createProtos()
    {
        if (protos == null)
            protos = new LimObjectArrayList();
    }

    public LimObject slotsBySymbol(LimSeq symbol)
    {
        LimSeq s = this.getState().symbols[symbol.value] as LimSeq;
        if (s == null) return null;
        return slots[s] as LimObject;
    }

    public LimObject rawGetSlot(LimSeq slot)
    {
        LimObject context = null;
        LimObject v = rawGetSlotContext(slot, out context);
        return v;
    }

    public LimObject rawGetSlotContext(LimSeq slot, out LimObject context)
    {
        if (slot == null)
        {
            context = null;
            return null;
        }
        LimObject v = null;
        context = null;
        if (slotsBySymbol(slot) != null)
        {
            v = slotsBySymbol(slot) as LimObject;
            if (v != null)
            {
                context = this;
                return v;
            }
        }
        hasDoneLookup = true;
        foreach (LimObject proto in protos)
        {
            if (proto.hasDoneLookup)
                continue;
            v = proto.rawGetSlotContext(slot, out context);
            if (v != null) break;
        }
        hasDoneLookup = false;

        return v;
    }

    public virtual void print()
    {
        System.Console.Write(this);
    }

    public override string ToString()
    {
        return getName() + "+" + uniqueId;
    }
}