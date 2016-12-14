using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP_automaton.Logic
{
    public class Automaton
    {
        private List<State> _states = new List<State>();
        public string LastError;
        public int ErrorNum;

        public Automaton()
        {
            var state = new State(0);
            state.Rules.Add(new State.Rule('x', "z0", state, "x1", "z0"));
            state.Rules.Add(new State.Rule('x', "x1", state, "x2"));
            state.Rules.Add(new State.Rule('x', "x2", state, "x3"));
            state.Rules.Add(new State.Rule('x', "x3", state, "x1", "x3"));
            _states.Add(state);

            state = new State(1);
            _states[0].Rules.Add(new State.Rule('b', "x3", state, "e"));
            state.Rules.Add(new State.Rule('b', "x3", state, "e"));
            _states.Add(state);

            state = new State(2);
            _states.Last().Rules.Add(new State.Rule('0', "z0", state, "z0"));
            _states[0].Rules.Add(new State.Rule('0', "z0", state, "z0"));
            state.Rules.Add(new State.Rule('z', "z0", state, "z","z0"));
            state.Rules.Add(new State.Rule('z', "z", state, "z", "z"));
            _states.Add(state);

            state = new State(3);
            _states.Last().Rules.Add(new State.Rule('a', "z", state, "a1"));
            state.Rules.Add(new State.Rule('a', "z", state, "a1"));
            state.Rules.Add(new State.Rule('a', "a1", state, "a2"));
            state.Rules.Add(new State.Rule('a', "a2", state, "e"));
            _states.Add(state);

            state = new State(4);
            _states[2].Rules.Add(new State.Rule('0', "z0", state, "e"));
            _states.Last().Rules.Add(new State.Rule('0', "z0", state, "e"));
        }

        public bool ProcessChain(string chain)
        {
            List<Branch> branches = new List<Branch>();
            branches.Add(new Branch() { CurrentState = _states[0] });
            //branches[0].stack.Push("z0");
            for (int i = 0; i < chain.Length; i++)
            {
                List<Branch> newBranches = new List<Branch>();
                List<Branch> epsilonClosure = findEpsilonClosure(branches);
                branches.AddRange(epsilonClosure);


                for (int br = 0; br < branches.Count; br++)
                {
                    if (branches[br].Faulted)
                        continue;

                    List<State.Rule> properRules;
                    if (branches[br].stack.Count == 0)
                    {
                        branches[br].Faulted = true;
                        continue;
                    }
                    string stackSymb = branches[br].stack.Pop();
                    properRules = branches[br].CurrentState.Rules.Where(x => x.Symbol == chain[i] && x.StackSymbol ==  stackSymb).ToList();

                    for (int newBr = 1; newBr < properRules.Count; newBr++)
                    {
                        newBranches.Add(new Branch()
                        {
                            CurrentState = properRules[newBr].State,
                            stack = branches[br].stack
                        });

                        for (int s = properRules[newBr].NewStackChain.Count - 1; s >= 0; s--)
                        {
                            if (properRules[0].NewStackChain[s] == "e") continue;
                            newBranches.Last().stack.Push(properRules[newBr].NewStackChain[s]);
                        }
                    }

                    if (properRules.Count == 0)
                    {
                        branches[br].Faulted = true;
                        branches[br].stack.Push(stackSymb);
                        continue;
                    }

                    branches[br].CurrentState = properRules[0].State;
                    for (int s = properRules[0].NewStackChain.Count - 1; s >= 0; s--)
                    {
                        if (properRules[0].NewStackChain[s] == "e") continue;
                        branches[br].stack.Push(properRules[0].NewStackChain[s]);
                    }

                }
                if (branches.All(x => x.Faulted))
                {
                    LastError = DescrError(branches, i+1);
                    ErrorNum = i;
                    return false;
                }

                branches.RemoveAll(x => x.Faulted);
                branches.AddRange(newBranches);
            }

            if (branches.Any(x => x.stack.Count == 0))
                return true;
            else
            {
                LastError = DescrError(branches, chain.Length+1);
                ErrorNum = chain.Length + 1;
                return false;
            }
        }

        private List<Branch> findEpsilonClosure(List<Branch> branches)
        {
            List<Branch> closure = new List<Branch>();
            foreach (Branch br in branches)
            {
                string symb;
                if (br.stack.Count > 0)
                    symb = br.stack.Peek();
                else
                    symb = "e";
                    

                foreach(State.Rule r in br.CurrentState.Rules.Where(x=>x.Symbol == 'e' && x.StackSymbol == symb) )
                {
                    closure.Add(new Branch()
                    {
                        CurrentState = r.State,
                        stack = new Stack<string>()
                    });
                    closure.Last().stack = br.stack;
                    closure.Last().stack.Pop();
                    for (int s = r.NewStackChain.Count - 1; s >= 0; s--)
                    {
                        if (r.NewStackChain[s] == "e") continue;
                        closure.Last().stack.Push(r.NewStackChain[s]);
                    }
                }
            }
            return closure;
        }

        private string DescrError(List<Branch> newBranches, int num)
        {
            string desc = "Ошибка в позиции " + num + ", ожидалось:";
            HashSet<string> symbols = new HashSet<string>();
            List<Branch> branches = new List<Branch>();
            branches.AddRange(newBranches);
            branches.AddRange(findEpsilonClosure(branches));

            for (int br = 0; br < branches.Count; br++)
            {
                if (branches[br].stack.Count == 0) symbols.Add(" конец строки");
                else
                {
                    List<State.Rule> rules;
                    var smb = branches[br].stack.Pop();
                    rules = branches[br].CurrentState.Rules.Where(x => x.StackSymbol == smb).ToList();

                    for (int i = 0; i < rules.Count; i++)
                    {
                        if (rules[i].Symbol == 'e')
                        {
                            var rules2 = rules[i].State.Rules.Where(x => x.StackSymbol == smb).ToList();
                        }
                        else
                        symbols.Add(" " + rules[i].Symbol);
                    }
                }
            }

            if (symbols.Count > 0)
            {
                desc += " " + symbols.First();
                foreach (string s in symbols)
                {
                    if (s == symbols.First()) continue;

                    desc += " или";
                    desc += " " + s;
                }
            }

            return desc;
        }

        class Branch
        {
            public State CurrentState;
            public Stack<string> stack = new Stack<string>();
            public bool Faulted = false;

            public Branch()
            {
                stack.Push("z0");
            }
        }
    }
}
