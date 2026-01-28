using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Automata.Transitions
{
    public abstract class TransitionTable
    {
        protected readonly Dictionary<(State State, char Symbol), HashSet<State>> _transitions;

        public TransitionTable()
        {
            _transitions = [];
        }

        public TransitionTable(HashSet<TransitionTable> others)
        {
            _transitions = [];

            // Add all states before transitions.
            foreach (TransitionTable other in others)
            {
                foreach (State state in other.GetAllStates())
                {
                    AddState(state);
                }
            }

            // Add all transitions.
            foreach (TransitionTable other in others)
            {
                foreach (State state in other.GetAllStates())
                {
                    foreach (char symbol in Alphabet.Instance.Symbols)
                    {
                        foreach (State nextState in other.GetAllNextStates(state, symbol))
                        {
                            AddTransition(state, symbol, nextState);
                        }
                    }
                }
            }
        }

        public HashSet<State> GetAllStates()
        {
            return _transitions.Keys.Select(key => key.State).ToHashSet();
        }

        public HashSet<State> GetAllNextStates(State source, char symbol)
        {
            return _transitions[(source, symbol)];
        }

        public bool ContainsState(State state)
        {
            return _transitions.Keys.Any(key => key.State == state);
        }

        public virtual void AddState(State state)
        {
            if (ContainsState(state))
            {
                throw new Exception($"State {state.Name} already in transition table.");
            }

            foreach (char symbol in Alphabet.Instance.Symbols)
            {
                _transitions.Add((state, symbol), []);
            }
        }

        public void AddTransition(State source, char symbol, State destination)
        {
            ValidateTransition(source, symbol, destination);

            _transitions[(source, symbol)].Add(destination);
        }

        protected virtual void ValidateTransition(State source, char symbol, State destination)
        {
            if (!ContainsState(source))
            {
                throw new Exception($"Source state {source.Name} is not part of transition table.");
            }

            if (!ContainsState(destination))
            {
                throw new Exception($"Destination state {destination.Name} is not part of transition table.");
            }
        }
    }
}
