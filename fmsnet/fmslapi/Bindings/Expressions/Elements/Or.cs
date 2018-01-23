namespace fmslapi.Bindings.Expressions.Elements
{
    public class Or : BaseBinary
    {
        public Or(BaseExpression Op1, BaseExpression Op2) : base(Op1, Op2)
        {
        }

        protected override IValue InternalValue
        {
            get
            {
                var v1 = Oper1.Value?.Value;
                var v2 = Oper2.Value?.Value;

                if (v1 is bool && v2 is bool)
                    return new Value(((bool)v1 || (bool)v2));

                return new Value(false);
            }
        }
    }
}
