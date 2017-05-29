public class LimMessage : LimObject
{
    public override string getName() { return "Message"; }
    public LimSeq messageName;
    public LimObjectArrayList args;
    public LimMessage next;
    public LimObject cachedResult;
    public int lineNumber;
    public LimSeq label;

    public static LimMessage createProto(LimState state)
    {
        LimMessage m = new LimMessage();
        return m.proto(state) as LimMessage;
    }

    public static LimMessage createObject(LimState state)
    {
        LimMessage pro = new LimMessage();
        return pro.clone(state) as LimMessage;
    }

    public override LimObject proto(LimState state)
    {
        LimMessage pro = new LimMessage();
        pro.setState(state);
        pro.createSlots();
        pro.createProtos();
        pro.uniqueId = 0;
        pro.messageName = LimSeq.createSymbolInMachine(state, "anonymous");
        pro.label = LimSeq.createSymbolInMachine(state, "unlabeled");
        pro.args = new LimObjectArrayList();
        state.registerProtoWithFunc(getName(), new LimStateProto(getName(), pro, new LimStateProtoFunc(this.proto)));
        pro.protos.Add(state.protoWithInitFunc("Object"));

        LimCFunction[] methodTable = new LimCFunction[] {
                new LimCFunction("name", new LimMethodFunc(LimMessage.slotName)),
                new LimCFunction("setName", new LimMethodFunc(LimMessage.slotSetName)),
                new LimCFunction("next", new LimMethodFunc(LimMessage.slotNext)),
                new LimCFunction("setNext", new LimMethodFunc(LimMessage.slotSetNext)),
                new LimCFunction("code", new LimMethodFunc(LimMessage.slotCode)),
                new LimCFunction("arguments", new LimMethodFunc(LimMessage.slotArguments)),
                new LimCFunction("appendArg", new LimMethodFunc(LimMessage.slotAppendArg)),
                new LimCFunction("argAt", new LimMethodFunc(LimMessage.slotArgAt)),
                new LimCFunction("argCount", new LimMethodFunc(LimMessage.slotArgCount)),
                new LimCFunction("asString", new LimMethodFunc(LimMessage.slotCode)),
                new LimCFunction("cachedResult", new LimMethodFunc(LimMessage.slotCachedResult)),
                new LimCFunction("setCachedResult", new LimMethodFunc(LimMessage.slotSetCachedResult)),
                new LimCFunction("removeCachedResult", new LimMethodFunc(LimMessage.slotRemoveCachedResult)),
                new LimCFunction("hasCachedResult", new LimMethodFunc(LimMessage.slotHasCachedResult)),

            };

        pro.addTaglessMethodTable(state, methodTable);
        return pro;
    }

    public override void cloneSpecific(LimObject _from, LimObject _to)
    {
        LimMessage from = _from as LimMessage;
        LimMessage to = _to as LimMessage;
        to.messageName = from.messageName;
        to.label = from.label;
        to.args = new LimObjectArrayList();
    }

    // Published Slots

    public static LimObject slotName(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        return self.messageName;
    }

    public static LimObject slotSetName(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        LimMessage msg = message as LimMessage;
        LimSeq s = msg.localsSymbolArgAt(locals, 0);
        self.messageName = s;
        return self;
    }

    public static LimObject slotNext(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        return self.next == null ? target.getState().LimNil : self.next;
    }

    public static LimObject slotSetNext(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        LimMessage msg = message as LimMessage;
        LimObject m = msg.localsMessageArgAt(locals, 0) as LimObject;
        LimMessage mmm = null;
        if (m == target.getState().LimNil)
        {
            mmm = null;
        }
        else if (m.getName().Equals("Message"))
        {
            mmm = m as LimMessage;
        }
        else
        {
            System.Console.WriteLine("argument must be Message or Nil");
        }
        self.next = mmm;
        return self;
    }

    public static LimObject slotCode(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        string s = "";
        s = self.descriptionToFollow(true);
        return LimSeq.createObject(self.getState(), s);
    }

    public static LimObject slotArguments(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        LimList list = LimList.createObject(target.getState());
        foreach (LimObject o in self.args.getIter())
        {
            list.append(o);
        }
        return list;
    }

    public static LimObject slotAppendArg(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        LimMessage msg = message as LimMessage;
        LimMessage m = msg.localsMessageArgAt(locals, 0) as LimMessage;
        self.args.Add(m);
        return self;
    }

    public static LimObject slotArgCount(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        return LimNumber.newWithDouble(target.getState(), System.Convert.ToDouble(self.args.Count));
    }

    public static LimObject slotArgAt(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        LimMessage m = message as LimMessage;
        int index = m.localsNumberArgAt(locals, 0).asInt();
        LimObject v = self.args.Get(index) as LimObject;
        return v != null ? v : self.getState().LimNil;
    }

    public static LimObject slotCachedResult(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage m = target as LimMessage;
        return m.cachedResult == null ? target.getState().LimNil : m.cachedResult;
    }

    public static LimObject slotSetCachedResult(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        LimMessage msg = message as LimMessage;
        self.cachedResult = msg.localsValueArgAt(locals, 0);
        return self;
    }

    public static LimObject slotRemoveCachedResult(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        self.cachedResult = null;
        return self;
    }

    public static LimObject slotHasCachedResult(LimObject target, LimObject locals, LimObject message)
    {
        LimMessage self = target as LimMessage;
        return self.cachedResult == null ? target.getState().LimFalse : target.getState().LimTrue;
    }

    // Message Public Raw Methods

    public static LimMessage newWithName(LimState state, LimSeq limSymbol)
    {
        LimMessage msg = LimMessage.createObject(state);
        msg.messageName = limSymbol;
        return msg;
    }

    public LimMessage newFromTextLabel(LimState state, string code, string label)
    {
        LimSeq labelSymbol = LimSeq.createSymbolInMachine(state, label);
        return newFromTextLabelSymbol(state, code, labelSymbol);
    }

    public string descriptionToFollow(bool follow)
    {
        LimMessage m = this;
        string s = "";
        do
        {
            s += m.messageName;

            if (m.args.Count > 0)
            {
                s += "(";

                for (int i = 0; i < m.args.Count; i++)
                {
                    LimMessage arg = m.args.Get(i) as LimMessage;
                    s += arg.descriptionToFollow(true);
                    if (i != m.args.Count - 1)
                    {
                        s += ", ";
                    }
                }

                s += ")";
            }

            if (!follow)
            {
                return s;
            }

            if (m.next != null && !m.messageName.value.Equals(";"))
            {
                s += " ";
            }
            if (m.messageName.value.Equals(";"))
            {
                s += "\n";
            }

        } while ((m = m.next) != null);

        return s;
    }

    public LimObject localsPerformOn(LimObject target, LimObject locals)
    {
        LimObject result = target;
        LimObject cachedTarget = target;
        LimMessage msg = this;
        do
        {
            if (msg.messageName.Equals(msg.getState().semicolonSymbol))
            {
                target = cachedTarget;
            }
            else
            {
                result = msg.cachedResult;
                if (result == null)
                {
                    result = target.perform(target, locals, msg);
                }
                if (result == null)
                {
                    System.Console.WriteLine("Message chains intermediate mustn't be null");
                }
                target = result;
            }
        } while ((msg = msg.next) != null);
        return result;
    }

    public override string ToString()
    {
        return messageName.ToString();// +args.ToString();
    }

    public override void print()
    {
        string code = this.descriptionToFollow(true);
        System.Console.Write(code);
    }

    public LimMessage rawArgAt(int p)
    {
        LimMessage argIsMessage = args.Get(p) as LimMessage;
        return argIsMessage;
    }

    public LimObject localsValueArgAt(LimObject locals, int i)
    {
        LimMessage m = i < args.Count ? args.Get(i) as LimMessage : null;
        if (m != null)
        {
            if (m.cachedResult != null && m.next == null)
            {
                return m.cachedResult;
            }

            return m.localsPerformOn(locals, locals);
        }
        return this.getState().LimNil;
    }

    public LimSeq localsSymbolArgAt(LimObject locals, int i)
    {
        LimObject o = localsValueArgAt(locals, i);
        if (!o.getName().Equals("Sequence"))
        {
            localsNumberArgAtErrorForType(locals, i, "Sequence");

        }
        return o as LimSeq;
    }

    public LimObject localsMessageArgAt(LimObject locals, int n)
    {
        LimObject v = localsValueArgAt(locals, n);
        if (!v.getName().Equals("Message") && v != getState().LimNil)
        {
            localsNumberArgAtErrorForType(locals, n, "Message");

        }
        return v;
    }

    public LimNumber localsNumberArgAt(LimObject locals, int i)
    {
        LimObject o = localsValueArgAt(locals, i);
        if (o == null || !o.getName().Equals("Number"))
        {
            localsNumberArgAtErrorForType(locals, i, "Number");

        }
        return o as LimNumber;
    }

    // Private Methods

    void localsNumberArgAtErrorForType(LimObject locals, int i, string p)
    {
        LimObject v = localsValueArgAt(locals, i);
        System.Console.WriteLine("argument {0} to method '{1}' must be a {2}, not a '{3}'",
            i, this.messageName, p, v.getName());
    }

    LimMessage newParse(LimState state, LimLexer lexer)
    {
        if (lexer.errorToken != null)
        {
        }

        if (lexer.topType() == LimTokenType.TERMINATOR_TOKEN)
        {
            lexer.pop();
        }

        if (lexer.top() != null && lexer.top().isValidMessageName())
        {
            LimMessage self = newParseNextMessageChain(state, lexer);
            if (lexer.topType() != LimTokenType.NO_TOKEN)
            {
                state.error(self, "compile error: %s", "unused tokens");
            }
            return self;
        }

        return newWithNameReturnsValue(state, LimSeq.createSymbolInMachine(state, "nil"), state.LimNil);

    }

    LimMessage newWithNameReturnsValue(LimState state, LimSeq symbol, LimObject v)
    {
        LimMessage self = clone(state) as LimMessage;
        self.messageName = symbol;
        self.cachedResult = v;
        return self;
    }

    LimMessage newParseNextMessageChain(LimState state, LimLexer lexer)
    {
        LimMessage msg = clone(state) as LimMessage;

        if (lexer.top() != null && lexer.top().isValidMessageName())
        {
            msg.parseName(state, lexer);
        }

        if (lexer.topType() == LimTokenType.OPENPAREN_TOKEN)
        {
            msg.parseArgs(lexer);
        }

        if (lexer.top() != null && lexer.top().isValidMessageName())
        {
            msg.parseNext(lexer);
        }

        while (lexer.topType() == LimTokenType.TERMINATOR_TOKEN)
        {
            lexer.pop();

            if (lexer.top() != null && lexer.top().isValidMessageName())
            {
                LimMessage eol = LimMessage.newWithName(state, state.semicolonSymbol);
                msg.next = eol;
                eol.parseNext(lexer);
            }
        }

        return msg;
    }

    void parseName(LimState state, LimLexer lexer)
    {
        LimToken token = lexer.pop();
        messageName = LimSeq.createSymbolInMachine(state, token.name);
        ifPossibleCacheToken(token);
        //rawSetLineNumber(token.lineNumber);
        //rawSetCharNumber(token.charNumber);
    }

    void ifPossibleCacheToken(LimToken token)
    {
        LimSeq method = this.messageName;
        LimObject r = null;
        switch (token.type)
        {
            case LimTokenType.TRIQUOTE_TOKEN:
                break;
            case LimTokenType.MONOQUOTE_TOKEN:
                r = LimSeq.createSymbolInMachine(
                        method.getState(),
                        LimSeq.rawAsUnescapedSymbol(
                            LimSeq.rawAsUnquotedSymbol(
                                LimSeq.createObject(method.getState(), method.value)
                            )
                        ).value
                    );
                break;
            case LimTokenType.NUMBER_TOKEN:
                r = LimNumber.newWithDouble(this.getState(), System.Convert.ToDouble(method.value));
                break;
            default:
                if (method.value.Equals("nil"))
                {
                    r = getState().LimNil;
                }
                else if (method.value.Equals("true"))
                {
                    r = getState().LimTrue;
                }
                else if (method.value.Equals("false"))
                {
                    r = getState().LimFalse;
                }
                break;


        }
        this.cachedResult = r;
    }

    void parseNext(LimLexer lexer)
    {
        LimMessage nextMessage = newParseNextMessageChain(this.getState(), lexer);
        this.next = nextMessage;
    }

    void parseArgs(LimLexer lexer)
    {
        lexer.pop();

        if (lexer.top() != null && lexer.top().isValidMessageName())
        {
            LimMessage arg = newParseNextMessageChain(this.getState(), lexer);
            addArg(arg);

            while (lexer.topType() == LimTokenType.COMMA_TOKEN)
            {
                lexer.pop();

                if (lexer.top() != null && lexer.top().isValidMessageName())
                {
                    LimMessage arg2 = newParseNextMessageChain(this.getState(), lexer);
                    addArg(arg2);
                }
                else
                {
                }
            }
        }

        if (lexer.topType() != LimTokenType.CLOSEPAREN_TOKEN)
        {
            // TODO: Exception, missing close paren
        }
        lexer.pop();
    }

    void addArg(LimMessage arg)
    {
        args.Add(arg);
    }

    LimMessage newFromTextLabelSymbol(LimState state, string code, LimSeq labelSymbol)
    {
        LimLexer lexer = new LimLexer();
        LimMessage msg = new LimMessage();
        msg = msg.clone(state) as LimMessage;
        lexer.s = code;
        lexer.lex();
        msg = this.newParse(state, lexer);
        msg.opShuffle();
        msg.label = labelSymbol;
        return msg;
    }

    LimObject opShuffle()
    {
        LimObject context = null;
        LimObject m = this.rawGetSlotContext(getState().opShuffleMessage.messageName, out context);
        if (m != null)
        {
            getState().opShuffleMessage.localsPerformOn(this, getState().lobby);
        }
        return this;
    }

}