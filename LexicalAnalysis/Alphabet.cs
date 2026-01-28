using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis
{
    public class Alphabet
    {
        public const char Epsilon = '\0';

        public static readonly List<char> UpperLettersRange = new("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        public static readonly List<char> LowerLettersRange = new("abcdefghijklmnopqrstuvwxyz");
        public static readonly List<char> DigitsRange = new("0123456789");
        public static readonly HashSet<char> SpecialSymbols = [.. ",.;:\"'=<>+-*/%_?!|&()[]{} \t\r\n"];

        public static Alphabet Instance { get => _instance.Value; }

        public HashSet<char> Symbols { get; init; }
        public List<List<char>> Ranges { get; init; }

        private static readonly Lazy<Alphabet> _instance = new(() => new());

        private Alphabet()
        {
            Ranges = [UpperLettersRange, LowerLettersRange, DigitsRange];
            HashSet<char> symbols = SpecialSymbols;
            foreach (List<char> range in Ranges)
            {
                symbols.UnionWith(range);
            }
            Symbols = symbols;
        }

        public List<char> GetSymbolsFromRange(char begin, char end)
        {
            if (!Contains(begin)) throw new Exception($"Begin symbol {begin} is not part of the alphabet.");
            if (!Contains(end)) throw new Exception($"End symbol {end} is not part of the alphabet.");

            foreach (List<char> range in Ranges)
            {
                int beginIndex = range.IndexOf(begin);
                if (beginIndex != -1)
                {
                    int endIndex = range.IndexOf(end);
                    if (beginIndex == -1) throw new Exception($"End symbol {end} could not be found in range of begin symbol {begin}.");

                    // Ranges with the same begin and end symbols are permitted.
                    if (endIndex < beginIndex) throw new Exception($"Range from begin symbol {begin} to end symbol {end} is in reverse order.");

                    int count = endIndex - beginIndex + 1;
                    return range.GetRange(beginIndex, count);
                }
            }
            throw new Exception($"No range found for begin symbol {begin}.");
        }

        public bool Contains(char symbol)
        {
            return Symbols.Contains(symbol);
        }
    }
}
