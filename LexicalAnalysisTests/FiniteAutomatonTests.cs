using LexicalAnalysis.Automata;
using LexicalAnalysis.Automata.Transitions;
using LexicalAnalysis.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysisTests
{
    public class FiniteAutomatonTests
    {
        private State etatInitial = new State("etat initial");
        private State etatFinal = new State("etat final");
        private Dictionary<State, TokenName> etatsAcceptant;
        private NondeterministicTransitionTable tableDeTransitions;

        [SetUp]
        public void Setup()
        {

            etatsAcceptant = new Dictionary<State, TokenName> { { etatFinal, TokenName.Id } };
            tableDeTransitions = new NondeterministicTransitionTable();
            tableDeTransitions.AddState(etatFinal);
            tableDeTransitions.AddState(etatInitial);
            tableDeTransitions.AddTransition(etatInitial, 'a', etatFinal);
        }

        [Test]
        public void Constructor_WhenInitialStateNotInTransitions_ShouldThrowException()
        {
            State initialInvalide = new State("etatInitialInvalide");
            try
            {
                new NondeterministicFiniteAutomaton(initialInvalide, etatsAcceptant, tableDeTransitions);
                Assert.Fail();
            } catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo($"Initial state {initialInvalide} is not part of transition table."));
            }
        }

        [Test]
        public void Constructor_WhenAcceptingStateNotInTransitions_ShouldThrowException()
        {
            State finalInvalide = new State("etatFinalInvalide");
            try
            {
                Dictionary<State, TokenName> etatsAcceptantInvalide = new Dictionary<State, TokenName> { { finalInvalide, TokenName.Id } };
                NondeterministicFiniteAutomaton automate = new NondeterministicFiniteAutomaton(etatInitial, etatsAcceptantInvalide, tableDeTransitions);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo($"Accepting state {finalInvalide} is not part of transition table."));
            }
        }
    }
}
