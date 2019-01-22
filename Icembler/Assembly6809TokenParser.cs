using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Icembler
{
    /// <summary>
    /// Assembly6809TokenParser
    /// </summary>
    /// <remarks>
    /// Assembly6809TokenParser is the main parser engine for converting input into lexical tokens.
    /// </remarks>
    public class Assembly6809TokenParser
    {
        private readonly Dictionary<Tokens, string> _tokens;
        private readonly Dictionary<Tokens, MatchCollection> _regExMatchCollection;
        private string _inputString;
        private int _index;

        /// <summary>
        /// Tokens is an enumeration of all possible token values.
        /// </summary>
        public enum Tokens
        {
            Undefined = 0,
            Identifier = 1,
            Bcc = 2,
            Lbcc = 3,
            Bcs = 4,
            Lbcs = 5,
            Beq = 6,
            Lbeq = 7,
            Bge = 8,
            Lbge = 9,
            Bgt = 10,
            Lbgt = 11,
            Bhi = 12,
            Lbhi = 13,
            Bhs = 14,
            Lbhs = 15,
            Ble = 16,
            Lble = 17,
            Blo = 18,
            Lblo = 19,
            Bls = 20,
            Lbls = 21,
            Blt = 22,
            Lblt = 23,
            Bmi = 24,
            Lbmi = 25,
            Bne = 26,
            Lbne = 27,
            Bpl = 28,
            Lbpl = 29,
            Bra = 30,
            Lbra = 31,
            Brn = 32,
            Lbrn = 33,
            Bsr = 34,
            Lbsr = 35,
            Bvc = 36,
            Lbvc = 37,
            Bvs = 38,
            Lbvs = 39,
            Abx = 40,
            Adca = 41,
            Adcb = 42,
            Adda = 43,
            Addb = 44,
            Addd = 45,
            Anda = 46,
            Andb = 47,
            Andcc = 48,
            Asla = 49,
            Aslb = 50,
            Asl = 51,
            Asra = 52,
            Asrb = 53,
            Asr = 54,
            Bita = 55,
            Bitb = 56,
            Clra = 57,
            Clrb = 58,
            Clr = 59,
            Cmpa = 60,
            Cmpb = 61,
            Cmpd = 62,
            Cmps = 63,
            Cmpu = 64,
            Cmpx = 65,
            Cmpy = 66,
            Coma = 67,
            Comb = 68,
            Com = 69,
            Cwai = 70,
            Daa = 71,
            Deca = 72,
            Decb = 73,
            Dec = 74,
            Eora = 75,
            Eorb = 76,
            Exg = 77,
            Inca = 78,
            Incb = 79,
            Inc = 80,
            Jmp = 81,
            Jsr = 82,
            Lda = 83,
            Ldb = 84,
            Ldd = 85,
            Lds = 86,
            Ldu = 87,
            Ldx = 88,
            Ldy = 89,
            Leas = 90,
            Leau = 91,
            Leax = 92,
            Leay = 93,
            Lsla = 94,
            Lslb = 95,
            Lsl = 96,
            Lsra = 97,
            Lsrb = 98,
            Lsr = 99,
            Mul = 100,
            Nega = 101,
            Negb = 102,
            Neg = 103,
            Nop = 104,
            Ora = 105,
            Orb = 106,
            Orcc = 107,
            Pshs = 108,
            Pshu = 109,
            Puls = 110,
            Pulu = 111,
            Rola = 112,
            Rolb = 113,
            Rol = 114,
            Rora = 115,
            Rorb = 116,
            Ror = 117,
            Rti = 118,
            Rts = 119,
            Sbca = 120,
            Sbcb = 121,
            Sex = 122,
            Sta = 123,
            Stb = 124,
            Std = 125,
            Sts = 126,
            Stu = 127,
            Stx = 128,
            Sty = 129,
            Suba = 130,
            Subb = 131,
            Subd = 132,
            Swi = 133,
            Swi2 = 134,
            Swi3 = 135,
            Sync = 136,
            Tfr = 137,
            Tsta = 138,
            Tstb = 139,
            Tst = 140,
            End = 141,
            Equ = 142,
            Org = 143,
            HexNumber = 144,
            BinaryNumber = 145,
            Integer = 146,
            PoundSign = 147,
            Register = 148,
            Comma = 149,
            Increment2 = 150,
            Increment1 = 151,
            Decrement2 = 152,
            Decrement1 = 153,
            Comment = 154,
            Whitespace = 155,
            Newline = 156
        }

        /// <summary>
        /// InputString Property
        /// </summary>
        /// <value>
        /// The string value that holds the input string.
        /// </value>
        public virtual string InputString
        {
            set
            {
                _inputString = value;
                PrepareRegex();
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <remarks>
        /// The constructor initalizes memory and adds all of the tokens to the token dictionary.
        /// </remarks>
        public Assembly6809TokenParser()
        {
            _tokens = new Dictionary<Tokens, string>();
            _regExMatchCollection = new Dictionary<Tokens, MatchCollection>();
            _index = 0;
            _inputString = string.Empty;

            _tokens.Add(Tokens.Identifier, "[ \\t]?[a-zA-Z_][a-zA-Z0-9_]{4,}");
            _tokens.Add(Tokens.Bcc, "[ \\t][Bb][Cc][Cc]");
            _tokens.Add(Tokens.Lbcc, "[ \\t][Ll][Bb][Cc][Cc]");
            _tokens.Add(Tokens.Bcs, "[ \\t][Bb][Cc][Ss]");
            _tokens.Add(Tokens.Lbcs, "[ \\t][Ll][Bb][Cc][Ss]");
            _tokens.Add(Tokens.Beq, "[ \\t][Bb][Ee][Qq]");
            _tokens.Add(Tokens.Lbeq, "[ \\t][Ll][Bb][Ee][Qq]");
            _tokens.Add(Tokens.Bge, "[ \\t][Bb][Gg][Ee]");
            _tokens.Add(Tokens.Lbge, "[ \\t][Ll][Bb][Gg][Ee]");
            _tokens.Add(Tokens.Bgt, "[ \\t][Bb][Gg][Tt]");
            _tokens.Add(Tokens.Lbgt, "[ \\t][Ll][Bb][Gg][Tt]");
            _tokens.Add(Tokens.Bhi, "[ \\t][Bb][Hh][Ii]");
            _tokens.Add(Tokens.Lbhi, "[ \\t][Ll][Bb][Hh][Ii]");
            _tokens.Add(Tokens.Bhs, "[ \\t][Bb][Hh][Ss]");
            _tokens.Add(Tokens.Lbhs, "[ \\t][Ll][Bb][Hh][Ss]");
            _tokens.Add(Tokens.Ble, "[ \\t][Bb][Ll][Ee]");
            _tokens.Add(Tokens.Lble, "[ \\t][Ll][Bb][Ll][Ee]");
            _tokens.Add(Tokens.Blo, "[ \\t][Bb][Ll][Oo]");
            _tokens.Add(Tokens.Lblo, "[ \\t][Ll][Bb][Ll][Oo]");
            _tokens.Add(Tokens.Bls, "[ \\t][Bb][Ll][Ss]");
            _tokens.Add(Tokens.Lbls, "[ \\t][Ll][Bb][Ll][Ss]");
            _tokens.Add(Tokens.Blt, "[ \\t][Bb][Ll][Tt]");
            _tokens.Add(Tokens.Lblt, "[ \\t][Ll][Bb][Ll][Tt]");
            _tokens.Add(Tokens.Bmi, "[ \\t][Bb][Mm][Ii]");
            _tokens.Add(Tokens.Lbmi, "[ \\t][Ll][Bb][Mm][Ii]");
            _tokens.Add(Tokens.Bne, "[ \\t][Bb][Nn][Ee]");
            _tokens.Add(Tokens.Lbne, "[ \\t][Ll][Bb][Nn][Ee]");
            _tokens.Add(Tokens.Bpl, "[ \\t][Bb][Pp][Ll]");
            _tokens.Add(Tokens.Lbpl, "[ \\t][Ll][Bb][Pp][Ll]");
            _tokens.Add(Tokens.Bra, "[ \\t][Bb][Rr][Aa]");
            _tokens.Add(Tokens.Lbra, "[ \\t][Ll][Bb][Rr][Aa]");
            _tokens.Add(Tokens.Brn, "[ \\t][Bb][Rr][Nn]");
            _tokens.Add(Tokens.Lbrn, "[ \\t][Ll][Bb][Rr][Nn]");
            _tokens.Add(Tokens.Bsr, "[ \\t][Bb][Ss][Rr]");
            _tokens.Add(Tokens.Lbsr, "[ \\t][Ll][Bb][Ss][Rr]");
            _tokens.Add(Tokens.Bvc, "[ \\t][Bb][Vv][Cc]");
            _tokens.Add(Tokens.Lbvc, "[ \\t][Ll][Bb][Vv][Cc]");
            _tokens.Add(Tokens.Bvs, "[ \\t][Bb][Vv][Ss]");
            _tokens.Add(Tokens.Lbvs, "[ \\t][Ll][Bb][Vv][Ss]");
            _tokens.Add(Tokens.Abx, "[ \\t][Aa][Bb][Xx]");
            _tokens.Add(Tokens.Adca, "[ \\t][Aa][Dd][Cc][Aa]");
            _tokens.Add(Tokens.Adcb, "[ \\t][Aa][Dd][Cc][Bb]");
            _tokens.Add(Tokens.Adda, "[ \\t][Aa][Dd][Dd][Aa]");
            _tokens.Add(Tokens.Addb, "[ \\t][Aa][Dd][Dd][Bb]");
            _tokens.Add(Tokens.Addd, "[ \\t][Aa][Dd][Dd][Dd]");
            _tokens.Add(Tokens.Anda, "[ \\t][Aa][Nn][Dd][Aa]");
            _tokens.Add(Tokens.Andb, "[ \\t][Aa][Nn][Dd][Bb]");
            _tokens.Add(Tokens.Andcc, "[ \\t][Aa][Nn][Dd][Cc][Cc]");
            _tokens.Add(Tokens.Asla, "[ \\t][Aa][Ss][Ll][Aa]");
            _tokens.Add(Tokens.Aslb, "[ \\t][Aa][Ss][Ll][Bb]");
            _tokens.Add(Tokens.Asl, "[ \\t][Aa][Ss][Ll]");
            _tokens.Add(Tokens.Asra, "[ \\t][Aa][Ss][Rr][Aa]");
            _tokens.Add(Tokens.Asrb, "[ \\t][Aa][Ss][Rr][Bb]");
            _tokens.Add(Tokens.Asr, "[ \\t][Aa][Ss][Rr]");
            _tokens.Add(Tokens.Bita, "[ \\t][Bb][Ii][Tt][Aa]");
            _tokens.Add(Tokens.Bitb, "[ \\t][Bb][Ii][Tt][Bb]");
            _tokens.Add(Tokens.Clra, "[ \\t][Cc][Ll][Rr][Aa]");
            _tokens.Add(Tokens.Clrb, "[ \\t][Cc][Ll][Rr][Bb]");
            _tokens.Add(Tokens.Clr, "[ \\t][Cc][Ll][Rr]");
            _tokens.Add(Tokens.Cmpa, "[ \\t][Cc][Mm][Pp][Aa]");
            _tokens.Add(Tokens.Cmpb, "[ \\t][Cc][Mm][Pp][Bb]");
            _tokens.Add(Tokens.Cmpd, "[ \\t][Cc][Mm][Pp][Dd]");
            _tokens.Add(Tokens.Cmps, "[ \\t][Cc][Mm][Pp][Ss]");
            _tokens.Add(Tokens.Cmpu, "[ \\t][Cc][Mm][Pp][Uu]");
            _tokens.Add(Tokens.Cmpx, "[ \\t][Cc][Mm][Pp][Xx]");
            _tokens.Add(Tokens.Cmpy, "[ \\t][Cc][Mm][Pp][Yy]");
            _tokens.Add(Tokens.Coma, "[ \\t][Cc][Oo][Mm][Aa]");
            _tokens.Add(Tokens.Comb, "[ \\t][Cc][Oo][Mm][Bb]");
            _tokens.Add(Tokens.Com, "[ \\t][Cc][Oo][Mm]");
            _tokens.Add(Tokens.Cwai, "[ \\t][Cc][Ww][Aa][Ii]");
            _tokens.Add(Tokens.Daa, "[ \\t][Dd][Aa][Aa]");
            _tokens.Add(Tokens.Deca, "[ \\t][Dd][Ee][Cc][Aa]");
            _tokens.Add(Tokens.Decb, "[ \\t][Dd][Ee][Cc][Bb]");
            _tokens.Add(Tokens.Dec, "[ \\t][Dd][Ee][Cc]");
            _tokens.Add(Tokens.Eora, "[ \\t][Ee][Oo][Rr][Aa]");
            _tokens.Add(Tokens.Eorb, "[ \\t][Ee][Oo][Rr][Bb]");
            _tokens.Add(Tokens.Exg, "[ \\t][Ee][Xx][Gg]");
            _tokens.Add(Tokens.Inca, "[ \\t][Ii][Nn][Cc][Aa]");
            _tokens.Add(Tokens.Incb, "[ \\t][Ii][Nn][Cc][Bb]");
            _tokens.Add(Tokens.Inc, "[ \\t][Ii][Nn][Cc]");
            _tokens.Add(Tokens.Jmp, "[ \\t][Jj][Mm][Pp]");
            _tokens.Add(Tokens.Jsr, "[ \\t][Jj][Ss][Rr]");
            _tokens.Add(Tokens.Lda, "[ \\t][Ll][Dd][Aa]");
            _tokens.Add(Tokens.Ldb, "[ \\t][Ll][Dd][Bb]");
            _tokens.Add(Tokens.Ldd, "[ \\t][Ll][Dd][Dd]");
            _tokens.Add(Tokens.Lds, "[ \\t][Ll][Dd][Ss]");
            _tokens.Add(Tokens.Ldu, "[ \\t][Ll][Dd][Uu]");
            _tokens.Add(Tokens.Ldx, "[ \\t][Ll][Dd][Xx]");
            _tokens.Add(Tokens.Ldy, "[ \\t][Ll][Dd][Yy]");
            _tokens.Add(Tokens.Leas, "[ \\t][Ll][Ee][Aa][Ss]");
            _tokens.Add(Tokens.Leau, "[ \\t][Ll][Ee][Aa][Uu]");
            _tokens.Add(Tokens.Leax, "[ \\t][Ll][Ee][Aa][Xx]");
            _tokens.Add(Tokens.Leay, "[ \\t][Ll][Ee][Aa][Yy]");
            _tokens.Add(Tokens.Lsla, "[ \\t][Ll][Ss][Ll][Aa]");
            _tokens.Add(Tokens.Lslb, "[ \\t][Ll][Ss][Ll][Bb]");
            _tokens.Add(Tokens.Lsl, "[ \\t][Ll][Ss][Ll]");
            _tokens.Add(Tokens.Lsra, "[ \\t][Ll][Ss][Rr][Aa]");
            _tokens.Add(Tokens.Lsrb, "[ \\t][Ll][Ss][Rr][Bb]");
            _tokens.Add(Tokens.Lsr, "[ \\t][Ll][Ss][Rr]");
            _tokens.Add(Tokens.Mul, "[ \\t][Mm][Uu][Ll]");
            _tokens.Add(Tokens.Nega, "[ \\t][Nn][Ee][Gg][Aa]");
            _tokens.Add(Tokens.Negb, "[ \\t][Nn][Ee][Gg][Bb]");
            _tokens.Add(Tokens.Neg, "[ \\t][Nn][Ee][Gg]");
            _tokens.Add(Tokens.Nop, "[ \\t][Nn][Oo][Pp]");
            _tokens.Add(Tokens.Ora, "[ \\t][Oo][Rr][Aa]");
            _tokens.Add(Tokens.Orb, "[ \\t][Oo][Rr][Bb]");
            _tokens.Add(Tokens.Orcc, "[ \\t][Oo][Rr][Cc][Cc]");
            _tokens.Add(Tokens.Pshs, "[ \\t][Pp][Ss][Hh][Ss]");
            _tokens.Add(Tokens.Pshu, "[ \\t][Pp][Ss][Hh][Uu]");
            _tokens.Add(Tokens.Puls, "[ \\t][Pp][Uu][Ll][Ss]");
            _tokens.Add(Tokens.Pulu, "[ \\t][Pp][Uu][Ll][Uu]");
            _tokens.Add(Tokens.Rola, "[ \\t][Rr][Oo][Ll][Aa]");
            _tokens.Add(Tokens.Rolb, "[ \\t][Rr][Oo][Ll][Bb]");
            _tokens.Add(Tokens.Rol, "[ \\t][Rr][Oo][Ll]");
            _tokens.Add(Tokens.Rora, "[ \\t][Rr][Oo][Rr][Aa]");
            _tokens.Add(Tokens.Rorb, "[ \\t][Rr][Oo][Rr][Bb]");
            _tokens.Add(Tokens.Ror, "[ \\t][Rr][Oo][Rr]");
            _tokens.Add(Tokens.Rti, "[ \\t][Rr][Tt][Ii]");
            _tokens.Add(Tokens.Rts, "[ \\t][Rr][Tt][Ss]");
            _tokens.Add(Tokens.Sbca, "[ \\t][Ss][Bb][Cc][Aa]");
            _tokens.Add(Tokens.Sbcb, "[ \\t][Ss][Bb][Cc][Bb]");
            _tokens.Add(Tokens.Sex, "[ \\t][Ss][Ee][Xx]");
            _tokens.Add(Tokens.Sta, "[ \\t][Ss][Tt][Aa]");
            _tokens.Add(Tokens.Stb, "[ \\t][Ss][Tt][Bb]");
            _tokens.Add(Tokens.Std, "[ \\t][Ss][Tt][Dd]");
            _tokens.Add(Tokens.Sts, "[ \\t][Ss][Tt][Ss]");
            _tokens.Add(Tokens.Stu, "[ \\t][Ss][Tt][Uu]");
            _tokens.Add(Tokens.Stx, "[ \\t][Ss][Tt][Xx]");
            _tokens.Add(Tokens.Sty, "[ \\t][Ss][Tt][Yy]");
            _tokens.Add(Tokens.Suba, "[ \\t][Ss][Uu][Bb][Aa]");
            _tokens.Add(Tokens.Subb, "[ \\t][Ss][Uu][Bb][Bb]");
            _tokens.Add(Tokens.Subd, "[ \\t][Ss][Uu][Bb][Dd]");
            _tokens.Add(Tokens.Swi, "[ \\t][Ss][Ww][Ii]");
            _tokens.Add(Tokens.Swi2, "[ \\t][Ss][Ww][Ii][2]");
            _tokens.Add(Tokens.Swi3, "[ \\t][Ss][Ww][Ii][3]");
            _tokens.Add(Tokens.Sync, "[ \\t][Ss][Yy][Nn][Cc]");
            _tokens.Add(Tokens.Tfr, "[ \\t][Tt][Ff][Rr]");
            _tokens.Add(Tokens.Tsta, "[ \\t][Tt][Ss][Tt][Aa]");
            _tokens.Add(Tokens.Tstb, "[ \\t][Tt][Ss][Tt][Bb]");
            _tokens.Add(Tokens.Tst, "[ \\t][Tt][Ss][Tt]");
            _tokens.Add(Tokens.End, "[ \\t][Ee][Nn][Dd]");
            _tokens.Add(Tokens.Equ, "[ \\t][Ee][Qq][Uu]");
            _tokens.Add(Tokens.Org, "[ \\t][Oo][Rr][Gg]");
            _tokens.Add(Tokens.HexNumber, "\\$[0-9a-fA-F]+");
            _tokens.Add(Tokens.BinaryNumber, "\\%[01]+");
            _tokens.Add(Tokens.Integer, "[0-9]+");
            _tokens.Add(Tokens.PoundSign, "#");
            _tokens.Add(Tokens.Register, "[ABDXYSUabdxysu]{1}");
            _tokens.Add(Tokens.Comma, "\\,");
            _tokens.Add(Tokens.Increment2, "\\+\\+");
            _tokens.Add(Tokens.Increment1, "\\+");
            _tokens.Add(Tokens.Decrement2, "\\-\\-");
            _tokens.Add(Tokens.Decrement1, "\\-");
            _tokens.Add(Tokens.Comment, "\\*.*");
            _tokens.Add(Tokens.Whitespace, "[ \\t]+");
            _tokens.Add(Tokens.Newline, "[\\r\\n]+");
        }

        /// <summary>
        /// PrepareRegex prepares the regex for parsing by pre-matching the Regex tokens.
        /// </summary>
        protected virtual void PrepareRegex()
        {
            _regExMatchCollection.Clear();
            foreach (KeyValuePair<Tokens, string> pair in _tokens)
            {
                _regExMatchCollection.Add(pair.Key, Regex.Matches(_inputString, pair.Value));
            }
        }

        /// <summary>
        /// ResetParser resets the parser to its inital state. Reloading InputString is required.
        /// </summary>
        /// <seealso cref="InputString" />
        public virtual void ResetParser()
        {
            _index = 0;
            _inputString = string.Empty;
            _regExMatchCollection.Clear();
        }

        /// <summary>
        /// GetToken gets the next token in queue
        /// </summary>
        /// <remarks>
        /// GetToken attempts to the match the next character(s) using the
        /// Regex rules defined in the dictionary. If a match can not be
        /// located, then an Undefined token will be created with an empty
        /// string value. In addition, the token pointer will be incremented
        /// by one so that this token doesn't attempt to get identified again by
        /// GetToken()
        /// </remarks>
        public virtual Token GetToken()
        {
            if (_index >= _inputString.Length)
                return null;

            foreach (KeyValuePair<Tokens, MatchCollection> pair in _regExMatchCollection)
            {
                foreach (Match match in pair.Value)
                {
                    if (match.Index == _index)
                    {
                        _index += match.Length;
                        return new Token(pair.Key, match.Value);
                    }

                    if (match.Index > _index)
                    {
                        break;
                    }
                }
            }
            _index++;
            return new Token(Tokens.Undefined, (_inputString[_index - 1]).ToString());
        }

        /// <summary>
        /// Returns the next token that GetToken() will return.
        /// </summary>
        /// <seealso cref="Peek(PeekToken)" />
        public virtual PeekToken Peek()
        {
            return Peek(new PeekToken(_index, new Token(Tokens.Undefined, string.Empty)));
        }

        /// <summary>
        /// Returns the next token after the Token passed here
        /// </summary>
        /// <param name="peekToken">The PeekToken token returned from a previous Peek() call</param>
        /// <seealso cref="Peek()" />
        public virtual PeekToken Peek(PeekToken peekToken)
        {
            int oldIndex = _index;

            _index = peekToken.TokenIndex;

            if (_index >= _inputString.Length)
            {
                _index = oldIndex;
                return null;
            }



            foreach (KeyValuePair<Tokens, string> pair in _tokens)
            {
                Regex r = new Regex(pair.Value);
                Match m = r.Match(_inputString, _index);

                if (m.Success && m.Index == _index)
                {
                    _index += m.Length;
                    PeekToken pt = new PeekToken(_index, new Token(pair.Key, m.Value));
                    _index = oldIndex;
                    return pt;
                }
            }
            PeekToken pt2 = new PeekToken(_index + 1, new Token(Tokens.Undefined, (_inputString[_index]).ToString()));
            _index = oldIndex;
            return pt2;
        }

        /// <summary>
        /// A PeekToken object class
        /// </summary>
        /// <remarks>
        /// A PeekToken is a special pointer object that can be used to Peek() several
        /// tokens ahead in the GetToken() queue.
        /// </remarks>
        public partial class PeekToken
        {
            public int TokenIndex { get; set; }

            public Token TokenPeek { get; set; }

            public PeekToken(int index, Token value)
            {
                TokenIndex = index;
                TokenPeek = value;
            }
        }

        /// <summary>
        /// a Token object class
        /// </summary>
        /// <remarks>
        /// A Token object holds the token and token value.
        /// </remarks>
        public class Token
        {
            public Tokens TokenName { get; set; }

            public string TokenValue { get; set; }

            public Token(Tokens name, string value)
            {
                TokenName = name;
                TokenValue = value;
            }
        }
    }
}
