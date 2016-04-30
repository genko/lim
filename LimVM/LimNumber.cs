using System;

    public class LimNumber : LimObject
    {
 		public override string name { get { return "Number"; } }
        public object value
        {
            get
            {
                if (isInteger)
                {
                    return longValue;
                }
                else
                {
                    return doubleValue;
                }
            }
        }
        public int accuracy = 0;
        public double doubleValue;
        public bool isInteger = true;
        public int longValue;

		public new static LimNumber createProto(LimState state)
		{
			LimNumber number = new LimNumber();
			return number.proto(state) as LimNumber;
		}

		public override LimObject proto(LimState state)
		{
			LimNumber pro = new LimNumber();
			pro.state = state;
            pro.createSlots();
            pro.createProtos();
            pro.doubleValue = 0;
            pro.longValue = 0;
            pro.isInteger = true;
            state.registerProtoWithFunc(name, new LimStateProto(pro.name, pro, new LimStateProtoFunc(pro.proto)));
			pro.protos.Add(state.protoWithInitFunc("Object"));

            LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("asNumber", new LimMethodFunc(LimNumber.slotAsNumber)),
                new LimCFunction("+", new LimMethodFunc(LimNumber.slotAdd)),
                new LimCFunction("-", new LimMethodFunc(LimNumber.slotSubstract)),
                new LimCFunction("*", new LimMethodFunc(LimNumber.slotMultiply)),                  
                new LimCFunction("/", new LimMethodFunc(LimNumber.slotDivide)),
                new LimCFunction("log10", new LimMethodFunc(LimNumber.slotLog10)),
                new LimCFunction("log2", new LimMethodFunc(LimNumber.slotLog2)),
                new LimCFunction("log", new LimMethodFunc(LimNumber.slotLog)),
                new LimCFunction("pow", new LimMethodFunc(LimNumber.slotPow)),
                new LimCFunction("pi", new LimMethodFunc(LimNumber.slotPi)),
                new LimCFunction("e", new LimMethodFunc(LimNumber.slotE)),
                new LimCFunction("minPositive", new LimMethodFunc(LimNumber.slotMinPositive)),
                new LimCFunction("exp", new LimMethodFunc(LimNumber.slotExp)),
                new LimCFunction("round", new LimMethodFunc(LimNumber.slotRound)),
//                new LimCFunction("asString", new LimMethodFunc(this.asString))
            };

			pro.addTaglessMethodTable(state, methodTable);
			return pro;
		}

        public static LimNumber newWithDouble(LimState state, double n)
        {
			LimNumber fab = new LimNumber();
			LimNumber num = state.protoWithInitFunc(fab.name) as LimNumber;
            num = num.clone(state) as LimNumber;
            num.isInteger = false;
            num.doubleValue = n;

            if (Double.Equals(n, 0) ||
                (!Double.IsInfinity(n) && !Double.IsNaN(n) &&
                !n.ToString().Contains(".") &&
                !n.ToString().Contains("E") &&
                !n.ToString().Contains("e")
                )
            )
            {
                try
                {
                    num.longValue = Convert.ToInt32(n);
                    num.isInteger = true;
                }
                catch (OverflowException)
                {

                }
            }
			return num;
        }

		public override int GetHashCode()
		{
			return Convert.ToInt32(uniqueIdCounter);
		}

		public override string ToString()
		{
			return isInteger ? longValue.ToString()
                : doubleValue.ToString("G");
		}

        public override int compare(LimObject v)
        {
            LimNumber o = this as LimNumber;
            if (v is LimNumber)
            {
                if (Convert.ToDouble((v as LimNumber).value) == Convert.ToDouble(o.value))
                {
                    return 0;
                }
                double d = (v as LimNumber).isInteger ? (v as LimNumber).longValue : (v as LimNumber).doubleValue;
                double thisValue = o.isInteger ? o.longValue : o.doubleValue;

                return thisValue < d ? -1 : 1;
            }
            return base.compare(v);
        }

        public long asLong()
        {
            return Convert.ToInt64(value);
        }

        public int asInt()
        {
            return Convert.ToInt32(value);
        }

        public float asFloat()
        {
            return Convert.ToSingle(value);
        }

        public double asDouble()
        {
            return Convert.ToDouble(value);
        }

        public override void print()
        {
            Console.Write("{0}", this.ToString());
        }

        public static LimObject slotAsNumber(LimObject target, LimObject locals, LimObject message)
        {
            return target;
        }

        public static LimObject slotAdd(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber other = (message as LimMessage).localsNumberArgAt(locals, 0);
            LimNumber self = target as LimNumber;
			if (other == null) return self;
            return LimNumber.newWithDouble(target.state,
                (self.isInteger ? self.longValue : self.doubleValue) +
                (other.isInteger ? other.longValue : other.doubleValue)
                    );
        }

        public new static LimObject slotSubstract(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber other = (message as LimMessage).localsNumberArgAt(locals, 0);
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                (self.isInteger ? self.longValue : self.doubleValue) -
                (other.isInteger ? other.longValue : other.doubleValue) 
                );
        }

        public static LimObject slotMultiply(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber other = (message as LimMessage).localsNumberArgAt(locals, 0);
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                (self.isInteger ? self.longValue : self.doubleValue) *
                (other.isInteger ? other.longValue : other.doubleValue) 
                );
        }

        public static LimObject slotDivide(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber other = (message as LimMessage).localsNumberArgAt(locals, 0);
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                (self.isInteger ? self.longValue : self.doubleValue) /
                (other.isInteger ? other.longValue : other.doubleValue) 
                );
        }

        public static LimObject slotLog10(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                Math.Log10(self.isInteger ? self.longValue : self.doubleValue)
                );
        }

        public static LimObject slotLog2(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                Math.Log(self.isInteger ? self.longValue : self.doubleValue, 2)
                );
        }

        public static LimObject slotPi(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state, Math.PI);
        }

        public static LimObject slotMinPositive(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state, Double.Epsilon);
        }

        public static LimObject slotE(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state, Math.E);
        }

        public static LimObject slotLog(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber other = (message as LimMessage).localsNumberArgAt(locals, 0);
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                Math.Log(self.isInteger ? self.longValue : self.doubleValue,
                other.isInteger ? other.longValue : other.doubleValue)
                );
        }

        public static LimObject slotPow(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber other = (message as LimMessage).localsNumberArgAt(locals, 0);
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                Math.Pow(self.isInteger ? self.longValue : self.doubleValue,
                other.isInteger ? other.longValue : other.doubleValue)
                );
        }

        public static LimObject slotExp(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                Math.Exp(self.isInteger ? self.longValue : self.doubleValue)
                );
        }

        public static LimObject slotRound(LimObject target, LimObject locals, LimObject message)
        {
            LimNumber self = target as LimNumber;
            return LimNumber.newWithDouble(target.state,
                Math.Round(self.isInteger ? self.longValue : self.doubleValue)
                );
        }

    }