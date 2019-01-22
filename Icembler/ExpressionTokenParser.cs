using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Icembler
{
    /// <summary>
    /// ExpressionTokenParser
    /// </summary>
    /// <remarks>
    /// ExpressionTokenParser is the main parser engine for converting input into lexical tokens.
    /// </remarks>
    public class ExpressionTokenParser
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
            Lparen = 1,
            Rparen = 2,
            Asterisk = 3,
            Plus = 4,
            Minus = 5,
            HexNumber = 6,
            BinaryNumber = 7,
            Identifier = 8,
            Integer = 9,
            Newline = 10,
            Whitespace = 11
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
        public ExpressionTokenParser()
        {
            _tokens = new Dictionary<Tokens, string>();
            _regExMatchCollection = new Dictionary<Tokens, MatchCollection>();
            _index = 0;
            _inputString = string.Empty;

            _tokens.Add(Tokens.Lparen, "\\(");
            _tokens.Add(Tokens.Rparen, "\\)");
            _tokens.Add(Tokens.Asterisk, "\\*");
            _tokens.Add(Tokens.Plus, "\\+");
            _tokens.Add(Tokens.Minus, "\\-");
            _tokens.Add(Tokens.HexNumber, "\\$[0-9A-Fa-f]+");
            _tokens.Add(Tokens.BinaryNumber, "\\%[01]+");
            _tokens.Add(Tokens.Identifier, "[a-zA-Z_][a-zA-Z0-9_]*");
            _tokens.Add(Tokens.Integer, "[0-9]+");
            _tokens.Add(Tokens.Newline, "[\\r\\n]+");
            _tokens.Add(Tokens.Whitespace, "[ \\t]+");
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
