using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.NetworkInformation;
// ReSharper disable InconsistentNaming

namespace fmsldr
{
    public static class Program
    {
        private const string fmsldrconf = @"ldr.ini";

        private static string _domainname;
        private static bool _copylocal;
        private static string _codebase;

        #region Частные данные
        private static byte[] asm;

#if DEBUG
        private static byte[] pdb;
#else
        private const byte[] pdb = null;
#endif

        private static UdpClient _udp;
        private static System.Threading.Timer _sendtimer;
        private static readonly AutoResetEvent _evt = new AutoResetEvent(false);
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Environment.GetCommandLineArgs().Any(x => x.Trim().ToLowerInvariant() == "/adjpath"))
                // ReSharper disable once AssignNullToNotNullAttribute
                Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            foreach (var f in Directory.GetFiles(".", "*.udel", SearchOption.AllDirectories))
                try
                {
                    File.Delete(f);
                }
                catch (IOException) { }

            foreach (var f in Directory.GetFiles(".", "*.utmp", SearchOption.AllDirectories))
                try
                {
                    File.Replace(f, f.Replace(".utmp", ""), null);
                    File.Delete(f);
                }
                catch (IOException) { }

            while (true)
            {
                bool newmutex;
                var m = new Mutex(true, @"Global\fmslupd", out newmutex);
                m.Close();
                Thread.Sleep(10);
                if (newmutex)
                    break;
            }

            var cmdline = Environment.GetCommandLineArgs();

            if (Control.CheckParams(cmdline))
                return;

            var dom = AppDomain.CreateDomain("fmsloaderdom");

            dom.SetData("ldrcmdline", cmdline);

            dom.DoCallBack(Start);
            //Start();
        }

        private static void Start()
        {
            if (!File.Exists(fmsldrconf))
            {
                MessageBox.Show(@"Конфигурационный файл не найден!", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            var crx = new Regex(@"\[global] \s* ( ( ( (DomainName \s* = \s* (?<d>.\S*)) | (CopyLocal \s* = \s* (?<dl>.\S*)) | (CodeBase \s* = \s* (?<cb>.\S*)) ) .*?$\s* ) | (\s*\#.*?$\s*) | ( [^\[] .*?$\s* ) )*",
                RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

            var m = crx.Match(File.ReadAllText(fmsldrconf));

            if (!m.Success)
                return;

            var g = m.Groups;

            _domainname = g["d"].Value;
            var cl = g["cl"].Value.ToLower();
            _copylocal = cl == "1" || cl == "yes" || cl == "on" || cl == "true";
            _codebase = g["cb"].Value;

            if (string.IsNullOrWhiteSpace(_codebase))
                _codebase = @"./";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var fw = new FWaiting();
            
            CheckFiles();

            var ass = Assembly.Load(asm, pdb);
            var tp = ass.GetType("fmslstrap.Manager");

            var admPoint = new IPEndPoint(IPAddress.Loopback, 3275);

            var mi = tp.GetMethod("Start");

            var d = new Dictionary<string, object>();

            d["cfg"] = fmsldrconf;
            d["ipe"] = admPoint;
            d["wf"] = fw;
            d["asm"] = asm;

#if DEBUG
            d["pdb"] = pdb;
#endif

            mi.Invoke(null, new object[] { d });
            
            Application.Run();
        }

        private static void CheckFiles()
        {
            asm = GetFile(@"fmslstrap.dll");

#if DEBUG
            pdb = GetFile(@"fmslstrap.pdb");
#endif

            if (asm == null)
            {
                var ifs = NetworkInterface.GetAllNetworkInterfaces().Where(i => i.OperationalStatus == OperationalStatus.Up &&
                                                                i.NetworkInterfaceType != NetworkInterfaceType.Loopback);
                var ifswa = ifs.Where(i => i.GetIPProperties().UnicastAddresses.Count > 0).ToArray();

                var bcs = (from i in ifswa
                           from uips in i.GetIPProperties().UnicastAddresses
                           where uips.Address.AddressFamily == AddressFamily.InterNetwork
                           select uips).ToArray();

                var bcsa = new IPEndPoint[bcs.Length];

                for (int i = 0; i < bcs.Length; i++)
                {
                    var ip = bcs[i].Address.GetAddressBytes();
                    var m = bcs[i].IPv4Mask.GetAddressBytes();

                    for (int j = 0; j < ip.Length; j++)
                        ip[i] = ip[i] |= (byte)(~m[i]);

                    bcsa[i] = new IPEndPoint(new IPAddress(ip[i]), 3275);
                }

                _udp = new UdpClient(new IPEndPoint(IPAddress.Any, 0)) { EnableBroadcast = true };
                StartReceive();

                var ms = new MemoryStream();
                var wr = new BinaryWriter(ms);
                wr.Write((byte)'A');
                wr.Write(_domainname);

                var bmsg = ms.ToArray();

                _sendtimer = new System.Threading.Timer(x =>
                    {
                        foreach (var t in bcsa)
                        {
                            try
                            {
                                _udp.Send(bmsg, bmsg.Length, t);
                            }
                            catch (SocketException) { }
                        }
                    }, null, 5, 50);

                while (!_evt.WaitOne(20))
                    Application.DoEvents();

                _udp.Close();

                if (_copylocal)
                {
                    if (asm != null) File.WriteAllBytes(_codebase + @"fmslstrap.dll", asm);

#if DEBUG
                    if (pdb != null) File.WriteAllBytes(_codebase + @"fmslstrap.pdb", pdb);
#endif
                }
            }
        }

        private static void StartReceive()
        {
            var done = false;
            while (!done)
            {
                try
                {
                    _udp.BeginReceive(Received, null);
                    done = true;
                }
                catch (SocketException) { }
            }
        }

        private static void Received(IAsyncResult ar)
        {
            _sendtimer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                var ipe = new IPEndPoint(IPAddress.Any, 0);
                var rdr = new BinaryReader(new MemoryStream(_udp.EndReceive(ar, ref ipe)));
                
                ipe.Port = rdr.ReadUInt16();

                var tcp = new TcpClient();
                tcp.Connect(ipe);
                var ns = tcp.GetStream();
                var gz = new GZipStream(ns, CompressionMode.Decompress);
                var brdr = new BinaryReader(gz);

                var la = brdr.ReadInt32();
                var pa = brdr.ReadInt32();

                asm = brdr.ReadBytes(la);

#if DEBUG
                pdb = pa > 0 ? brdr.ReadBytes(pa) : null;
#endif

                tcp.Close();

                _evt.Set();
            }
            catch (SocketException)
            {
                _sendtimer.Change(5, 50);
                StartReceive();
            }
        }

        private static byte[] GetFile(string FileName)
        {
            if (!File.Exists(FileName))
            {
#if DEBUG
                FileName = @"..\..\..\fmslstrap\bin\x86\debug\" + FileName;
                if (!File.Exists(FileName))
                {
                    return null;
                }
#else
                return null;
#endif
            }

            byte[] file = File.ReadAllBytes(FileName);

            return file;
        }
    }
}
