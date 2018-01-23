namespace fmslapi.Bindings.Expressions.Elements
{
    public class ToVisibility : BaseUnary
    {
        public ToVisibility(BaseExpression Op) : base(Op)
        {
        }

        protected override IValue InternalValue
        {
            get
            {
                var v = Operand.Value?.Value;

                if (!(v is bool))
                    return new Value("Collapsed");

                return new Value((bool)v ? "Visible" : "Collapsed");
            }
        }
    }

    public class ToVisibilityHid : BaseUnary
    {
        public ToVisibilityHid(BaseExpression Op)
            : base(Op)
        {
        }

        protected override IValue InternalValue
        {
            get
            {
                var v = Operand.Value?.Value;

                if (!(v is bool))
                    return new Value("Hidden");

                return new Value((bool)v ? "Visible" : "Hidden");
            }
        }
    }
}
