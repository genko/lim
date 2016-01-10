using System;

namespace lim {

   public enum LimTokenType {
      NO_TOKEN = 0,
      OPENPAREN_TOKEN,
      COMMA_TOKEN,
      CLOSEPAREN_TOKEN,
      MONOQUOTE_TOKEN,
      TRIQUOTE_TOKEN,
      IDENTIFIER_TOKEN,
      TERMINATOR_TOKEN,
      COMMENT_TOKEN,
      NUMBER_TOKEN,
      HEXNUMBER_TOKEN
   }
   
   public class LimToken {
      string name_;
      LimTokenType type_;
      int charNumber_;
      int lineNumber_;
      LimToken nextToken_;
      string error_;

      public string name { set { name_ = value; } get { return name_; } }
      public LimTokenType type { set { type_ = value; } get { return type_; } }
      public int charNumber { set { charNumber_ = value; } get { return charNumber_; } }
      public int lineNumber  { set { lineNumber_ = value; } get { return lineNumber_; } }
      public LimToken nextToken { 
         set { 
            if (this == value) 
               throw new Exception("next = self!");
            nextToken_ = value;
         } 
         get { return nextToken_; }
        }
      public string error { get { return error_; } set { error_ = value; } } 
      public LimToken() { name = null; charNumber = -1; }
      public string typeName() {
         switch (this.type) {
            case LimTokenType.NO_TOKEN:			return "NoToken";
            case LimTokenType.OPENPAREN_TOKEN:	return "OpenParen";
            case LimTokenType.COMMA_TOKEN:		return "Comma";
            case LimTokenType.CLOSEPAREN_TOKEN:	return "CloseParen";
            case LimTokenType.MONOQUOTE_TOKEN:	return "MonoQuote";
            case LimTokenType.TRIQUOTE_TOKEN:	return "TriQuote";
            case LimTokenType.IDENTIFIER_TOKEN:	return "Identifier";
            case LimTokenType.TERMINATOR_TOKEN:	return "Terminator";
            case LimTokenType.COMMENT_TOKEN:		return "Comment";
            case LimTokenType.NUMBER_TOKEN:		return "Number";
            case LimTokenType.HEXNUMBER_TOKEN:	return "HexNumber";
         }
         return null;
        }   
        public void quoteName(string name) { name = "\"" + name_ + "\""; }
        public int nameIs(string n) {
         return name.CompareTo(n);  
      }
        public bool isValidMessageName()
        {
            switch (this.type)
            {
                case LimTokenType.IDENTIFIER_TOKEN:
                case LimTokenType.MONOQUOTE_TOKEN:
                case LimTokenType.TRIQUOTE_TOKEN:
                case LimTokenType.NUMBER_TOKEN:
                case LimTokenType.HEXNUMBER_TOKEN:
                    return true;
                default:
                    return false;
            }
        }
         public void print() { printSelf(); }
      public void printSelf() { System.Console.Write("'" + name + "'"); }

   }
}
