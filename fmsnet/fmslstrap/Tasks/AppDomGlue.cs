using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace fmslstrap.Tasks
{
    public class AppDomGlue
    {
        private readonly AppDomain _remotedom;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly fmsldr.IAppDomGlue _remoteglue;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly fmsldr.IAppDomGlue _selfglue;

        private static readonly byte[] _g;

        private static readonly Assembly _glueasm;

        static AppDomGlue()
        {
            _g = GetResource("glue");

            AppDomain.CurrentDomain.AssemblyResolve += AR;

            _glueasm = Assembly.Load(_g, null);
        }

        public AppDomGlue(string DomainName)
        {
            _remotedom = AppDomain.CreateDomain(DomainName);

            _remotedom.SetData("glue", _g);

            _remotedom.DoCallBack(fmsldr.AppDomGlue.InitGlue);

            _selfglue = Activator.CreateInstance(_glueasm.GetType("Glue.CallbackGlue")) as fmsldr.IAppDomGlue;

            _remoteglue = _remotedom.CreateInstanceAndUnwrap("glue", "Glue.AppDomGlue") as fmsldr.IAppDomGlue;

            Debug.Assert(_remoteglue != null, "_remoteglue != null");

            var m0 = _remoteglue.GetMethod(0) as Action<fmsldr.IAppDomGlue>;

            Debug.Assert(m0 != null, "m0 != null");

            m0(_selfglue);

            _m1 = _remoteglue.GetMethod(1) as Action;
            _m2 = _remoteglue.GetMethod(2) as Func<string, string, string, bool, IDictionary<string, object>, string>;

            Debug.Assert(_selfglue != null, "_selfglue != null");

            _selfglue.SetMethod(10, new Action<object, object>(CreateVirtualChannel));

            //AppDomain.CurrentDomain.AssemblyResolve -= AR;
        }

        private void CreateVirtualChannel(object SendVirtualChannel, object ReceiveVirtualChannel)
        {
            Pipe.PipeManager.New(new AppDomainTaskTransport(SendVirtualChannel, ReceiveVirtualChannel));
        }

        private static Assembly AR(object sender, ResolveEventArgs args)
        {
            return _glueasm;
        }

        public void Close()
        {
            AppDomain.Unload(_remotedom);
        }

        private static byte[] GetResource(string Key)
        {
            try
            {
                var str = Assembly.GetExecutingAssembly().GetManifestResourceStream(Key);
                
                Debug.Assert(str != null, "str != null");

                var s = new GZipStream(str, CompressionMode.Decompress);

                var ms = new MemoryStream();
                s.CopyTo(ms);

                return ms.ToArray();
            }
            catch (IOException)
            {
                return null;
            }
        }

        private readonly Action _m1;
        private readonly Func<string, string, string, bool, IDictionary<string, object>, string> _m2;

        public Action RaiseUnloadDomain { get { return _m1; } }
        public Func<string, string, string, bool, IDictionary<string, object>, string> LaunchAssembly { get { return _m2; } }
    }
}
