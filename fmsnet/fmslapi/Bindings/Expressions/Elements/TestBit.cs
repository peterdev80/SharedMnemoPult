using System;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class TestBit : BaseUnary
    {
        private readonly ulong _bmask;

        public TestBit(BaseExpression Op, BaseExpression BitNum) : base(Op)
        {
            var bn = BitNum as Literal;

            if (bn == null)
                throw new InvalidOperationException("Неверный операнд функции bit");

            _bmask = (UInt64)1 << Convert.ToInt32(bn.Value?.Value);
        }

        protected override IValue InternalValue
        {
            get
            {
                var v = Operand.Value?.Value;

                if (v == null)
                    return new Value(false);

                try
                {
                    return new Value(((UInt64)(Convert.ToInt64(v)) & _bmask) != 0);
                }
                catch (InvalidCastException)
                {
                    return new Value(false);
                }
            }
        }
    }
}
