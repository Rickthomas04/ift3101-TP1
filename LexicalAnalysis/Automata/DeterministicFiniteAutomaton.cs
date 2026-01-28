using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalysis.Automata.Transitions;
using LexicalAnalysis.Tokens;

namespace LexicalAnalysis.Automata
{
    public class DeterministicFiniteAutomaton(State initialState, Dictionary<State, TokenName> acceptingStates, DeterministicTransitionTable transitions)
        : FiniteAutomaton(initialState, acceptingStates, transitions)
    {
        
    }
}
