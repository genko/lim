namespace lim {

    public class LimSystem : LimObject
    {
      public override string name { get { return "System"; } }


      public new static LimSystem createProto(LimState state)
      {
         LimSystem console = new LimSystem();
         return console.proto(state) as LimSystem;
      }

      public override LimObject proto(LimState state)
      {
         LimSystem pro = new LimSystem();
         pro.state = state;
            pro.createSlots();
            pro.createProtos();
            state.registerProtoWithFunc(name, new LimStateProto(pro.name, pro, new LimStateProtoFunc(pro.proto)));
         pro.protos.Add(state.protoWithInitFunc("Object"));

            LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("args", new LimMethodFunc(LimSystem.args))
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

        public static LimList args(LimObject target, LimObject locals, LimObject message)
        {
           LimSystem o = target as LimSystem;
           LimList l = LimList.createObject(o.state);
           for (int i = 0; i < LimCLI.m_args.Length; i++)
           {
              l.append(LimSeq.createObject(o.state, LimCLI.m_args[i]));
           }
           return l;
        }
    }
}