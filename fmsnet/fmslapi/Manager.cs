using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using fmslapi.Storage;
using fmslapi.Tasks;
using fmslapi.Channel;
using ch = fmslapi.Channel;
using System.Reflection;
using System.Windows.Threading;
using fmslapi.Channel.Transport;
// ReSharper disable All

namespace fmslapi
{
    #region Делегаты

    #region Делегаты обычных каналов

    /// <summary>
    /// Делегат приема данных из канала
    /// </summary>
    /// <param name="Sender">Отправитель сообщения</param>
    /// <param name="Message">Принятые данные</param>
    public delegate void DataReceived(ISenderChannel Sender, ReceivedMessage Message);
    #endregion

    #region Делегаты каналов переменных
    /// <summary>
    /// Событие изменения переменной
    /// </summary>
    public delegate void VariableChanged(IVariable Sender, bool IsInit);

    /// <summary>
    /// Событие изменения пакета переменных
    /// </summary>
    public delegate void VariablesChanged(bool IsInit, IVariable[] ChangedList);
    #endregion
    #endregion

    /// <summary>
    /// Исключение, выбрасываемое делегатами, для сигнализации отмены ожидания появления
    /// связи с fmsldr
    /// </summary>
    public class ConnectionAbortException : Exception { }

    public class Manager : IManager, IInternalManager, IManager1, IManager2
    {
        #region Внутренние типы
        private class ConfigSection : IConfigSection
        {
            #region Обертка настраиваемого доступа по ключу
            private class PrefixedConfigSection : IConfigSection
            {
                private readonly ConfigSection _section;
                private readonly Func<string, string> _format;

                public PrefixedConfigSection(ConfigSection Section, Func<string, string> Format)
                {
                    _section = Section;
                    _format = Format;
                }

                #region Члены IConfigSection

                public string this[string key]
                {
                    get { return _section[_format(key)]; }
                }

                public string[] AsArray(string key)
                {
                    return _section.AsArray(_format(key));
                }

                public int GetInt(string key)
                {
                    return _section.GetInt(_format(key));
                }

                public double GetDouble(string key)
                {
                    return _section.GetDouble(_format(key));
                }

                public bool GetBool(string key)
                {
                    return _section.GetBool(_format(key));
                }

                public bool HasKey(string Key)
                {
                    return _section.HasKey(_format(Key));
                }

                public IConfigSection GetPrefixed(Func<string, string> Format)
                {
                    return _section.GetPrefixed(Format);
                }

                int IConfigSection.GetHashCode()
                {
                    return ((IConfigSection)_section).GetHashCode();
                }

                public string[] AsWordsArray(string key)
                {
                    return _section.AsWordsArray(_format(key));
                }

                #endregion
            }
            #endregion

            private readonly IDictionary<string, List<string>> _section;

            public ConfigSection(IDictionary<string, List<string>> Section)
            {
                _section = Section;
            }

            public string this[string key]
            {
                get
                {
                    _section.TryGetValue(key.ToLower(), out var v);

                    if (v == null)
                        return null;

                    if (v.Count == 0)
                        return null;

                    return v[0];
                }
            }

            public string[] AsArray(string key)
            {
                _section.TryGetValue(key.ToLower(), out var v);

                if (v == null)
                    return new string[0];

                return v.ToArray();
            }

            /// <summary>
            /// Возвращает целочисленное значение ключа
            /// </summary>
            /// <param name="key">Ключ</param>
            /// <returns>Значение</returns>
            public int GetInt(string key)
            {
                var v = this[key];
                if (v == null)
                    return default(int);

                try
                {
                    return int.Parse(v, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    return default(int);
                }
            }

            /// <summary>
            /// Возвращает логическое значение ключа
            /// </summary>
            /// <param name="key">Ключ</param>
            /// <returns>Значение</returns>
            /// <remarks>
            /// За истину принимаются значения
            /// 1, yes, true, on
            /// </remarks>
            public bool GetBool(string key)
            {
                var v = this[key];
                return v == "1" || v == "yes" || v == "true" || v == "on";
            }

            /// <summary>
            /// Возвращает значение ключа плавающей точкой повышенной точности
            /// </summary>
            /// <param name="key">Ключ</param>
            /// <returns>Значение</returns>
            public double GetDouble(string key)
            {
                var v = this[key].Replace(",", ".");
                double d = 0;
                try
                {
                    d = double.Parse(v, CultureInfo.InvariantCulture);
                }
                catch (FormatException) { }
                catch (NullReferenceException) { }

                return d;
            }

            /// <summary>
            /// Возвращает наличие указанного ключа
            /// </summary>
            /// <param name="Key">Имя ключа</param>
            /// <returns>Флаг наличия ключа</returns>
            public bool HasKey(string Key)
            {
                return _section.ContainsKey(Key);
            }

            /// <summary>
            /// Возвращает уникальный код идентифицирующий содержимое раздела
            /// </summary>
            /// <remarks>
            /// Изменение порядка следования ключей в секции не приводит к изменению отпечатка.
            /// Изменение порядка следования значений в ключе приводит к изменению отпечатка.
            /// </remarks>
            int IConfigSection.GetHashCode()
            {
                return (from k in _section 
                        let vh = k.Value.Aggregate(-1091576147, (acc, s) => (acc << 2) ^ (acc >> 2) ^ s.GetHashCode())
                        select vh ^ k.Key.GetHashCode()).Aggregate(-559038737, (acc, s) => acc ^ s);
            }

            public IConfigSection GetPrefixed(Func<string, string> Format)
            {
                return Format == null ? (IConfigSection)this : new PrefixedConfigSection(this, Format);
            }

            public string[] AsWordsArray(string key)
            {
                var pxs = from l in AsArray(key)
                          let le = l.Split(new[] { ' ' })
                          from ll in le
                          let ple = ll.Trim()
                          where !string.IsNullOrEmpty(ple)
                          select ple;

                return pxs.ToArray();
            }
        }
        #endregion

        #region Частные данные
        //private static Manager manager;
        private static readonly object lockobj = new object();
        private Dictionary<string, Dictionary<string, List<string>>> _confcache;
        
        /// <summary>
        /// Локальный административный канал
        /// </summary>
        private IChannel _admloc;

        /// <summary>
        /// Общеадминистративный канал
        /// </summary>
        private IChannel _adm;

        private readonly ManualResetEvent _confevt = new ManualResetEvent(false);

        /// <summary>
        /// Принимаемые конфигурационные секции
        /// </summary>
        private readonly HashSet<Guid> _acceptconftokens = new HashSet<Guid>();

        /// <summary>
        /// Интерфейс доступа к постоянному хранилищу
        /// </summary>
        private IPersistStorage _pstg;

        private static string _taskname;

        private static bool _ldrembedded;

        private object _domcbglue;

        private readonly ManualResetEventSlim _loctasksevt = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _hostsevt = new ManualResetEventSlim(false);

        private IList<string> _localtasks;
        private Dictionary<string, List<string>> _hosts;

        private static int _strapAPIVersion;

        #endregion

        #region Конструкторы

        static Manager()
        {
            _closeevt = new EventWaitHandle(false, EventResetMode.ManualReset, "fmslapicloseall");

            ThreadPool.RegisterWaitForSingleObject(_closeevt, (State, Out) => { RaiseOnUnloadProcess(); }, null,
                Timeout.Infinite, true);
        }

        private Manager()
        {
            var vals = AppDomain.CurrentDomain.GetData("fmslapi:APIVALS") as IDictionary<string, object>;

            if (vals == null)
                return;

            vals.TryGetValue("AppDomCBGlue", out _domcbglue);
        }

        #endregion

        #region Статические методы

        /// <summary>
        /// Возвращает интерфейс доступа к API
        /// </summary>
        /// <param name="EndPointName">Имя клиента</param>
        /// <param name="ComponentID">Ключ компонента</param>
        /// <returns>Интерфейс доступа к API</returns>
        public static IManager GetAPI(string EndPointName, Guid ComponentID)
        {
            return GetAPI(EndPointName, ComponentID, null);
        }

        private static IManager GetAPI(IDictionary<string, object> Vals)
        {
            AppDomain.CurrentDomain.SetData("fmslapi:APIVALS", Vals);

            object o;
            if (Vals.TryGetValue("TaskName", out o))
                _taskname = o.ToString();

            Guid cid;
            if (Vals.TryGetValue("ComponentID", out o))
                cid = (Guid)o;
            else
                cid = Guid.Empty;

            return GetAPI(TaskName, cid) as Manager;
        }

        /// <summary>
        /// Возвращает интерфейс доступа к API
        /// </summary>
        /// <param name="EndPointName">Имя клиента</param>
        /// <param name="ComponentID">Ключ компонента</param>
        /// <param name="WaitForLdrHandler">Функция вызываемая, для индикации ожидания подключения к обмену</param>
        /// <returns>Интерфейс доступа к API</returns>
        public static IManager GetAPI(string EndPointName, Guid ComponentID, Func<Action> WaitForLdrHandler)
        {
            var iid = Guid.Empty;

            var pars = Environment.GetCommandLineArgs().FirstOrDefault(x => x.ToLower().StartsWith("/instance="));
            if (pars != null)
            {
                try
                {
                    iid = new Guid(pars.Substring(10).Trim());
                }
                catch (FormatException) { }
            }

            lock (lockobj)
            {
                var manager = new Manager
                {
                    EndPointName = EndPointName,
                    ComponentID = ComponentID,
                    InstanceID = iid
                };

                var t = manager.GetTransport();

                if (!(t is PipeTransport))
                    return manager;

                if (!t.TryConnect(TimeSpan.Zero))
                    return null;

                var rd = new BinaryReader(new MemoryStream(t.Read()), Encoding.UTF8);
                t.Close();

                var sl = rd.BaseStream.Length;

                if (sl >= 5)
                    if ((char)rd.ReadByte() == 'M')
                    {
                        manager.StrapAPIVersion = rd.ReadInt32();

                        if (sl > 5)
                        {
                            manager.DomainName = rd.ReadString();
                            try
                            {
                                manager.VersionString = rd.ReadString();
                                manager.ServerFMSLAPIPath = rd.ReadString();
                            }
                            catch (EndOfStreamException) { }
                        }
                    }

                return manager;
            }
        }

        public static void EmbedLdr()
        {
            if (IsFmsldrLoaded)
                return;

            var m = Type.GetType("fmsldr.Program, fmsldr").GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
            _ldrembedded = true;
            ThreadPool.QueueUserWorkItem(x => m.Invoke(null, null));

            while (!IsFmsldrLoaded)
                Thread.Sleep(25);
        }

        public static bool IsLdrEmbedded
        {
            get { return _ldrembedded; }
        }

        public void ShutdownLdr()
        {
            if (!_ldrembedded)
                return;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write('L');

            _admloc.SendMessage(ms.ToArray());
        }
        #endregion

        #region Общие свойства

        /// <summary>
        /// Имя клиента
        /// </summary>
        public string EndPointName { get; private set; }

        public Guid ComponentID { get; private set; }
        public Guid InstanceID { get; private set; }

        public string DomainName { get; private set; }
        public string VersionString { get; private set; }
        public string ServerFMSLAPIPath { get; private set; }

        #region IManager1

        IChannel1 IManager1.JoinChannel(string Channel)
        {
            return JoinChannel(Channel, null, null) as IChannel1;
        }

        IChannel1 IManager1.JoinChannel(string Channel, DataReceived Received)
        {
            return JoinChannel(Channel, Received) as IChannel1;
        }

        IChannel1 IManager1.AdmLocChannel
        {
            get { return AdmLocChannel as IChannel1; }
        }

        #endregion

        public IChannel AdmLocChannel
        {
            get
            {
                CheckAdmLoc();
                return _admloc;
            }
        }

        public static string TaskName
        {
            get { return _taskname ?? ""; }
            set { _taskname = value; }
        }

        public IChannel ADM
        {
            get
            {
                lock (this)
                {
                    return _adm ?? (_adm = JoinChannel("ADM", null));
                }
            }
        }
        #endregion

        #region IManager

        #region Обычное подключение
        private event Action OnConfigReloadIntrnl;

        public bool HardConnectionCheck { get; set; }

        /// <summary>
        /// Событие происходит при перезагрузке конфигурации
        /// </summary>
        public event Action OnConfigReload
        {
            add
            {
                OnConfigReloadIntrnl += value;
                CheckAdmLoc();
            }
            remove
            {
                OnConfigReloadIntrnl -= value;
            }
        }

        /// <summary>
        /// Возвращает интерфейс конечной точки канала
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Событие приема данных из канала</param>
        /// <returns>Интерфейс конечной точки канала</returns>
        public IChannel JoinChannel(string Channel, DataReceived Received)
        {
            return JoinChannel(Channel, Received, null);
        }

        /// <summary>
        /// Возвращает интерфейс конечной точки канала
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Функция обработки принятых данных</param>
        /// <param name="Changed">Делегат изменения состояния подключения канала</param>
        /// <returns>Интерфейс конечной точки канала</returns>
        public IChannel JoinChannel(string Channel, DataReceived Received, ChannelStateChanged Changed)
        {
            var p = new ChannelParams
            {
                Manager = this,
                Channel = Channel,
                EndPoint = null,
                VarMap = null,
                VariablesChanged = null,
                ChannelStateChanged = Changed,
                ChannelMode = ChannelMode.Regular,
                ComponentID = ComponentID,
                InstanceID = InstanceID,
            };

            var c = new ch.Channel(p);
            c.Received += Received;

            return c;
        }
        #endregion

        #region Потокобезопасное подключение
        /// <summary>
        /// Возвращает интерфейс конечной точки канала
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Событие приема данных из канала</param>
        /// <param name="Changed">Делегат изменения состояния подключения канала</param>
        /// <returns>Интерфейс конечной точки канала</returns>
        public IChannel SafeJoinChannel(string Channel, DataReceived Received, ChannelStateChanged Changed)
        {
            var p = new ChannelParams
            {
                Manager = this,
                Channel = Channel,
                EndPoint = null,
                VarMap = null,
                VariablesChanged = null,
                ChannelStateChanged = Changed,
                ChannelMode = ChannelMode.Regular,
                ComponentID = ComponentID,
                InstanceID = InstanceID,
            };

            var c = new ThreadSafeChannel(p);
            c.Received += Received;

            return c;
        }

        /// <summary>
        /// Возвращает интерфейс конечной точки канала
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Событие приема данных из канала</param>
        /// <returns>Интерфейс конечной точки канала</returns>
        public IChannel SafeJoinChannel(string Channel, DataReceived Received)
        {
            return SafeJoinChannel(Channel, Received, null);
        }
        #endregion

        #region Конфигурация
        public event Func<Action[]> OnFMSLDRNotAvailable;

        /// <summary>
        /// Возвращает конфигурационную секцию
        /// </summary>
        /// <param name="SectionName">Имя секции</param>
        /// <returns>Конфигурационная секция</returns>
        public IConfigSection GetSection(string SectionName)
        {
            if (_confcache == null)
            {
                _confcache = new Dictionary<string, Dictionary<string, List<string>>>();
            }

            RetreiveSection(SectionName);
            return new ConfigSection(_confcache[SectionName ?? "<default>"]);

            //Application.Run(new WaitForFMSLDR());

            //return new ConfigSection(new Dictionary<string, List<string>>());
        }

        /// <summary>
        /// Загружает раздел конфигурации
        /// </summary>
        /// <param name="Name">Имя раздела</param>
        private void RetreiveSection(string Name)
        {
            var ms = new MemoryStream();
            var bwr = new BinaryWriter(ms);

            if (Name == null)
                Name = "<default>";

            var token = Guid.NewGuid();

            lock (_acceptconftokens)
            {
                _acceptconftokens.Add(token);
            }

            bwr.Write('B');
            bwr.Write(ComponentID.ToByteArray());
            bwr.Write(InstanceID.ToByteArray());
            bwr.Write(Name);
            bwr.Write(token.ToByteArray());
            var bf = ms.ToArray();
            AdmLocChannel.SendMessage(bf);

            var loaded = false;
            while (!loaded)
            {
                lock (_confcache)
                {
                    loaded = _confcache.ContainsKey(Name);
                }
                _confevt.Reset();
                _confevt.WaitOne(5);
            }
        }

        /// <summary>
        /// Возвращает конфигурационную секцию по умолчанию, заданную для 
        /// текущего запущенного экземпляра компонента
        /// </summary>
        public IConfigSection DefaultSection 
        {
            get
            {
                return GetSection(null);
            }
        }
        #endregion

        #endregion

        #region IManager.SnapshotVariables

        #region Обычное подключение
        /// <summary>
        /// Возвращает интерфейс конечной точки канала переменных
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="StateChanged">Делегат изменения состояния подключения канала</param>
        /// <param name="Changed">Событие изменения пакета переменных</param>
        /// <param name="VarMap">Метод отправки данных в канал</param>
        /// <returns>Интерфейс конечной точки канала</returns>
        public IVariablesChannel JoinVariablesChannel(string Channel, string VarMap, ChannelStateChanged StateChanged, VariablesChanged Changed)
        {
            return SafeJoinVariablesChannel(Channel, VarMap, StateChanged, Changed);
        }

        /// <summary>
        /// Возвращает интерфейс конечной точки канала переменных
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="EndPoint">Конечная точка подключения канала</param>
        /// <param name="StateChanged">Делегат изменения состояния подключения канала</param>
        /// <param name="Changed">Событие изменения пакета переменных</param>
        /// <param name="VarMap">Имя карты переменных</param>
        /// <returns>Интерфейс конечной точки канала</returns>
        public IVariablesChannel JoinVariablesChannel(string Channel, string EndPoint, string VarMap, ChannelStateChanged StateChanged, VariablesChanged Changed)
        {
            return SafeJoinVariablesChannel(Channel, EndPoint, VarMap, StateChanged, Changed);
        }

        #endregion

        #region Потокобезопасное подключение
        /// <summary>
        /// Возвращает интерфейс конечной точки канала переменных
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="StateChanged">Делегат изменения состояния подключения канала</param>
        /// <param name="Changed">Событие изменения пакета переменных</param>
        /// <param name="VarMap">Метод отправки данных в канал</param>
        /// <returns>Интерфейс конечной точки канала</returns>
        public IVariablesChannel SafeJoinVariablesChannel(string Channel, string VarMap, ChannelStateChanged StateChanged, VariablesChanged Changed)
        {
            return SafeJoinVariablesChannel(Channel, null, VarMap, StateChanged, Changed);
        }

        /// <summary>
        /// Возвращает интерфейс конечной точки канала переменных
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="EndPoint">Конечная точка подключения канала</param>
        /// <param name="StateChanged">Делегат изменения состояния подключения канала</param>
        /// <param name="Changed">Событие изменения пакета переменных</param>
        /// <param name="VarMap">Имя карты переменных</param>
        /// <returns>Интерфейс конечной точки канала</returns>
        public IVariablesChannel SafeJoinVariablesChannel(string Channel, string EndPoint, string VarMap, ChannelStateChanged StateChanged, VariablesChanged Changed)
        {
            var p = new ChannelParams
            {
                Manager = this,
                Channel = Channel,
                EndPoint = EndPoint,
                VarMap = VarMap,
                VariablesChanged = Changed,
                ChannelStateChanged = StateChanged,
                ChannelMode = ChannelMode.Variables,
                ComponentID = ComponentID,
                InstanceID = InstanceID,
            };
            
            var c = new ThreadSafeChannel(p);

            return c;
        }
        #endregion

        #endregion

        #region Частные методы
        private void CheckAdmLoc()
        {
            if (_admloc != null) 
                return;

            var tcc = HardConnectionCheck;
            HardConnectionCheck = true;
            _admloc = JoinChannel("ADMLOC", AdmLocReceived);
            HardConnectionCheck = tcc;
        }

        private void AdmLocReceived(ISenderChannel Sender, ReceivedMessage Message)
        {
            var ms = new MemoryStream(Message.Data);
            var brdr = new BinaryReader(ms);

            var cmd = (char)brdr.ReadByte();

            switch (cmd)
            {
                case 'A':
                    _confcache = null;
                    if (OnConfigReloadIntrnl != null)
                        OnConfigReloadIntrnl();
                    break;

                case 'C':
                    //var gz = new GZipStream(ms, CompressionMode.Decompress, true);
                    var zrd = new BinaryReader(ms);

                    var section = zrd.ReadString();
                    var token = new Guid(zrd.ReadBytes(16));

                    lock (_acceptconftokens)
                    {
                        if (!_acceptconftokens.Contains(token))
                            break;

                        _acceptconftokens.Remove(token);
                    }

                    var sect = new Dictionary<string, List<string>>();
                    var cnt = zrd.ReadInt32();
                    for (var i = 0; i < cnt; i++)
                    {
                        var key = zrd.ReadString();
                        var kcnt = zrd.ReadInt32();
                        var lst = new List<string>();
                        sect[key] = lst;
                        for (var j = 0; j < kcnt; j++)
                            lst.Add(zrd.ReadString());
                    }

                    if (_confcache == null)
                        _confcache = new Dictionary<string, Dictionary<string, List<string>>>();

                    lock (_confcache)
                    {
                        _confcache[section] = sect;
                    }
                    break;

                case 'I':
                    // Список локальных задач
                    cnt = brdr.ReadUInt16();

                    var t = new List<string>();

                    for (var i = 0; i < cnt; i++)
                        t.Add(brdr.ReadString());

                    _localtasks = t;

                    _loctasksevt.Set();
                    break;

                case 'O':
                    // Список хостов
                    cnt = brdr.ReadUInt16();
                    var hl = new Dictionary<string, List<string>>();
                    for (var i = 0; i < cnt; i++)
                    {
                        var h = brdr.ReadString();

                        var hc = brdr.ReadInt16();

                        var hll = new List<string>();

                        for (var j = 0; j < hc; j++)
                            hll.Add(brdr.ReadString());

                        hl.Add(h, hll);
                    }

                    _hosts = hl;
                    _hostsevt.Set();
                    break;
            }
        }
        #endregion

        #region Публичные методы
        public Action[] RaiseOnFMSLDRNotAvailable()
        {
            return OnFMSLDRNotAvailable != null ? OnFMSLDRNotAvailable() : null;
        }

        #endregion

        #region События
        /// <summary>
        /// Событие происходящее перед внешним завершением выполняемого домена приложения
        /// </summary>
        public static event Action OnUnloadProcess;

        private static bool _raised;

        private static EventWaitHandle _closeevt;

        private static void RaiseOnUnloadProcess()
        {
            if (_raised)
                return;

            if (OnUnloadProcess == null)
                return;

            _raised = true;

            foreach (var i in OnUnloadProcess.GetInvocationList())
            {
                var t = i.Target;
                var f = t as Form;
                var d = t as DispatcherObject;

                if (f != null && f.InvokeRequired)
                    f.Invoke(i);
                else if (d != null && !d.Dispatcher.CheckAccess())
                    d.Dispatcher.Invoke(DispatcherPriority.Normal, i);
                else
                    i.Method.Invoke(i.Target, null);
            }
        }
        #endregion

        #region Постоянное хранилище
        public IPersistStorage PersistStorage
        {
            get { return _pstg ?? (_pstg = new PersistStorage(ADM)); }
        }
        #endregion

        #region Управление задачами
        /// <summary>
        /// Запускает локальную задачу
        /// </summary>
        /// <param name="Name">Имя задачи</param>
        public ITask StartLocalTask(string Name)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write('F');
            wr.Write(Name);

            _admloc.SendMessage(ms.ToArray());

            return null;
        }

        /// <summary>
        /// Запускает группу задач в домене
        /// </summary>
        /// <param name="Name"></param>
        public void StartTasksGroup(string Name)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write('D');
            wr.Write(Name);

            CheckAdmLoc();

            _admloc.SendMessage(ms.ToArray());
        }

        /// <summary>
        /// Возвращает список локальных выполняющихся задач
        /// </summary>
        /// <returns>Cписок локальных выполняющихся задач</returns>
        public IList<string> GetLocalTaskNames()
        {
            _loctasksevt.Reset();

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write('H');

            _admloc.SendMessage(ms.ToArray());

            _loctasksevt.Wait();

            return _localtasks;
        }
        #endregion

        #region Проверка наличия запущенного fmsldr
        public static bool IsFmsldrLoaded
        {
            get
            {
                using (var c = new NamedPipeClientStream(".", "fmschanpipe"))
                {
                    try
                    {
                        c.Connect(250);

                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }
        #endregion

        #region Всплывающие оповещения
        /// <summary>
        /// Отображает всплывающее сообщение в трее
        /// </summary>
        /// <param name="Duration">Длительность отображения</param>
        /// <param name="Caption">Заголовок сообщения</param>
        /// <param name="Text">Текст сообщения</param>
        /// <param name="Icon">Иконка сообщения</param>
        /// <param name="Force">Игнорировать параметр Silent в главном конфигурационном файле</param>
        public void ShowBalloonTip(TimeSpan Duration, string Caption, string Text, ToolTipIcon Icon, bool Force)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms, Encoding.UTF8);

            wr.Write('J');
            wr.Write(Convert.ToInt32(Duration.TotalMilliseconds));
            wr.Write(Caption);
            wr.Write(Text);
            wr.Write((byte)Icon);
            wr.Write(Force);

            AdmLocChannel.SendMessage(ms.ToArray());
        }
        #endregion

        #region Журналирование
        /// <summary>
        /// Запись строки в журнал
        /// </summary>
        /// <param name="Log">Строка журнала</param>
        public void WriteLogLine(string Log)
        {
            WriteLogLine(TaskName, Log);
        }

        /// <summary>
        /// Запись строки в журнал
        /// </summary>
        /// <param name="Category">Категория записи</param>
        /// <param name="Log">Строка журнала</param>
        public void WriteLogLine(string Category, string Log)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms, Encoding.UTF8);

            wr.Write('K');
            wr.Write(Encoding.UTF8.CodePage);
            wr.Write(Log);
            wr.Write(Category);

            AdmLocChannel.SendMessage(ms.ToArray());
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_admloc != null)
            {
                _admloc.Leave();
                _admloc = null;
            }

            if (_adm != null)
            {
                _adm.Leave();
                _adm = null;
            }
        }
        #endregion

        #region Каналы связи
        internal ITransport GetTransport()
        {
            if (_domcbglue != null)
                return new AppDomainTransport(_domcbglue);
            else
                return new PipeTransport();
        }
        #endregion


        public int ClientAPIVersion
        {
            get { return 2; }
        }

        public int StrapAPIVersion
        {
            get
            {
                return _strapAPIVersion;
            }
            internal set
            {
                Debug.Assert(value > 0);
                Debug.Assert(_strapAPIVersion == 0 || _strapAPIVersion == value);

                _strapAPIVersion = value;
            }
        }

        private void RetreiveHostsInfo()
        {
            _hostsevt.Reset();

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write('N');

            AdmLocChannel.SendMessage(ms.ToArray());

            _hostsevt.Wait();
        }

        /// <summary>
        /// Список хостов в домене
        /// </summary>
        /// <returns></returns>
        public IList<string> GetDomainHostsNames()
        {
            RetreiveHostsInfo();

            return _hosts.Keys.ToList();
        }

        public bool IsHostInChannel(string Channel, string Host)
        {
            RetreiveHostsInfo();

            var h = _hosts;

            return h.ContainsKey(Host) && h[Host].Contains(Channel);
        }

        #region Расширения IManager2

        public string GetActualFMSLAPIPath()
        {
            return ServerFMSLAPIPath;
        }

        public byte[] GetActualFMSLAPI()
        {
            var p = GetActualFMSLAPIPath();

            if (string.IsNullOrWhiteSpace(p))
                return null;

            if (!File.Exists(p))
                return null;

            try
            {
                return File.ReadAllBytes(p);
            }
            catch (IOException)
            {
                return null;
            }
        }

        #endregion

    }
}
