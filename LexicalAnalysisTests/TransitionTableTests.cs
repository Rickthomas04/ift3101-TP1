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
    public class TransitionTableTests
    {
        private State etatInitial = new State("etat initial");
        private State etatFinal = new State("etat final");
        private NondeterministicTransitionTable tableDeTransitions;

        [SetUp]
        public void Setup()
        {
            tableDeTransitions = new NondeterministicTransitionTable();
            tableDeTransitions.AddState(etatFinal);
            tableDeTransitions.AddState(etatInitial);
            tableDeTransitions.AddTransition(etatInitial, 'a', etatFinal);
        }

        [Test]
        public void AddState_WhenStateInTable_ShouldThrowException()
        {
            try
            {
                tableDeTransitions.AddState(etatInitial);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo($"State {etatInitial.Name} already in transition table."));
            }
        }

        [Test]
        public void AddTransition_WhenSourceNotInTable_ShouldThrowException()
        {
            State source = new State("Source");
            try
            {
                tableDeTransitions.AddTransition(source, 'a', etatFinal);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo($"Source state {source.Name} is not part of transition table."));
            }
        }

        [Test]
        public void AddTransition_WhenDestinationNotInTable_ShouldThrowException()
        {
            State destination = new State("Destination");
            try
            {
                tableDeTransitions.AddTransition(etatInitial, 'a', destination);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo($"Destination state {destination.Name} is not part of transition table."));
            }
        }
    }
}
