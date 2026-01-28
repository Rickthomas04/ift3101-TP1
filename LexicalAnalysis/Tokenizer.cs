using LexicalAnalysis.Automata;
using LexicalAnalysis.Automata.Transitions;
using LexicalAnalysis.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis
{
    public class Tokenizer(InputReader inputReader, DeterministicFiniteAutomaton automaton)
    {
        private readonly InputReader _inputReader = inputReader;
        private readonly DeterministicFiniteAutomaton _automaton = automaton;

        public Token GetNextToken()
        {
            State? _currentState = _automaton.InitialState;
            State? lastAcceptingState = _automaton.AcceptingStates.ContainsKey(_currentState) ? _currentState : null;
            int retractNeeded = 0;
            while (_currentState != null && _inputReader.CurrentChar != InputReader.SpecialMarker)
            {
                _inputReader.AdvanceForwardPointer();
                if (_inputReader.CurrentChar != InputReader.SpecialMarker)
                {
                    HashSet<State> nextStates = _automaton.Transitions.GetAllNextStates(_currentState, _inputReader.CurrentChar);
                    _currentState = nextStates.Count > 0 ? nextStates.First() : null;
                    if (_currentState != null && _automaton.AcceptingStates.ContainsKey(_currentState))
                    {
                        lastAcceptingState = _currentState;
                        retractNeeded = 0;
                    }

                    retractNeeded++;
                }
            }

            if (lastAcceptingState != null)
            {
                _inputReader.RetractForwardPointer(retractNeeded);
                string lexeme = _inputReader.ConsumeLexeme();
                TokenName tokenName = _automaton.AcceptingStates[lastAcceptingState];
                return new(tokenName, lexeme);
            }

            if (_inputReader.CurrentChar == InputReader.SpecialMarker)
            {
                _inputReader.CloseFile();
                return new(TokenName.End, InputReader.SpecialMarker);
            }

            throw new Exception("Unable to generate next token.");
        }
    }
}
