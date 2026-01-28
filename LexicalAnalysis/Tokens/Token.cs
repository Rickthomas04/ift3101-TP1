using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Tokens
{
    public enum TokenName
    {
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
        End,
    }

    public class Token(TokenName name, object value)
    {
        public TokenName Name { get; init; } = name;
        public object Value { get; init; } = value;
    }
}
