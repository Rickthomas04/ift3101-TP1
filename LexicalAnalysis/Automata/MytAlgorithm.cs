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
    internal static class MytAlgorithm
    {
        public static NondeterministicFiniteAutomaton BuildFromOperatorTree(IOperatorNode rootNode, TokenName tokenName)
        {
            return BuildOperator(rootNode, tokenName);
        }

        private static NondeterministicFiniteAutomaton BuildOperator(IOperatorNode node, TokenName tokenName)
        {
            return node switch
            {
                GroupOperatorNode groupNode => BuildGroup(groupNode, tokenName),
                StarOperatorNode starNode => BuildStar(starNode, tokenName),
                PlusOperatorNode plusNode => BuildPlus(plusNode, tokenName),
                OptionalOperatorNode optionalNode => BuildOptional(optionalNode, tokenName),
                ConcatenationOperatorNode concatenationNode => BuildConcatenation(concatenationNode, tokenName),
                UnionOperatorNode unionNode => BuildUnion(unionNode, tokenName),
                CharacterSetOperatorNode characterSetNode => BuildCharacterSet(characterSetNode, tokenName),
                SymbolOperatorNode symbolNode => BuildSymbol(symbolNode, tokenName),
                _ => throw new Exception("Operation not supported.")
            };
        }

        private static NondeterministicFiniteAutomaton BuildGroup(GroupOperatorNode node, TokenName tokenName)
        {
            NondeterministicFiniteAutomaton automate = BuildOperator(node.Group, tokenName);
            return automate;
        }

        private static NondeterministicFiniteAutomaton BuildStar(StarOperatorNode node, TokenName tokenName)
        {
            NondeterministicFiniteAutomaton automate = BuildOperator(node.Operand, tokenName);
            State initial = new State("initial" + node.Pattern);
            State final = new State("final" + node.Pattern);

            Dictionary<State, TokenName> etatsAcceptant = new Dictionary<State, TokenName> { { final, tokenName } };

            NondeterministicTransitionTable tableDeTransitions = new NondeterministicTransitionTable( new HashSet<TransitionTable> { automate.Transitions });
            tableDeTransitions.AddState(initial);
            tableDeTransitions.AddState(final);

            foreach (State state in automate.AcceptingStates.Keys)
            {
                tableDeTransitions.AddTransition(state, Alphabet.Epsilon, final);
                tableDeTransitions.AddTransition(state, Alphabet.Epsilon, automate.InitialState);
            }

            tableDeTransitions.AddTransition(initial, Alphabet.Epsilon, final);
            tableDeTransitions.AddTransition(initial, Alphabet.Epsilon, automate.InitialState);
            return new NondeterministicFiniteAutomaton(initial, etatsAcceptant, tableDeTransitions);
        }

        private static NondeterministicFiniteAutomaton BuildPlus(PlusOperatorNode node, TokenName tokenName)
        {
            NondeterministicFiniteAutomaton automate1 = BuildOperator(node.Operand, tokenName);
            NondeterministicFiniteAutomaton automate2 = BuildOperator(node.Operand, tokenName);
            State final = new State("final" + node.Pattern);
            State initial1 = automate1.InitialState;
            State initial2 = automate2.InitialState;
            foreach (State state in automate2.AcceptingStates.Keys)
            {
                automate2.Transitions.AddTransition(state, Alphabet.Epsilon, automate2.InitialState);
            }
            
            Dictionary<State, TokenName> etatsAcceptant = new Dictionary<State, TokenName> { { final, tokenName } };
            NondeterministicTransitionTable tableDeTransitions = new NondeterministicTransitionTable(new HashSet<TransitionTable> { automate1.Transitions, automate2.Transitions });
            tableDeTransitions.AddState(final);
            NondeterministicFiniteAutomaton automateFinal = new NondeterministicFiniteAutomaton(automate1.InitialState, etatsAcceptant, tableDeTransitions);
            foreach (State state in automate1.AcceptingStates.Keys)
            {
                automateFinal.Transitions.AddTransition(state, Alphabet.Epsilon, final);
                automateFinal.Transitions.AddTransition(state, Alphabet.Epsilon, automate2.InitialState);
            }
            foreach (State state in automate2.AcceptingStates.Keys)
            {
                automateFinal.Transitions.AddTransition(state, Alphabet.Epsilon, final);
            }
            return automateFinal;
        }

        private static NondeterministicFiniteAutomaton BuildOptional(OptionalOperatorNode node, TokenName tokenName)
        {
            NondeterministicFiniteAutomaton automate = BuildOperator(node.Operand, tokenName);
            State initial = new State("initial" + node.Pattern);
            State final = new State("final" + node.Pattern);
            Dictionary<State, TokenName> etatsAcceptant = new Dictionary<State, TokenName> { { final, tokenName } };
            NondeterministicTransitionTable tableDeTransitions = new NondeterministicTransitionTable(new HashSet<TransitionTable> { automate.Transitions });
            tableDeTransitions.AddState(initial);
            tableDeTransitions.AddState(final);
            foreach (State state in automate.AcceptingStates.Keys)
            {
                tableDeTransitions.AddTransition(state, Alphabet.Epsilon, final);
            }
            tableDeTransitions.AddTransition(initial, Alphabet.Epsilon, final);
            tableDeTransitions.AddTransition(initial, Alphabet.Epsilon, automate.InitialState);
            return new NondeterministicFiniteAutomaton(initial, etatsAcceptant, tableDeTransitions);
        }

        private static NondeterministicFiniteAutomaton BuildConcatenation(ConcatenationOperatorNode node, TokenName tokenName)
        {
            NondeterministicFiniteAutomaton automateGauche = BuildOperator(node.LeftFactor, tokenName);
            NondeterministicFiniteAutomaton automateDroite = BuildOperator(node.RightFactor, tokenName);
            NondeterministicTransitionTable tableDeTransitions = new NondeterministicTransitionTable(new HashSet<TransitionTable> { automateGauche.Transitions, automateDroite.Transitions });
            foreach (State state in automateGauche.AcceptingStates.Keys)
            {
                tableDeTransitions.AddTransition(state, Alphabet.Epsilon, automateDroite.InitialState);
            }
            return new NondeterministicFiniteAutomaton(automateGauche.InitialState, automateDroite.AcceptingStates, tableDeTransitions);
        }

        private static NondeterministicFiniteAutomaton BuildUnion(UnionOperatorNode node, TokenName tokenName)
        {
            NondeterministicFiniteAutomaton automateGauche = BuildOperator(node.LeftTerm, tokenName);
            NondeterministicFiniteAutomaton automateDroite = BuildOperator(node.RightTerm, tokenName);
            State initial = new State("initial" + node.Pattern);
            State final = new State("final" + node.Pattern);
            Dictionary<State, TokenName> etatsAcceptant = new Dictionary<State, TokenName> { { final, tokenName } };
            NondeterministicTransitionTable tableDeTransitions = new NondeterministicTransitionTable(new HashSet<TransitionTable> { automateGauche.Transitions, automateDroite.Transitions });
            tableDeTransitions.AddState(initial);
            tableDeTransitions.AddState(final);
            foreach(State state in automateGauche.AcceptingStates.Keys)
            {
                tableDeTransitions.AddTransition(state, Alphabet.Epsilon, final);
            }
            foreach (State state in automateDroite.AcceptingStates.Keys)
            {
                tableDeTransitions.AddTransition(state, Alphabet.Epsilon, final);
            }
            tableDeTransitions.AddTransition(initial, Alphabet.Epsilon, automateGauche.InitialState);
            tableDeTransitions.AddTransition(initial, Alphabet.Epsilon, automateDroite.InitialState);
            return new NondeterministicFiniteAutomaton(initial, etatsAcceptant, tableDeTransitions);
        }

        private static NondeterministicFiniteAutomaton BuildCharacterSet(CharacterSetOperatorNode node, TokenName tokenName)
        {
            State etatinitial = new State("initial" + node.Pattern);
            State etatfinal = new State("final" + node.Pattern);
            Dictionary<State, TokenName> etatsAcceptant = new Dictionary<State, TokenName> { { etatfinal, tokenName } };
            NondeterministicTransitionTable tableDeTransitions = new NondeterministicTransitionTable();
            tableDeTransitions.AddState(etatinitial);
            tableDeTransitions.AddState(etatfinal);
            NondeterministicFiniteAutomaton automate = new NondeterministicFiniteAutomaton(etatinitial, etatsAcceptant, tableDeTransitions);
            foreach (SymbolOperatorNode symbolnode in node.Symbols)
            {
                tableDeTransitions.AddTransition(etatinitial, symbolnode.Symbol, etatfinal);
            }
            return new NondeterministicFiniteAutomaton(etatinitial, etatsAcceptant, tableDeTransitions); ;
        }

        private static NondeterministicFiniteAutomaton BuildSymbol(SymbolOperatorNode node, TokenName tokenName)
        {
            State etatinitial = new State("initial" + node.Pattern);
            State etatfinal = new State("final" + node.Pattern);
            Dictionary<State, TokenName> etatsAcceptant = new Dictionary<State, TokenName> { { etatfinal, tokenName } };
            NondeterministicTransitionTable tableDeTransitions = new NondeterministicTransitionTable();
            tableDeTransitions.AddState(etatfinal);
            tableDeTransitions.AddState(etatinitial);
            tableDeTransitions.AddTransition(etatinitial, node.Symbol , etatfinal);
            NondeterministicFiniteAutomaton automate = new NondeterministicFiniteAutomaton(etatinitial, etatsAcceptant, tableDeTransitions);

            return automate;
        }
    }
}
