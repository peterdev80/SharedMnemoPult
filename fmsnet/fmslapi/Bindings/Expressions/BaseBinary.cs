using fmslapi.Bindings.Expressions.Elements;
using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings.Expressions
{
    /// <summary>
    /// Базовое операция с двумя операндами
    /// </summary>
    public abstract class BaseBinary : BaseExpression
    {
        protected readonly BaseExpression Oper1;
        protected readonly BaseExpression Oper2;

        protected BaseBinary(BaseExpression Op1, BaseExpression Op2)
        {
            Oper1 = Op1;
            Oper2 = Op2;
        }

        public static BaseExpression Parse(Scanner Scanner, BaseExpression Op1, ExpressionContext Context)
        {
            var r = ParseOr(Scanner, Op1, Context);

            return r;
        }

        private static BaseExpression ParseOr(Scanner Scanner, BaseExpression Op1, ExpressionContext Context)
        {
            var r = ParseAnd(Scanner, Op1, Context);

            while (true)
            {
                var t = Scanner.Token;

                if (t == null)
                    return r;

                t = t.ToLowerInvariant();

                if (t != "|" && t != "or")
                    return r;

                Scanner.Next();
                r = new Or(r, ParseAnd(Scanner, BaseUnary.Parse(Scanner, Context), Context));
            }
        }

        private static BaseExpression ParseAnd(Scanner Scanner, BaseExpression Op1, ExpressionContext Context)
        {
            var r = ParseEql(Scanner, Op1, Context);

            while (true)
            {
                var t = Scanner.Token;

                if (t == null)
                    return r;

                t = t.ToLowerInvariant();

                if (t != "&" && t != "and")
                    return r;

                Scanner.Next();
                r = new And(r, ParseEql(Scanner, BaseUnary.Parse(Scanner, Context), Context));
            }
        }

        private static BaseExpression ParseEql(Scanner Scanner, BaseExpression Op1, ExpressionContext Context)
        {
            var r = ParseRel(Scanner, Op1, Context);

            while (true)
            {
                switch (Scanner.Token)
                {
                    case "==":
                        Scanner.Next();
                        r = new Equality(r, ParseRel(Scanner, BaseUnary.Parse(Scanner, Context), Context), Equality.Op.Equal);
                        break;

                    case "!=":
                        Scanner.Next();
                        r = new Equality(r, ParseRel(Scanner, BaseUnary.Parse(Scanner, Context), Context), Equality.Op.NotEqual);
                        break;

                    case "~=":
                        Scanner.Next();
                        r = new Match(r, ParseRel(Scanner, BaseUnary.Parse(Scanner, Context), Context));
                        break;

                    default:
                        return r;
                }
            }
        }

        private static BaseExpression ParseRel(Scanner Scanner, BaseExpression Op1, ExpressionContext Context)
        {
            var r = ParseAdd(Scanner, Op1, Context);

            while (true)
            {
                switch (Scanner.Token)
                {
                    case "<":
                    case "lss":
                        Scanner.Next();
                        r = new Equality(r, ParseAdd(Scanner, BaseUnary.Parse(Scanner, Context), Context), Equality.Op.Less);
                        break;

                    case ">":
                    case "gtr":
                        Scanner.Next();
                        r = new Equality(r, ParseAdd(Scanner, BaseUnary.Parse(Scanner, Context), Context), Equality.Op.Greater);
                        break;

                    case ">=":
                        Scanner.Next();
                        r = new Equality(r, ParseRel(Scanner, BaseUnary.Parse(Scanner, Context), Context), Equality.Op.GreaterOrEqual);
                        break;

                    case "<=":
                        Scanner.Next();
                        r = new Equality(r, ParseRel(Scanner, BaseUnary.Parse(Scanner, Context), Context), Equality.Op.LessOrEqual);
                        break;

                    default:
                        return r;
                }
            }
        }

        private static BaseExpression ParseAdd(Scanner Scanner, BaseExpression Op1, ExpressionContext Context)
        {
            var r = ParseMul(Scanner, Op1, Context);

            while (true)
            {
                switch (Scanner.Token)
                {
                    case "+":
                        Scanner.Next();
                        r = new Add(r, ParseMul(Scanner, BaseUnary.Parse(Scanner, Context), Context));
                        break;

                    case "-":
                        Scanner.Next();
                        r = new Sub(r, ParseMul(Scanner, BaseUnary.Parse(Scanner, Context), Context));
                        break;

                    default:
                        return r;
                }
            }
        }

        private static BaseExpression ParseMul(Scanner Scanner, BaseExpression Op1, ExpressionContext Context)
        {
            var r = Op1;

            while (true)
            {
                switch (Scanner.Token)
                {
                    case "*":
                        Scanner.Next();
                        r = new Mul(r, BaseUnary.Parse(Scanner, Context));
                        break;

                    case "/":
                        Scanner.Next();
                        r = new Div(r, BaseUnary.Parse(Scanner, Context));
                        break;

                    default:
                        return r;
                }
            }
        }

        public override void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            Oper1.ValueChanged += OnValueChanged;
            Oper2.ValueChanged += OnValueChanged;

            Oper1.Init(AttachedTo, DataContext);
            Oper2.Init(AttachedTo, DataContext);
        }

        private void OnValueChanged(object v)
        {
            UpdateTarget();
        }
    }
}
