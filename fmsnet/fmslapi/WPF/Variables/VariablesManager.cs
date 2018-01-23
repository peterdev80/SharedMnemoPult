using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using fmslapi.Channel;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Управляющий объект, используемый для взаимодействия с fmsldr
    /// </summary>
    [DesignTimeVisible(false)]
    public class VariablesManager : FrameworkElement
    {
        #region Частные данные
        private string _managerkey = "<def>";
        private string _channel;
        private string _varmap;
        private string _endpoint;

        private IVariablesChannel _varchan;

        private readonly Dictionary<int, ImVar> _rvi = new Dictionary<int, ImVar>();
        private readonly Dictionary<string, ImVar> _rvn = new Dictionary<string, ImVar>();

        public static readonly RoutedEvent ConnectionLostEvent = EventManager.RegisterRoutedEvent("ConnectionLost", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VariablesManager));

        public event RoutedEventHandler ConnectionLost
        {
            add => AddHandler(ConnectionLostEvent, value);
            remove => RemoveHandler(ConnectionLostEvent, value);
        }

        public static readonly RoutedEvent ConnectionRestoreEvent = EventManager.RegisterRoutedEvent("ConnectionRestore", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VariablesManager));

        public event RoutedEventHandler ConnectionRestore
        {
            add => AddHandler(ConnectionRestoreEvent, value);
            remove => RemoveHandler(ConnectionRestoreEvent, value);
        }
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Игнорировать события инициализации значений переменных
        /// </summary>
        public bool IgnoreInitEvents
        {
            get;
            set;
        }

        [Category("Компоненты fmsldr")]
        public string Channel
        {
            get => _channel;
            set => _channel = value;
        }

        [Category("Компоненты fmsldr")]
        public string VariablesMap
        {
            get => _varmap;
            set => _varmap = value;
        }

        [Category("Компоненты fmsldr")]
        public string Endpoint
        {
            get => _endpoint;
            set => _endpoint = value;
        }

        public APIHost APIHost
        {
            get;
            set;
        }

        public string Key
        {
            get => _managerkey;
            set => _managerkey = value;
        }

        public IVariablesChannel NativeVariablesChannel => _varchan;

        #endregion

        #region Подключение

        /// <summary>
        /// Осуществляет подключение к fmsldr
        /// </summary>
        private void CheckConnect()
        {
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
#endif

            if (_varchan != null)
                return;

            if (string.IsNullOrWhiteSpace(Channel) || string.IsNullOrWhiteSpace(VariablesMap) || string.IsNullOrWhiteSpace(Endpoint)) 
                return;

            var z = APIHost.Manager;

            _varchan = z.SafeJoinVariablesChannel(_channel, _endpoint, _varmap, StateChanged, VariablesChanged);
        }
        #endregion

        #region Регистрация переменных

        /// <summary>
        /// Регистрирует переменную в fmsldr
        /// </summary>
        /// <param name="var">Переменная</param>
        /// <param name="NativeVariable">Низкоуровневая переменная для подключения</param>
        /// <returns>true - в случае успешной регистрации, false - в случае отсутствия переменной с заданным именем</returns>
        public bool RegisterVariable(Variable var, out IVariable NativeVariable)
        {
            NativeVariable = null;

            CheckConnect();

            if (_varchan == null)
                return false;

            if (ValidateVariableName != null)
                var.VariableName = ValidateVariableName(var.VariableName);

            lock (_rvi)
            {
                ImVar iv;
                if (!_rvn.TryGetValue(var.VariableName, out iv))
                {
                    var nv = _varchan.GetVariable(var.VariableName);

                    if (nv.Index == -1)
                        return false;

                    iv = new ImVar(nv);

                    _rvi.Add(nv.Index, iv);
                    _rvn.Add(nv.VariableName, iv);
                }

                NativeVariable = iv.NativeVariable;
                iv.AttachWpfEp(var);
            }

            return true;
        }
        #endregion

        #region Отправка изменений
        /// <summary>
        /// Отправляет пакет изменившихся переменных
        /// </summary>
        public void SendChanges()
        {
            _varchan?.SendChanges();
        }

        /// <summary>
        /// Отправляет изменившуюся переменную
        /// </summary>
        /// <remarks>
        /// Только если она действительно была изменена
        /// </remarks>
        public void SendChanges(Variable Variable)
        {
            if (_varchan == null)
                return;

            var v = Variable;
            if (v == null)
                return;

            _varchan.SendChanges(v.NativeVariable);
        }

        /// <summary>
        /// Отправляет ограниченный пакет изменившихся переменных
        /// </summary>
        /// <param name="Source">
        /// Отправляются только переменные, присутствующие в этом списке
        /// </param>
        public void SendChanges(IEnumerable<Variable> Source)
        {
            _varchan?.SendChanges(from v in Source where v != null select v.NativeVariable);
        }
        #endregion

        #region Публичные события
        public event Func<string, string> ValidateVariableName;
        #endregion

        #region События
        /// <summary>
        /// Изменение пакета переменных
        /// </summary>
        /// <param name="IsInit"></param>
        /// <param name="ChangedList"></param>
        private void VariablesChanged(bool IsInit, IVariable[] ChangedList)
        {
            foreach (var cv in ChangedList)
            {
                ImVar iv;
                if (!_rvi.TryGetValue(cv.Index, out iv))
                    continue;

                iv.AvoidEvents = IsInit && IgnoreInitEvents;

                iv.NativeEp = cv.VariableType == VariableType.KMD ? true : cv.Value;
            }
        }

        /// <summary>
        /// Изменение состояния канала
        /// </summary>
        /// <param name="args">Новое состояние</param>
        private void StateChanged(ChannelStateChangedStates args)
        {
            switch (args)
            {
                case ChannelStateChangedStates.Connected:

                    var evr = new RoutedEventArgs(ConnectionRestoreEvent);
                    RaiseEvent(evr);
                    break;

                case ChannelStateChangedStates.CantConnect:
                case ChannelStateChangedStates.Disconnected:
                    var ev = new RoutedEventArgs(ConnectionLostEvent);
                    RaiseEvent(ev);
                    break;
            }
        }
        #endregion

        #region Сброс сторожевых переменных
        public void ResetWatchDogs(IEnumerable<BooleanVariable> Variables)
        {
            NativeVariablesChannel.ResetWatchDogs(Variables.Select(v => v.NativeVariable).OfType<IWatchDogVariable>());
        }
        #endregion
    }
}
