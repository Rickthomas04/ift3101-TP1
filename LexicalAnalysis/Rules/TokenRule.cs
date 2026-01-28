using LexicalAnalysis.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Rules
{
    public class TokenRule(TokenName name, string pattern)
    {
        public TokenName Name { get; init; } = name;
        public string Pattern { get; init; } = pattern;
    }
}
