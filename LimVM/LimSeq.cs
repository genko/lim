    public class LimSeq : LimObject
    {
        public override string getName() { return "Sequence"; }
        public string value = System.String.Empty;

        public char[] asCharArray { get { return value.ToCharArray(); } }

		public new static LimSeq createProto(LimState state)
		{
			LimSeq s = new LimSeq();
			return s.proto(state) as LimSeq;
		}

        public new static LimSeq createObject(LimState state)
        {
            LimSeq s = new LimSeq();
            return s.clone(state) as LimSeq;
        }

        public static LimSeq createObject(LimSeq symbol)
        {
            LimSeq seq = new LimSeq();
            seq = seq.clone(symbol.getState()) as LimSeq;
            seq.value = symbol.value;
            return seq;
        }

        public static LimSeq createObject(LimState state, string symbol)
        {
            LimSeq seq = new LimSeq();
            seq = seq.clone(state) as LimSeq;
            seq.value = symbol;
            return seq;
        }

        public static LimSeq createSymbolInMachine(LimState state, string symbol)
        {
            if (state.symbols[symbol] == null)
                state.symbols[symbol] = LimSeq.createObject(state, symbol);
            return state.symbols[symbol] as LimSeq;
        }

        public override LimObject proto(LimState state)
		{
			LimSeq pro = new LimSeq();
            pro.setState(state);
		//	pro.tag.cloneFunc = new IoTagCloneFunc(this.clone);
        //    pro.tag.compareFunc = new IoTagCompareFunc(this.compare);
            pro.createSlots();
            pro.createProtos();
            state.registerProtoWithFunc(getName(), new LimStateProto(getName(), pro, new LimStateProtoFunc(this.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("appendSeq", new LimMethodFunc(LimSeq.slotAppendSeq)),
                new LimCFunction("at", new LimMethodFunc(LimSeq.slotAt)),
                new LimCFunction("reverse", new LimMethodFunc(LimSeq.slotReverse)),
                new LimCFunction("size", new LimMethodFunc(LimSeq.size)),
            };

			pro.addTaglessMethodTable(state, methodTable);
			return pro;
		}

        public static LimObject slotAppendSeq(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq o = target as LimSeq;
            LimSeq arg = m.localsSymbolArgAt(locals, 0);
            o.value += arg.value.Replace(@"\""", "\"");
            return o;
        }

        public static LimObject slotAt(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq o = target as LimSeq;
            LimSeq res = LimSeq.createObject(target.getState());
            LimNumber arg = m.localsNumberArgAt(locals, 0);
            res.value += o.value.Substring(arg.asInt(),1);
            return res;
        }

        public static LimObject slotReverse(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq o = target as LimSeq;
            LimSeq res = LimSeq.createObject(target.getState());
            char[] A = o.asCharArray;
            System.Array.Reverse(A);
            res.value = new string(A);
            return res;
        }

        public static LimObject size(LimObject target, LimObject locals, LimObject message)
        {
            LimMessage m = message as LimMessage;
            LimSeq o = target as LimSeq;
            return LimNumber.newWithDouble(target.getState(), o.value.Length);
        }


        public override LimObject clone(LimState state)
		{
			LimSeq proto = state.protoWithInitFunc(getName()) as LimSeq;
			LimSeq result = new LimSeq();
			result.setState(state);
            result.value = proto.value;
			result.createProtos();
			result.createSlots();
			result.protos.Add(proto);
			return result;
		}

        public override int compare(LimObject v)
        {
			if (v is LimSeq) return this.value.CompareTo((v as LimSeq).value);
            return base.compare(v);
        }

        public override void print()
        {
            System.Console.Write("{0}", value);
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public static LimSeq rawAsUnquotedSymbol(LimSeq s)
        {
            string str = "";
            if (s.value.StartsWith("\"")) str = s.value.Substring(1, s.value.Length - 1);
            if (s.value.EndsWith("\"")) str = str.Substring(0,s.value.Length-2);
            return LimSeq.createObject(s.getState(), str);
        }

        public static LimSeq rawAsUnescapedSymbol(LimSeq s)
        {
            string str = "";
            int i = 0;
            while (i < s.value.Length)
            {
                char c = s.value[i];
                if (c != '\\')
                {
                    str += c;
                }
                else
                {
                    c = s.value[i];
                    switch (c)
                    {
                        case 'a': c = '\a'; break;
                        case 'b': c = '\b'; break;
                        case 'f': c = '\f'; break;
                        case 'n': c = '\n'; break;
                        case 'r': c = '\r'; break;
                        case 't': c = '\t'; break;
                        case 'v': c = '\v'; break;
                        case '\0': c = '\\'; break;
                        default:
                            if (c > '0' && c < '9')
                            {
                                c -= '0';
                            }
                            break;
                    }
                    str += c;
                }

                i++;
            }
            return LimSeq.createObject(s.getState(), str);
        }
    }