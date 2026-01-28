using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Rules.OperatorNodes
{
    public class SymbolOperatorNode(string pattern, char symbol) : OperatorNode(pattern)
    {
        public char Symbol { get; init; } = symbol;
    }
}
