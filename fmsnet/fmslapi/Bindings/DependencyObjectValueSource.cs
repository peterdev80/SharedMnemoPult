using System;
using System.ComponentModel;
using System.Windows;
using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings
{
    public class DependencyObjectValueSource<T> : IValueSource where T : class 
    {
        private T V => (T)_obj.GetValue(_prop);

        private DependencyObject _obj;
        private readonly DependencyProperty _prop;
        private bool _silent;

        public DependencyObjectValueSource(DependencyProperty Property)
        {
            _prop = Property;
        }

        public void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            _obj = AttachedTo as DependencyObject;

            if (_obj == null)
                return;

            var dpd = DependencyPropertyDescriptor.FromProperty(_prop, _obj.GetType());

            dpd.AddValueChanged(_obj, vc);

            ValueChanged?.Invoke(new Value(V));
        }

        private void vc(object Sender, EventArgs E)
        {
            if (_silent)
                return;

            ValueChanged?.Invoke(new Value(V));
        }

        public event SourceValueChanged ValueChanged;

        public void UpdateTarget()
        {
            throw new NotImplementedException();
        }

        public void UpdateSource(object NewValue)
        {
            try
            {
                _silent = true;

                _obj.SetValue(_prop, NewValue);
            }
            finally
            {
                _silent = false;
            }
        }

        public IValue Value => new Value(V);

        public Type ValueType => typeof(T);
    }

    public class DependencyObjectValueSource : DependencyObjectValueSource<object>
    {
        public DependencyObjectValueSource(DependencyProperty Property) : base(Property) { }
    }
}
