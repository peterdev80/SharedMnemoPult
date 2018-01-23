namespace fmslapi.Bindings.Expressions.Elements
{
    public class And : BaseBinary
    {
        public And(BaseExpression Op1, BaseExpression Op2)
            : base(Op1, Op2)
        {
        }
        
        
        protected override IValue InternalValue
        {
            get
            {
                var v1 = Oper1.Value?.Value;
                var v2 = Oper2.Value?.Value;

                return new Value(v1 is bool && v2 is bool && ((bool)v1 && (bool)v2));
            }
        }
    }
}
