using System;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Cond : BaseTernary
    {
        public Cond(BaseExpression Op1, BaseExpression Op2, BaseExpression Op3) : base(Op1, Op2, Op3)
        {
        }

        protected override IValue InternalValue
        {
            get
            {
                var op1v = Oper1.Value?.Value;

                var cond = op1v != null && Convert.ToBoolean(op1v);

                return cond ? Oper2.Value : Oper3.Value;
            }
        }
    }
}