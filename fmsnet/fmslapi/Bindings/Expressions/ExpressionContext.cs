using System.Collections.Generic;

namespace fmslapi.Bindings.Expressions
{
    public class ExpressionContext
    {
        public readonly Dictionary<string, BaseExpression> IdentCache = new Dictionary<string, BaseExpression>();
    }
}
