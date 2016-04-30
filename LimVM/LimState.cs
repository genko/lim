
using System;
using System.Collections;
using System.IO;

namespace lim {

    public class IoContext
    {
        public LimObject target;
        public LimObject locals;
        public LimMessage message;
        public LimObject slotContext;
    }

    public class LimState {

        public ArrayList contextList = new ArrayList();

		public Hashtable primitives = new Hashtable(); // keys are raw strings
        public Hashtable symbols = new Hashtable(); // keys are raw strings

		// coroutines
		public LimObject objectProto;
        // public IoCoroutine mainCoroutine;    // the object that represents the main "thread"
        // public IoCoroutine currentCoroutine; // the object whose coroutine is active
		public Stack currentIoStack;      // quick access to current coro's retain stack
        public LimClr clrProto;

		// quick access objects
		public LimSeq activateSymbol;
        public LimSeq callSymbol;
        public LimSeq forwardSymbol;
        public LimSeq noShufflingSymbol;
		public LimSeq opShuffleSymbol;
		public LimSeq semicolonSymbol;
		public LimSeq selfSymbol;
		public LimSeq setSlotSymbol;
		public LimSeq setSlotWithTypeSymbol;
		public LimSeq stackSizeSymbol;
		public LimSeq typeSymbol;

		public LimSeq updateSlotSymbol;
		public LimObject setSlotBlock;
		public LimObject localsUpdateSlotCFunc;
		public LimObject localsProto;

		public LimMessage asStringMessage;
		public LimMessage collectedLinkMessage;
		public LimMessage compareMessage;
		public LimMessage initMessage;
        public LimMessage selfMessage;
		public LimMessage mainMessage;
		public LimMessage nilMessage;
        public LimMessage forwardMessage;
        public LimMessage activateMessage;
        public LimMessage opShuffleMessage;
		public LimMessage printMessage;
		public LimMessage referenceIdForObjectMessage;
		public LimMessage objectForReferenceIdMessage;
		public LimMessage runMessage;
		public LimMessage willFreeMessage;
		public LimMessage yieldMessage;
		public LimMessage typeMessage;
		
		public LimObjectArrayList cachedNumbers;

		// singletons
		public LimObject LimNil;
		public LimObject LimTrue;
		public LimObject LimFalse;

		// Flow control singletons
		public LimObject LimNormal;
		public LimObject LimBreak;
		public LimObject LimContinue;
		public LimObject LimReturn;
		public LimObject LimEol;

		// execution context
		public LimObject lobby;
		public LimObject core;

		// current execution state
        public IoStopStatus stopStatus;
		public object returnValue;

		// debugger
		public int debugOn;
		public LimObject debugger;
		public LimMessage vmWillSendMessage;

		// SandBox limits
		public int messageCountLimit;
		public int messageCount;
		public double timeLimit;
		public double endTime;

		// tail calls
		public LimMessage tailCallMessage;

		// exiting
		public int shouldExit;
		public int exitResult;

        public LimSeq IOSYMBOL(string name)
        {
            return LimSeq.createSymbolInMachine(this, name);
        }

		public void registerProtoWithFunc(string name, LimStateProto stateProto)
		{
			primitives[name] = stateProto;
		}

		public LimObject protoWithInitFunc(string name)
		{
			LimStateProto stateProto = primitives[name] as LimStateProto;
			return stateProto.proto;
		}

        public void error(LimMessage m, string s)
        {
        }


		public LimState()
		{
			objectProto = LimObject.createProto(this);
			core = objectProto.clone(this);
			lobby = objectProto.clone(this);

            LimSeq seqProto = LimSeq.createProto(this);

            setupSingletons();
            setupSymbols();

            objectProto.protoFinish(this);

			LimMessage messageProto = LimMessage.createProto(this);

            nilMessage = LimMessage.createObject(this) as LimMessage;
            nilMessage.cachedResult = LimNil;
            nilMessage.messageName = IOSYMBOL("nil");

            LimMap mapProto = LimMap.createProto(this);
			LimNumber numProto = LimNumber.createProto(this);
		    LimConsole consProto = LimConsole.createProto(this);
            LimSystem sysProto = LimSystem.createProto(this);
			LimCFunction cfProto = LimCFunction.createProto(this);
            LimBlock blockProto = LimBlock.createProto(this);
            LimCall callProto = LimCall.createProto(this);
            LimList listProto = LimList.createProto(this);
            clrProto = LimClr.createProto(this);
            LimClrAssembly asmProto = LimClrAssembly.createProto(this);
            LimClrObject clrObjProto = LimClrObject.createProto(this);

            LimObject protos = objectProto.clone(this);
			protos.slots["Core"] = core;
			protos.slots["Addons"] = null;

			lobby.slots["Lobby"] = lobby;
			lobby.slots["Protos"] = protos;

			core.slots["Object"] = objectProto;
			core.slots["Map"] = mapProto;
			core.slots["Message"] = messageProto;
			core.slots["CFunction"] = cfProto;
			core.slots["Number"] = numProto;
		    core.slots["Console"] = consProto;
		    core.slots["System"] = sysProto;
            core.slots["Block"] = blockProto;
            core.slots["Call"] = callProto;
            core.slots["Locals"] = localsProto = objectProto.localsProto(this);
            core.slots["List"] = listProto;
            core.slots["Sequence"] = seqProto;
            core.slots["CLR"] = clrProto;
            core.slots["CLRAssembly"] = asmProto;
            core.slots["CLRObject"] = clrObjProto;
			
			objectProto.protos.Add(lobby);
            lobby.protos.Add(protos);
            protos.protos.Add(core);

            localsUpdateSlotCFunc = new LimCFunction(this, "localsUpdate", LimObject.localsUpdateSlot);

            initMessage = LimMessage.newWithName(this, IOSYMBOL("init"));
            forwardMessage = LimMessage.newWithName(this, IOSYMBOL("forward"));
            activateMessage = LimMessage.newWithName(this, IOSYMBOL("activate"));
            selfMessage = LimMessage.newWithName(this, IOSYMBOL("self"));
            opShuffleMessage = LimMessage.newWithName(this, IOSYMBOL("opShuffle"));
            mainMessage = LimMessage.newWithName(this, IOSYMBOL("main"));
			typeMessage = LimMessage.newWithName(this, IOSYMBOL("type"));
		}

        public void Return(LimObject v)
        {
            stopStatus = IoStopStatus.MESSAGE_STOP_STATUS_RETURN;
            returnValue = v;
        }

        public void resetStopStatus()
        {
            stopStatus = IoStopStatus.MESSAGE_STOP_STATUS_RETURN;
        }

        public int handleStatus()
        {
            switch (stopStatus)
            {
                case IoStopStatus.MESSAGE_STOP_STATUS_RETURN:
                    return 1;

                case IoStopStatus.MESSAGE_STOP_STATUS_BREAK:
                    resetStopStatus();
                    return 1;

                case IoStopStatus.MESSAGE_STOP_STATUS_CONTINUE:
                    resetStopStatus();
                    return 0;

                default:
                    return 0;
            }
        }


        public void setupSymbols()
        {
            activateSymbol = IOSYMBOL("activate");
            callSymbol = IOSYMBOL("call");
            forwardSymbol = IOSYMBOL("forward");
            noShufflingSymbol = IOSYMBOL("__noShuffling__");
            opShuffleSymbol = IOSYMBOL("opShuffle");
            semicolonSymbol = IOSYMBOL(";");
            selfSymbol = IOSYMBOL("self");
            setSlotSymbol = IOSYMBOL("setSlot");
            setSlotWithTypeSymbol = IOSYMBOL("setSlotWithType");
            stackSizeSymbol = IOSYMBOL("stackSize");
            typeSymbol = IOSYMBOL("type");
            updateSlotSymbol = IOSYMBOL("updateSlot");
        }

		public void setupSingletons()
		{
			LimNil = objectProto.clone(this);
            LimNil.slots["type"] = IOSYMBOL("nil");
			core.slots["nil"] = LimNil;
			
			LimTrue = LimObject.createObject(this);
            LimTrue.slots["type"] = IOSYMBOL("true");
			core.slots["true"] = LimTrue;

			LimFalse = LimObject.createObject(this);
            LimFalse.slots["type"] = IOSYMBOL("false");
			core.slots["false"] = LimFalse;
		}

        public void error(LimMessage self, string p, string p_3)
        {
        }

		public LimObject onDoCStringWithLabel(LimObject target, string code, string label)
		{
			LimMessage msg = new LimMessage();
			msg = msg.clone(this) as LimMessage;
			msg = msg.newFromTextLabel(this, code, label);
			return msg.localsPerformOn(target, target);
		}

        public LimObject loadFile(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
			LimObject result = null;
			string s = sr.ReadToEnd();
				result = onDoCStringWithLabel(lobby, s, fileName);
            return result;
        }

        public LimObject processBootstrap()
        {
            LimObject result = null;
            result = onDoCStringWithLabel(lobby, LimBootstrap.bootstrap, "bootstrap");
            Console.WriteLine("Bootstrap successfully loaded.");
            return result;
        }


        /// <summary>
        /// Command line prompt, interpreted stuff is added to specified state
        /// </summary>
        /// <param name="state">State object of the language</param>
        public void prompt(LimState state)
		{
			LimObject result = null;
            processBootstrap();
		   //onDoCStringWithLabel(lobby, "\"Geben Sie einen Namen ein: \" print\n\rblub := Console readLine", "propmt");
           //onDoCStringWithLabel(lobby, "\"Hallo, \" print\n\rblub print\n\r\"!\" println", "prompt:");
		   //return;
            while (true)
            {
                Console.Write("Io> ");
                string s = Console.ReadLine();
                if (s.Equals("quit") || s.Equals("exit")) break;
                result = onDoCStringWithLabel(lobby, s, "prompt:");
                Console.Write("==> ");
                if (result != null)
                    result.print();
                else Console.WriteLine("why null?");
                Console.WriteLine();

            }
		}

    }

    public enum IoStopStatus
    {
        MESSAGE_STOP_STATUS_NORMAL = 0,
        MESSAGE_STOP_STATUS_BREAK = 1,
        MESSAGE_STOP_STATUS_CONTINUE = 2,
        MESSAGE_STOP_STATUS_RETURN = 4,
        MESSAGE_STOP_STATUS_EOL = 8
    }
}
