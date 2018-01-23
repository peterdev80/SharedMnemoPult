
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace fmslapi.Bindings.Expressions.Elements
{
    public class Match : BaseUnary
    {
        private readonly Regex _r;

        public Match(BaseExpression Op, BaseExpression Regex) : base(Op)
        {
            var l = Regex as Literal;

            Debug.Assert(l != null, "l != null");

            _r = new Regex(l.Value.ToString());
        }

        protected override IValue InternalValue
        {
            get
            {
                var v = Operand.Value?.Value;

                if (v == null)
                    return new Value(false);

                var s = v.ToString();

                return new Value(_r.IsMatch(s));
            }
        }
    }
}
