using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace lim
{
    public class LimClrObject : LimObject
    {
        public override string name { get { return "CLRObject"; } }
        public Type clrType;
        public object clrInstance;
        public LimClrObject() : base() { isActivatable = false; }

        public new static LimClrObject createProto(LimState state)
        {
            LimClrObject cf = new LimClrObject();
            return cf.proto(state) as LimClrObject;
        }

        public new static LimClrObject createObject(LimState state)
        {
            LimClrObject cf = new LimClrObject();
            return cf.proto(state).clone(state) as LimClrObject;
        }

        public LimClrObject(LimState state, string name)
        {
            isActivatable = true;
            this.state = state;
            createSlots();
            createProtos();
            uniqueId = 0;
        }

        public override LimObject proto(LimState state)
        {
            LimClrObject pro = new LimClrObject();
            pro.state = state;
            pro.uniqueId = 0;
            pro.createSlots();
            pro.createProtos();
            pro.isActivatable = true;
            state.registerProtoWithFunc(pro.name, new LimStateProto(pro.name, pro, new LimStateProtoFunc(pro.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            LimCFunction[] methodTable = new LimCFunction[] {
				new LimCFunction("type", new LimMethodFunc(LimObject.slotType))
			};

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public override LimObject clone(LimState state)
        {
            LimClrObject proto = state.protoWithInitFunc(name) as LimClrObject;
            LimClrObject result = new LimClrObject();
            result.isActivatable = true;
            uniqueIdCounter++;
            result.uniqueId = uniqueIdCounter;
            result.state = state;
            result.createProtos();
            result.createSlots();
            result.protos.Add(proto);
            return result;
        }

        public LimClrFunction getMethod(LimMessage message)
        {
            string methodName = message.messageName.value;
            if (clrType == null) return null;
            ConstructorInfo[] searchConstructors = null;
            Type[] parameters = null;
            ArrayList args = null;
            MethodBase mb = null;

            args = new ArrayList();
            parameters = new Type[message.args.Count];

            for (int i = 0; i < message.args.Count; i++)
            {
                LimObject o = message.localsValueArgAt(message, i);
                args.Add(o);
                Type t = null;
                switch (o.name)
                {
                    case "Number": t = typeof(double); break;
                    case "Object": t = typeof(object); break;
                    case "CLRObject": t = (o as LimClrObject).clrType; break;
                    case "Sequence": t = typeof(string); break;
                }
                parameters[i] = t;
            }

            if (methodName.Equals("new"))
            {
                searchConstructors = this.clrType.GetConstructors();
                if (searchConstructors.Length > 0)
                    mb = searchConstructors[0];
            }
            else
            {
                try
                {
                    mb = this.clrType.GetMethod(methodName, parameters);
                }
                catch { }
            }
            
            LimClrFunction clrFunction = LimClrFunction.createObject(this.state);
            clrFunction.methodInfo = mb;
            clrFunction.parametersTypes = parameters;
            clrFunction.evaluatedParameters = args;
            return clrFunction;
        }

        public override LimObject activate(LimObject self, LimObject target, LimObject locals, LimMessage m, LimObject slotContext)
        {
            return self;
        }

        public override string ToString()
        {
			if (clrInstance == null) {
				if (clrType == null) return name;
				return clrType.ToString();
			}
			return clrInstance.ToString();
        }
    }
}
