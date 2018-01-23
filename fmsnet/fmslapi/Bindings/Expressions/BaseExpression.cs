using System;
using fmslapi.Bindings.Expressions.Elements;
using fmslapi.Bindings.WPF;
using fmslapi.VDL.WPF;

namespace fmslapi.Bindings.Expressions
{
    public abstract class BaseExpression : IValueSource
    {
        protected bool _isdirty = true;
        protected IValue _cachedval;

        public static BaseExpression Parse(string Expression)
        {
            return Parse(new Scanner(Expression), new ExpressionContext());
        }

        protected static BaseExpression Parse(Scanner Scanner, ExpressionContext Context)
        {
            var op1 = BaseUnary.Parse(Scanner, Context);

            var r = BaseBinary.Parse(Scanner, op1, Context);

            if (Scanner.Token == "?")
            {
                Scanner.Get();

                var pos = Parse(Scanner, Context);

                if (Scanner.Get() != ":")
                    throw new ArgumentException("Ошибка в выражении");

                var neg = Parse(Scanner, Context);

                r = new Cond(r, pos, neg);
            }

            return r;
        }


        public virtual void Init(object AttachedTo, VariablesDataContext DataContext)
        {
        }

        public event SourceValueChanged ValueChanged;

        public void UpdateTarget()
        {
            _isdirty = true;

            var h = ValueChanged;
            if (h == null)
                return;

            var v = Value;

            if (v is ScriptValueSource.DoNothing)
                return;

            h(v);
        }

        public virtual void UpdateSource(object NewValue) { }

        protected void SetDirty()
        {
            _isdirty = true;
        }

        public IValue Value
        {
            get
            {
                if (_isdirty)
                {
                    _cachedval = InternalValue;

                    _isdirty = false;
                }

                return _cachedval;
            }
        }

        public virtual Type ValueType => null;

        protected abstract IValue InternalValue { get; }
    }
}
