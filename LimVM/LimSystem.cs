using System.Collections;
using System;
using System.Globalization;
using System.IO;

public class LimSystem : LimObject
{
    public override string getName() { return "System"; }

    public new static LimSystem createProto(LimState state)
    {
        LimSystem console = new LimSystem();
        return console.proto(state) as LimSystem;
    }

    public override LimObject proto(LimState state)
    {
        LimSystem pro = new LimSystem();
        pro.setState(state);
        pro.createSlots();
        pro.createProtos();
        state.registerProtoWithFunc(getName(), new LimStateProto(pro.getName(), pro, new LimStateProtoFunc(pro.proto)));
        pro.protos.Add(state.protoWithInitFunc("Object"));

        LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("args", new LimMethodFunc(LimSystem.args)),
                new LimCFunction("exit", new LimMethodFunc(LimSystem.exit))
            };

        pro.addTaglessMethodTable(state, methodTable);
        return pro;
    }

    public override int GetHashCode()
    {
        return Convert.ToInt32(uniqueIdCounter);
    }

    public override void print()
    {
        Console.Write("{0}", this.ToString());
    }

    public static LimList args(LimObject target, LimObject locals, LimObject message)
    {
        LimSystem o = target as LimSystem;
        LimList l = LimList.createObject(o.getState());

        l.append(LimSeq.createObject(o.getState(), System.AppDomain.CurrentDomain.FriendlyName));

        for (int i = 0; i < LimState.args.Length; i++)
        {
            l.append(LimSeq.createObject(o.getState(), LimState.args[i]));
        }
        return l;
    }

    public static LimObject exit(LimObject target, LimObject locals, LimObject message)
    {
        System.Environment.Exit(0);
        return LimObject.createObject(target.getState());
    }
}