using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Automata.Transitions
{
    public class NondeterministicTransitionTable : TransitionTable
    {
        public NondeterministicTransitionTable() : base() { }

        public NondeterministicTransitionTable(HashSet<TransitionTable> others) : base(others)
        {
            foreach (TransitionTable other in others)
            {
                foreach (State state in other.GetAllStates())
                {
                    foreach (State nextState in other.GetAllNextStates(state, Alphabet.Epsilon))
                    {
                        AddTransition(state, Alphabet.Epsilon, nextState);
                    }
                }
            }
        }

        public override void AddState(State state)
        {
            base.AddState(state);

            _transitions.Add((state, Alphabet.Epsilon), []);
        }

        protected override void ValidateTransition(State source, char symbol, State destination)
        {
            base.ValidateTransition(source, symbol, destination);

            if (symbol != Alphabet.Epsilon && !Alphabet.Instance.Contains(symbol))
            {
                throw new Exception($"Symbol {symbol} is not part of alphabet.");
            }

            if (_transitions[(source, symbol)].Contains(destination))
            {
                throw new Exception($"Transition from source state {source.Name} with symbol {symbol} to destination state {destination.Name} already exists.");
            }
        }
    }
}
