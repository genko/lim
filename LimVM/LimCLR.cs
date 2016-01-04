using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;

namespace lim
{
    public class LimClr : LimObject
    {
        public override string name { get { return "CLR"; } }
        public Hashtable usingNamespaces = new Hashtable();
        public Hashtable loadedAssemblies = new Hashtable();
        public LimClr() : base() { isActivatable = true; }

        public new static LimClr createProto(LimState state)
        {
            LimClr cf = new LimClr();
            return cf.proto(state) as LimClr;
        }

        public new static LimClr createObject(LimState state)
        {
            LimClr cf = new LimClr();
            return cf.proto(state).clone(state) as LimClr;
        }

        public LimClr(LimState state, string name)
        {
            isActivatable = true;
            this.state = state;
            createSlots();
            createProtos();
            uniqueId = 0;
        }

        public override LimObject proto(LimState state)
        {
            LimClr pro = new LimClr();
            pro.state = state;
            pro.uniqueId = 0;
            pro.createSlots();
            pro.createProtos();
            pro.isActivatable = true;
            state.registerProtoWithFunc(pro.name, new LimStateProto(pro.name, pro, new LimStateProtoFunc(pro.proto)));
            //pro.protos.Add(state.protoWithInitFunc("Object"));

            LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("loadAssembly", new LimMethodFunc(LimClr.slotLoadAssembly)),
                new LimCFunction("using", new LimMethodFunc(LimClr.slotUsing)),
                new LimCFunction("getType", new LimMethodFunc(LimClr.slotGetType)),
			};

            pro.addTaglessMethodTable(state, methodTable);
            return pro;
        }

        public override void cloneSpecific(LimObject from, LimObject to)
        {
            to.isActivatable = true;
        }

        // Published Slots

        public static LimObject slotUsing(LimObject target, LimObject locals, LimObject message)
        {
            LimClr self = target as LimClr;
            LimMessage m = message as LimMessage;
            LimSeq nameSpace = m.localsSymbolArgAt(locals, 0);
            bool validNamespace = false;
            LimClrAssembly foundInAssembly = null;
            foreach (LimClrAssembly asm in self.loadedAssemblies.Values)
            {
                if (asm.assemblyNamespaces[nameSpace.value] != null)
                {
                    validNamespace = true;
                    foundInAssembly = asm;
                    break;
                }
            }
            if (!validNamespace)
            {
                Console.WriteLine("Namespace '{0}' is not valid.", nameSpace.value);
                return self;
            }

            if (self.usingNamespaces[nameSpace.value] == null)
                self.usingNamespaces[nameSpace.value] = foundInAssembly;
            return self;
        }

        public static LimObject slotLoadAssembly(LimObject target, LimObject locals, LimObject message)
        {
            LimClr self = target as LimClr;
            LimMessage m = message as LimMessage;
            LimSeq assemblyName = m.localsSymbolArgAt(locals, 0);
            LimClrAssembly asm = self.loadedAssemblies[assemblyName.value] as LimClrAssembly;
            if (asm != null)
            {
                return asm;
            }

            asm = LimClrAssembly.createObject(target.state);

            asm.assembly = Assembly.LoadWithPartialName(assemblyName.value);
            if (asm.assembly == null) return self;

            self.loadedAssemblies[assemblyName.value] = asm;

            asm.assemblyTypes = asm.assembly.GetTypes();
            asm.assemblyNamespaces = new Hashtable();
            foreach (Type t in asm.assemblyTypes)
            {
                string theNameSpace = t.FullName.LastIndexOf(".") == -1 ? "-" : t.FullName.Substring(0, t.FullName.LastIndexOf("."));
                string theClass = t.FullName.LastIndexOf(".") == -1 ? t.FullName : t.FullName.Substring(t.FullName.LastIndexOf(".") + 1);
                if (theClass.Equals("Form"))
                {
                    int i = 0;
                }
                if (asm.assemblyNamespaces.ContainsKey(theNameSpace))
                {
                    Hashtable a = asm.assemblyNamespaces[theNameSpace] as Hashtable;
                    a[theClass] = t;
                }

                else
                {
                    Hashtable classes = new Hashtable();
                    classes[theClass] = t;
                    asm.assemblyNamespaces[theNameSpace] = classes;
                }

            }
            return asm;
        }

        public static LimObject slotGetType(LimObject target, LimObject locals, LimObject message)
        {
            LimClr self = target as LimClr;
            LimMessage m = message as LimMessage;
            LimSeq typeName = m.localsSymbolArgAt(locals, 0);
            LimObject obj = self.getType(target.state, typeName.value);
            return obj == null ? target.state.LimNil : obj;
        }

        // Public methos

        public LimObject getType(LimState state, string typeName)
        {
            Type t = null;
            foreach (string s in this.usingNamespaces.Keys)
            {
                LimClrAssembly asm = this.usingNamespaces[s] as LimClrAssembly;
                t = asm.assembly.GetType(s + "." + typeName);
                if (t != null)
                {
                    LimClrObject obj = LimClrObject.createObject(state) as LimClrObject;
                    obj.clrType = t;
                    obj.clrInstance = null;
                    return obj;
                }
            }
            if (t == null)
            {
                foreach (string s in this.loadedAssemblies.Keys)
                {
                    LimClrAssembly asm = this.loadedAssemblies[s] as LimClrAssembly;
                    t = asm.assembly.GetType(typeName);
                    if (t != null)
                    {
                        LimClrObject obj = LimClrObject.createObject(state) as LimClrObject;
                        obj.clrType = t;
                        obj.clrInstance = null;
                        return obj;
                    }
                }
            }
            return null;
        }

        public LimClrObject getType(string typeName)
        {
            Type t = null;
            foreach (string s in this.usingNamespaces.Keys)
            {
                LimClrAssembly asm = this.usingNamespaces[s] as LimClrAssembly;
                t = asm.assembly.GetType(s + typeName);
                if (t != null)
                {
                    LimClrObject obj = new LimClrObject();
                    obj.clrType = t;
                    obj.clrInstance = null;
                }
            }
            return null;
        }

        public override LimObject activate(LimObject self, LimObject target, LimObject locals, LimMessage m, LimObject slotContext)
        {
            return self;
        }
    }
}
