using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Rules.OperatorNodes
{
    public class StarOperatorNode(string pattern, IOperatorNode operand) : ModifierOperatorNode(pattern, operand) { }
}
