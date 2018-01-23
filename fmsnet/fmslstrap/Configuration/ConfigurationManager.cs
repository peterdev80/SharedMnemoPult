using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using fmslstrap.Channel;
using System.Threading;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Diagnostics;
using fmslstrap.Administrator;

namespace fmslstrap.Configuration
{
    /// <summary>
    /// Управление глобальным хранилищем конфигурационных данных
    /// </summary>
    internal static class ConfigurationManager
    {
        #region Частные данные
        private static byte[] RetreiveConfigCommand;

        private static string _primarycfg;

        private static Timer _timer;
        private static AdmChannel _admchannel;
        private static bool _isconfigkeeper;

        private static readonly ManualResetEvent ConfigReady = new ManualResetEvent(false);
        private static bool _hasconfig;

        private static readonly Regex LnkRgx = new Regex(@"@\{(.*)\}");

        /// <summary>
        /// Хранилище конфигурационных данных
        /// </summary>
        private static Dictionary<string, Dictionary<string, List<string>>> _confstore = new Dictionary<string, Dictionary<string, List<string>>>();

        /// <summary>
        /// Хранилище предопределенных конфигурационных данных
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, List<string>>> DefConfStore = new Dictionary<string, Dictionary<string, List<string>>>();

        private static readonly MultiFileWatcher wtch = new MultiFileWatcher();
        #endregion

        #region Конструкторы
        /// <summary>
        /// Загрузка предопределенной конфигурации
        /// </summary>
        static ConfigurationManager()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var defconf = new GZipStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(@"PredefinedConfig"), CompressionMode.Decompress);
            DefConfStore.Clear();
            ParseStream(defconf, DefConfStore);
        }
        #endregion

        #region События
        /// <summary>
        /// Событие происходит при перезагрузке изменившихся конфигурационных данных
        /// </summary>
        public static event Action OnConfigReload;
        #endregion

        #region Инициализация
        public static void InitPreconfiguration(string ConfigFile)
        {
            Debug.Assert(ConfigFile != null);

            _primarycfg = ConfigFile;

#pragma warning disable 168
            // ReSharper disable once UnusedVariable
            ParseConfigurationFile(ConfigFile, out var cfiles);
#pragma warning restore 168
        }
        
        /// <summary>
        /// Загрузка и разбор файла конфигурации
        /// </summary>
        /// <param name="AdmChannel">Имя конфигурационного файла</param>
        /// <param name="ConfigFile">Административный канал обмена</param>
        public static void InitConfiguration(string ConfigFile, AdmChannel AdmChannel)
        {
            Debug.Assert(ConfigFile != null);

            // Бинарная команда требования конфигурации
            var ms = new MemoryStream();
            var bwr = new BinaryWriter(ms);
            bwr.Write((byte)'A');
            bwr.Write(Config.WorkstationName);
            RetreiveConfigCommand = ms.ToArray();

            _primarycfg = ConfigFile;

            _admchannel = AdmChannel;
            _admchannel.RegisterAdmCommand('A', SendConf);
            _admchannel.RegisterAdmCommand('B', ReceiveConf);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (ConfigFile == null || !File.Exists(ConfigFile))
            {
                _timer = new Timer(ConfTimer, null, 0, 100);

                // Ожидание пока все конфигурационные данные не будут так или иначе загружены
                ConfigReady.WaitOne();

                return;
            }

            ParseConfigurationFile(ConfigFile, out var cfiles);
            _isconfigkeeper = true;

            wtch.SetFilesWorWatch(cfiles);

            wtch.Changed += (s) =>
                {
                    wtch.EnableRaisingEvents = false;
                    ConfigChanged(ConfigFile);
                    Thread.Sleep(1500);
                    wtch.EnableRaisingEvents = true;
                };

            wtch.EnableRaisingEvents = true;

            _hasconfig = true;
            ConfigReady.Set();
        }
        #endregion

        #region Перезагрузка конфигурации
        private static void ConfigChanged(string ConfigFile)
        {
            Debug.WriteLine("ConfigChanged execute!");

            var reparsed = false;

            while (!reparsed)
            {
                try
                {
                    ParseConfigurationFile(ConfigFile, out var cfiles);

                    wtch.SetFilesWorWatch(cfiles);

                    SendConfig();
                    reparsed = true;

                    OnConfigReload?.Invoke();
                }
                catch (IOException) { }
            }
        }
        #endregion

        #region Обмен конфигурацией с внешними хостами
        private static void ConfTimer(object State)
        {
            _admchannel.SendMessage(RetreiveConfigCommand);
        }

        private static void SendConf(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            if (_hasconfig && _isconfigkeeper)
            {
                // Отправляем имеющийся конфиг
                string rhost = Reader.ReadString();
                SendConfig(rhost);
            }
        }

        private static void ReceiveConf(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            if (_isconfigkeeper)
                return;

            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            // Принимаем внешний конфиг
            var gz = new GZipStream(Stream, CompressionMode.Decompress, true);
            var sc = new Dictionary<string, Dictionary<string, List<string>>>();
            Dictionary<string, List<string>> cs = null;
            List<string> kk = null;
            var srdr = new BinaryReader(gz);
            var c = srdr.ReadString();
            var rdr = new StringReader(c);
            while (true)
            {
                var s = rdr.ReadLine();
                if (s == null || s == "#$")
                    break;

                if (s.StartsWith("##"))
                {
                    s = s.Replace("##", "");
                    cs = new Dictionary<string, List<string>>();
                    sc.Add(s, cs);
                    continue;
                }

                if (kk == null)
                {
                    kk = new List<string>();
                    Debug.Assert(cs != null, "cs != null");

                    cs.Add(s, kk);
                    continue;
                }

                if (s == "#!")
                {
                    kk = null;
                    continue;
                }

                kk.Add(s);
            }

            _confstore = sc;

            ConfigReady.Set();

            OnConfigReload?.Invoke();
        }

        /// <summary>
        /// Отправка конфигурации в административный канал указанному хосту
        /// </summary>
        /// <param name="ToHost">Имя хоста получателя</param>
        private static void SendConfig(string ToHost = null)
        {
            var ms = new MemoryStream();
            ms.WriteByte((byte)'B');
            var gz = new GZipStream(ms, CompressionMode.Compress, true);
            var gwr = new BinaryWriter(gz);
            gwr.Write(PackConfig());
            gz.Flush();
            gz.Close();

            var msa = ms.ToArray();

            Debug.WriteLine($"Send Config Length={msa.Length}, ToHost={ToHost}");

            _admchannel.SendMessage(msa, ToHost);
        }

        /// <summary>
        /// Упаковка конфгурационных данных
        /// </summary>
        /// <returns>Упакованный вид конфигурационных данных</returns>
        private static string PackConfig()
        {
            var wr = new StringWriter();
            var cs = _confstore;
            foreach (var s in cs)
            {
                wr.WriteLine("##" + s.Key);

                foreach (var k in s.Value)
                {
                    wr.WriteLine(k.Key);
                    foreach (var v in k.Value)
                        wr.WriteLine(v);
                    wr.WriteLine("#!");
                }
            }
            wr.WriteLine("#$");

            return wr.ToString();
        }

        /// <summary>
        /// Запись упакованного вида конфигурационных данных в поток
        /// </summary>
        /// <param name="Stream">Поток для записи</param>
        public static void PackConfigToStream(Stream Stream)
        {
            var wr = new BinaryWriter(Stream, Encoding.UTF8);

            wr.Write(PackConfig());
        }

        public static IList<string> GetConfigFileNames()
        {
            return wtch.FileNames;
        }
        #endregion

        #region Загрузка и разбор файла конфигурации
        /// <summary>
        /// Загрузка и разбор файла конфигурации
        /// </summary>
        /// <param name="ConfigFile">Имя файла конфигурации</param>
        /// <param name="AllConfigFiles">Список всех конфигурационных файлов</param>
        private static void ParseConfigurationFile(string ConfigFile, out IEnumerable<string> AllConfigFiles)
        {
            Console.Write(ConfigFile);

            var confstore = new Dictionary<string, Dictionary<string, List<string>>>();

            #region Загрузка в параметры преодпределенной конфигурации
            foreach (var k1 in DefConfStore)
            {
                confstore.Add(k1.Key, new Dictionary<string, List<string>>());

                var k = confstore[k1.Key];
                foreach (var k2 in k1.Value)
                    k.Add(k2.Key, new List<string>(k2.Value));
            }
            #endregion

            var lst = new List<string>();
            AllConfigFiles = lst;
            lst.Add(Path.GetFullPath(ConfigFile));

            ParseStream(new FileStream(ConfigFile, FileMode.Open, FileAccess.Read), confstore);

            if (confstore.ContainsKey("global"))
                if (confstore["global"].ContainsKey("additional.config"))
                    foreach (var c in confstore["global"]["additional.config"].Select(Environment.ExpandEnvironmentVariables))
                        if (File.Exists(c))
                        {
                            ParseStream(new FileStream(c, FileMode.Open, FileAccess.Read), confstore);
                            lst.Add(Path.GetFullPath(c));
                        }

            #region Объединение наследования
            var deps = new Dictionary<string, List<string>>();
            var order = new List<string>();
            var safesections = new HashSet<string>();

            // Формируется список зависимостей
            foreach (var s in confstore.Keys)
            {
                var sect = confstore[s];

                if (!sect.ContainsKey("@inherit"))
                {
                    safesections.Add(s);
                    continue;
                }

                var l = new List<string>();
                l.AddRange(sect["@inherit"]);
                deps[s] = l;
            }

            // Формируется список очередности наследования
            while (deps.Count > 0)
            {
                var b = (from a in deps
                         where a.Value.All(safesections.Contains)
                         select a.Key).ToArray();

                order.AddRange(b);
                foreach (var bb in b)
                {
                    safesections.Add(bb);
                    deps.Remove(bb);
                }
            }
            #endregion

            #region Слияние наследуемых секций
            foreach (var s in order)
            {
                var s1 = s;
                foreach (var iv in from inh in confstore[s]["@inherit"]
                                   from iv in confstore[inh]
                                   where iv.Key.ToLower() != "@inherit"
                                   where !confstore[s1].ContainsKey(iv.Key)
                                   select iv)
                {
                    confstore[s][iv.Key] = new List<string>(iv.Value);
                }
                confstore[s].Remove("@inherit");
            }

            #endregion

            #region Замещение мягких ссылок
            var z = (from s in confstore
                     let sv = s.Value
                     from k in sv
                     let kk = k.Key
                     where kk.StartsWith("@")
                     let wk = kk.Replace("@", "")
                     let tk = sv.ContainsKey(wk) ? sv[wk] : null
                     select new { Section = sv, WeakKey = wk, TargetKey = tk, WeakList = k.Value }).ToArray();

            foreach (var k in z)
            {
                if (k.TargetKey == null)
                    k.Section.Add(k.WeakKey, k.WeakList);

                k.Section.Remove("@" + k.WeakKey);
            }
            #endregion

            #region Подстановка ссылок
            foreach (var s in confstore)
            {
                foreach (var v in s.Value)
                {
                    if (v.Value.Count != 1)
                        continue;

                    var m = LnkRgx.Match(v.Value[0]);
                    if (!m.Success)
                        continue;

                    var l = m.Groups[1].Value;

                    if (!s.Value.ContainsKey(l))
                        continue;

                    v.Value.Clear();
                    v.Value.AddRange(s.Value[l]);
                }
            }
            #endregion

            #region Удаление ненужных секций
            var dels = (from l in confstore["global"]["section.trash"]
                        let le = l.Split(' ')
                        from ll in le
                        let ple = ll.Trim()
                        where !string.IsNullOrEmpty(ple)
                        let rg = new Regex($"^{ple.Replace("*", "(.*)")}$")
                        from c in confstore.Keys
                        where rg.IsMatch(c)
                        select c).ToArray();
                        
            foreach (var d in dels) 
                confstore.Remove(d);
            #endregion

            #region Удаление ненужных ключей
            var kdels = (from l in confstore["global"]["key.trash"]
                         let le = l.Split(' ')
                         from ll in le
                         let ple = ll.Trim()
                         where !string.IsNullOrEmpty(ple)
                         let ples = ple.Replace("*", "(.*)").Split(new[] {'/'}, 2)
                         let rg1 = new Regex($"^{ples[0]}$")
                         from s in confstore
                         where rg1.IsMatch(s.Key)
                         let rg2 = new Regex($"^{ples[1]}$")
                         from v in s.Value
                         where rg2.IsMatch(v.Key)
                         select new { Section = s.Value, v.Key }).ToArray();

            foreach (var kd in kdels) 
                kd.Section.Remove(kd.Key);
            #endregion

            _confstore = confstore;
        }

        private static void ParseStream(Stream Stream, Dictionary<string, Dictionary<string, List<string>>> ConfStore)
        {
            // Текущая секция
            var curlst = new Dictionary<string, List<string>>();
            
            // Имя текущей секции
            var curname = "global";

            if (!ConfStore.ContainsKey(curname))
                ConfStore.Add(curname, curlst);
            else
                curlst = ConfStore[curname];

            using (var rdr = new StreamReader(Stream))
            {
                while (!rdr.EndOfStream)
                {
                    var line = rdr.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    var ln = line.Split('#')[0].Trim();

                    if (string.IsNullOrWhiteSpace(ln))
                        continue;

                    // Объявление секции
                    if (ln[0] == '[')
                    {
                        ln = ln.Substring(1, ln.Length - 2).ToLower();

                        if (ConfStore.ContainsKey(ln))
                        {
                            curlst = ConfStore[ln];
                        }
                        else
                        {
                            curlst = new Dictionary<string, List<string>>();
                            ConfStore[ln] = curlst;
                        }

                        curname = ln;
                        continue;
                    }

                    if (!ln.Contains('='))
                        continue;

                    // Объявление параметра
                    var kv = ln.Split('=');
                    var k = kv[0].Trim().ToLower();
                    var v = kv[1].Trim();

                    if (k == "@inherit")
                    {
                        v = v.ToLower();

                        if (curname == "global")
                        {
                            System.Windows.Forms.MessageBox.Show(@"В секции [global] наследование запрещено.");
                            System.Windows.Forms.Application.Exit();
                        }
                    }

                    if (!curlst.ContainsKey(k))
                        curlst[k] = new List<string>();

                    curlst[k].Add(v);
                }
            }
        }
        #endregion

        #region Доступ к конфигурационным данным
        /// <summary>
        /// Возвращает раздел конфигурации
        /// </summary>
        /// <param name="Name">Имя раздела</param>
        /// <returns>Содержимое раздела</returns>
        public static ConfigSection GetSection(string Name)
        {
            var rs = GetRawSection(Name);

            if (rs == null)
                return null;

            return new ConfigSection(rs);
        }

        /// <summary>
        /// Возвращает раздел конфигурации
        /// </summary>
        /// <param name="Name">Имя раздела</param>
        /// <returns>Содержимое раздела</returns>
        public static Dictionary<string, List<string>> GetRawSection(string Name)
        {
            _confstore.TryGetValue(Name.ToLower(), out var sect);
            return sect;
        }
        #endregion

        public static string PrimaryConfigFile => _primarycfg;
    }
}
