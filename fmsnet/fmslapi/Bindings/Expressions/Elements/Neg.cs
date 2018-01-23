using System;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Neg : BaseUnary
    {
        public Neg(BaseExpression Op) : base(Op)
        {
        }

        protected override IValue InternalValue
        {
            get
            {
                var v = Operand.Value?.Value;

                if (v == null)
                    return new Value(0D);

                return new Value(v is int || v is float || v is double ? -Convert.ToDouble(Operand.Value.Value) : 0D);
            }
        }
    }
}
