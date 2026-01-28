using LexicalAnalysis.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Rules
{
    public static class Definitions
    {
        public static readonly TokenRule Id = new(TokenName.Id, @"[A-Za-z_][A-Za-z_0-9]*");
        public static readonly TokenRule Number = new(TokenName.Number, @"[0-9]+(\.[0-9]+)?");
        public static readonly TokenRule String = new(TokenName.String, @"""[A-Za-z0-9\. ]*""");
        public static readonly TokenRule Char = new(TokenName.Char, @"'[A-Za-z0-9\. ]'");
        public static readonly TokenRule Arithmetic = new(TokenName.Arithmetic, @"\+|\-|\*|/|%");
        public static readonly TokenRule Relational = new(TokenName.Relational, @"==|!=|<|>|<=|>=");
        public static readonly TokenRule Logical = new(TokenName.Logical, @"&&|\|\||!");
        public static readonly TokenRule Assign = new(TokenName.Assign, @"=|\+=|\-=|\*=|/=|%=|\+\+|\-\-");
        public static readonly TokenRule Punctuation = new(TokenName.Punctuation, @"[\(\){}\[\];:,\.]");
        public static readonly TokenRule Whitespace = new(TokenName.Whitespace, @"[ \\t\\r\\n]+");

        // Rules should be ordered by priority in case of ambiguity.
        public static readonly List<TokenRule> Rules =
        [
            Id,
            Number,
            String,
            Char,
            Arithmetic,
            Relational,
            Logical,
            Assign,
            Punctuation,
            Whitespace,
        ];

        public static TokenName GetHighestPriorityToken(HashSet<TokenName> tokenNames)
        {
            if (tokenNames.Count == 0)
            {
                throw new Exception("Cannot get highest priority token of empty collection.");
            }

            int lowestIndex = Rules.Count;
            TokenName priority = tokenNames.First();

            foreach (TokenName tokenName in tokenNames)
            {
                int index = Rules.FindIndex(rule => rule.Name == tokenName);
                if (index < lowestIndex)
                {
                    lowestIndex = index;
                    priority = tokenName;
                }
            }

            return priority;
        }
    }
}
