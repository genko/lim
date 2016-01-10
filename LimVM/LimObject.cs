
using System;
using System.Collections;

namespace lim
{
    // DEBUG HELPER

    public class LimObjectArrayList : ArrayList
    {
        public override string ToString()
        {
            string s = " (";
            for (int i = 0; i < Count; i++)
            {
                s += base[i].ToString();
                if (i != Count - 1)
                    s += ",";
            }
            s += ")";
            return s;
        }
    }

    // SYMBOL HANDLING HELPER

    public class LimSeqObjectHashtable : Hashtable
    {
        public LimState state = null;
        public LimSeqObjectHashtable(LimState s) { state = s; }
        public override object this[object key]
        {
            get
            {
                return base[state.IOSYMBOL(key.ToString())];
            }
            set
            {
                base[state.IOSYMBOL(key.ToString())] = value;
            }
        }
    }

    public class LimStateProto
    {
        public string name;
        public LimStateProtoFunc func;
        public LimObject proto;
        public LimStateProto(string name, LimObject proto, LimStateProtoFunc func)
        {
            this.name = name;
            this.func = func;
            this.proto = proto;
        }
    }

    public delegate LimObject LimStateProtoFunc(LimState state);

    public class LimObject
    {
      public LimState _state = null;
      public LimState state { set { _state = value; 
         if (slots != null) slots.state = value; } get { return _state; } }
        public static long uniqueIdCounter = 0;
        public long uniqueId = 0;
        public virtual string name { get { return "Object"; } }
        public LimSeqObjectHashtable slots;
        public LimObjectArrayList listeners;
        public LimObjectArrayList protos;
        public bool hasDoneLookup;
        public bool isActivatable;
        public bool isLocals;

        public static LimObject createProto(LimState state)
        {
            LimObject pro = new LimObject();
            return pro.proto(state);
        }

        public static LimObject createObject(LimState state)
        {
            LimObject pro = new LimObject();
            return pro.clone(state);
        }
      
        public virtual LimObject proto(LimState state)
        {
            LimObject pro = new LimObject();
            pro.state = state;
            pro.createSlots();
            pro.createProtos();
            pro.uniqueId = 0;
            state.registerProtoWithFunc(name, new LimStateProto(pro.name, pro, new LimStateProtoFunc(pro.proto)));
            return pro;
        }

        public virtual LimObject clone(LimState state)
        {
            LimObject proto = state.protoWithInitFunc(name);
            LimObject o = Activator.CreateInstance(this.GetType()) as LimObject;//typeof(this)new LimObject();
            uniqueIdCounter++;
            o.uniqueId = uniqueIdCounter;
         o.state = proto.state;
            o.createSlots();
            o.createProtos();
            o.protos.Add(proto);
         cloneSpecific(this, o);
            return o;
        }

      public virtual void cloneSpecific(LimObject from, LimObject to) 
      {
      }

        // proto finish must be called only before first Sequence proto created

        public LimObject protoFinish(LimState state)
        {
            LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("compare", new LimMethodFunc(LimObject.slotCompare)),
                new LimCFunction("==", new LimMethodFunc(LimObject.slotEquals)),
                new LimCFunction("!=", new LimMethodFunc(LimObject.slotNotEquals)),
                new LimCFunction(">=", new LimMethodFunc(LimObject.slotGreaterThanOrEqual)),
                new LimCFunction("<=", new LimMethodFunc(LimObject.slotLessThanOrEqual)),
                new LimCFunction(">", new LimMethodFunc(LimObject.slotGreaterThan)),
                new LimCFunction("<", new LimMethodFunc(LimObject.slotLessThan)),
                new LimCFunction("-", new LimMethodFunc(LimObject.slotSubstract)),
                new LimCFunction("", new LimMethodFunc(LimObject.slotEevalArg)),
                new LimCFunction("self", new LimMethodFunc(LimObject.slotSelf)),
                new LimCFunction("clone", new LimMethodFunc(LimObject.slotClone)),
                new LimCFunction("return", new LimMethodFunc(LimObject.slotReturn)),
                new LimCFunction("cloneWithoutInit", new LimMethodFunc(LimObject.slotCloneWithoutInit)),
                new LimCFunction("doMessage", new LimMethodFunc(LimObject.slotDoMessage)),
                new LimCFunction("print", new LimMethodFunc(LimObject.slotPrint)),
                new LimCFunction("println", new LimMethodFunc(LimObject.slotPrintln)),
                new LimCFunction("slotNames", new LimMethodFunc(LimObject.slotSlotNames)),
                new LimCFunction("type", new LimMethodFunc(LimObject.slotType)),
                new LimCFunction("evalArg", new LimMethodFunc(LimObject.slotEevalArg)),
                new LimCFunction("evalArgAndReturnSelf", new LimMethodFunc(LimObject.slotEevalArgAndReturnSelf)),
                new LimCFunction("do", new LimMethodFunc(LimObject.slotDo)),
                new LimCFunction("getSlot", new LimMethodFunc(LimObject.slotGetSlot)),
                new LimCFunction("updateSlot", new LimMethodFunc(LimObject.slotUpdateSlot)),
                new LimCFunction("setSlot", new LimMethodFunc(LimObject.slotSetSlot)),
                new LimCFunction("setSlotWithType", new LimMethodFunc(LimObject.slotSetSlotWithType)),
                new LimCFunction("message", new LimMethodFunc(LimObject.slotMessage)),
                new LimCFunction("method", new LimMethodFunc(LimObject.slotMethod)),
                new LimCFunction("block", new LimMethodFunc(LimObject.slotBlock)),
                new LimCFunction("init", new LimMethodFunc(LimObject.slotSelf)),
                new LimCFunction("thisContext", new LimMethodFunc(LimObject.slotSelf)),
                new LimCFunction("thisMessage", new LimMethodFunc(LimObject.slotThisMessage)),
                new LimCFunction("thisLocals", new LimMethodFunc(LimObject.slotThisLocals)),
                new LimCFunction("init", new LimMethodFunc(LimObject.slotSelf)),
                new LimCFunction("if", new LimMethodFunc(LimObject.slotIf)),
                new LimCFunction("yield", new LimMethodFunc(LimObject.slotYield)),
                new LimCFunction("@@", new LimMethodFunc(LimObject.slotAsyncCall)),
                new LimCFunction("yieldingCoros", new LimMethodFunc(LimObject.slotYieldingCoros)),
                new LimCFunction("while", new LimMethodFunc(LimObject.slotWhile))
            };
            LimObject o = state.protoWithInitFunc(name);
            o.addTaglessMethodTable(state, methodTable);
            return o;
        }

        // Published Slots

        public static LimObject slotCompare(LimObject self, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject o = m.localsValueArgAt(locals, 0);
            return LimNumber.newWithDouble(self.state, Convert.ToDouble(self.compare(o)));
        }

        public virtual int compare(LimObject v)
        {
         return uniqueId.CompareTo(v.uniqueId);
        }

        public static LimObject slotEquals(LimObject self, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) == 0 ? self.state.LimTrue : self.state.LimFalse;
        }

        public static LimObject slotNotEquals(LimObject self, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) != 0 ? self.state.LimTrue : self.state.LimFalse;
        }

        public static LimObject slotGreaterThanOrEqual(LimObject self, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) >= 0 ? self.state.LimTrue : self.state.LimFalse;
        }

        public static LimObject slotLessThanOrEqual(LimObject self, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) <= 0 ? self.state.LimTrue : self.state.LimFalse;
        }

        public static LimObject slotLessThan(LimObject self, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) < 0 ? self.state.LimTrue : self.state.LimFalse;
        }

        public static LimObject slotSubstract(LimObject self, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimNumber o = m.localsNumberArgAt(locals, 0);
            return LimNumber.newWithDouble(self.state, - o.asDouble());
        }

        public static LimObject slotGreaterThan(LimObject self, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject o = m.localsValueArgAt(locals, 0);
            return self.compare(o) > 0 ? self.state.LimTrue : self.state.LimFalse;
        }

        public static LimObject slotSelf(LimObject self, LimObject locals, LimObject m)
        {
            return self;
        }

        public static LimObject slotThisMessage(LimObject self, LimObject locals, LimObject m)
        {
            return m;
        }

        public static LimObject slotThisLocals(LimObject self, LimObject locals, LimObject m)
        {
            return locals;
        }


        public static LimObject slotClone(LimObject target, LimObject locals, LimObject m)
        {
            //LimObject newObject = target.tag.cloneFunc(target.state);
            LimObject newObject = target.clone(target.state);
            //newObject.protos.Clear();
            newObject.protos.Add(target);
            return target.initClone(target, locals, m as LimMessage, newObject);
        }

        public static LimObject slotReturn(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject v = m.localsValueArgAt(locals, 0);
            target.state.Return(v);
            return target;
        }

        public static LimObject slotCloneWithoutInit(LimObject target, LimObject locals, LimObject m)
        {
            return target.clone(target.state);
        }

        public static LimObject slotDoMessage(LimObject self, LimObject locals, LimObject m)
        {
            LimMessage msg = m as LimMessage;
            LimMessage aMessage = msg.localsMessageArgAt(locals, 0) as LimMessage;
            LimObject context = self;
            if (msg.args.Count >= 2)
            {
                context = msg.localsValueArgAt(locals, 1);
            }
            return aMessage.localsPerformOn(context, self);
        }

        public static LimObject slotPrint(LimObject target, LimObject locals, LimObject m)
        {
            target.print();
            return target;
        }

        public static LimObject slotPrintln(LimObject target, LimObject locals, LimObject m)
        {
            target.print();
            Console.WriteLine();
            return target;
        }

        public static LimObject slotSlotNames(LimObject target, LimObject locals, LimObject message)
        {
            if (target.slots == null || target.slots.Count == 0) return target;
            foreach (object key in target.slots.Keys)
            {
                Console.Write(key.ToString() + " ");
            }
            Console.WriteLine();
            return target;
        }

        public static LimObject slotType(LimObject target, LimObject locals, LimObject message)
        {
         return LimSeq.createObject(target.state, target.name);
        }

        public static LimObject slotEevalArg(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            return m.localsValueArgAt(locals, 0);
        }

        public static LimObject slotEevalArgAndReturnSelf(LimObject target, LimObject locals, LimObject message)
        {
            LimObject.slotEevalArg(target, locals, message);
            return target;
        }

        public static LimObject slotDo(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            if (m.args.Count != 0)
            {
                LimMessage argMessage = m.rawArgAt(0);
                argMessage.localsPerformOn(target, target);
            }
            return target;
        }

        public static LimObject slotGetSlot(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq slotName = m.localsSymbolArgAt(locals, 0);
            LimObject slot = target.rawGetSlot(slotName);
            return slot == null ? target.state.LimNil : slot;
        }

        public static LimObject slotSetSlot(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq slotName = m.localsSymbolArgAt(locals, 0);
            LimObject slotValue = m.localsValueArgAt(locals, 1);
            if (slotName == null) return target;
            target.slots[slotName] = slotValue;
            return slotValue;
        }

        public static LimObject localsUpdateSlot(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq slotName = m.localsSymbolArgAt(locals, 0);
            if (slotName == null) return target;
            LimObject obj = target.rawGetSlot(slotName);
            if (obj != null)
            {
                LimObject slotValue = m.localsValueArgAt(locals, 1);
                target.slots[slotName] = slotValue;
                return slotValue;
            }
            else
            {
                LimObject theSelf = target.rawGetSlot(target.state.selfMessage.messageName);
                if (theSelf != null)
                {
                    return theSelf.perform(theSelf, locals, m);
                }
            }
            return target.state.LimNil;
        }

        public static LimObject slotUpdateSlot(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq slotName = m.localsSymbolArgAt(locals, 0);
            LimObject slotValue = m.localsValueArgAt(locals, 1);
            if (slotName == null) return target;

            if (target.rawGetSlot(slotName) != null)
            {
                target.slots[slotName] = slotValue;
            }
            else
            {
                Console.WriteLine("Slot {0} not found. Must define slot using := operator before updating.", slotName.value);
            }
            
            return slotValue;
        }

        public static LimObject slotSetSlotWithType(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq slotName = m.localsSymbolArgAt(locals, 0);
            LimObject slotValue = m.localsValueArgAt(locals, 1);
            target.slots[slotName] = slotValue;
            if (slotValue.slots[target.state.typeSymbol] == null)
            {
                slotValue.slots[target.state.typeSymbol] = slotName;
            }
            return slotValue;
        }

        public static LimObject slotMessage(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            return m.args.Count > 0 ? m.rawArgAt(0) : target.state.LimNil;
        }

        public static LimObject slotMethod(LimObject target, LimObject locals, LimObject message)
        {
            return LimBlock.slotMethod(target, locals, message);
        }

        public static LimObject slotBlock(LimObject target, LimObject locals, LimObject message)
        {
            return LimBlock.slotBlock(target, locals, message);
        }

        public static LimObject slotLocalsForward(LimObject target, LimObject locals, LimObject message)
        {
            LimObject o = target.slots[target.state.selfSymbol] as LimObject;
            if (o != null && o != target)
                return target.perform(o, locals, message);
            return target.state.LimNil;
        }

        public static LimObject slotIf(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject r = m.localsValueArgAt(locals, 0);
            bool condition = r != target.state.LimNil && r != target.state.LimFalse;
           int index = condition ? 1 : 2;
           if (index < m.args.Count) 
              return m.localsValueArgAt(locals, index);
            return condition ? target.state.LimTrue : target.state.LimFalse;
        }

        public static LimObject slotAsyncCall(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage msg = message as LimMessage;
            LimMessage aMessage = msg.rawArgAt(0);
            LimObject context = target;
            if (msg.args.Count >= 2)
            {
                context = msg.localsValueArgAt(locals, 1);
            }

            LimBlock o = target.rawGetSlot(aMessage.messageName) as LimBlock;
            if (o != null)
            {
                LimMessage mmm = o.containedMessage;
                mmm.async = true;


                IoContext ctx = new IoContext();
                ctx.target = context;
                ctx.locals = target;
                ctx.message = mmm;
                mmm.async = true;
                LimState state = target.state;
                LimObject future = LimObject.createObject(state);
                IEnumerator e = LimMessage.asyncCall(ctx, future);
                state.contextList.Add(e);
                return future;
            }
            else
            {
                LimCFunction cf = target.rawGetSlot(aMessage.messageName) as LimCFunction;
                if (cf != null)
                {
                    cf.async = true;
                    return cf.activate(target, locals, aMessage, null);
                }
            }
            return aMessage.localsPerformOn(target, locals);
        }

        public static LimObject slotYieldingCoros(LimObject target, LimObject locals, LimObject message) {
            return LimNumber.newWithDouble(target.state, target.state.contextList.Count);
        }

        public static LimObject slotYield(LimObject target, LimObject locals, LimObject message)
        {
            LimState state = target.state;
            ArrayList toDeleteThread = new ArrayList();
            for (int i = 0; i < state.contextList.Count; i++) {
                IEnumerator e  = state.contextList[i] as IEnumerator;
                bool end = e.MoveNext();
                if (!end) toDeleteThread.Add(e);
            }
            foreach (object e in toDeleteThread)
                state.contextList.Remove(e);
            return LimNumber.newWithDouble(state, state.contextList.Count);
        }

        public class EvaluateArgsEventArgs : EventArgs
        {
            public int Position = 0;
            public EvaluateArgsEventArgs(int pos) { Position = pos;  }
        }

        public delegate void EvaluateArgsEventHandler(LimMessage msg, EvaluateArgsEventArgs e, out LimObject res);

        public static IEnumerator slotAsyncWhile(LimObject target, LimObject locals, LimObject message, LimObject future)
        {
            LimMessage m = message as LimMessage;
            LimObject result = target.state.LimNil;
            LimObject cond = null;

            while (true)
            {
                cond = m.localsValueArgAt(locals, 0);
                //evaluateArgs(m, new EvaluateArgsEventArgs(0), out cond);

                if (cond == target.state.LimFalse || cond == target.state.LimNil)
                {
                    break;
                }

                //result = m.localsValueArgAt(locals, 1);
                //evaluateArgs(m, new EvaluateArgsEventArgs(1), out result);

                LimMessage msg = 1 < m.args.Count ? m.args[1] as LimMessage : null;
                if (msg != null)
                {
                    if (msg.cachedResult != null && msg.next == null)
                    {
                        result = msg.cachedResult;
                        yield return result;
                    }
                    //result = localMessage.localsPerformOn(locals, locals);

                    result = target;
                    LimObject cachedTarget = target;
                    LimObject savedPrevResultAsYieldResult = null;

                    do
                    {
                        if (msg.messageName.Equals(msg.state.semicolonSymbol))
                        {
                            target = cachedTarget;
                        }
                        else
                        {
                            result = msg.cachedResult;
                            if (result == null)
                            {
                                if (msg.messageName.value.Equals("yield"))
                                {
                                    yield return result;
                                }
                                else
                                {
                                    result = target.perform(target, locals, msg);
                                }
                            }
                            if (result == null)
                            {
                                target = cachedTarget;
                                //result = savedPrevResultAsYieldResult;
                            }
                            else
                            {
                                target = result;
                            }
                            savedPrevResultAsYieldResult = result;
                        }
                    } while ((msg = msg.next) != null);
                    future.slots["future"] = result;

                    yield return null;
                }

                result = m.state.LimNil;

                if (target.state.handleStatus() != 0)
                {
                    goto done;
                }

            }
        done:
            yield return null;
        }

        public static LimObject slotWhile(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;

            if (m.async)
            {
                LimState state = target.state;
                LimObject future = LimObject.createObject(state);
                IEnumerator e = LimObject.slotAsyncWhile(target, locals, message, future);
                state.contextList.Add(e);
                return future;
            }

            LimObject result = target.state.LimNil;
            while (true)
            {
                bool sasync = m.async;
                m.async = false;
                LimObject cond = m.localsValueArgAt(locals, 0);
                if (cond == target.state.LimFalse || cond == target.state.LimNil)
                {
                    break;
                }
                m.async = sasync;
                result = m.localsValueArgAt(locals, 1);
                if (target.state.handleStatus() != 0)
                {
                    goto done;
                }

            }
done:
            return result;
        }

        // Object Public Raw Methods
        
        public LimObject initClone(LimObject target, LimObject locals, LimMessage m, LimObject newObject)
        {
            LimObject context = null;
            LimObject initSlot = target.rawGetSlotContext(target.state.initMessage.messageName, out context);
            if (initSlot != null)
               initSlot.activate(initSlot, newObject, locals, target.state.initMessage, context);
            return newObject;
        }

        public void addTaglessMethodTable(LimState state, LimCFunction[] table)
        {
            //foreach (IoMethodTableEntry entry in table)
            //    slots[entry.name] = new LimCFunction(state, entry.name, entry.func);
            foreach (LimCFunction entry in table)
            {
                entry.state = state;
                slots[entry.funcName] = entry;
            }
        }

        public LimObject forward(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimObject context = null;
            LimObject forwardSlot = target.rawGetSlotContext(target.state.forwardMessage.messageName, out context);
            
            //if (forwardSlot != null)
            //    return forwardSlot.activate(forwardSlot, locals, m, context);

            Console.WriteLine("'{0}' does not respond to message '{1}'",
                target.name, m.messageName.ToString());
            return target;
        }

       public LimObject perform(LimObject target, LimObject locals, LimObject message)
       {
         LimMessage msg = message as LimMessage;
            LimObject context = null;
            LimObject slotValue = target.rawGetSlotContext(msg.messageName, out context);
            
            if (slotValue == null)
                slotValue = target.clrGetSlot(msg);
         
            if (slotValue != null)
         {
                return slotValue.activate(slotValue, target, locals, msg, context);
         }
         if (target.isLocals)
         {
            return LimObject.slotLocalsForward(target, locals, message);
         }
         return target.forward(target, locals, message);
       }

        public LimObject localsProto(LimState state)
        {
            
            LimObject obj = LimObject.createObject(state);
            LimObject firstProto = obj.protos[0] as LimObject;
            foreach (object key in firstProto.slots.Keys)
                obj.slots[key] = firstProto.slots[key];
            firstProto.protos.Clear();
            obj.slots["setSlot"] = new LimCFunction(state, "setSlot", new LimMethodFunc(LimObject.slotSetSlot));
            obj.slots["setSlotWithType"] = new LimCFunction(state, "setSlotWithType", new LimMethodFunc(LimObject.slotSetSlotWithType));
            obj.slots["updateSlot"] = new LimCFunction(state, "updateSlot", new LimMethodFunc(LimObject.localsUpdateSlot));
            obj.slots["thisLocalContext"] = new LimCFunction(state, "thisLocalContext", new LimMethodFunc(LimObject.slotThisLocals));
            obj.slots["forward"] = new LimCFunction(state, "forward", new LimMethodFunc(LimObject.slotLocalsForward));
            return obj;
        }

      
        public virtual LimObject activate(LimObject self, LimObject target, LimObject locals, LimMessage m, LimObject slotContext)
        {
         return self.isActivatable ? self.activate(self, target, locals, m) : self;
        }
      
        public LimObject activate(LimObject self, LimObject target, LimObject locals, LimMessage m)
        {
            if (self.isActivatable)
            {
                LimObject context = null;
                LimObject slotValue = self.rawGetSlotContext(self.state.activateMessage.messageName, out context);
                if (slotValue != null)
                {
               // ?? мы шо в цикле ???
               return activate(slotValue, target, locals, m, context); 
                }
            return state.LimNil;
            } else
            return self;
        }

        public void createSlots()
        {
            if (slots == null)
                slots = new LimSeqObjectHashtable(state);
         if (state == null)
         {
            int x = 0;
         }
        }

        public void createProtos()
        {
            if (protos == null)
                protos = new LimObjectArrayList();
        }

      public LimObject slotsBySymbol(LimSeq symbol)
      {
            LimSeq s = this.state.symbols[symbol.value] as LimSeq;
            if (s == null) return null;
            return slots[s] as LimObject;
      }

        public LimObject rawGetSlot(LimSeq slot)
        {
            LimObject context = null;
            LimObject v = rawGetSlotContext(slot, out context);
            return v;
        }

        public LimObject clrGetSlot(LimMessage message)
        {
            LimObject v = null;
            if (this is LimClrObject)
            {
                v = (this as LimClrObject).getMethod(message);
            }
            if (v == null)
                v = this.state.clrProto.getType(this.state, message.messageName.value);
            return v;
        }

        public LimObject rawGetSlotContext(LimSeq slot, out LimObject context)
        {
            if (slot == null)
            {
                context = null;
                return null;
            }
            LimObject v = null;
            context = null;
            if (slotsBySymbol(slot) != null)
            {
                v = slotsBySymbol(slot) as LimObject;
                if (v != null)
                {
                    context = this;
                    return v;
                }
            }
            hasDoneLookup = true;
            foreach (LimObject proto in protos)
            {
                if (proto.hasDoneLookup)
                    continue;
                v = proto.rawGetSlotContext(slot, out context);
                if (v != null) break;
            }
            hasDoneLookup = false;

            return v;
        }

      public virtual void print()
      {
            //LimSeq type = this.slots["type"] as LimSeq;
         //if (type == null)
         //		type = (this.rawGetSlot(state.typeMessage.messageName) as LimCFunction).func(this, this, this) as LimSeq;
            //string printedName = type == null ? ToString() : type.value;
         Console.Write(this);
      }

      public override string ToString()
      {
         return name + "+" + uniqueId;
      }
   }

}
