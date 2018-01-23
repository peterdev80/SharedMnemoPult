using System;
using System.Globalization;
using System.Linq;
using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Format : BaseExpression
    {
        private readonly BaseExpression _formatstring;
        private readonly BaseExpression[] _ops;

        public Format(params BaseExpression[] Operands)
        {
            if (Operands.Length == 0)
                throw new ArgumentException("Ошибка в выражении");

            _ops = Operands.Skip(1).ToArray();
            _formatstring = Operands[0];
        }

        public override void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            base.Init(AttachedTo, DataContext);

            _formatstring.Init(AttachedTo, DataContext);
            _formatstring.ValueChanged += OperandChanged;

            foreach (var o in _ops)
            {
                o.Init(AttachedTo, DataContext);
                o.ValueChanged += OperandChanged;
            }
            
        }

        private void OperandChanged(object Val)
        {
            UpdateTarget();
        }

        protected override IValue InternalValue
        {
            get
            {
                var fs = _formatstring.Value?.Value;

                if (fs == null)
                    return new Value("");

                var vals = _ops.Select(x => x.Value?.Value).ToArray();
                return new Value(string.Format(CultureInfo.InvariantCulture, fs.ToString(), vals));
            }
        }
    }
}
