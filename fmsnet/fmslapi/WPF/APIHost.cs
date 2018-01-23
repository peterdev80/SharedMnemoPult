using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using fmslapi.WPF.Variables;
using System.Windows.Markup;
using System.Diagnostics;
using fmslapi.Storage;

namespace fmslapi.WPF
{
    [ContentProperty("VariablesManagers")]
    [DesignTimeVisible(false)]
    public class APIHost : FrameworkElement
    {
        #region Конструкторы
        public APIHost()
        {
            //_manager = fmslapi.Manager.GetAPI(EndPoint, ComponentID, Guid.Empty);

            VariablesManagers = new ObservableCollection<VariablesManager>();
        }
        #endregion

        #region Частные данные
        private IManager _manager;
        #endregion

        #region Свойства зависимостей
        public static readonly DependencyProperty IDProperty = DependencyProperty.Register("ID", typeof(string), typeof(APIHost), new PropertyMetadata(null, IDChanged));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", typeof(string), typeof(APIHost), new PropertyMetadata(null, EndPointChanged));
        public static readonly DependencyProperty ComponentIDProperty = DependencyProperty.Register("ComponentID", typeof(Guid), typeof(APIHost), new PropertyMetadata(Guid.Empty, EndPointChanged));
        public static readonly DependencyProperty VariablesManagersProperty = DependencyProperty.Register("VariablesManagers", typeof(ObservableCollection<VariablesManager>), typeof(APIHost), new FrameworkPropertyMetadata(null, ManagersChanged));
        #endregion

        #region Публичные методы

        public static APIHost GetAssociatedAPIHost(string HostKey)
        {
            var l = EnsureCollection();

            if (HostKey == null)
                if (l.Count > 0)
                    return l.Last().Value;
                else
                    return null;

            HostKey = HostKey.ToLowerInvariant();

            return l[HostKey];
        }

        public VariablesManager GetManager(string Key)
        {
            return VariablesManagers.FirstOrDefault(m => m.Key == Key);
        }

        #endregion

        #region Публичные свойства
        public string ID
        {
            get => (string)GetValue(IDProperty);
            set => SetValue(IDProperty, value);
        }

        [DependsOn("EndPoint")]
        [DependsOn("ComponentID")]
        public ObservableCollection<VariablesManager> VariablesManagers
        {
            get => (ObservableCollection<VariablesManager>)GetValue(VariablesManagersProperty);
            set => SetValue(VariablesManagersProperty, value);
        }

        public string EndPoint
        {
            get => (string)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        /// <summary>
        /// Уникальный код компонента
        /// </summary>
        /// <remarks>
        /// Используется для маппинга конфигурационных секций
        /// </remarks>
        public Guid ComponentID
        {
            get => (Guid)GetValue(ComponentIDProperty);
            set => SetValue(ComponentIDProperty, value);
        }

        public IManager Manager
        {
            get 
            {
                if (_manager == null)
                {
                    _manager = fmslapi.Manager.GetAPI(EndPoint, ComponentID);
                    _manager.HardConnectionCheck = true;
                }

                return _manager; 
            }
        }
        #endregion

        #region Обработка событий
#pragma warning disable 649
        // ReSharper disable once MemberCanBePrivate.Global
        internal static Action OnNewAPIHost;
#pragma warning restore 649

        private static void IDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var am = obj as APIHost;

            Debug.Assert(am != null, "am != null");

#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(am))
                return;
#endif

            var l = EnsureCollection();

            l.Add(am.ID.ToLowerInvariant(), am);

            OnNewAPIHost?.Invoke();
        }

        private static void EndPointChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var am = obj as APIHost;
            
            Debug.Assert(am != null, "am != null");

#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(am))
                return;
#endif
            if (am.ComponentID == Guid.Empty || string.IsNullOrEmpty(am.EndPoint))
                return;

            am._manager = fmslapi.Manager.GetAPI(am.EndPoint, am.ComponentID);
            am._manager.HardConnectionCheck = true;
        }

        private static void ManagersChanged(object Obj, DependencyPropertyChangedEventArgs e)
        {
            var h = Obj as APIHost;
            var c = e.NewValue as ObservableCollection<VariablesManager>;

            Debug.Assert(c != null, "c != null");

            c.CollectionChanged += (s2, e2) =>
                {
                    //var coll = s2 as ObservableCollection<VariablesManager>;
                    if (e2.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        var it = e2.NewItems[0] as VariablesManager;
                        
                        Debug.Assert(it != null, "it != null");

                        it.APIHost = h;

                        OnNewAPIHost?.Invoke();
                    }
                };
        }
        #endregion

        #region Вспомогательные методы
        private static Dictionary<object, APIHost> EnsureCollection()
        {
            var d = Application.Current.Resources;

            lock (d)
            {
                if (!d.Contains("fmslapihosts"))
                    d["fmslapihosts"] = new Dictionary<object, APIHost>();

                return d["fmslapihosts"] as Dictionary<object, APIHost>;
            }
        }
        #endregion

        #region Постоянное хранилище
        public IPersistStorage PersistStorage => _manager.PersistStorage;

        #endregion
    }
}
