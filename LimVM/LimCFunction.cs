   public delegate LimObject LimMethodFunc(LimObject target, LimObject locals, LimObject message);

   public class LimCFunction : LimObject
   {
      public bool async = false;
      public override string getName() { return "CFunction"; }
      public LimMethodFunc func;
      public string funcName;
      public LimCFunction() : base() { isActivatable = true; }

      public new static LimCFunction createProto(LimState state)
      {
         LimCFunction cf = new LimCFunction();
         return cf.proto(state) as LimCFunction;
      }

      public new static LimCFunction createObject(LimState state)
      {
         LimCFunction cf = new LimCFunction();
         return cf.proto(state).clone(state) as LimCFunction;
      }

      public LimCFunction(string name, LimMethodFunc func) : this(null, name, func) { }

      public LimCFunction(LimState state, string name, LimMethodFunc func)
      {
         isActivatable = true;
         this.setState(state);
         createSlots();
         createProtos();
         uniqueId = 0;
         funcName = name;
         this.func = func;
      }

      public override LimObject proto(LimState state)
      {
         LimCFunction pro = new LimCFunction();
         pro.setState(state);
         pro.uniqueId = 0;
         pro.createSlots();
         pro.createProtos();
         pro.isActivatable = true;
         state.registerProtoWithFunc(pro.getName(), new LimStateProto(pro.getName(), pro, new LimStateProtoFunc(pro.proto)));
         pro.protos.Add(state.protoWithInitFunc("Object"));

         LimCFunction[] methodTable = new LimCFunction[] {
                //new LimCFunction("perform", new LimMethodFunc(pro.slotPerform)),
			};

         pro.addTaglessMethodTable(state, methodTable);
         return pro;
      }

      public override void cloneSpecific(LimObject _from, LimObject _to)
      {
         LimCFunction from = _from as LimCFunction;
         LimCFunction to = _to as LimCFunction;
         to.isActivatable = true;
         to.funcName = from.funcName;
         to.func = from.func;
      }

      public override LimObject activate(LimObject self, LimObject target, LimObject locals, LimMessage m, LimObject slotContext)
      {
         if (func == null) return self;
         return func(target, locals, m);
      }
   }