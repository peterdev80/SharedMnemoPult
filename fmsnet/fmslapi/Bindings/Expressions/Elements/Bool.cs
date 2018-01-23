namespace fmslapi.Bindings.Expressions.Elements
{
    public class Bool : BaseUnary
    {
        public Bool(BaseExpression Op) : base(Op)
        {
        }

        protected override IValue InternalValue
        {
            get
            {
                var v = Operand.Value?.Value;

                if (v == null)
                    return new Value(false);

                if (v is bool)
                    return new Value(v);

                var sv = v.ToString().Trim().ToLowerInvariant();

                return new Value(sv == "1" || sv == "on" || sv == "true" || sv == "yes");
            }
        }
    }
}
