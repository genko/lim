namespace lim {

    public enum LimCallStatus
    {
        MESSAGE_STOP_STATUS_NORMAL = 0,
        MESSAGE_STOP_STATUS_BREAK = 1,
        MESSAGE_STOP_STATUS_CONTINUE = 2,
        MESSAGE_STOP_STATUS_RETURN = 4,
        MESSAGE_STOP_STATUS_EOL = 8
    }

	public class LimCall : LimObject
    {
        public override string name { get { return "Call"; } }
        public LimObject sender;
        public LimObject msg;
        public LimObject target;
        public LimObject slotContext;
        public LimObject activated;
        public LimObject coroutine;
        public LimCallStatus stopStatus;

        public new static LimCall createProto(LimState state)
        {
            LimCall call = new LimCall();
            return call.proto(state) as LimCall;
        }

        public new static LimCall createObject(LimState state)
        {
            LimCall call = new LimCall();
            return call.clone(state) as LimCall;
        }

        public override LimObject proto(LimState state)
        {
            LimCall pro = new LimCall();
            pro.state = state;
            pro.createSlots();
            pro.createProtos(); 
            state.registerProtoWithFunc(name, new LimStateProto(pro.name, pro, new LimStateProtoFunc(pro.proto)));
            pro.protos.Add(state.protoWithInitFunc("Object"));

            LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("sender", new LimMethodFunc(LimCall.slotSender)),
                new LimCFunction("target", new LimMethodFunc(LimCall.slotTarget)),
                new LimCFunction("message", new LimMethodFunc(LimCall.slotCallMessage)),
            };

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public static LimObject slotSender(LimObject target, LimObject locals, LimObject message)
        {
            LimCall self = target as LimCall;
            return self.sender;
        }

        public static LimObject slotTarget(LimObject target, LimObject locals, LimObject message)
        {
            LimCall self = target as LimCall;
            return self.target;
        }


        public static LimObject slotCallMessage(LimObject target, LimObject locals, LimObject message)
        {
            // setSlot("A", Object clone do(setSlot("B", method(call message))))
            LimCall self = target as LimCall;
            return self.msg;
        }

        public static LimObject with(LimState state, LimObject sender, LimObject target,
            LimObject message, LimObject slotContext, LimObject activated, LimObject coro)
        {
            LimCall call = LimCall.createObject(state);
            call.sender = sender;
            call.target = target;
            call.msg = message;
            call.slotContext = slotContext;
            call.activated = activated;
            call.coroutine = coro;
            call.stopStatus = LimCallStatus.MESSAGE_STOP_STATUS_NORMAL;
            return call;
        }
    }
}
