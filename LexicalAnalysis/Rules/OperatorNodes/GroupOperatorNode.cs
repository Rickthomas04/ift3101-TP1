using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Rules.OperatorNodes
{
    public class GroupOperatorNode(string pattern, IOperatorNode group) : OperatorNode(pattern)
    {
        public IOperatorNode Group { get; init; } = group;
    }
}
