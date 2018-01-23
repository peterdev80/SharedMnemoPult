using System;
using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Ident : BaseExpression
    {
        private readonly IValueSource _ivs;

        internal IValueSource ValueSource => _ivs;

        private Ident(string Ident, string Key)
        {
            _ivs = VariableValueSource.Create(Ident, Key);

            _ivs.ValueChanged += v => UpdateTarget();
        }

        public new static BaseExpression Parse(Scanner Scanner, ExpressionContext Context)
        {
            BaseExpression r;
            var id = Scanner.Get();

            string k = null;
            var t = Scanner.Token;

            if (t != null && t.StartsWith("@"))
                k = Scanner.Get();

            var key = id;
            if (k != null)
            {
                key += k;

                k = k.Replace("@", "");
            }

            if (Context.IdentCache.TryGetValue(key, out r))
                return r;

            r = new Ident(id, k);
            Context.IdentCache.Add(key, r);

            return r;
        }

        public override void UpdateSource(object NewValue)
        {
            _ivs?.UpdateSource(NewValue);
        }

        public override Type ValueType => _ivs?.ValueType;

        protected override IValue InternalValue => _ivs.Value;

        public override void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            _ivs.Init(AttachedTo, DataContext);
        }
    }
}
