using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Rules.OperatorNodes
{
    public class CharacterSetOperatorNode(string pattern, List<SymbolOperatorNode> symbols) : OperatorNode(pattern)
    {
        public List<SymbolOperatorNode> Symbols { get; init; } = symbols;
    }
}
