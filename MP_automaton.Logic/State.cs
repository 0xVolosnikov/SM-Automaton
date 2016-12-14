using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP_automaton.Logic
{
    class State
    {
        public struct Rule
        {
            public char Symbol;
            public string StackSymbol;
            public List<string> NewStackChain;
            public State State;

            public Rule(char symbol, string stackSymbol, State state, params string[] newStringChain)
            {
                Symbol = symbol;
                State = state;
                StackSymbol = stackSymbol;
                NewStackChain = new List<string>(newStringChain);
            }
        }

        public int Number;
        public List<Rule> Rules = new List<Rule>();

        public State(int number)
        {
            Number = number;
        }
    }
}
