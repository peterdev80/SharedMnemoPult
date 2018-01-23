using System;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Equality : BaseBinary
    {
        public enum Op
        {
            Equal,
            NotEqual,
            GreaterOrEqual,
            LessOrEqual,
            Less,
            Greater
        }

        private readonly Op _op;

        public Equality(BaseExpression Op1, BaseExpression Op2, Op Op)
            : base(Op1, Op2)
        {
            _op = Op;
        }

        private bool Cmp<T>(T A, T B) where T : IComparable
        {
            var a = A as IComparable;
            var b = B as IComparable;

            switch (_op)
            {
                case Op.Equal:
                    return a.CompareTo(b) == 0;
                case Op.NotEqual:
                    return a.CompareTo(b) != 0;
                case Op.GreaterOrEqual:
                    return a.CompareTo(b) >= 0;
                case Op.LessOrEqual:
                    return a.CompareTo(b) <= 0;
                case Op.Less:
                    return a.CompareTo(b) < 0;
                case Op.Greater:
                    return a.CompareTo(b) > 0;
                default:
                    return false;
            }
        }

        protected override IValue InternalValue
        {
            get
            {
                var v1 = Oper1.Value?.Value;
                var v2 = Oper2.Value?.Value;

                if (v1 == null || v2 == null)
                    return new Value(false);

                if (v1 is bool || v2 is bool)
                    return new Value(Cmp((bool)v1, (bool)v2));

                if (v1.GetType().IsEnum)
                    v1 = Convert.ToInt32(v1);

                if (v2.GetType().IsEnum)
                    v2 = Convert.ToInt32(v2);

                v1 = Convert.ToDouble(v1);
                v2 = Convert.ToDouble(v2);

                return new Value(Cmp((double)v1, (double)v2));
            }
        }
    }
}
