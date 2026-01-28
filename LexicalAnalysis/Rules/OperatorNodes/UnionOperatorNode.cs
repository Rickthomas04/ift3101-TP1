using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Rules.OperatorNodes
{
    public class UnionOperatorNode(string pattern, IOperatorNode leftTerm, IOperatorNode rightTerm) : OperatorNode(pattern)
    {
        public IOperatorNode LeftTerm { get; init; } = leftTerm;
        public IOperatorNode RightTerm { get; init; } = rightTerm;
    }
}
