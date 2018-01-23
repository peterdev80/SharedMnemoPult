using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace fmslapi.Bindings.WPF
{
    public class VariablesRootDataContext : VariablesDataContext
    {
        private static readonly Dictionary<string, IVariablesChannel> _channels =
            new Dictionary<string, IVariablesChannel>();  

        public string _endpoint;

        internal VariablesRootDataContext(DependencyObject AttachedTo)
            : base(AttachedTo)
        {
            
        }

        public IVariablesChannel GetVariablesChannel(string Name)
        {
            if (_channels.TryGetValue(Name, out var vc))
                return vc;

            vc = Manager.JoinVariablesChannel(Name, _endpoint, Name, null, null);

            _channels.Add(Name, vc);

            return vc;
        }
    }
}
