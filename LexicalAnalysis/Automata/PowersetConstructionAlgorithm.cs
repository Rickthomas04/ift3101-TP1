using LexicalAnalysis.Automata.Transitions;
using LexicalAnalysis.Rules;
using LexicalAnalysis.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Automata
{
    internal static class PowersetConstructionAlgorithm
    {
        public static DeterministicFiniteAutomaton Convert(NondeterministicFiniteAutomaton automaton)
        {
            DeterministicTransitionTable transitions = new();
            Dictionary<HashSet<State>, State> stateMap = [];

            // Add initial state.
            HashSet<State> initialClosure = automaton.GetEpsilonClosure([automaton.InitialState]);
            State initialState = new(initialClosure);
            transitions.AddState(initialState);
            stateMap.Add(initialClosure, initialState);

            Dictionary<State, TokenName> acceptingStates = [];
            AddAcceptingState(automaton, acceptingStates, initialState, initialClosure);

            // Add other states.
            Stack<HashSet<State>> unmarked = new([initialClosure]);
            while (unmarked.Count > 0)
            {
                HashSet<State> currentClosure = unmarked.Pop();
                foreach (char symbol in Alphabet.Instance.Symbols)
                {
                    HashSet<State> nextClosure = automaton.GetEpsilonClosureAfterTransition(currentClosure, symbol);
                    if (nextClosure.Count > 0)
                    {
                        bool found = false;
                        foreach (HashSet<State> closure in stateMap.Keys)
                        {
                            if (closure.SetEquals(nextClosure))
                            {
                                transitions.AddTransition(stateMap[currentClosure], symbol, stateMap[closure]);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            State newState = new(nextClosure);
                            transitions.AddState(newState);
                            transitions.AddTransition(stateMap[currentClosure], symbol, newState);
                            stateMap.Add(nextClosure, newState);
                            unmarked.Push(nextClosure);
                            AddAcceptingState(automaton, acceptingStates, newState, nextClosure);
                        }
                    }
                }
            }

            return new(initialState, acceptingStates, transitions);
        }

        private static void AddAcceptingState(NondeterministicFiniteAutomaton automaton, Dictionary<State, TokenName> acceptingStates, State currentState, HashSet<State> closure)
        {
            HashSet<TokenName> candidates = [];
            foreach (State state in closure)
            {
                if (automaton.AcceptingStates.TryGetValue(state, out TokenName tokenName))
                {
                    candidates.Add(tokenName);
                }
            }
            if (candidates.Count > 0)
            {
                TokenName priority = Definitions.GetHighestPriorityToken(candidates);
                acceptingStates.Add(currentState, priority);
            }
        }
    }
}
