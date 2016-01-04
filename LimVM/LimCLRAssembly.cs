using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;

namespace lim
{
    public class LimClrAssembly : LimObject
    {
        public override string name { get { return "CLRAssembly"; } }
        public Assembly assembly;
        public string assemblyName;
        public Type[] assemblyTypes;
        public Hashtable assemblyNamespaces;
        public LimClrAssembly() : base() { isActivatable = false; }

        public new static LimClrAssembly createProto(LimState state)
        {
            LimClrAssembly cf = new LimClrAssembly();
            return cf.proto(state) as LimClrAssembly;
        }

        public new static LimClrAssembly createObject(LimState state)
        {
            LimClrAssembly cf = new LimClrAssembly();
            return cf.proto(state).clone(state) as LimClrAssembly;
        }

        public LimClrAssembly(LimState state, string name)
        {
            isActivatable = true;
            this.state = state;
            createSlots();
            createProtos();
            uniqueId = 0;
        }

        public override LimObject proto(LimState state)
        {
            LimClrAssembly pro = new LimClrAssembly();
            pro.state = state;
            pro.uniqueId = 0;
            pro.createSlots();
            pro.createProtos();
            pro.isActivatable = true;
            state.registerProtoWithFunc(pro.name, new LimStateProto(pro.name, pro, new LimStateProtoFunc(pro.proto)));
            //pro.protos.Add(state.protoWithInitFunc("Object"));

            LimCFunction[] methodTable = new LimCFunction[] {
			    new LimCFunction("namespaces", new LimMethodFunc(LimClrAssembly.slotNamespaces)),
            };

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        // published slots

        public static LimObject slotNamespaces(LimObject target, LimObject locals, LimObject message)
        {
            LimClrAssembly self = target as LimClrAssembly;
            LimMessage m = message as LimMessage;
            foreach (string s in self.assemblyNamespaces.Keys)
            {
                Console.Write(s + " ");
            }
            Console.WriteLine();
            return self;
        }
    }
}
