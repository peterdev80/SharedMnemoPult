using System;
using System.Windows.Markup;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using ut = fmslapi.UpdateTriggers;

namespace fmslapi.WPF.Variables
{
    public delegate void VariableChanged(Variable Variable);

    public delegate void VariableChangedEventHandler(object sender, VariableChangedEventArgs e);

    public class VariableChangedEventArgs : RoutedEventArgs
    {
        public object NewValue;

        public VariableChangedEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            NewValue = ((Variable)source).GetValue(Variable.ValueProperty);
        }
    }

    /// <summary>
    /// Базовый класс объектов переменных для WPF
    /// </summary>
    [ContentProperty("Value")]
    [DesignTimeVisible(false)]
#if DEBUG
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
#endif
    public class Variable : FrameworkElement
    {
        #region Частные данные
        private IVariable _nativevariable;
        protected bool _isregistered;
        private bool _ischanged;
        private ut.TriggerBase _updtrigger;
        #endregion

        #region События
        public event Action<Variable> RegistrationComplete;

        public event Func<string, string> ValidateVariableName;

        /// <summary>
        /// Событие происходит при внешнем, по отношению к приложению, изменении переменной 
        /// </summary>
        public event VariableChangedEventHandler VariableChanged
        {
            add => AddHandler(VariableChangedEvent, value);
            remove => RemoveHandler(VariableChangedEvent, value);
        }
        #endregion

        #region Конструкторы
        static Variable()
        {
            VariablesHost.ManagerProperty.AddOwner(typeof(Variable), new FrameworkPropertyMetadata(mc));
        }

        public Variable()
        {
            Initialized += (s, e) => TryRegister();
        }
        #endregion

        #region Свойства зависимостей
        public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register("VariableName", typeof(string), typeof(Variable), new PropertyMetadata(null));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(Variable));
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register("IsValid", typeof(object), typeof(Variable), new PropertyMetadata(false));

        public static readonly RoutedEvent VariableChangedEvent = EventManager.RegisterRoutedEvent("VariableChanged", RoutingStrategy.Direct, typeof(VariableChangedEventHandler), typeof(Variable));
        #endregion

        #region Публичные свойства
        internal IVariable NativeVariable => _nativevariable;

        public VariableType Type
        {
            get
            {
                if (_nativevariable == null)
                    return VariableType.Unknown;

                return _nativevariable.VariableType;
            }
        }

        /// <summary>
        /// После установки, значение автоматически сохраняется в постоянном хранилище
        /// </summary>
        public bool PersistentVariable
        {
            get;
            set;
        }

        /// <summary>
        /// Автоматическая отправка пакета изменений при изменении этой переменной
        /// </summary>
        /// <remarks>
        /// Отправляется весь пакет изменившихся переменных с момента последней отправки
        /// </remarks>
        public bool AutoSend
        {
            get;
            set;
        }

        /// <summary>
        /// Проверять и игнорировать при отправке одно и то же значение
        /// </summary>
        public bool CheckDups
        {
            get;
            set;
        }

        internal string OriginalVariableName { get; set; }

        /// <summary>
        /// Имя переменной
        /// </summary>
        public string VariableName
        {
            get => (string)GetValue(VariableNameProperty);
            set => SetValue(VariableNameProperty, value);
        }

        /// <summary>
        /// Объект менеджера переменных
        /// </summary>
        [DependsOn("VariableName")]
        [DependsOn("Name")]
        public VariablesManager Manager
        {
            get => (VariablesManager)GetValue(VariablesHost.ManagerProperty);
            set => SetValue(VariablesHost.ManagerProperty, value);
        }

        /// <summary>
        /// Переменная успешно зарегистрирована и имеет верное значение
        /// </summary>
        public bool IsValid => (bool)GetValue(IsValidProperty);

        /// <summary>
        /// Признак изменения переменной внешним источником
        /// </summary>
        /// <remarks>
        /// При чтении обнуляется
        /// </remarks>
        public bool IsChanged
        {
            get
            {
                try
                {
                    return _ischanged;
                }
                finally
                {
                    _ischanged = false;
                }
            }
            internal set => _ischanged = value;
        }

        public object Value
        {
            // ReSharper disable once ValueParameterNotUsed
            set
            {
            }
        }

        public ut.TriggerBase UpdateTrigger
        {
            get => _updtrigger;

            set
            {
                if (_nativevariable != null)
                    _nativevariable.UpdateTrigger = value;

                _updtrigger = value;
            }
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Сохраняет значение переменной в постоянном хранилище
        /// </summary>
        public void SavePersistent()
        {
            _nativevariable?.SavePersistent();
        }
        #endregion

        #region Обработка событий
        private static void mc(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as Variable)?.TryRegister();
        }

        private void TryRegister()
        {
            if (_isregistered)
                return;

            if (Manager == null)
                return;

            if (string.IsNullOrEmpty(VariableName) && !string.IsNullOrEmpty(Name))
                VariableName = Name;

            if (!string.IsNullOrEmpty(VariableName) && ValidateVariableName != null)
                VariableName = ValidateVariableName(VariableName);

            if (string.IsNullOrEmpty(VariableName))
                return;

            var isv = Manager.RegisterVariable(this, out var nv);
            if (!isv)
                return;

            _isregistered = true;
            _nativevariable = nv;
            nv.UpdateTrigger = _updtrigger;

            if (this is KVariable v)
                v.NativeVariable = nv;

            SetValue(IsValidProperty, true);

            RegistrationComplete?.Invoke(this);
        }
        #endregion

        #region Визуализация отладки
#if DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                if (!IsValid)
                    return string.Format("{0} = ???", VariableName);
                
                return string.Format("{0} = {1} ({2})", VariableName,
                                                        GetValue(ValueProperty), 
                                                        Type);
            }
        }
#endif
        #endregion
    }
}
