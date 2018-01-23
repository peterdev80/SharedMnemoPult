using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using fmslapi.Channel;
// ReSharper disable All

namespace fmslapi.Components
{
    public delegate void VariablesChanged(object sender, FMSVariable[] ChangedVars);

    [Designer("fmslapi.designer.Components.FMSLVarManagerDesigner, fmslapi.designer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=800057c2b283a020")]
    public partial class FMSLVarManager : Component
    {
        #region Конструкторы
        public FMSLVarManager()
        {
            InitializeComponent();
        }

        public FMSLVarManager(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
        #endregion

        #region Частные данные
        private LVarMap _varmap;
        private bool _active;
        private readonly List<FMSVariable> _varlist = new List<FMSVariable>();

        private readonly HashSet<FMSVariable> _varthathaveownevents = new HashSet<FMSVariable>();

// ReSharper disable RedundantDefaultFieldInitializer
        private bool _connected = false;
// ReSharper restore RedundantDefaultFieldInitializer

        private IManager _apimanager;
        private IVariablesChannel _varchan;
        #endregion

        #region Публичные свойства
        [Browsable(true)]
        public Guid ComponentID
        {
            get;
            set;
        }

        [Browsable(false)]
        public string EndPointName
        {
            get;
            set;
        }
        
        [Description("Карта переменных")]
        public LVarMap VarMap
        {
            get { return _varmap ?? (_varmap = new LVarMap()); }
            //set
            //{
//                _varmap = value;
            //}
        }

        private string _channame;

        [
        Description("Имя канала"), Category("SnapshotVariables"), Localizable(false), DefaultValue(""),
        RefreshProperties(RefreshProperties.All)
        ]
        public string ChannelName
        {
            get
            {
                return _channame;
            }
            set
            {
                _channame = value;
                Connect();
            }
        }

        [
        Description("Имя карты переменных"), Category("SnapshotVariables"), Localizable(false), DefaultValue(""),
        RefreshProperties(RefreshProperties.All),
        Editor("fmslapi.designer.Components.LVariablesMapTypeEditor", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0")
        ]
        public string VariablesMap { get; set; }

        [Description("true разрешает подключение к fmsloader"), Category("SnapshotVariables"), Localizable(false), DefaultValue(false)]
        public bool Active 
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                Connect();
            }
        }

        [
        Description("Зарегистрированные экземпляры переменных"), Category("SnapshotVariables"), Localizable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),Browsable(false)
        ]
        public FMSVariable[] RegisteredVariables
        {
            get
            {
                return _varlist.ToArray();
            }
        }
        #endregion

        #region Публичные методы
        public void RegisterVariableInstance(FMSVariable variable)
        {
            _varlist.Add(variable);
        }

        public void UnregisterVariableInstance(FMSVariable variable)
        {
            _varlist.Remove(variable);
        }

        /// <summary>
        /// Отсылает изменившиеся переменные в канал
        /// </summary>
        public void SendChanges()
        {
            _varchan.SendChanges();
        }
        #endregion

        #region Внутернние методы
        internal void IHaveChangeEvent(FMSVariable var)
        {
            _varthathaveownevents.Add(var);
        }

        internal void IDontHaveChangeEvent(FMSVariable var)
        {
            _varthathaveownevents.Remove(var);
        }

        private void Connect()
        {
            if (!_connected && Active)
            {
                // Подключаемся
#if DEBUG
                if (DesignMode)
                {
                    _connected = false;
                    return;
                }
#endif

                _apimanager = Manager.GetAPI(EndPointName, ComponentID);
                _varchan = _apimanager.SafeJoinVariablesChannel(ChannelName, VariablesMap, RaiseStateChanged, RouteVariablesChanged);

                PerformVariablesRegistration();

                _connected = true;

                return;
            }

            if (_connected && !Active)
            {
                // Отключаемся
                foreach (var v in _varlist) v.Detach();

                _varchan.Leave();

                _connected = false;
            }
        }

        private void PerformVariablesRegistration()
        {
            lock (_varlist)
            {
                foreach (var var in _varlist)
                {
                    var.NativeVariable = _varchan.GetVariable(var.VarName);
                }
            }
        }
        #endregion

        #region Публичные события
        public event VariablesChanged VariablesChanged;

        /// <summary>
        /// Связь с сервером потеряна
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Связь с сервером установлена
        /// </summary>
        public event EventHandler Connected;

        private void RaiseStateChanged(ChannelStateChangedStates args)
        {
            switch (args)
            {
                case ChannelStateChangedStates.FirstConnect:
                case ChannelStateChangedStates.Connected:
                    if (Connected != null)
                    {
                        Connected(this, EventArgs.Empty);
                    }
                    break;

                case ChannelStateChangedStates.Disconnected:
                case ChannelStateChangedStates.CantConnect:
                    if (Disconnected != null)
                    {
                        Disconnected(this, EventArgs.Empty);
                    }
                    break;
            }

        }

        private void RouteVariablesChanged(bool IsInit, IVariable[] ChangedList)
        {
            if (VariablesChanged == null) 
                return;

            var form = (Form)VariablesChanged.Target;
            if (form.InvokeRequired)
            {
                form.BeginInvoke(VariablesChanged, this, null);
            }
            else
            {
                VariablesChanged(this, null);
            }
        }

        #endregion
    }
}
