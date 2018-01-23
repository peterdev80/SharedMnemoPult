using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using fmslapi.Annotations;
using System.Windows.Media;

namespace fmslapi.Bindings.WPF
{
    public static class VariablesDataContextExt
    {
        public static VariablesDataContext GetVariablesDataContext(this DependencyObject Source)
        {
            return VariablesDataContext.GetVariablesDataContext(Source);
        }
    }

    public class VariablesDataContext : DependencyObject, INotifyPropertyChanged
    {
        #region Частные данные

        private IManager _manager;
        private IVariablesChannel _variablesChannel;
        private string _formatstring;
        private VariablesRootDataContext _rootcontext;
        private readonly DependencyObject _attachedTo;
        private string _varchanname;
        private string _cachedformatstring;
        private IManager _cachedmanager;
        private VariablesDataContext _forceparent;
        private bool _isnamed;

        // ReSharper disable once UnusedMember.Local
        private static Dictionary<TimeSpan, DispatcherTimer> _registeredUpdateTimers =
            new Dictionary<TimeSpan, DispatcherTimer>();

        private static readonly Dictionary<string, VariablesDataContext> _namedcontexts =
            new Dictionary<string, VariablesDataContext>();

        #endregion

        #region Конструкторы

        public VariablesDataContext()
        {
        }

        internal VariablesDataContext(DependencyObject AttachedTo)
        {
            _attachedTo = AttachedTo;

            Debug.Assert(_attachedTo != null);
        }

        public VariablesDataContext Clone(DependencyObject NewAttachedTo)
        {
            var r = new VariablesDataContext(NewAttachedTo);

            // Свойства обязательно наследуемые из корневого
            r._manager = _manager;
            r._rootcontext = _rootcontext;

            // Перекрываемые свойства
            // Копируем все, т.к. по умолчанию null -> значение из родительского контекста
            r._formatstring = _formatstring;
            r._varchanname = _varchanname;

            return r;
        }

        #endregion

        #region Свойства зависимостей

        public static readonly DependencyProperty VariablesDataContextProperty = DependencyProperty.RegisterAttached(
            "VariablesDataContext", typeof(VariablesDataContext), typeof(VariablesDataContext),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        #endregion
        
        public static void SetVariablesDataContext(DependencyObject element, VariablesDataContext value)
        {
            element.SetValue(VariablesDataContextProperty, value);
        }

        public static VariablesDataContext GetVariablesDataContext(DependencyObject element)
        {
            var nc =
                Attribute.GetCustomAttribute(element.GetType(), typeof(UseNamedVariablesContextAttribute)) as
                UseNamedVariablesContextAttribute;

            if (nc == null)
                return (VariablesDataContext)element.GetValue(VariablesDataContextProperty);

            if (!string.IsNullOrWhiteSpace(nc.Name))
                // ReSharper disable once ArrangeStaticMemberQualifier
                return VariablesDataContext.GetNamedContext(nc.Name);

            if (nc.RType == null)
                return null;

            var n = (element as IGetVariablesContextName)?.Name;

            return string.IsNullOrWhiteSpace(n) ? null : GetNamedContext(n);
        }

        public VariablesDataContext Parent 
        {
            get
            {
                if (_forceparent != null)
                    return _forceparent;

                if (_isnamed)
                    return null;

                var p = LogicalTreeHelper.GetParent(_attachedTo) ?? VisualTreeHelper.GetParent(_attachedTo);

                return p != null ? p.GetVariablesDataContext() : _rootcontext;
            }
            set => _forceparent = value;
        }

        public IManager Manager
        {
            get 
            {
                if (_manager != null)
                    return _manager;

                var p = Parent;

                if (p == null && _cachedmanager != null)
                    return _cachedmanager;

                return p?.Manager;
            }
            set
            {
                if (Equals(value, _manager)) 
                    return;

                _manager = value;
                _cachedmanager = value;
                OnPropertyChanged(nameof(Manager));
            }
        }

        public string FormatString 
        {
            get
            {
                if (_formatstring != null)
                    return _formatstring;

                var p = Parent;

                if (p == null && _cachedformatstring != null)
                    return _cachedformatstring;

                return p == null ? "0" : p.FormatString;
            }
            set
            {
                if (value == _formatstring) 
                    return;

                _formatstring = value;
                _cachedformatstring = value;
                OnPropertyChanged(nameof(FormatString));
            }
        }

        public IVariablesChannel VariablesChannel
        {
            get
            {
                if (_variablesChannel == null && _isnamed)
                    _variablesChannel = RootContext.GetVariablesChannel(_varchanname);

                return _variablesChannel ?? (_variablesChannel = RootContext.GetVariablesChannel(VariablesChannelName));
            }
        }

        public string VariablesChannelName 
        {
            get => _varchanname ?? Parent.VariablesChannelName;
            set => _varchanname = value;
        }

        public VariablesRootDataContext RootContext
        {
            get
            {
                if (_isnamed)
                    return this as VariablesRootDataContext;

                if (_rootcontext == null)
                {
                    var rc = this;
                    while (rc != null && !rc.IsRoot)
                        rc = rc.Parent;

                    _rootcontext = rc as VariablesRootDataContext;
                }

                return _rootcontext; 
            }
        }

        public bool IsRoot => this is VariablesRootDataContext;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public static VariablesRootDataContext GetRootContext(DependencyObject RootObject, string EndPoint)
        {
            var r = new VariablesRootDataContext(RootObject)
                    {
                        FormatString = "0.000",
                        _endpoint = EndPoint
                    };

            r._rootcontext = r;

            Application.Current.Resources[EndPoint] = r;

            RootObject.SetValue(VariablesDataContextProperty, r);
            
            return r;
        }

        public static VariablesDataContext GetNamedContext(string Name)
        {
            if (_namedcontexts.TryGetValue(Name, out var o))
                return o;

            var r = new VariablesRootDataContext(new FrameworkElement())
                    {
                        FormatString = "0.000",
                        _endpoint = Name,
                        _isnamed = true
                    };

            _namedcontexts[Name] = r;

            return r;
        }

        //public fmslapi.WPF.Variables.Variable GetVariable(string Name)
        //{
        //    var v = new fmslapi.WPF.Variables.Variable { Name = Name };

        //    v.TryRegister(this);

        //    return v;
        //}
    }
}
