using System;
using System.Collections.Generic;
using System.Linq;

namespace fmslapi
{
    /// <summary>
    /// Снимок состояния значений списка переменных
    /// </summary>
    public class VariablesSnapshot
    {
        private IVariable[] _snapshotvarslist;
        private readonly IVariablesChannel _chan;
        private readonly string _name;

        internal VariablesSnapshot(IEnumerable<IVariable> Variables, IVariablesChannel Channel, string Name)
        {
            _snapshotvarslist = Variables.ToArray();
            _chan = Channel;
            _name = Name;
        }

        public IVariable[] VariablesInSnapshot
        {
            get => _snapshotvarslist;
            set => _snapshotvarslist = value;
        }

        public string SnapshotName => _name;

        public void MakeSnapshot(IEnumerable<IVariable> Variables)
        {
            _snapshotvarslist = Variables.ToArray();
            MakeSnapshot();
        }

        public void MakeSnapshot()
        {
            var vs = _chan as IVariablesChannelSupport;

            if (vs == null)
                throw new NotSupportedException();

            vs.MakeSnapshot(_name, _snapshotvarslist);
        }

        public void RestoreSnapshot()
        {
            var vs = _chan as IVariablesChannelSupport;

            if (vs == null)
                throw new NotSupportedException();

            vs.RestoreSnapshot(_name);
        }
    }
}
