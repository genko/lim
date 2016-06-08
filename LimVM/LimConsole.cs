public class LimConsole : LimObject
{
    public override string getName() { return "Console"; }


    public static LimConsole createProto(LimState state)
    {
        LimConsole console = new LimConsole();
        return console.proto(state) as LimConsole;
    }

    public override LimObject proto(LimState state)
    {
        LimConsole pro = new LimConsole();
        pro.setState(state);
        pro.createSlots();
        pro.createProtos();
        state.registerProtoWithFunc(getName(), new LimStateProto(pro.getName(), pro, new LimStateProtoFunc(pro.proto)));
        pro.protos.Add(state.protoWithInitFunc("Object"));

        LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("readLine", new LimMethodFunc(LimConsole.readLine)),
                new LimCFunction("readKey", new LimMethodFunc(LimConsole.readKey))
            };

        pro.addTaglessMethodTable(state, methodTable);
        return pro;
    }

    public override int GetHashCode()
    {
        return System.Convert.ToInt32(uniqueIdCounter);
    }

    public override void print()
    {
        System.Console.Write("{0}", this.ToString());
    }

    public static LimSeq readLine(LimObject target, LimObject locals, LimObject message)
    {
        LimConsole o = target as LimConsole;
        return LimSeq.createObject(o.getState(), System.Console.ReadLine());
    }

    public static LimSeq readKey(LimObject target, LimObject locals, LimObject message)
    {
        LimConsole o = target as LimConsole;
        return LimSeq.createObject(o.getState(), System.Console.ReadKey().KeyChar.ToString());
    }

}