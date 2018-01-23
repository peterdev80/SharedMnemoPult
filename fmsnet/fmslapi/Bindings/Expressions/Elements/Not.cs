using System;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Not : BaseUnary
    {
        public Not(BaseExpression Op) : base(Op)
        {
        }

        protected override IValue InternalValue
        {
            get
            {
                var value = Operand.Value?.Value;

                try
                {
                    var v = value != null && Convert.ToBoolean(value);
                    return new Value(!v);
                }
                catch (FormatException)
                {
                    return new Value(false);
                }

            }
        }

        public override void UpdateSource(object NewValue)
        {
            if (NewValue is bool)
                Operand?.UpdateSource(!(bool)NewValue);
        }
    }
}
