using LexicalAnalysis.Automata.Transitions;
using LexicalAnalysis.Rules;
using LexicalAnalysis.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Automata
{
    internal static class PartitioningAlgorithm
    {
        public static DeterministicFiniteAutomaton Minimize(DeterministicFiniteAutomaton automaton)
        {
            // Build partition.
            HashSet<HashSet<State>> partition = GetPartition(automaton);

            // Build states.
            DeterministicTransitionTable transitions = new();
            Dictionary<State, TokenName> acceptingStates = [];

            Dictionary<State, State> stateMap = [];
            foreach (HashSet<State> group in partition)
            {
                State minimalState = new(group);
                transitions.AddState(minimalState);

                HashSet<TokenName> candidates = [];
                foreach (State state in group)
                {
                    stateMap.Add(state, minimalState);
                    if (automaton.AcceptingStates.TryGetValue(state, out TokenName tokenName))
                    {
                        candidates.Add(tokenName);
                    }
                }

                if (candidates.Count > 0)
                {
                    TokenName priority = Definitions.GetHighestPriorityToken(candidates);
                    acceptingStates.Add(minimalState, priority);
                }
            }

            // Add transitions.
            foreach (KeyValuePair<State, State> pair in stateMap)
            {
                foreach (char symbol in Alphabet.Instance.Symbols)
                {
                    if (transitions.GetAllNextStates(pair.Value, symbol).Count == 0)
                    {
                        HashSet<State> nextStates = automaton.Transitions.GetAllNextStates(pair.Key, symbol);
                        if (nextStates.Count > 0)
                        {
                            State nextState = nextStates.First();
                            transitions.AddTransition(pair.Value, symbol, stateMap[nextState]);
                        }
                    }
                }
            }

            return new(stateMap[automaton.InitialState], acceptingStates, transitions);
        }

        private static HashSet<HashSet<State>> GetPartition(DeterministicFiniteAutomaton automaton)
        {
            // Split states for initial partition.
            // Accepting states producing different tokens should be part of different groups.
            HashSet<State> states = automaton.Transitions.GetAllStates();
            HashSet<State> unacceptingStates = states.Except(automaton.AcceptingStates.Keys).ToHashSet();
            HashSet<HashSet<State>> partition = [unacceptingStates];
            Dictionary<TokenName, HashSet<State>> acceptingGroups = [];
            foreach (KeyValuePair<State, TokenName> pair in automaton.AcceptingStates)
            {
                if (acceptingGroups.TryGetValue(pair.Value, out HashSet<State>? acceptingGroup))
                {
                    acceptingGroup.Add(pair.Key);
                }
                else
                {
                    acceptingGroups.Add(pair.Value, [pair.Key]);
                }
            }
            partition.UnionWith(acceptingGroups.Values);

            bool Done = false;
            while (!Done)
            {
                Done = true;
                HashSet<HashSet<State>> newPartition = new HashSet<HashSet<State>> { };
                foreach (var group in partition)
                {
                    var splitGroup = SplitIntoSubGroups(group, partition, automaton);
                    newPartition.UnionWith(splitGroup);
                    if (splitGroup.Count() > 1)
                    {
                        Done = false;
                    }
                }
                partition = newPartition;
            }

            return partition;
        }

        private static HashSet<HashSet<State>> SplitIntoSubGroups(HashSet<State> group, HashSet<HashSet<State>> partition, DeterministicFiniteAutomaton automaton)
        {
            Dictionary<HashSet<State>, HashSet<State>> newGroups = new Dictionary<HashSet<State>, HashSet<State>> { };
            HashSet<HashSet<State>> newPartition;
            foreach (State state in group)
            {
                List<HashSet<State>> groupList = new List<HashSet<State>> { };
                foreach (char symbol in Alphabet.Instance.Symbols)
                {
                    HashSet<State> neighborStates = automaton.Transitions.GetAllNextStates(state, symbol);
                    if (neighborStates.Count() > 0)
                    {
                        foreach (State neighborState in neighborStates)
                        {
                            groupList.Add(GetGroupOfState(neighborState, partition));
                        }
                    }
                }
                bool newStateIsFound = false;
                HashSet<State> groupKeys = groupList.SelectMany(set => set).ToHashSet();
                foreach (var key in newGroups.Keys)
                {
                    if (key.SetEquals(groupKeys))
                    {
                        bool distinguishable = false;
                        foreach (State existingState in newGroups[key])
                        {
                            distinguishable = AreDistinguishable(existingState, state, partition, automaton);
                        }
                        if (!distinguishable)
                        {
                            newGroups[key].Add(state);
                            newStateIsFound = true;
                        }
                    }
                }
                if (!newStateIsFound)
                {
                    newGroups[groupKeys] = new HashSet<State> { state };
                }
            }
            newPartition = new HashSet<HashSet<State>>  (newGroups.Values) ;
            return newPartition;
        }

        private static bool AreDistinguishable(State firstState, State secondState, HashSet<HashSet<State>> partition, DeterministicFiniteAutomaton automaton)
        {
            foreach (var symbol in Alphabet.Instance.Symbols)
            {
             var neighborState1 = automaton.Transitions.GetAllNextStates(firstState, symbol);
             var neighborState2 = automaton.Transitions.GetAllNextStates(secondState, symbol);
             if ((neighborState1.Count() > 0) && (neighborState2.Count() > 0))
                {
                    foreach (State state1 in neighborState1)
                    {
                        foreach(State state2 in neighborState2)
                        {
                            var group1 = GetGroupOfState(state1, partition);
                            var group2 = GetGroupOfState(state2, partition);
                            if (!(group1.SetEquals(group2)))
                            {
                                return true;
                            } else if (neighborState1.Count() != neighborState2.Count())
                            {
                                return true;
                            }
                        }
                    }
                } else if (neighborState1.Count() != neighborState2.Count())
                {
                    return true;
                }
            }
                return false;
        }

        private static HashSet<State> GetGroupOfState(State state, HashSet<HashSet<State>> partition)
        {
            foreach (HashSet<State> group in partition)
            {
                if (group.Contains(state))
                {
                    return group;
                }
            }
            throw new Exception($"State {state.Name} not found in partition.");
        }
    }
}
