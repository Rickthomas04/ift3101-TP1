using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalysis.Automata.Transitions;
using LexicalAnalysis.Tokens;

namespace LexicalAnalysis.Automata
{
    public abstract class FiniteAutomaton
    {
        public State InitialState { get; }
        public Dictionary<State, TokenName> AcceptingStates { get; }
        public TransitionTable Transitions { get; }

        public FiniteAutomaton(State initialState, Dictionary<State, TokenName> acceptingStates, TransitionTable transitions)
        {
            if (!transitions.ContainsState(initialState))
            {
                throw new Exception($"Initial state {initialState} is not part of transition table.");
            }

            foreach (State acceptingState in acceptingStates.Keys)
            {
                if (!transitions.ContainsState(acceptingState))
                {
                    throw new Exception($"Accepting state {acceptingState} is not part of transition table.");
                }
            }

            InitialState = initialState;
            AcceptingStates = acceptingStates;
            Transitions = transitions;
        }
    }
}
