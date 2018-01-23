using System;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Sqrt : BaseUnary
    {
        public Sqrt(BaseExpression Op) : base(Op)
        {
        }

        protected override IValue InternalValue
        {
            get
            {
                var v = Operand.Value?.Value;

                if (v == null)
                    return new Value(0D);

                var vd = v is int || v is float || v is double ? Convert.ToDouble(v) : 0D;

                return new Value(Math.Sqrt(vd));
            }
        }
    }
}
