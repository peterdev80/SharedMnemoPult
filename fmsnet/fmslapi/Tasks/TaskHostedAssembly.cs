using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace fmslapi.Tasks
{
    public static class TaskHostedAssembly
    {
        private static MethodInfo _regmethod;

        public static void RegisterHostedAssembly(string Name, byte[] Assembly, byte[] PDB)
        {
            if (_regmethod == null)
            {
                var la =
                    AppDomain.CurrentDomain.GetAssemblies()
                             .FirstOrDefault(x => x.FullName.ToLowerInvariant().Contains("fmsldr"));

                Debug.Assert(la != null, "la != null");

                var gt = la.GetType("fmsldr.AppDomGlue");
                _regmethod = gt.GetMethod("RegisterHostedAssembly", BindingFlags.Static | BindingFlags.Public);
            }

            _regmethod.Invoke(null, new object[] { Name, Assembly, PDB });
        }
    }
}
