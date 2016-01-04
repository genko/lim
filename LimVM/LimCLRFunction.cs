using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace lim
{
    public class LimClrFunction : LimObject
    {
        public bool async = false;
        public override string name { get { return "CLRFunction"; } }
        public MethodBase methodInfo;
        public Type[] parametersTypes;
        public ArrayList evaluatedParameters;
        public LimClrFunction() : base() { isActivatable = true; }

        public new static LimClrFunction createProto(LimState state)
        {
            LimClrFunction cf = new LimClrFunction();
            return cf.proto(state) as LimClrFunction;
        }

        public new static LimClrFunction createObject(LimState state)
        {
            LimClrFunction cf = new LimClrFunction();
            return cf.proto(state).clone(state) as LimClrFunction;
        }

        public LimClrFunction(LimState state, string name)
        {
            isActivatable = true;
            this.state = state;
            createSlots();
            createProtos();
            uniqueId = 0;
        }

        public override LimObject proto(LimState state)
        {
            LimClrFunction pro = new LimClrFunction();
            pro.state = state;
            pro.uniqueId = 0;
            pro.createSlots();
            pro.createProtos();
            pro.isActivatable = true;
            state.registerProtoWithFunc(pro.name, new LimStateProto(pro.name, pro, new LimStateProtoFunc(pro.proto)));
            //pro.protos.Add(state.protoWithInitFunc("Object"));

            LimCFunction[] methodTable = new LimCFunction[] {
			};

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public override LimObject activate(LimObject self, LimObject target, LimObject locals, LimMessage m, LimObject slotContext)
        {
            LimClrFunction method = self as LimClrFunction;
            LimClrObject obj = target as LimClrObject;
            object result = null;

            object[] parameters = new object[method.evaluatedParameters.Count];
            for (int i = 0; i < method.evaluatedParameters.Count; i++)
            {
                LimObject ep = method.evaluatedParameters[i] as LimObject;
                switch (ep.name)
                {
                    case "Object": parameters[i] = ep; break;
                    case "Number":
                        {
                            LimNumber num = ep as LimNumber;
                            if (num.isInteger)
                            {
                                parameters[i] = num.longValue;
                            }
                            else
                            {
                                parameters[i] = num.doubleValue;
                            }

                        }
                        break;
                    case "Sequence": parameters[i] = (ep as LimSeq).value; break;
                    case "CLRObject": parameters[i] = (ep as LimClrObject).clrInstance; break;
                }

            }

            LimClrObject clr = LimClrObject.createObject(self.state);

            try
            {
                if (method.methodInfo is ConstructorInfo)
                {
                    ConstructorInfo ci = method.methodInfo as ConstructorInfo;
                    result = ci.Invoke(parameters);
                }
                else if (method.methodInfo is MethodInfo)
                {
                    MethodInfo mi = method.methodInfo as MethodInfo;
                    result = mi.Invoke(obj.clrInstance, parameters);
                }
                clr.clrType = result != null ? result.GetType() : null;
                clr.clrInstance = result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                clr.clrType = null;
                clr.clrInstance = null;
            }
            
            return clr;
        }

    }
}
