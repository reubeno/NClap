using System;
using System.Collections.Generic;
using NClap.Exceptions;
using NClap.Utilities;

namespace NClap.Expressions
{
    /// <summary>
    /// Expression parser.
    /// </summary>
    internal class ExpressionParser
    {
        /* ========= [GRAMMAR] =========

        expr => space* inner_expr space*

        inner_expr =
            concat_expr |
            "if" space+ concat_expr space+ "then" space+ inner_expr space+ "else" space+ inner_expr

        concat_expr =>
            unary_expr |
            concat_expr space+ "+" space+ unary_expr

        unary_expr =>
            primary_expr |
            unary_op space* "(" space* unary_expr space* ")"

        primary_expr =>
            "$" variable_name |
            "\"" literal "\"" |
            "(" expr ")"

        variable_name => alnum+

        */

        private const int MaxStateDepth = 50;

        private readonly string _content;
        private ParseState _state;
        private Stack<ParseState> _olderStates = new Stack<ParseState>();

        private ExpressionParser(string content)
        {
            _content = content;
        }

        /// <summary>
        /// Tries to parse an expression out of a given string.
        /// </summary>
        /// <param name="content">String to parse.</param>
        /// <param name="expr">On success, receives the
        /// expression.</param>
        /// <returns>true on success; false otherwise.</returns>
        public static bool TryParse(string content, out Expression expr)
        {
            var parser = new ExpressionParser(content);

            return parser.TryParse(out expr);
        }

        private bool AtEnd => _state.Cursor >= _content.Length;

        private bool TryParse(out Expression expr)
        {
            ResetState();

            var parsedExpr = TryParseExpression();
            if (!parsedExpr.HasValue)
            {
                expr = null;
                return false;
            }

            if (!AtEnd)
            {
                expr = null;
                return false;
            }

            expr = parsedExpr.Value;
            return true;
        }

        private Maybe<Expression> TryParseExpression()
        {
            StartSpeculation();

            Maybe<Expression> innerExpr = new None();

            var result =
                TryConsumeZeroOrMoreWhiteSpaceChars() &&
                (innerExpr = TryParseInnerExpr()).HasValue &&
                TryConsumeZeroOrMoreWhiteSpaceChars();

            if (result)
            {
                CommitSpeculation();
                return innerExpr;
            }
            else
            {
                DiscardSpeculation();
                return new None();
            }
        }

        private Maybe<Expression> TryParseInnerExpr()
        {
            Maybe<Expression> expr;

            expr = Speculate(TryParseIfThenElseExpr);
            if (expr.HasValue) return expr;

            expr = Speculate(TryParseConcatExpr);
            if (expr.HasValue) return expr;

            return new None();
        }

        private Maybe<Expression> TryParseConcatExpr()
        {
            Maybe<Expression> expr;

            expr = Speculate(TryParseConcatOpExpr);
            if (expr.HasValue) return expr;

            expr = Speculate(TryParseUnaryExpr);
            if (expr.HasValue) return expr;

            return new None();
        }

        private Maybe<Expression> TryParseUnaryExpr()
        {
            Maybe<Expression> expr;

            expr = Speculate(TryParseUnaryOpApplicationExpr);
            if (expr.HasValue) return expr;

            expr = Speculate(TryParsePrimaryExpr);
            if (expr.HasValue) return expr;

            return new None();
        }

        private Maybe<Expression> TryParsePrimaryExpr()
        {
            Maybe<Expression> expr;

            expr = Speculate(TryParseVariableExpr);
            if (expr.HasValue) return expr;

            expr = Speculate(TryParseLiteralExpr);
            if (expr.HasValue) return expr;

            expr = Speculate(TryParseParentheticalExpr);
            if (expr.HasValue) return expr;

            return new None();
        }

        private Maybe<Expression> TryParseIfThenElseExpr()
        {
            StartSpeculation();

            Maybe<Expression> condition = new None();
            Maybe<Expression> thenExpr = new None();
            Maybe<Expression> elseExpr = new None();

            var result =
                TryConsumeKeyword(Keywords.If) &&
                TryConsumeOneOrMoreWhiteSpaceChars() &&
                (condition = TryParseConcatExpr()).HasValue &&
                TryConsumeOneOrMoreWhiteSpaceChars() &&
                TryConsumeKeyword(Keywords.Then) &&
                TryConsumeOneOrMoreWhiteSpaceChars() &&
                (thenExpr = TryParseInnerExpr()).HasValue &&
                TryConsumeOneOrMoreWhiteSpaceChars() &&
                TryConsumeKeyword(Keywords.Else) &&
                TryConsumeOneOrMoreWhiteSpaceChars() &&
                (elseExpr = TryParseInnerExpr()).HasValue;

            if (result)
            {
                CommitSpeculation();
                return new ConditionalExpression(condition.Value, thenExpr.Value, elseExpr);
            }
            else
            {
                DiscardSpeculation();
                return new None();
            }
        }

        private Maybe<Expression> TryParseConcatOpExpr()
        {
            StartSpeculation();

            Maybe<Expression> left = new None();
            Maybe<Expression> right = new None();

            var result =
                (left = TryParseUnaryExpr()).HasValue &&
                TryConsumeOneOrMoreWhiteSpaceChars() &&
                TryConsumeKeyword(Symbols.ConcatOperator) &&
                TryConsumeOneOrMoreWhiteSpaceChars() &&
                (right = TryParseConcatExpr()).HasValue;

            if (result)
            {
                CommitSpeculation();
                return new ConcatenationExpression(left.Value, right.Value);
            }
            else
            {
                DiscardSpeculation();
                return new None();
            }
        }

        private Maybe<Expression> TryParseUnaryOpApplicationExpr()
        {
            StartSpeculation();

            Maybe<Operator> op = new None();
            Maybe<Expression> operand = new None();

            var result =
                (op = TryParseUnaryOperator()).HasValue &&
                TryConsumeZeroOrMoreWhiteSpaceChars() &&
                TryConsumeKeyword(Symbols.GroupStart) &&
                (operand = TryParseExpression()).HasValue &&
                TryConsumeKeyword(Symbols.GroupEnd);

            if (result)
            {
                CommitSpeculation();
                return new OperatorExpression(op.Value, operand.Value);
            }
            else
            {
                DiscardSpeculation();
                return new None();
            }
        }

        private Maybe<Expression> TryParseVariableExpr()
        {
            StartSpeculation();

            Maybe<string> name = new None();

            var result =
                TryConsumeKeyword(Symbols.VariablePrefix) &&
                (name = TryParseVariableName()).HasValue;

            if (result)
            {
                CommitSpeculation();
                return new VariableExpression(name.Value);
            }
            else
            {
                DiscardSpeculation();
                return new None();
            }
        }

        private Maybe<Expression> TryParseLiteralExpr()
        {
            StartSpeculation();

            Maybe<string> literal = new None();

            var result =
                TryConsumeKeyword(Symbols.LiteralStart) &&
                (literal = TryParseLiteral()).HasValue &&
                TryConsumeKeyword(Symbols.LiteralEnd);

            if (result)
            {
                CommitSpeculation();
                return new StringLiteral(literal.Value);
            }
            else
            {
                DiscardSpeculation();
                return new None();
            }
        }

        private Maybe<Expression> TryParseParentheticalExpr()
        {
            StartSpeculation();

            Maybe<Expression> innerExpr = new None();

            var result =
                TryConsumeKeyword(Symbols.GroupStart) &&
                (innerExpr = TryParseExpression()).HasValue &&
                TryConsumeKeyword(Symbols.GroupEnd);

            if (result)
            {
                CommitSpeculation();
                return new ParenthesisExpression(innerExpr.Value);
            }
            else
            {
                DiscardSpeculation();
                return new None();
            }
        }

        private Maybe<Operator> TryParseUnaryOperator()
        {
            if (TryConsumeKeyword(Operators.LowerCase)) return Operator.ConvertToLowerCase;
            if (TryConsumeKeyword(Operators.UpperCase)) return Operator.ConvertToUpperCase;

            return new None();
        }

        private Maybe<string> TryParseVariableName() =>
            ConsumeCharsWhile(VariableExpression.IsValidInVariableName);

        private Maybe<string> TryParseLiteral()
        {
            var chars = new List<char>();
            var inEscapeSequence = false;
            while (!AtEnd)
            {
                var c = Peek();

                if (inEscapeSequence)
                {
                    switch (c)
                    {
                        case '\'':
                        case '\"':
                        case '\\':
                            chars.Add(c);
                            inEscapeSequence = false;
                            break;
                        case 'n':
                            chars.Add('\n');
                            inEscapeSequence = false;
                            break;
                        case 't':
                            chars.Add('\t');
                            inEscapeSequence = false;
                            break;
                        default:
                            return new None();
                    }
                }
                else if (c == '\\')
                {
                    inEscapeSequence = true;
                }
                else if (c == Symbols.InvalidLiteralChar)
                {
                    break;
                }
                else
                {
                    chars.Add(c);
                }

                ConsumeChar();
            }

            if (inEscapeSequence)
            {
                return new None();
            }

            return new string(chars.ToArray());
        }

        private bool TryConsumeKeyword(string keyword) =>
            TryConsumeString(keyword, StringComparison.Ordinal);

        private bool TryConsumeString(string value, StringComparison comparison)
        {
            if (_state.Cursor + value.Length > _content.Length)
            {
                return false;
            }

            var possibleMatch = _content.Substring(_state.Cursor, value.Length);
            if (!possibleMatch.Equals(value, comparison))
            {
                return false;
            }

            _state.Cursor += possibleMatch.Length;

            return true;
        }

        private bool TryConsumeOneOrMoreWhiteSpaceChars() =>
            TryConsumeCountedWhiteSpaceChars(1);

        private bool TryConsumeZeroOrMoreWhiteSpaceChars() =>
            TryConsumeCountedWhiteSpaceChars(0);

        private bool TryConsumeCountedWhiteSpaceChars(int minCount)
        {
            var charsConsumed = 0;

            while (!AtEnd && ConsumeCharIf(char.IsWhiteSpace))
            {
                ++charsConsumed;
            }

            return charsConsumed >= minCount;
        }

        private Maybe<string> ConsumeCharsWhile(Predicate<char> predicate)
        {
            var chars = new List<char>();
            while (ConsumeCharIf(predicate, out char c))
            {
                chars.Add(c);
            }

            if (chars.Count == 0)
            {
                return new None();
            }

            return new string(chars.ToArray());
        }

        private bool ConsumeCharIf(Predicate<char> predicate) =>
            ConsumeCharIf(predicate, out char _);

        private bool ConsumeCharIf(Predicate<char> predicate, out char consumedChar)
        {
            if (AtEnd)
            {
                consumedChar = default(char);
                return false;
            }

            var c = Peek();
            if (predicate(c))
            {
                ConsumeChar();

                consumedChar = c;
                return true;
            }
            else
            {
                consumedChar = default(char);
                return false;
            }
        }

        private char ConsumeChar()
        {
            if (AtEnd) throw new InternalInvariantBrokenException();
            return _content[_state.Cursor++];
        }

        private char Peek()
        {
            if (AtEnd) throw new InternalInvariantBrokenException();
            return _content[_state.Cursor];
        }

        private void ResetState()
        {
            _state = default(ParseState);
            _olderStates.Clear();
        }

        private Maybe<T> Speculate<T>(Func<Maybe<T>> func)
        {
            StartSpeculation();

            var result = func.Invoke();

            if (result.HasValue)
            {
                CommitSpeculation();
            }
            else
            {
                DiscardSpeculation();
            }

            return result;
        }

        private void StartSpeculation()
        {
            // To avoid failing with a StackOverflowException in the event of a logic
            // error, we detect having a deeper stack than we should ever see.
            if (_olderStates.Count > MaxStateDepth)
            {
                throw new InternalInvariantBrokenException();
            }

            _olderStates.Push(_state);
        }

        private void CommitSpeculation() => _olderStates.Pop();

        private void DiscardSpeculation() => _state = _olderStates.Pop();

        private static class Keywords
        {
            public const string If = "if";
            public const string Then = "then";
            public const string Else = "else";
        }

        private static class Operators
        {
            public const string LowerCase = "lower";
            public const string UpperCase = "upper";
        }

        private static class Symbols
        {
            public const string ConcatOperator = "+";
            public const string GroupStart = "(";
            public const string GroupEnd = ")";
            public const string VariablePrefix = "$";
            public const string LiteralStart = "\"";
            public const string LiteralEnd = "\"";
            public const char InvalidLiteralChar = '\"';
        }

        private struct ParseState
        {
            public int Cursor { get; set; }
        }
    }
}
