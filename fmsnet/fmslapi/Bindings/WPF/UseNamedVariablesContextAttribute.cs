using System;
using System.Diagnostics;

namespace fmslapi.Bindings.WPF
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class UseNamedVariablesContextAttribute : Attribute
    {
        private string _vdcname;
        private Type _rt;

        public UseNamedVariablesContextAttribute(string Name)
        {
            _vdcname = Name;
        }

        // ReSharper disable once InconsistentNaming
        public UseNamedVariablesContextAttribute(Type RT)
        {
            Debug.Assert(RT != null);

            if (RT == typeof(object))
                return;

            if (!RT.IsAssignableFrom(typeof(IGetVariablesContextName)))
                return;

            _rt = RT;
        }

        public string Name => _vdcname;

        public Type RType => _rt;
    }

    public interface IGetVariablesContextName
    {
        string Name { get; }
    }
}
