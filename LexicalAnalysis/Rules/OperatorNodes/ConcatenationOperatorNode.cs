using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Rules.OperatorNodes
{
    public class ConcatenationOperatorNode(string pattern, IOperatorNode leftFactor, IOperatorNode rightFactor) : OperatorNode(pattern)
    {
        public IOperatorNode LeftFactor { get; init; } = leftFactor;
        public IOperatorNode RightFactor { get; init; } = rightFactor;
    }
}
