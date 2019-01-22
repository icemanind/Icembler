using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icembler
{
    public class Assembler
    {
        private readonly Dictionary<string, ushort> _labelTable;
        private readonly Dictionary<string, ushort> _equateTable;
        private readonly Dictionary<string, int> _lineNumberTable;
        private int _lineNumber;
        private ushort _directPage;
        private ushort _origination;
        private ushort _numberOfBytes;

        public ushort StartingAddress => _origination;
        public ushort ExecutionAddress { get; private set; }

        public Assembler()
        {
            _labelTable = new Dictionary<string, ushort>();
            _equateTable = new Dictionary<string, ushort>();
            _lineNumberTable = new Dictionary<string, int>();
            _lineNumber = 0;
            _directPage = 0;
            _origination = 0;
            _numberOfBytes = 0;
            ExecutionAddress = 0;
        }

        public byte[] Assemble(string program)
        {
            _labelTable.Clear();
            _lineNumberTable.Clear();
            _equateTable.Clear();
            _lineNumber = 1;
            _numberOfBytes = 0;
            var byteList = new List<byte>();
            var parser = new Assembly6809TokenParser { InputString = program };

            var token = parser.GetToken();

            // Phase 1 - Scan for labels and build up our label table
            while (token != null)
            {
                string line = "";
                while (token != null && token.TokenName != Assembly6809TokenParser.Tokens.Newline)
                {
                    line += token.TokenValue;
                    token = parser.GetToken();
                }

                line += '\n';
                ParseLine(line, true);
                _lineNumber++;
                token = parser.GetToken();
            }

            _lineNumber = 1;
            parser = new Assembly6809TokenParser { InputString = program };
            token = parser.GetToken();

            // Phase 2 - Actually assemble the program
            while (token != null)
            {
                string line = "";
                while (token != null && token.TokenName != Assembly6809TokenParser.Tokens.Newline)
                {
                    line += token.TokenValue;
                    token = parser.GetToken();
                }

                line += '\n';
                byteList.AddRange(ParseLine(line, false));
                _lineNumber++;
                token = parser.GetToken();
            }

            string zzz  = BitConverter.ToString(byteList.ToArray()).Replace("-", " ");
            return byteList.ToArray();
        }

        private List<byte> ParseLine(string line, bool labelScan)
        {
            var byteList = new List<byte>();
            var parser = new Assembly6809TokenParser { InputString = line };
            var token = parser.GetToken();
            string lastLabel = "";

            while (token != null && token.TokenName != Assembly6809TokenParser.Tokens.Newline)
            {
                if (token.TokenName == Assembly6809TokenParser.Tokens.Identifier && labelScan)
                {
                    if (_labelTable.ContainsKey(token.TokenValue.Trim().ToUpper()))
                    {
                        throw new Exceptions.LabelDefinedMoreThanOnceException(
                            $"The label {token.TokenValue.Trim().ToUpper()} has been defined more than once, on lines {_lineNumberTable[token.TokenValue.Trim().ToUpper()]} and {_lineNumber}");
                    }
                    if (_equateTable.ContainsKey(token.TokenValue.Trim().ToUpper()))
                    {
                        throw new Exceptions.LabelDefinedMoreThanOnceException(
                            $"The label {token.TokenValue.Trim().ToUpper()} has been defined more than once, on lines {_lineNumberTable[token.TokenValue.Trim().ToUpper()]} and {_lineNumber}");
                    }
                    _labelTable.Add(token.TokenValue.Trim().ToUpper(), _numberOfBytes);
                    lastLabel = token.TokenValue.Trim().ToUpper();
                    _lineNumberTable.Add(token.TokenValue.Trim().ToUpper(), _lineNumber);
                    token = parser.GetToken();
                    continue;
                }
                else if (token.TokenName == Assembly6809TokenParser.Tokens.Identifier && !labelScan)
                {
                    token = parser.GetToken();
                    continue;
                }

                string expression;
                ushort parsedExpression;
                byte msb;
                byte lsb;

                switch (token.TokenName)
                {
                    case Assembly6809TokenParser.Tokens.End:
                        SkipWhiteSpace(parser);
                        if (parser.Peek() == null || parser.Peek().TokenPeek == null ||
                            parser.Peek().TokenPeek.TokenName != Assembly6809TokenParser.Tokens.Identifier)
                        {
                            throw new Exceptions.InvalidLabelException($"Invalid label on line {_lineNumber}");
                        }

                        token = parser.GetToken();
                        if (!_labelTable.ContainsKey(token.TokenValue.Trim().ToUpper()) && !labelScan)
                        {
                            throw new Exceptions.UndefinedLabelException($"The label {token.TokenValue.Trim().ToUpper()} is not defined in line {_lineNumber}");
                        }

                        ExecutionAddress = (ushort)(_labelTable[token.TokenValue.Trim().ToUpper()] + _origination);
                        break;
                    case Assembly6809TokenParser.Tokens.Equ:
                        if (string.IsNullOrWhiteSpace(lastLabel) || !_labelTable.ContainsKey(lastLabel))
                        {
                            throw new Exceptions.LabelRequiredException(
                                $"An EQU instruction requires a label to precede it, on line {_lineNumber}!");
                        }
                        _labelTable.Remove(lastLabel);
                        SkipWhiteSpace(parser);
                        token = parser.GetToken();

                        string exp = "";
                        while (token != null && token.TokenName != Assembly6809TokenParser.Tokens.Newline)
                        {
                            exp = exp + token.TokenValue;
                            SkipWhiteSpace(parser);
                            token = parser.GetToken();
                        }

                        _equateTable.Add(lastLabel, ParseExpression(exp, labelScan));
                        lastLabel = "";
                        break;
                    case Assembly6809TokenParser.Tokens.Org:
                        SkipWhiteSpace(parser);
                        expression = GetExpression(parser);
                        _origination = ParseExpression(expression, labelScan);
                        break;
                    case Assembly6809TokenParser.Tokens.Lda:
                        SkipWhiteSpace(parser);
                        if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                            parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.PoundSign) // LDA Immediate
                        {
                            LdaImmediate(parser, byteList, labelScan);
                        }
                        break;
                    case Assembly6809TokenParser.Tokens.Ldx:
                        SkipWhiteSpace(parser);
                        if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                            parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.PoundSign) // LDX Immediate
                        {
                            LdxImmediate(parser, byteList, labelScan);
                        }
                        break;
                    case Assembly6809TokenParser.Tokens.Rts:
                        SkipWhiteSpace(parser);
                        Rts(parser, byteList);
                        IgnoreLine(parser);
                        break;
                    case Assembly6809TokenParser.Tokens.Sta:
                        SkipWhiteSpace(parser);
                        if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                            (parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Comma ||
                             parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Decrement1 ||
                             parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Integer ||
                             parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.BinaryNumber ||
                             parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.HexNumber ||
                             parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Identifier)) // STA Indexed
                        {
                            StaIndexed(parser, byteList, labelScan);
                        }

                        break;
                }

                token = parser.GetToken();
            }

            return byteList;
        }

        private string GetExpression(Assembly6809TokenParser parser)
        {
            string exp = "";

            SkipWhiteSpace(parser);
            var token = parser.GetToken();

            while (token != null && token.TokenName != Assembly6809TokenParser.Tokens.Newline)
            {
                exp = exp + token.TokenValue;
                SkipWhiteSpace(parser);
                token = parser.GetToken();
            }

            return exp;
        }

        private void IgnoreLine(Assembly6809TokenParser parser)
        {
            var pt = parser.Peek();

            while (pt?.TokenPeek != null && pt.TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Newline)
            {
                parser.GetToken();
                pt = parser.Peek();
            }
        }

        private void SkipWhiteSpace(Assembly6809TokenParser parser)
        {
            var pt = parser.Peek();

            while (pt?.TokenPeek != null && pt.TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Whitespace)
            {
                parser.GetToken();
                pt = parser.Peek();
            }
        }

        private void SkipWhiteSpace(ExpressionTokenParser parser)
        {
            var pt = parser.Peek();

            while (pt?.TokenPeek != null && pt.TokenPeek.TokenName == ExpressionTokenParser.Tokens.Whitespace)
            {
                parser.GetToken();
                pt = parser.Peek();
            }
        }

        private ushort ParseExpression(string exp, bool labelScan)
        {
            var parser = new ExpressionTokenParser { InputString = exp };
            var values = new Stack<ushort>();
            var operators = new Stack<ExpressionTokenParser.Tokens>();

            SkipWhiteSpace(parser);
            var token = parser.GetToken();

            while (token != null)
            {
                SkipWhiteSpace(parser);

                if (token.TokenName == ExpressionTokenParser.Tokens.Integer ||
                    token.TokenName == ExpressionTokenParser.Tokens.HexNumber ||
                    token.TokenName == ExpressionTokenParser.Tokens.HexNumber ||
                    token.TokenName == ExpressionTokenParser.Tokens.Identifier)
                {
                    ushort val;

                    switch (token.TokenName)
                    {
                        case ExpressionTokenParser.Tokens.HexNumber:
                            val = ushort.Parse(token.TokenValue.Remove(0, 1), System.Globalization.NumberStyles.HexNumber);
                            break;
                        case ExpressionTokenParser.Tokens.BinaryNumber:
                            val = Convert.ToUInt16(token.TokenValue.Remove(0, 1), 2);
                            break;
                        case ExpressionTokenParser.Tokens.Identifier:
                            if (!_equateTable.ContainsKey(token.TokenValue.Trim().ToUpper()))
                            {
                                if (!labelScan)
                                    throw new Exceptions.UndefinedLabelException(
                                        $"The label {token.TokenValue.Trim().ToUpper()} has not been defined, at line {_lineNumber}!");
                            }

                            val = _equateTable.ContainsKey(token.TokenValue.Trim().ToUpper())
                                ? _equateTable[token.TokenValue.Trim().ToUpper()]
                                : (ushort)0;
                            break;
                        default:
                            val = ushort.Parse(token.TokenValue);
                            break;
                    }

                    values.Push(val);
                }
                else if (token.TokenName == ExpressionTokenParser.Tokens.Lparen)
                {
                    operators.Push(token.TokenName);
                }
                else if (token.TokenName == ExpressionTokenParser.Tokens.Rparen)
                {
                    while (operators.Peek() != ExpressionTokenParser.Tokens.Lparen)
                    {
                        values.Push(ApplyOperation(operators.Pop(), values.Pop(), values.Pop()));
                    }

                    operators.Pop();
                }
                else if (token.TokenName == ExpressionTokenParser.Tokens.Plus ||
                         token.TokenName == ExpressionTokenParser.Tokens.Minus ||
                         token.TokenName == ExpressionTokenParser.Tokens.Asterisk)
                {
                    while (operators.Count > 0 && HasPrecedence(token.TokenName, operators.Peek()))
                    {
                        if (values.Count == 0) values.Push(0);
                        values.Push(ApplyOperation(operators.Pop(), values.Pop(), values.Pop()));
                    }

                    operators.Push(token.TokenName);
                }

                SkipWhiteSpace(parser);
                token = parser.GetToken();
            }

            while (operators.Count > 0)
            {
                values.Push(ApplyOperation(operators.Pop(), values.Pop(), values.Pop()));
            }

            return values.Pop();
        }

        private bool HasPrecedence(ExpressionTokenParser.Tokens operator1, ExpressionTokenParser.Tokens operator2)
        {
            if (operator2 == ExpressionTokenParser.Tokens.Lparen || operator2 == ExpressionTokenParser.Tokens.Rparen)
            {
                return false;
            }

            if (operator1 == ExpressionTokenParser.Tokens.Asterisk && (operator2 == ExpressionTokenParser.Tokens.Plus ||
                operator2 == ExpressionTokenParser.Tokens.Minus))
            {
                return false;
            }

            return true;
        }

        private ushort ApplyOperation(ExpressionTokenParser.Tokens operation, ushort a, ushort b)
        {
            switch (operation)
            {
                case ExpressionTokenParser.Tokens.Plus:
                    return (ushort)(a + b);
                case ExpressionTokenParser.Tokens.Minus:
                    return (ushort)(b - a);
                case ExpressionTokenParser.Tokens.Asterisk:
                    return (ushort)(a * b);
            }

            return 0;
        }

        private ushort ConvertToTwosCompliment(ushort val)
        {
            return (ushort) ((ushort) ~val + 1);
        }

        private byte ConvertToTwosCompliment(byte val)
        {
            return (byte) ((byte) ~val + 1);
        }

        #region Opcode Methods

        private void LdaImmediate(Assembly6809TokenParser parser, List<byte> byteList, bool labelScan)
        {
            parser.GetToken();
            string expression = GetExpression(parser);
            ushort parsedExpression = ParseExpression(expression, labelScan);
            if (parsedExpression > 255)
            {
                throw new Exceptions.OverflowException($"Overflow error at line {_lineNumber}");
            }
            byte lsb = (byte)(parsedExpression & 0xFFu);
            byteList.Add(0x86);
            byteList.Add(lsb);
            _numberOfBytes += 2;
        }

        private void LdxImmediate(Assembly6809TokenParser parser, List<byte> byteList, bool labelScan)
        {
            parser.GetToken();
            string expression = GetExpression(parser);
            ushort parsedExpression = ParseExpression(expression, labelScan);
            byte lsb = (byte)(parsedExpression & 0xFFu);
            byte msb = (byte)((parsedExpression >> 8) & 0xFFu);
            byteList.Add(0x8E);
            byteList.Add(msb);
            byteList.Add(lsb);
            _numberOfBytes += 3;
        }

        private void Rts(Assembly6809TokenParser parser, List<byte> byteList)
        {
            byteList.Add(0x39);
            _numberOfBytes += 1;
        }

        private void StaIndexed(Assembly6809TokenParser parser, List<byte> byteList, bool labelScan)
        {
            ushort indexValue = 0;
            bool negate = false;

            if (parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Comma)
            {
                parser.GetToken();
            }
            else
            {
                string expression = "";
                var token = parser.GetToken();
                if (token.TokenName == Assembly6809TokenParser.Tokens.Decrement1)
                {
                    negate = true;
                    token = parser.GetToken();
                }
                while (token != null && token.TokenName != Assembly6809TokenParser.Tokens.Comma)
                {
                    expression += token.TokenValue;
                    token = parser.GetToken();
                }

                ushort parsedExpression = ParseExpression(expression, labelScan);
                
                indexValue = parsedExpression;
            }

            byte operand = 128;
            bool hasByteOperand = false;
            bool hasWordOperand = false;
            ushort wordOperand = 0;
            byte byteOperand = 0;

            if (indexValue == 0)
            {
                operand = 132;
            }
            else if (indexValue > 0 && indexValue <= 16)
            {
                operand = (byte) indexValue;
                if (negate)
                {
                    //operand = (byte) (operand | 16);
                    operand = ConvertToTwosCompliment(operand);
                    operand = (byte) (operand & 0x1f);
                }
            }
            else if (indexValue >= 17 && indexValue <= 128)
            {
                operand = (byte) (operand | 8);
                byteOperand = (byte) (negate ? ConvertToTwosCompliment((byte)indexValue) : indexValue);
                hasByteOperand = true;
            }
            else if (indexValue >= 129)
            {
                operand = (byte) (operand | 9);
                wordOperand = negate ? ConvertToTwosCompliment(indexValue) : indexValue;
                hasWordOperand = true;
            }

            SkipWhiteSpace(parser);
            var registerToken = parser.GetToken();
            if (registerToken.TokenName != Assembly6809TokenParser.Tokens.Register &&
                registerToken.TokenValue.ToUpper() != "X" && registerToken.TokenValue.ToUpper() != "Y" &&
                registerToken.TokenValue.ToUpper() != "U" && registerToken.TokenValue.ToUpper() != "S")
            {
                throw new Exceptions.InvalidRegisterNameException($"Register is invalid at line {_lineNumber}");
            }

            string register = registerToken.TokenValue.ToUpper();

            switch (register)
            {
                case "X":
                    operand = (byte)(operand & 0x9f);
                    break;
                case "Y":
                    operand = (byte)(operand & 0xbf);
                    break;
                case "U":
                    operand = (byte)(operand & 0xdf);
                    break;
                case "S":
                    operand = (byte)(operand & 0xff);
                    break;
            }

            SkipWhiteSpace(parser);
            if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Increment1)
            {
                if (indexValue > 0)
                {
                    throw new Exceptions.InvalidOperandException($"Invalid Operand: Cannot increment with an offset at line {_lineNumber}");
                }

                operand = (byte)(operand & 0xe0);
                parser.GetToken();
            }
            if (parser.Peek() != null && parser.Peek().TokenPeek != null &&
                parser.Peek().TokenPeek.TokenName == Assembly6809TokenParser.Tokens.Increment2)
            {
                if (indexValue > 0)
                {
                    throw new Exceptions.InvalidOperandException($"Invalid Operand: Cannot increment with an offset at line {_lineNumber}");
                }

                operand = (byte)(operand & 0xe0);
                operand = (byte)(operand | 0x01);
                parser.GetToken();
            }
            byteList.Add(0xA7);
            byteList.Add(operand);

            _numberOfBytes += 2;
            if (hasByteOperand)
            {
                byteList.Add(byteOperand);
                _numberOfBytes += 1;
            } else if (hasWordOperand)
            {
                byteList.Add((byte)((wordOperand >> 8) & 0xFFu));
                byteList.Add((byte)(wordOperand & 0xFFu));
                _numberOfBytes += 2;
            }
        }
        #endregion
    }
}
