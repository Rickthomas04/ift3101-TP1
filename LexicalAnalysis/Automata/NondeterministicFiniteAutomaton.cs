using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalysis.Automata.Transitions;
using LexicalAnalysis.Tokens;

namespace LexicalAnalysis.Automata
{
    public class NondeterministicFiniteAutomaton(State initialState, Dictionary<State, TokenName> acceptingStates, NondeterministicTransitionTable transitions)
        : FiniteAutomaton(initialState, acceptingStates, transitions)
    {
        public HashSet<State> GetEpsilonClosure(HashSet<State> states)
        {
            HashSet<State> closure = new(states);
            Stack<State> unvisited = new(states);

            while (unvisited.Count > 0)
            {
                State currentState = unvisited.Pop();
                foreach (State nextState in Transitions.GetAllNextStates(currentState, Alphabet.Epsilon))
                {
                    if (!closure.Contains(nextState))
                    {
                        closure.Add(nextState);
                        unvisited.Push(nextState);
                    }
                }
            }

            return closure;
        }

        public HashSet<State> GetEpsilonClosureAfterTransition(HashSet<State> states, char symbol)
        {
            HashSet<State> closure = [];

            foreach (State state in states)
            {
                HashSet<State> nextStates = Transitions.GetAllNextStates(state, symbol);
                HashSet<State> stateClosure = GetEpsilonClosure(nextStates);
                closure.UnionWith(stateClosure);
            }

            return closure;
        }
    }
}
