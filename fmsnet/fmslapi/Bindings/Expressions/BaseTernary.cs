using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings.Expressions
{
    public abstract class BaseTernary : BaseExpression
    {
        protected readonly BaseExpression Oper1;
        protected readonly BaseExpression Oper2;
        protected readonly BaseExpression Oper3;

        protected BaseTernary(BaseExpression Op1, BaseExpression Op2, BaseExpression Op3)
        {
            Oper1 = Op1;
            Oper2 = Op2;
            Oper3 = Op3;
        }

        public override void Init(object AttachedTo, VariablesDataContext DataContext)
        {
            Oper1.ValueChanged += v => UpdateTarget();
            Oper2.ValueChanged += v => UpdateTarget();
            Oper3.ValueChanged += v => UpdateTarget();

            Oper1.Init(AttachedTo, DataContext);
            Oper2.Init(AttachedTo, DataContext);
            Oper3.Init(AttachedTo, DataContext);
        }
    }
}
