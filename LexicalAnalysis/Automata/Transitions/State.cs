using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalysis.Automata.Transitions
{
    public class State
    {
        public string Name { get; }

        public State(string name)
        {
            Name = name;
        }

        public State(HashSet<State> states)
        {
            Name = string.Join(" - ", states.Select(state => state.Name));
        }
    }
}
