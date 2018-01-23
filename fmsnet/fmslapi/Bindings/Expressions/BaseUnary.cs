using System;
using System.Collections.Generic;
using fmslapi.Bindings.Expressions.Elements;
using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings.Expressions
{
    public abstract class BaseUnary : BaseExpression
    {
        private static readonly Dictionary<string, Type> _unaries = new Dictionary<string, Type>();

        protected readonly BaseExpression Operand;

        static BaseUnary()
        {
            _unaries.Add("-", typeof(Neg));
            _unaries.Add("!", typeof(Not));
        }

        protected BaseUnary(BaseExpression Op)
        {
            Operand = Op;
        }

        public new static BaseExpression Parse(Scanner Scanner, ExpressionContext Context)
        {
            var uo = Scanner.Token;

            if (_unaries.TryGetValue(uo, out var ut))
            {
                Scanner.Get();

                var ue = Activator.CreateInstance(ut, BasePrimary.Parse(Scanner, Context)) as BaseExpression;
                return ue;
            }

            return BasePrimary.Parse(Scanner, Context);
        }

        public override void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            Operand.ValueChanged += v => UpdateTarget();

            Operand.Init(AttachedTo, DataContext);
        }
    }
}
