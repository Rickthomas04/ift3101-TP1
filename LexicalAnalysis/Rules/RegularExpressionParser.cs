using LexicalAnalysis.Rules.OperatorNodes;
using LexicalAnalysis.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LexicalAnalysis.Rules
{
    internal class ParsingContext
    {
        public string Pattern { get; init; }
        public int Position { get => _position; }
        public RegularExpressionToken CurrentToken { get => _currentToken; }

        private int _position;
        private RegularExpressionToken _currentToken;

        private static readonly Dictionary<char, RegularExpressionTokenName> SymbolToTokenNameMap = new()
        {
            { '(', RegularExpressionTokenName.LeftParenthesis },
            { ')', RegularExpressionTokenName.RightParenthesis },
            { '*', RegularExpressionTokenName.Star },
            { '+', RegularExpressionTokenName.Plus },
            { '?', RegularExpressionTokenName.Optional },
            { '|', RegularExpressionTokenName.Union },
            { '[', RegularExpressionTokenName.LeftSquareBracket },
            { ']', RegularExpressionTokenName.RightSquareBracket },
            { '-', RegularExpressionTokenName.Dash },
            { '\\', RegularExpressionTokenName.Escape },
        };

        public ParsingContext(string pattern)
        {
            Pattern = pattern;
            _position = 0;
            _currentToken = GetCurrentToken();
        }

        public string GetCurrentSubPattern(int start)
        {
            return Pattern[start.._position];
        }

        public void ConsumeToken(RegularExpressionTokenName name)
        {
            if (_currentToken.Name != name) throw new Exception($"Expected token {name} but found token {_currentToken.Name}.");

            _position++;
            _currentToken = GetCurrentToken();
        }

        private RegularExpressionToken GetCurrentToken()
        {
            if (_position >= Pattern.Length)
            {
                return new(RegularExpressionTokenName.End, '\0');
            }
            else
            {
                char currentSymbol = Pattern[_position];
                if (!SymbolToTokenNameMap.TryGetValue(currentSymbol, out RegularExpressionTokenName tokenName))
                {
                    tokenName = RegularExpressionTokenName.Symbol;
                }
                return new(tokenName, currentSymbol);
            }
        }
    }

    public class RegularExpressionParser
    {
        private static readonly Dictionary<string, char> EscapedSymbolsMap = new()
        {
            { "\\\\", '\\' },
            { "\\t", '\t' },
            { "\\r", '\r' },
            { "\\n", '\n' },
        };

        public IOperatorNode GetOperatorTree(string pattern)
        {
            ParsingContext context = new(pattern);
            return ParseExpression(context);
        }

        private IOperatorNode ParseExpression(ParsingContext context)
        {
            // An expression is a sequence of terms connected by the union operator.
            // Example : term1|term2|term3
            int start = context.Position;
            IOperatorNode node = ParseTerm(context);
            if (context.CurrentToken.Name == RegularExpressionTokenName.Union)
            {
                context.ConsumeToken(RegularExpressionTokenName.Union);
                IOperatorNode rightNode = ParseExpression(context);
                string pattern = context.GetCurrentSubPattern(start);
                node = new UnionOperatorNode(pattern, node, rightNode);
            }
            return node;
        }

        private IOperatorNode ParseTerm(ParsingContext context)
        {
            // A term is a sequence of factors concatenated together.
            // Example : factor1factor2factor3
            int start = context.Position;
            IOperatorNode node = ParseFactor(context);
            if (context.CurrentToken.Name == RegularExpressionTokenName.LeftParenthesis ||
                context.CurrentToken.Name == RegularExpressionTokenName.LeftSquareBracket ||
                context.CurrentToken.Name == RegularExpressionTokenName.Escape ||
                context.CurrentToken.Name == RegularExpressionTokenName.Symbol)
            {
                // No token to consume for concatenation.
                //IOperatorNode rightNode = ParseExpression(context);
                IOperatorNode rightNode = ParseTerm(context);
                string pattern = context.GetCurrentSubPattern(start);
                node = new ConcatenationOperatorNode(pattern, node, rightNode);
            }
            return node;
        }

        private IOperatorNode ParseFactor(ParsingContext context)
        {
            // A factor is a symbol or a group modified by an operator.
            // Example : group*
            int start = context.Position;
            IOperatorNode node = ParseOperand(context);
            if (context.CurrentToken.Name == RegularExpressionTokenName.Star)
            {
                context.ConsumeToken(RegularExpressionTokenName.Star);
                string pattern = context.GetCurrentSubPattern(start);
                node = new StarOperatorNode(pattern, node);
            }
            else if (context.CurrentToken.Name == RegularExpressionTokenName.Plus)
            {
                context.ConsumeToken(RegularExpressionTokenName.Plus);
                string pattern = context.GetCurrentSubPattern(start);
                node = new PlusOperatorNode(pattern, node);
            }
            else if (context.CurrentToken.Name == RegularExpressionTokenName.Optional)
            {
                context.ConsumeToken(RegularExpressionTokenName.Optional);
                string pattern = context.GetCurrentSubPattern(start);
                node = new OptionalOperatorNode(pattern, node);
            }
            return node;
        }

        private IOperatorNode ParseOperand(ParsingContext context)
        {
            // An operand is a symbol or a group over an expression.
            // Example : (expression)
            return context.CurrentToken.Name switch
            {
                RegularExpressionTokenName.LeftParenthesis => ParseGroup(context),
                RegularExpressionTokenName.LeftSquareBracket => ParseCharacterSet(context),
                RegularExpressionTokenName.Escape => ParseEscape(context),
                RegularExpressionTokenName.Symbol => ParseSymbol(context),
                _ => throw new Exception($"Found unexpected token {context.CurrentToken.Name}.")
            };
        }

        private GroupOperatorNode ParseGroup(ParsingContext context)
        {
            int start = context.Position;
            context.ConsumeToken(RegularExpressionTokenName.LeftParenthesis);
            IOperatorNode node = ParseExpression(context);
            context.ConsumeToken(RegularExpressionTokenName.RightParenthesis);
            string pattern = context.GetCurrentSubPattern(start); ;
            return new GroupOperatorNode(pattern, node);
        }

        private CharacterSetOperatorNode ParseCharacterSet(ParsingContext context)
        {
            int start = context.Position;
            context.ConsumeToken(RegularExpressionTokenName.LeftSquareBracket);

            List<SymbolOperatorNode> symbols = [];
            SymbolOperatorNode? lastNode = ParsePotentiallyEscapedSymbol(context);
            while (context.CurrentToken.Name != RegularExpressionTokenName.RightSquareBracket)
            {
                if (context.CurrentToken.Name == RegularExpressionTokenName.Dash)
                {
                    if (lastNode is null) throw new Exception($"Found unexpected token {context.CurrentToken.Name}.");

                    context.ConsumeToken(RegularExpressionTokenName.Dash);
                    SymbolOperatorNode endNode = ParsePotentiallyEscapedSymbol(context);
                    List<char> rangeSymbols = Alphabet.Instance.GetSymbolsFromRange(lastNode.Symbol, endNode.Symbol);
                    foreach (char rangeSymbol in rangeSymbols)
                    {
                        symbols.Add(new SymbolOperatorNode(rangeSymbol.ToString(), rangeSymbol));
                    }
                    lastNode = null;
                }
                else
                {
                    if (lastNode is not null)
                    {
                        symbols.Add(lastNode);
                    }
                    lastNode = ParsePotentiallyEscapedSymbol(context);
                }
            }
            if (lastNode is not null)
            {
                symbols.Add(lastNode);
            }

            context.ConsumeToken(RegularExpressionTokenName.RightSquareBracket);
            string pattern = context.GetCurrentSubPattern(start);
            return new CharacterSetOperatorNode(pattern, symbols);
        }

        private SymbolOperatorNode ParsePotentiallyEscapedSymbol(ParsingContext context)
        {
            if (context.CurrentToken.Name == RegularExpressionTokenName.Escape)
            {
                return ParseEscape(context);
            }
            else
            {
                return ParseSymbol(context);
            }
        }

        private SymbolOperatorNode ParseEscape(ParsingContext context)
        {
            int start = context.Position;
            context.ConsumeToken(RegularExpressionTokenName.Escape);

            if (context.CurrentToken.Name != RegularExpressionTokenName.Escape)
            {
                if (context.CurrentToken.Name == RegularExpressionTokenName.End) throw new Exception($"Found unexpected token {context.CurrentToken.Name}.");

                char symbol = context.CurrentToken.Value;

                if (!Alphabet.Instance.Contains(symbol)) throw new Exception($"Symbol {symbol} is not part of the alphabet.");

                context.ConsumeToken(context.CurrentToken.Name);
                string pattern = context.GetCurrentSubPattern(start);           
                return new(pattern, symbol);
            }
            else
            {
                int escapedStart = context.Position;
                context.ConsumeToken(RegularExpressionTokenName.Escape);
                context.ConsumeToken(RegularExpressionTokenName.Symbol);
                string escapedPattern = context.GetCurrentSubPattern(escapedStart);

                if (!EscapedSymbolsMap.TryGetValue(escapedPattern, out char symbol)) throw new Exception($"Escaped pattern {escapedPattern} not recognized.");

                string pattern = context.GetCurrentSubPattern(start);
                return new(pattern, symbol);
            }
        }

        private SymbolOperatorNode ParseSymbol(ParsingContext context)
        {
            if (!Alphabet.Instance.Contains(context.CurrentToken.Value)) throw new Exception($"Symbol {context.CurrentToken.Value} is not part of the alphabet.");

            char symbol = context.CurrentToken.Value;
            SymbolOperatorNode symbolNode = new(symbol.ToString(), symbol);
            context.ConsumeToken(RegularExpressionTokenName.Symbol);
            return symbolNode;
        }
    }
}
