   public class LimBlock : LimObject
   {
      public bool async = false;
      public override string name { get { return "Block"; } }
      public LimMessage containedMessage;
      public LimObjectArrayList argNames;
      public LimObject scope; // if 0x0, then use target as the locals proto
      public LimCallStatus passStops;

      // Prototypes and Clone

      public new static LimBlock createProto(LimState state)
      {
         LimBlock number = new LimBlock();
         return number.proto(state) as LimBlock;
      }

      public new static LimBlock createObject(LimState state)
      {
         LimBlock number = new LimBlock();
         return number.clone(state) as LimBlock;
      }

      public override LimObject proto(LimState state)
      {
         LimBlock pro = new LimBlock();
         pro.state = state;
         pro.createSlots();
         pro.createProtos();
         pro.containedMessage = state.nilMessage;
         pro.argNames = new LimObjectArrayList();
         state.registerProtoWithFunc(name, new LimStateProto(name, pro, new LimStateProtoFunc(this.proto)));
         pro.protos.Add(state.protoWithInitFunc("Object"));

         LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("call", new LimMethodFunc(LimBlock.slotCall)),
                new LimCFunction("code", new LimMethodFunc(LimBlock.slotCode)),
                new LimCFunction("block", new LimMethodFunc(LimBlock.slotBlock)),
                new LimCFunction("method", new LimMethodFunc(LimBlock.slotMethod)),
    	    };

         pro.addTaglessMethodTable(state, methodTable);
         return pro;
      }

      public override void cloneSpecific(LimObject _from, LimObject _to)
      {
         LimBlock to = _to as LimBlock;
         LimBlock from = _from as LimBlock;
         to.isActivatable = from.isActivatable;
         to.containedMessage = from.containedMessage;
         to.argNames = new LimObjectArrayList();
      }

      // Published Slots

      public new static LimObject slotMethod(LimObject target, LimObject locals, LimObject message)
      {
         LimState state = target.state;
         LimBlock self = LimBlock.createObject(state);
         LimMessage m = message as LimMessage;
         int nargs = m.args.Count;
         LimMessage lastArgAsMessage = (nargs > 0) ? m.rawArgAt(nargs - 1) : state.nilMessage;
         int i;

         self.containedMessage = lastArgAsMessage;
         self.isActivatable = true;

         for (i = 0; i < nargs - 1; i++)
         {
            LimMessage argMessage = m.rawArgAt(i);
            LimSeq name = argMessage.messageName;
            self.argNames.Add(name);
         }

         return self;
      }

      public new static LimObject slotBlock(LimObject target, LimObject locals, LimObject m)
      {
         LimBlock self = target as LimBlock;
         self = LimBlock.slotMethod(target, locals, m) as LimBlock;
         self.scope = locals;
         self.isActivatable = false;
         return self;
      }

      public static LimObject slotCode(LimObject target, LimObject locals, LimObject m)
      {
         string s = "";
         LimBlock self = target as LimBlock;
         if (self.scope != null)
            s += "block(";
         else
            s += "method(";
         int nargs = self.argNames.Count;
         for (int i = 0; i < nargs; i++)
         {
            LimSeq name = self.argNames[i] as LimSeq;
            s += name.value + ", ";
         }

         LimMessage msg = self.containedMessage;
         LimSeq seq = LimMessage.slotCode(msg, locals, m) as LimSeq;
         s += seq.value + ")";

         return LimSeq.createObject(target.state, s);
      }

      public static LimObject slotCall(LimObject target, LimObject locals, LimObject message)
      {
         return target.activate(target, locals, locals, message as LimMessage, locals);
      }

      // Call Public Raw Methods

      public override LimObject activate(LimObject sender, LimObject target, LimObject locals, LimMessage m, LimObject slotContext)
      {
         LimState state = sender.state;
         LimBlock self = sender as LimBlock;

         LimObjectArrayList argNames = self.argNames;
         LimObject scope = self.scope;

         LimObject blockLocals = state.localsProto.clone(state);
         LimObject result = null;
         LimObject callObject = null;

         blockLocals.isLocals = true;

         if (scope == null)
            scope = target;

         blockLocals.createSlots();

         callObject = LimCall.with(state, locals, target, m, slotContext, self, null/*state.currentCoroutine*/);

         LimSeqObjectHashtable bslots = blockLocals.slots;
         bslots["call"] = callObject;
         bslots["self"] = scope;
         bslots["updateSlot"] = state.localsUpdateSlotCFunc;

         if (argNames != null)
            for (int i = 0; i < argNames.Count; i++)
            {
               LimSeq name = argNames[i] as LimSeq;
               LimObject arg = m.localsValueArgAt(locals, i);
               blockLocals.slots[name] = arg;
            }

         if (self.containedMessage != null)
         {
            result = self.containedMessage.localsPerformOn(blockLocals, blockLocals);
         }

         if (self.passStops == LimCallStatus.MESSAGE_STOP_STATUS_NORMAL)
         {

         }

         return result;
      }

   }
