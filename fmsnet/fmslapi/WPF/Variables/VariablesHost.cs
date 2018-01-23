using System;
using System.Linq;
using System.Windows.Markup;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics;
using w = fmslapi.WPF.Variables;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Хост переменных для WPF контрола
    /// </summary>
    [ContentProperty("Variables")]
    public class VariablesHost : FrameworkElement
    {
        #region Свойства зависимостей
        public static DependencyProperty DefaultHostProperty = DependencyProperty.RegisterAttached("DefaultHost", typeof(VariablesHost), typeof(VariablesHost), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static DependencyProperty ManagerProperty = DependencyProperty.RegisterAttached("Manager", typeof(object), typeof(VariablesHost), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, null, CoerceManager));
        public static DependencyProperty ManagerKeyProperty = DependencyProperty.Register("ManagerKey", typeof(string), typeof(VariablesHost), new PropertyMetadata(KeyChanged));

        public static VariablesHost GetDefaultHost(DependencyObject element)
        {
            return (VariablesHost)element.GetValue(DefaultHostProperty);
        }

        public static void SetDefaultHost(FrameworkElement element, VariablesHost value)
        {
            element.SetValue(DefaultHostProperty, value);
        }
        #endregion

        #region Конструкторы
        public VariablesHost()
            : this(null)
        {
            SetBinding(ManagerProperty, new Binding { Source = this, Path = new PropertyPath(DefaultHostProperty) });
        }

        private static object CoerceManager(DependencyObject d, Object v)
        {
            var host = v as VariablesHost;
            return host != null ? host.Manager : v;
        }
        
        public VariablesHost(string ManagerKey)
        {
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
#endif

            if (ManagerKey != null)
                this.ManagerKey = ManagerKey;
        }
        #endregion

        #region События
        public event Func<string, string> ValidateVariableName;
        #endregion

        #region Частные данные
        private ObservableCollection<Variable> _innervars;
        #endregion

        #region Публичные свойства
        public string ManagerKey
        {
            get => (string)GetValue(ManagerKeyProperty);
            set => SetValue(ManagerKeyProperty, value);
        }

        /// <summary>
        /// Список управляемых этим хостом переменных
        /// </summary>
        public ObservableCollection<Variable> Variables
        {
            get
            {
                if (_innervars == null)
                {
                    _innervars = new ObservableCollection<Variable>();
                    _innervars.CollectionChanged += _innervars_CollectionChanged;
                }

                return _innervars;
            }
        }

        /// <summary>
        /// Объект менеджера, используемый для непосредственного взаимодействия
        /// </summary>
        public VariablesManager Manager
        {
            get => (VariablesManager)GetValue(ManagerProperty);
            set => SetValue(ManagerProperty, value);
        }
        #endregion

        #region Обработка событий
        void _innervars_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var v = e.NewItems[0] as Variable;

                    // ReSharper disable once PossibleNullReferenceException
                    v.ValidateVariableName += ValidateVariableName;
                    AddLogicalChild(v);
                    break;
            }
        }

        private static void KeyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var fe = o as VariablesHost;
            var ah = e.NewValue as string;

            Debug.Assert(fe != null, "fe != null");

            fe.AssignManager(ah);
        }
        #endregion

        #region Публичные методы
        internal void TryAssignManager()
        {
            if (Manager == null)
                AssignManager(ManagerKey);
        }

        // ReSharper disable once ParameterHidesMember
        internal void AssignManager(string ManagerKey)
        {
            var kk = ManagerKey.Split('\\', '/');
            string hk = null;
            string k;
            if (kk.Length == 1)
                k = kk[0];
            else
            {
                k = kk[1];
                hk = kk[0];
            }

#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
#endif

            var h = APIHost.GetAssociatedAPIHost(hk);
            if (h != null)
                Manager = h.GetManager(k);
        }
        #endregion

        #region Поддержка программной регистрации
        /// <summary>
        /// Регистрирует переменную с заданным именем
        /// </summary>
        /// <typeparam name="T">Тип регистрируемой переменной</typeparam>
        /// <param name="VariableName">Имя регистрируемой переменной</param>
        /// <returns>Переменная</returns>
        [DebuggerStepThrough]
        internal T GetVariable<T>(string VariableName) where T : Variable, new()
        {
            return GetVariable<T>(VariableName, null);
        }
        
        /// <summary>
        /// Регистрирует переменную с заданным именем
        /// </summary>
        /// <typeparam name="T">Тип регистрируемой переменной</typeparam>
        /// <param name="VariableName">Имя регистрируемой переменной</param>
        /// <param name="RegistrationComplete">Событие, вызываемое по окончанию регистрации переменной</param>
        /// <returns>Переменная</returns>
        internal T GetVariable<T>(string VariableName, Action<Variable> RegistrationComplete) where T : Variable, new()
        {
            var pv = _innervars?.FirstOrDefault(x => x.VariableName == VariableName && x is T);

            if (pv != null)
            {
                RegistrationComplete?.Invoke(pv);
                return (T)pv;
            }

            var man = Manager;
            if (man == null)
                return null;

            var vo = VariableName;

            if (ValidateVariableName != null)
                VariableName = ValidateVariableName(VariableName);

            var b = new T {VariableName = VariableName, OriginalVariableName = vo };
            if (RegistrationComplete != null)
                b.RegistrationComplete += RegistrationComplete;

            b.CoerceValue(Variable.ValueProperty);

            Variables.Add(b);

            return b;
        }

        /// <summary>
        /// Регистрирует логическую переменную с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public BooleanVariable GetBoolVariable(string VariableName)
        {
            return GetVariable<BooleanVariable>(VariableName);
        }        
        
        /// <summary>
        /// Регистрирует переменную одинарной точности с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public FloatVariable GetFloatVariable(string VariableName)
        {
            return GetVariable<FloatVariable>(VariableName);
        }

        /// <summary>
        /// Регистрирует переменную двойной точности с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public DoubleVariable GetDoubleVariable(string VariableName)
        {
            return GetVariable<DoubleVariable>(VariableName);
        }

        /// <summary>
        /// Регистрирует командную переменную с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public KVariable GetKVariable(string VariableName)
        {
            return GetVariable<KVariable>(VariableName);
        }

        /// <summary>
        /// Регистрирует целочисленную переменную с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public IntVariable GetIntVariable(string VariableName)
        {
            return GetVariable<IntVariable>(VariableName);
        }

        /// <summary>
        /// Регистрирует целочисленную переменную размером 64 бита с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public LongVariable GetLongVariable(string VariableName)
        {
            return GetVariable<LongVariable>(VariableName);
        }

        /// <summary>
        /// Регистрирует символьную переменную с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public CharVariable GetCharVariable(string VariableName)
        {
            return GetVariable<CharVariable>(VariableName);
        }

        /// <summary>
        /// Регистрирует строковую переменную с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public StringVariable GetStringVariable(string VariableName)
        {
            return GetVariable<StringVariable>(VariableName);
        }

        /// <summary>
        /// Регистрирует переменную байтовый массив с заданным именем
        /// </summary>
        /// <param name="VariableName">Имя переменной</param>
        /// <returns>Объект переменной</returns>
        public ByteArrayVariable GetByteArrayVariable(string VariableName)
        {
            return GetVariable<ByteArrayVariable>(VariableName);
        }

        #endregion

        #region Отправка изменений
        /// <summary>
        /// Отправляет пакет изменившихся переменных
        /// </summary>
        public void SendChanges()
        {
            Manager?.SendChanges();
        }
        #endregion
    }
}
