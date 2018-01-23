using System;
using System.Diagnostics;
using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings
{
    public abstract class BaseChainConverter : IValueSource
    {
        private readonly IValueSource _source;
        private IValue _outval;

        protected BaseChainConverter(IValueSource Source)
        {
            _source = Source;
        }

        public void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            _source.ValueChanged += SourceOnValueChanged;

            _source.Init(AttachedTo, DataContext);
        }

        private void SourceOnValueChanged(IValue NewValue)
        {
            _outval = Convert(NewValue);

            ValueChanged?.Invoke(_outval);
        }

        public IValue Value => _outval;

        public Type ValueType => null;

        public event SourceValueChanged ValueChanged;

        public void UpdateTarget()
        {
            Debug.Assert(_source != null, "_source != null");

            _source.UpdateTarget();
        }

        public void UpdateSource(object NewValue)
        {
            throw new NotImplementedException();
        }

        protected abstract IValue Convert(IValue Value);
    }
}
