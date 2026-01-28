using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Automata.Transitions
{
    public class DeterministicTransitionTable : TransitionTable
    {
        public DeterministicTransitionTable() : base() { }

        public DeterministicTransitionTable(HashSet<TransitionTable> others) : base(others) { }

        protected override void ValidateTransition(State source, char symbol, State destination)
        {
            base.ValidateTransition(source, symbol, destination);

            if (!Alphabet.Instance.Contains(symbol))
            {
                throw new Exception($"Symbol {symbol} is not part of alphabet.");
            }

            if (_transitions[(source, symbol)].Count > 0)
            {
                throw new Exception($"Transition from source state {source.Name} with symbol {symbol} already set.");
            }
        }
    }
}
