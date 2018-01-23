using System;
using System.Collections.Generic;
using fmslapi.Bindings.Expressions.Elements;

namespace fmslapi.Bindings.Expressions
{
    public abstract class BasePrimary : BaseExpression
    {
        public new static BaseExpression Parse(Scanner Scanner, ExpressionContext Context)
        {
            var rc = Literal.Parse(Scanner, Context);

            if (rc != null)
                return rc;

            rc = ParseIntrinsics(Scanner, Context);
            if (rc != null)
                return rc;


            if (Scanner.Token == "(")
            {
                Scanner.Next();
                var ie = BaseExpression.Parse(Scanner, Context);

                if (Scanner.Get() != ")")
                    throw new ArgumentException("Ошибка в выражении");

                return ie;
            }

            return Ident.Parse(Scanner, Context);
        }

        private static readonly Dictionary<string, Type> _intrinsics = new Dictionary<string, Type>();

        static BasePrimary()
        {
            _intrinsics.Add("abs", typeof(Abs));
            _intrinsics.Add("sqrt", typeof(Sqrt));
            _intrinsics.Add("hypot", typeof(Hypot));
            _intrinsics.Add("bit", typeof(TestBit));
            _intrinsics.Add("tovis", typeof(ToVisibility));
            _intrinsics.Add("tovis_h", typeof(ToVisibilityHid));
            _intrinsics.Add("bool", typeof(Bool));
            _intrinsics.Add("format", typeof(Format));
            _intrinsics.Add("neg", typeof(Neg));
            _intrinsics.Add("not", typeof(Not));
            _intrinsics.Add("getmetadata", typeof(GetMetadata));
        }

        public static void RegisterPrimaryHandler(string Keyword, Type type)
        {
            if (_intrinsics.ContainsKey(Keyword))
                return;

            _intrinsics.Add(Keyword, type);
        }

        private static BaseExpression ParseIntrinsics(Scanner Scanner, ExpressionContext Context)
        {
            var intr = Scanner.Token.ToLowerInvariant();

            if (!_intrinsics.TryGetValue(intr, out var intrt))
                return null;

            Scanner.Get();

            if (Scanner.Get() != "(")
                throw new ArgumentException("Ошибка в выражении");

            var opl = new List<object>();

            while (true)
            {
                opl.Add(BaseExpression.Parse(Scanner, Context));

                var nt = Scanner.Get();

                if (nt == ")")
                    break;

                if (nt != ",")
                    throw new ArgumentException("Ошибка в выражении");
            }

            return Activator.CreateInstance(intrt, opl.ToArray()) as BaseExpression;
        }
    }
}