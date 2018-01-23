using System.Globalization;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Literal : BaseExpression
    {
        private object _val;

        public new static BaseExpression Parse(Scanner Scanner, ExpressionContext Context)
        {
            var t = Scanner.Token;

            if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            {
                Scanner.Next();

                return new Literal { _val = v };
            }

            if (t.StartsWith("'") || t.StartsWith("`"))
            {
                var vs = t;

                Scanner.Next();

                return new Literal { _val = vs.Replace("'", "").Replace("`", "") };
            }

            return null;
        }

        protected override IValue InternalValue => new Value(_val);
    }
}
