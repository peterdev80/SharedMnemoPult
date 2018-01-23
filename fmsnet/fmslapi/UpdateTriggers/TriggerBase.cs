using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using ch = fmslapi.Channel;

namespace fmslapi.UpdateTriggers
{
    public class TriggerBase
    {
        private readonly Dictionary<ch.Channel, HashSet<Variable>> _chvars = new Dictionary<ch.Channel,HashSet<Variable>>();

        internal void RemoveVariable(Variable Variable)
        {
        }

        internal void AddVariable(Variable Variable)
        {
        }

        internal void AddChangedVariable(Variable Variable)
        {
            lock (_chvars)
            {
                var c = Variable.Channel;

                if (!_chvars.TryGetValue(c, out var h))
                {
                    h = new HashSet<Variable>();
                    _chvars[c] = h;
                }

                h.Add(Variable);
            }
        }

        private void RaiseChanged(Dictionary<ch.Channel, Variable[]> List)
        {
            foreach (var c in List)
            {
                foreach (var v in c.Value)
                {
                    try
                    {
                        v.RaiseVariableChanged(false);
                    }
                    catch (TargetInvocationException) { }
                }

                c.Key.RaiseDelegate(c.Key.VariablesChanged, false, c.Value.ToArray());
            }
        }

        protected void RaiseChanged(bool SeparateThread = true)
        {
            var cl = new Dictionary<ch.Channel, Variable[]>();

            lock (_chvars)
            {
                foreach (var v in _chvars.Keys)
                {
                    var a = _chvars[v].ToArray();

                    if (a.Length > 0)
                        cl[v] = a;

                    _chvars[v].Clear();
                }
            }

            if (SeparateThread)
                ThreadPool.QueueUserWorkItem(x => RaiseChanged(cl));
            else
                RaiseChanged(cl);
        }
    }
}
