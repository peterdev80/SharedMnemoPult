using System;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Mul : BaseBinary
    {
        public Mul(BaseExpression Op1, BaseExpression Op2)
            : base(Op1, Op2)
        {
            
        }

        protected override IValue InternalValue
        {
            get
            {
                var v1 = Oper1.Value?.Value;
                var v2 = Oper2.Value?.Value;

                double dv1, dv2;

                if (v1 is int || v1 is float || v1 is double)
                    dv1 = Convert.ToDouble(v1);
                else
                    return new Value((double)0);

                if (v2 is int || v2 is float || v2 is double)
                    dv2 = Convert.ToDouble(v2);
                else
                    return new Value((double)0);

                return new Value(dv1 * dv2);
            }
        }
    }
}
