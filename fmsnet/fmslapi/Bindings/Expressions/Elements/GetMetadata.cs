using System;
using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class GetMetadata : BaseExpression
    {
        private IValueSource _src;
        private string _metadataname;

        public GetMetadata(params BaseExpression[] Operands)
        {
            if (Operands.Length != 2)
                throw new ArgumentException("Ошибка в выражении");

            var op1 = Operands[0] as Ident;
            _src = op1?.ValueSource;

            var op2 = Operands[1] as Literal;
            _metadataname = op2?.Value?.Value?.ToString();

            if (op1 == null || op2 == null)
                throw new ArgumentException("Ошибка в выражени");
        }

        public override void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            base.Init(AttachedTo, DataContext);

            _src.Init(AttachedTo, DataContext);

            _src.ValueChanged += v => UpdateTarget();
        }

        protected override IValue InternalValue
        {
            get
            {
                var vm = _src.Value as IValueMetadata;

                return new Value(vm?.GetMetadata(_metadataname));
            }
        }
    }
}
