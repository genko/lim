public class LimLexer
{

    public static string specialChars = ":._";
    public string s;
    public int currentPos;
    public char current { get { return s[currentPos]; } }
    public ArrayList charLineIndex = new ArrayList();
    public long lineHint;
    public long maxChar;
    public Stack posStack = new Stack();
    public Stack tokenStack = new Stack();
    public ArrayList tokenStream = new ArrayList();
    public int resultIndex = 0;
    public LimToken errorToken;
    public string errorDescription;

    public void print()
    {
        LimToken first = tokenStream.Get(0) as LimToken;
        if (first != null)
        {
            first.print();
        }
        System.Console.WriteLine();
    }

    public void printTokens()
    {
        int i;
        for (i = 0; i < tokenStream.Count; i++)
        {
            LimToken t = tokenStream.Get(i) as LimToken;
            System.Console.Write("'{0}' {1}", t.name, t.typeName());
            if (i < tokenStream.Count - 1)
            {
                System.Console.Write(", ");
            }
        }
        System.Console.WriteLine();
    }


    public int lex()
    {
        pushPos();
        messageChain();

        if (!onNULL())
        {

            if (errorToken == null)
            {
                if (tokenStream.Count != 0)
                {
                    errorToken = currentToken();
                }
                else
                {
                    errorToken = addTokenStringType(s.Substring(currentPos, 30), LimTokenType.NO_TOKEN);
                }

                errorToken.error = "Syntax error near this location";
            }
            return -1;
        }
        return 0;
    }

    public LimToken top()
    {
        if (resultIndex >= tokenStream.Count) return null;
        return tokenStream.Get(resultIndex) as LimToken;
    }

    public int lastPos()
    {
        return System.Convert.ToInt32(posStack.Peek());
    }

    public void pushPos()
    {
        tokenStack.Push(tokenStream.Count - 1);
        posStack.Push(currentPos);
    }

    public void popPos()
    {
        tokenStack.Pop();
        posStack.Pop();
    }

    public LimTokenType topType()
    {
        if (top() == null) return 0;
        return top().type;
    }

    public LimToken pop()
    {
        LimToken first = top();
        resultIndex++;
        return first;
    }

    public void popPosBack()
    {
        int i = System.Convert.ToInt32(tokenStack.Pop());
        int topIndex = System.Convert.ToInt32(tokenStack.Peek());
        if (i > -1)
        {
            if (i != topIndex)
            {
                LimToken parent = currentToken();
                if (parent != null)
                {
                    parent.nextToken = null;
                }
            }
        }
        currentPos = System.Convert.ToInt32(posStack.Pop());
    }

    public char nextChar()
    {
        if (currentPos >= s.Length)
            return '\0';
        char c = current;
        currentPos++;
        return c;
    }

    public char peekChar()
    {
        if (currentPos >= s.Length)
            return '\0';
        char c = current;
        return c;
    }

    public char prevChar()
    {
        currentPos--;
        char c = current;
        return c;
    }

    public int readPadding()
    {

        int r = 0;
        while (readWhitespace() != 0 || readComment() != 0)
        {
            r = 1;
        }
        return r;
    }

    // grabbing

    public int grabLength()
    {
        int i1 = lastPos();
        int i2 = currentPos;
        return i2 - i1;
    }

    public LimToken grabTokenType(LimTokenType type)
    {
        int len = grabLength();

        string s1 = s.Substring(lastPos(), len);

        if (len == 0)
        {
            throw new System.Exception("LimLexer fatal error: empty token\n");
        }

        return addTokenStringType(s1, type);
    }

    public int currentLineNumber()
    {
        return 0;
    }

    public LimToken addTokenStringType(string s1, LimTokenType type)
    {

        LimToken top = currentToken();
        LimToken t = new LimToken();

        t.lineNumber = currentLineNumber();
        t.charNumber = currentPos;

        if (t.charNumber < 0)
        {
            System.Console.WriteLine("bad t->charNumber = %i\n", t.charNumber);
        }

        t.name = s1;
        t.type = type;

        if (top != null)
        {
            top.nextToken = t;
        }

        tokenStream.Add(t);
        return t;
    }

    public LimToken currentToken()
    {
        if (tokenStream.Count == 0) return null;
        return tokenStream.Get(tokenStream.Count - 1) as LimToken;
    }

    // message chain

    public void messageChain()
    {
        do
        {
            while (readTerminator() != 0 || readSeparator() != 0 || readComment() != 0) ;
        } while (readMessage() != 0);
    }

    // symbols

    public int readSymbol()
    {
        if (readNumber() != 0 || readOperator() != 0 || readIdentifier() != 0 || readQuote() != 0) return 1;
        return 0;
    }

    public int readIdentifier()
    {
        pushPos();
        while (readLetter() != 0 || readDigit() != 0 || readSpecialChar() != 0) ;
        if (grabLength() != 0)
        {
            if (s[currentPos - 1] == ':' && s[currentPos] == '=') prevChar();
            grabTokenType(LimTokenType.IDENTIFIER_TOKEN);
            popPos();
            return 1;
        }
        return 0;
    }

    public int readOperator()
    {
        char c;
        pushPos();
        c = nextChar();
        if (c == 0)
        {
            popPosBack();
            return 0;
        }
        else {
            prevChar();
        }

        while (readOpChar() != 0) ;

        if (grabLength() != 0)
        {
            grabTokenType(LimTokenType.IDENTIFIER_TOKEN);
            popPos();
            return 1;

        }

        popPosBack();
        return 0;

    }

    public bool onNULL()
    {
        return currentPos == s.Length;
    }

    // helpers

    public int readTokenCharsType(string chars, LimTokenType type)
    {
        foreach (char c in chars)
        {
            if (readTokenCharType(c, type) != 0)
                return 1;
        }
        return 0;
    }

    public int readTokenCharType(char c, LimTokenType type)
    {
        pushPos();

        if (readChar(c) != 0)
        {
            grabTokenType(type);
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }

    public int readTokenString(string s)
    {
        pushPos();

        if (readString(s) != 0)
        {
            grabTokenType(LimTokenType.IDENTIFIER_TOKEN);
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }

    public int readString(string str)
    {
        int len = str.Length;
        if (len > s.Length - currentPos)
            len = s.Length - currentPos;
        if (onNULL())
        {
            return 0;
        }

        string inmem = s.Substring(currentPos, len);

        if (inmem.Equals(str))
        {
            currentPos += len;
            return 1;
        }

        return 0;
    }

    public int readCharIn(string s)
    {
        if (!onNULL())
        {
            char c = nextChar();
            if (s.IndexOf(c) != -1)
            {
                return 1;
            }
            prevChar();
        }
        return 0;
    }

    public int readCharInRange(char first, char last)
    {
        if (!onNULL())
        {
            char c = nextChar();
            if ((int)c >= (int)first && (int)c <= (int)last)
            {
                return 1;
            }
            prevChar();
        }
        return 0;
    }

    public int readNonASCIIChar()
    {
        if (!onNULL())
        {
            char nc = nextChar();

            if (nc >= 0x80)
                return 1;

            prevChar();
        }
        return 0;
    }

    public int readChar(char ch)
    {
        if (!onNULL())
        {
            char c = nextChar();
            if (c == ch)
            {
                return 1;
            }
            prevChar();
        }
        return 0;
    }

    public int readCharAnyCase(char ch)
    {
        if (!onNULL())
        {
            char c = nextChar();
            if (System.Char.ToLower(c) == System.Char.ToLower(ch))
            {
                return 1;
            }
            prevChar();
        }
        return 0;
    }

    public bool readNonReturn()
    {
        if (onNULL()) return false;
        if (nextChar() != '\n') return true;
        prevChar();
        return false;
    }

    public bool readNonQuote()
    {
        if (onNULL()) return false;
        if (nextChar() != '"') return true;
        prevChar();
        return false;
    }

    // character definitions

    public int readCharacters()
    {
        int read = 0;
        while (readCharacter() != 0)
        {
            read = 1;
        }
        return read;
    }

    public int readCharacter()
    {
        return System.Convert.ToInt32(readLetter() != 0 || readDigit() != 0 || readSpecialChar() != 0 || readOpChar() != 0);
    }

    public int readOpChar()
    {
        return readCharIn(":'~!@$%^&*-+=|\\<>?/");
    }

    public int readSpecialChar()
    {
        return readCharIn(LimLexer.specialChars);
    }

    public int readDigit()
    {
        return readCharInRange('0', '9');
    }

    public int readLetter() // grab all symbols
    {
        return System.Convert.ToInt32(readCharInRange('A', 'Z') != 0 || readCharInRange('a', 'z') != 0
            || readNonASCIIChar() != 0);
    }

    // comments

    public int readComment()
    {
        return readSlashSlashComment();
    }

    private int readSlashSlashComment()
    {
        this.pushPos();
        if (nextChar() == '/')
        {
            if (nextChar() == '/')
            {
                while (readNonReturn()) { }
                popPos();
                return 1;
            }
        }
        popPosBack();
        return 0;
    }

    // quotes

    public int readQuote()
    {
        return System.Convert.ToInt32(readTriQuote() != 0 || readMonoQuote() != 0);
    }

    public int readMonoQuote()
    {
        pushPos();

        if (nextChar() == '"')
        {
            while (true)
            {
                char c = nextChar();

                if (c == '"')
                {
                    break;
                }

                if (c == '\\')
                {
                    nextChar();
                    continue;
                }

                if (c == 0)
                {
                    errorToken = currentToken();

                    if (errorToken != null)
                    {
                        errorToken.error = "unterminated quote";
                    }

                    popPosBack();
                    return 0;
                }
            }

            grabTokenType(LimTokenType.MONOQUOTE_TOKEN);
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }

    public int readTriQuote()
    {
        pushPos();

        if (readString("\"\"\"") != 0)
        {
            while (readString("\"\"\"") == 0)
            {
                char c = nextChar();

                if (c == 0)
                {
                    popPosBack();
                    return 0;
                }
            }

            grabTokenType(LimTokenType.TRIQUOTE_TOKEN);
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }

    // terminators

    public int readTerminator()
    {
        int terminated = 0;
        pushPos();
        readSeparator();

        while (readTerminatorChar() != 0)
        {
            terminated = 1;
            readSeparator();
        }

        if (terminated != 0)
        {
            LimToken top = currentToken();

            // avoid double terminators
            if (top != null && top.type == LimTokenType.TERMINATOR_TOKEN)
            {
                return 1;
            }

            addTokenStringType(";", LimTokenType.TERMINATOR_TOKEN);
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }

    public int readTerminatorChar()
    {
        return readCharIn(";\n");
    }

    // separators

    public int readSeparator()
    {
        pushPos();

        while (readSeparatorChar() != 0) ;

        if (grabLength() != 0)
        {
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }

    public int readSeparatorChar()
    {
        if (readCharIn(" \f\r\t\v") != 0)
        {
            return 1;
        }
        else
        {
            pushPos();
            if (readCharIn("\\") != 0)
            {
                while (readCharIn(" \f\r\t\v") != 0) ;
                if (readCharIn("\n") != 0)
                {
                    popPos();
                    return 1;
                }
            }
            popPosBack();
            return 0;
        }
    }

    // whitespace

    int readWhitespace()
    {
        pushPos();

        while (readWhitespaceChar() != 0) ;

        if (grabLength() != 0)
        {
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }

    public int readWhitespaceChar()
    {
        return readCharIn(" \f\r\t\v\n");
    }

    ///

    public int readDigits()
    {
        int read = 0;
        pushPos();

        while (readDigit() != 0)
        {
            read = 1;
        }

        if (read == 0)
        {
            popPosBack();
            return 0;
        }

        popPos();
        return read;
    }

    public int readNumber()
    {
        return System.Convert.ToInt32(readHexNumber() != 0 || readDecimal() != 0);
    }

    public int readExponent()
    {
        if (readCharAnyCase('e') != 0)
        {
            if (readChar('-') != 0 || readChar('+') != 0) { }

            if (readDigits() == 0)
            {
                return -1;
            }
            return 1;
        }
        return 0;
    }

    public int readDecimalPlaces()
    {
        if (readChar('.') != 0)
        {
            if (readDigits() == 0)
            {
                return -1;
            }
            return 1;
        }
        return 0;
    }


    public int readDecimal()
    {
        pushPos();

        if (readDigits() != 0)
        {
            if (readDecimalPlaces() == -1)
            {
                popPosBack();
                return 0;
            }
        }
        else
        {
            if (readDecimalPlaces() != 1)
            {
                popPosBack();
                return 0;
            }
        }

        if (readExponent() == -1)
        {
            popPosBack();
            return 0;
        }

        if (grabLength() != 0)
        {
            grabTokenType(LimTokenType.NUMBER_TOKEN);
            popPos();
            return 1;
        }
        popPosBack();
        return 0;
    }

    public int readHexNumber()
    {
        int read = 0;
        pushPos();

        if (readChar('0') != 0 && readCharAnyCase('x') != 0)
        {
            while (readDigits() != 0 || readCharacters() != 0)
            {
                read++;
            }
        }

        if (read != 0 && grabLength() != 0)
        {
            grabTokenType(LimTokenType.HEXNUMBER_TOKEN);
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }


    /// message

    public string nameForGroupChar(char groupChar)
    {
        switch (groupChar)
        {
            case '(': return "";
            case '[': return "squareBrackets";
            case '{': return "curlyBrackets";
        }

        throw new System.Exception("LimLexer: fatal error - invalid group char" + groupChar);
    }

    public void readMessageError(string name)
    {
        popPosBack();
        errorToken = currentToken();
        errorToken.error = name;
    }

    public int readMessage()
    {
        int foundSymbol = 0;
        pushPos();
        readPadding();
        foundSymbol = readSymbol();

        char groupChar;

        while (readSeparator() != 0 || readComment() != 0) ;

        groupChar = peekChar();

        // this is bug in original IoVM so I've commented this out

        if ("[{".IndexOf(groupChar) != -1 || (foundSymbol == 0 &&
         groupChar == '('))
        {
            string groupName = nameForGroupChar(groupChar);
            addTokenStringType(groupName, LimTokenType.IDENTIFIER_TOKEN);
        }

        if (readTokenCharsType("([{", LimTokenType.OPENPAREN_TOKEN) != 0)
        {
            readPadding();
            do
            {
                LimTokenType type = currentToken().type;
                readPadding();

                // Empty argument: (... ,)
                if (LimTokenType.COMMA_TOKEN == type)
                {
                    char c = current;
                    if (',' == c || ")]}".IndexOf(c) != -1)
                    {
                        readMessageError("missing argument in argument list");
                        return 0;
                    }
                }

                if (groupChar == '[') specialChars = "._";
                messageChain();
                if (groupChar == '[') specialChars = ":._";
                readPadding();

            } while (readTokenCharType(',', LimTokenType.COMMA_TOKEN) != 0);

            if (readTokenCharsType(")]}", LimTokenType.CLOSEPAREN_TOKEN) == 0)
            {
                if (groupChar == '(')
                {
                    readMessageError("unmatched ()s");
                }
                else if (groupChar == '[')
                {
                    readMessageError("unmatched []s");
                }
                else if (groupChar == '{')
                {
                    readMessageError("unmatched {}s");
                }
                return 0;
            }

            popPos();
            return 1;
        }

        if (foundSymbol != 0)
        {
            popPos();
            return 1;
        }

        popPosBack();
        return 0;
    }

}