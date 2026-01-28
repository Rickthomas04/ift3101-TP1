using LexicalAnalysis.Automata.Transitions;
using LexicalAnalysis.Rules.OperatorNodes;
using LexicalAnalysis.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Automata
{
    public static class AutomatonTools
    {
        public static NondeterministicFiniteAutomaton BuildFromOperatorTree(TokenName tokenName, IOperatorNode rootNode)
        {
            NondeterministicFiniteAutomaton baseAutomaton = MytAlgorithm.BuildFromOperatorTree(rootNode, tokenName);
            NondeterministicFiniteAutomaton augmentedAutomaton = AddOtherTransition(baseAutomaton, tokenName);
            return augmentedAutomaton;
        }

        public static NondeterministicFiniteAutomaton AddOtherTransition(NondeterministicFiniteAutomaton automaton, TokenName tokenName)
        {
            NondeterministicTransitionTable transitions = new([automaton.Transitions]);

            State newAcceptingState = new("other" + tokenName);
            transitions.AddState(newAcceptingState);

            foreach (State acceptingState in automaton.AcceptingStates.Keys)
            {
                foreach (char symbol in Alphabet.Instance.Symbols)
                {
                    if (automaton.Transitions.GetAllNextStates(acceptingState, symbol).Count == 0)
                    {
                        transitions.AddTransition(acceptingState, symbol, newAcceptingState);
                    }
                }
            }

            Dictionary<State, TokenName> acceptingStates = new()
            {
                { newAcceptingState, tokenName },
            };

            return new(automaton.InitialState, acceptingStates, transitions);
        }

        public static NondeterministicFiniteAutomaton MergeAll(HashSet<NondeterministicFiniteAutomaton> automatons)
        {
            NondeterministicTransitionTable transitions = new(automatons.Select(automaton => automaton.Transitions).ToHashSet());

            State initialState = new("init");
            transitions.AddState(initialState);

            Dictionary<State, TokenName> acceptingStates = [];
            foreach (NondeterministicFiniteAutomaton automaton in automatons)
            {
                transitions.AddTransition(initialState, Alphabet.Epsilon, automaton.InitialState);
                foreach (KeyValuePair<State, TokenName> pair in automaton.AcceptingStates)
                {
                    if (!acceptingStates.TryAdd(pair.Key, pair.Value))
                    {
                        throw new Exception($"State {pair.Key.Name} already accept token {acceptingStates[pair.Key]}, cannot also accept token {pair.Value}.");
                    }
                }
            }

            return new(initialState, acceptingStates, transitions);
        }

        public static DeterministicFiniteAutomaton ConvertToDeterministic(NondeterministicFiniteAutomaton automaton)
        {
            return PowersetConstructionAlgorithm.Convert(automaton);
        }

        public static DeterministicFiniteAutomaton Minimize(DeterministicFiniteAutomaton automaton)
        {
            return PartitioningAlgorithm.Minimize(automaton);
        }
    }
}
