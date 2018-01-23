using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using fmslstrap.Pipe;
using fmslstrap.Channel;
using fmslstrap.Variables;
using System.IO.Pipes;
using System.IO;
using System.Windows.Forms;
using fmslstrap.Configuration;
using fmslstrap.Tasks;
using fmslstrap.Administrator;
using fmslstrap.CommandSocket.PeerCommands;
using fmslstrap.Interface;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
// ReSharper disable ResourceItemNotResolved

namespace fmslstrap
{
    /// <summary>
    /// Общая точка входа API
    /// </summary>
    /// <remarks>
    /// Весь обмен данными и управление каналами осуществляется методами этого класса
    /// </remarks>
    public class Manager
    {
        #region Частные данные
        private static Mutex _globmutex;
        // ReSharper disable once NotAccessedField.Local
        private static VariablesManager _varman;
        private static DateTime LastWarnInvalidSignature = DateTime.Now.AddSeconds(-10);

        private static readonly ReaderWriterLockSlim _mychanslock = new ReaderWriterLockSlim();

        private static Dictionary<string, object> _initvals;

        private static string _versionstring;
        #endregion

        #region Инициализация
        private static object SafeGet(string Key)
        {
            _initvals.TryGetValue(Key, out var o);

            return o;
        }

        public static void Start(Dictionary<string, object> initvals)
        {
            _initvals = initvals;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                var msg = ex.GetType().ToString() + "\n--------------\n" + ex.Message + "\n--------------\n " + ex.StackTrace + "\r\n\r\n\r\n";
                Debug.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": " + msg);

                File.AppendAllText(Environment.ExpandEnvironmentVariables(@"%AllUsersProfile%\FMS700\excpts.txt"), msg);
                
#if DEBUG
                // ReSharper disable once RedundantNameQualifier
                if (!System.Diagnostics.Debugger.IsAttached)
                    MessageBox.Show(msg);
#else
                MessageBox.Show(msg);
#endif

            };

            InterfaceManager.Start(initvals["wf"] as fmsldr.FWaiting);

            ThreadPool.QueueUserWorkItem(x => InternalStart());
        }

        private static void InternalStart()
        {
            var rm = new ResourceManager("fmsresgen", Assembly.GetExecutingAssembly());
            _versionstring = rm.GetString("fms-version");

            BootstrapDeploy.InitData(SafeGet("asm") as byte[], SafeGet("pdb") as byte[]);

            bool globmutexwascreated;
            _globmutex = new Mutex(true, @"Global\fmslapi", out globmutexwascreated);

            if (!globmutexwascreated)
            {
                try
                {
                    var tc = new NamedPipeClientStream(".", "fmschanpipe", PipeDirection.InOut);
                    tc.Connect(2000);

                    // Если удалось подключиться - есть уже запущенный fmsldr
                    // Говорим ему - "Закройся"
                    tc.WriteByte((byte)'X');
                    tc.Flush();
                }
                catch (TimeoutException) { }
                catch (IOException) { }

                try
                {
                    if (!_globmutex.WaitOne(TimeSpan.FromSeconds(2)))
                    {
                        Application.Exit();
                        return;
                    }
                }
                catch (AbandonedMutexException) { }
            }

            //ThreadPool.SetMinThreads(5, 5);

            InterfaceManager.GlobalState = GlobalState.Initialization;

            var cfg = SafeGet("cfg") as string;

            ConfigurationManager.InitPreconfiguration(cfg);
            Config.Init();

            CommandSocket.CommandSocket.Create();

            NeighDiscCmd.InitNeighbourDiscovery();

            // Подключаемся к общему административному каналу
            //AdmChannel = new AdmChannel(JoinChannel("ADM", ChannelSocketSendType.TCP));
            AdmChannel = new AdmChannel(SubscribeToChannel("ADM", null, ChannelType.Regular));

            InterfaceManager.GlobalState = GlobalState.ConfigPending;

            // Загрузка конфигурации
            ConfigurationManager.InitConfiguration(cfg, AdmChannel);

            _varman = new VariablesManager();

            // Запуск сторожевых таймеров
            Variables.VarTypes.WVar.StartTimer();

            SnapshotsManager.Init(AdmChannel);
            AdmLocChannel.Init(AdmChannel);

            InterfaceManager.GlobalState = GlobalState.Active;

            InterfaceManager.ShowBalloonTip(1000, "Готовность", "Сетевой обмен готов к работе", ToolTipIcon.Info);

            Thread.Sleep(50);

            ConsoleRedirector.SetConsoleChannel(JoinChannel("ModelConsole", null, ChannelType.Regular));

            // Запуск компонентов автозагрузки
            TasksManager.Init(AdmChannel);

            // Теперь мы готовы принимать клиентские подключения
            PipeTransport.Init();

            var cmdline = AppDomain.CurrentDomain.GetData("ldrcmdline") as string[];

            if (cmdline != null)
                CmdLine.Execute(cmdline);
        }
        #endregion

        #region Публичные свойства

        public static int ClientAPIVersion => 2;

        public static string VersionString => _versionstring;

        #endregion

        #region Общая работа с каналами
        /// <summary>
        /// Возвращает теневую копию всех каналов, в которых состоит хост
        /// </summary>
        /// <returns>Список каналов</returns>
        internal static IEnumerable<ChanConfig> GetAllChannels()
        {
            try
            {
                _mychanslock.EnterReadLock();
                return MyChans.Values.ToArray();
            }
            finally
            {
                _mychanslock.ExitReadLock();
            }
        }

        internal static ChanConfig GetChannel(string Channel)
        {
            try
            {
                ChanConfig r;
                _mychanslock.EnterReadLock();

                MyChans.TryGetValue(Channel, out r);

                return r;
            }
            finally
            {
                _mychanslock.ExitReadLock();
            }
        }

        #region Подключение к каналу
        /// <summary>
        /// Запрос подключения к каналу
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Событие, происходящее при получении данных</param>
        /// <param name="ChanType">Тип канала</param>
        internal static ChanConfig JoinChannel(string Channel, ChanConfig.DataReceived Received, ChannelType ChanType)
        {
            try
            {
                _mychanslock.EnterUpgradeableReadLock();

                if (MyChans.ContainsKey(Channel))
                {
                    var mych = MyChans[Channel];

                    if (Received != null)
                        mych.OnDataReceived += Received;
                    
                    return mych;
                }

                ChanConfig pchan;

                if (ChanType == ChannelType.Local)
                {
                    pchan = new ChanConfig(Channel, null, ChanType);
                    pchan.OnDataReceived += Received;
                    _mychanslock.EnterWriteLock();
                    MyChans[Channel] = pchan;
                    _mychanslock.ExitWriteLock();
                    return pchan;
                }

                var pudp = new ChannelSocket();

                pchan = new ChanConfig(Channel, pudp, ChanType);
                pchan.OnDataReceived += Received;
                _mychanslock.EnterWriteLock();
                MyChans[Channel] = pchan;
                _mychanslock.ExitWriteLock();
                pchan.StartReceive();

                CommandSocket.CommandSocket.SendCommand(PeerHostConfCmd.GetCommand());
                CommandSocket.CommandSocket.SendCommand(PeerMembershipReqCmd.GetCommand());

                return pchan;
            }
            finally
            {
                _mychanslock.ExitUpgradeableReadLock();
            }
        }
        #endregion

        #region Отключение от канала
        /// <summary>
        /// Производит запрос на отключение от всех каналов
        /// </summary>
        internal static void LeaveAllChannels()
        {
            IEnumerable<string> arr;
            try
            {
                _mychanslock.EnterReadLock();
                arr = MyChans.Keys.ToArray();
            }
            finally
            {
                _mychanslock.ExitReadLock();
            }

            foreach (var ch in arr)
                LeaveChannel(ch);
        }

        /// <summary>
        /// Производит запрос отключения от канала
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        public static void LeaveChannel(string Channel)
        {
            ChanConfig chan;
            _mychanslock.EnterReadLock();
            MyChans.TryGetValue(Channel, out chan);
            _mychanslock.ExitReadLock();

            if (chan == null) 
                return;

            chan.Close();

            EndPointsList.RemoveHostFromChannel(Config.WorkstationName, Channel);

            _mychanslock.EnterWriteLock();
            MyChans.Remove(Channel);
            _mychanslock.ExitWriteLock();

            CommandSocket.CommandSocket.SendCommand(PeerHostConfCmd.GetCommand());
        }
        #endregion

        #endregion

        #region Сетевой обмен каналов
        /// <summary>
        /// Список каналов, в которых состоит этот хост
        /// </summary>
        private static readonly Dictionary<string, ChanConfig> MyChans = new Dictionary<string, ChanConfig>();
        #endregion

        #region Pipes
        /// <summary>
        /// Подписывается на оповещение о событии в канале
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Обработчик оповещения</param>
        /// <param name="ChannelType">Тип канала</param>
        /// <returns>Подключенный канал</returns>
        internal static ChanConfig SubscribeToChannel(string Channel, ChanConfig.DataReceived Received, ChannelType ChannelType)
        {
            var chan = JoinChannel(Channel, Received, ChannelType);
            chan.AddSubscriber();

            return chan;
        }

        /// <summary>
        /// Отписывается от оповещения о событии в канале
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Обработчик оповещения</param>
        internal static ChanConfig UnSubscribeFromChannel(string Channel, ChanConfig.DataReceived Received)
        {
            ChanConfig chan;

            try
            {
                _mychanslock.EnterReadLock();
                MyChans.TryGetValue(Channel, out chan);
            }
            finally
            {
                _mychanslock.ExitReadLock();
            }

            if (chan == null)
                return null;
            
            chan.OnDataReceived -= Received;
            chan.RemoveSubscriber();

            if (chan.SubscribersCount == 0)
                LeaveChannel(Channel);

            return chan;
        }
        #endregion

        #region Всплывающие сообщения в трее fmsldr
        public static void InvalidVariablesSignature()
        {
            if ((DateTime.Now - LastWarnInvalidSignature).TotalSeconds < 10)
                return;

            LastWarnInvalidSignature = DateTime.Now;

            InterfaceManager.ShowBalloonTip(2500, "Ошибка", "Обнаружены различающиеся сигнатуры каналов переменных у хостов в домене." +
                                                  "Возможно версии ПО в домене различаются.", ToolTipIcon.Error, true);
        }
        #endregion

        internal static AdmChannel AdmChannel
        {
            get;
            private set;
        }
    }
}
