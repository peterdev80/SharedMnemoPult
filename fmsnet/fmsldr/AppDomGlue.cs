using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace fmsldr
{
    public static class AppDomGlue
    {
        private const string HostedAssembliesID = "hostedassemblies";

        private class HostedAssemblyEntry
        {
            public string Name;
            public byte[] Binary;
            public byte[] PDB;
            public Assembly Assembly;
        }

        public static void InitGlue()
        {
            var cd = AppDomain.CurrentDomain;

            cd.SetData(HostedAssembliesID, new List<HostedAssemblyEntry>());

            RegisterHostedAssembly("glue", cd.GetData("glue") as byte[], null);

            AppDomain.CurrentDomain.AssemblyResolve += AR;

            Assembly.Load("glue");

            //AppDomain.CurrentDomain.AssemblyResolve -= AR;
        }

        private static Assembly AR(object sender, ResolveEventArgs args)
        {
            var l = AppDomain.CurrentDomain.GetData(HostedAssembliesID) as List<HostedAssemblyEntry>;

            Debug.Assert(l != null, "l != null");

            lock (l)
            {
                var a = l.Find(x => args.Name.ToLowerInvariant().StartsWith(x.Name));

                if (a == null)
                    return null;

                if (a.Assembly != null)
                    return a.Assembly;

                a.Assembly = Assembly.Load(a.Binary, a.PDB);
                a.Binary = null;
                a.PDB = null;

                return a.Assembly;
            }
        }

        public static void RegisterHostedAssembly(string Name, byte[] Assembly, byte[] PDB)
        {
            var l = AppDomain.CurrentDomain.GetData(HostedAssembliesID) as List<HostedAssemblyEntry>;
            Debug.Assert(l != null, "l != null");

            lock (l)
            {
                l.Add(new HostedAssemblyEntry { Name = Name, Binary = Assembly, PDB = PDB });
            }
        }
    }

    public interface IAppDomGlue
    {
        Delegate GetMethod(int N);
        void SetMethod(int N, Delegate Method);
    }
}
