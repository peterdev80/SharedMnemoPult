using System.Windows;
using System.Windows.Data;
using fmslapi.Bindings.Expressions;
using fmslapi.Bindings.Expressions.Elements;

namespace fmslapi.Bindings.WPF
{
    public class ExpressionBinding : BaseValueBinding
    {
        private readonly string _expression;

        // ReSharper disable once IntroduceOptionalParameters.Global
        public ExpressionBinding(string Expression) : this(Expression, BindingMode.Default)
        {
        }

        public ExpressionBinding(string Expression, BindingMode Mode)
        {
            _expression = Expression;
            BindingMode = Mode;
        }

        public override IValueSource GetSource()
        {
            var vs = BaseExpression.Parse(_expression);

            return vs is Ident ident ? ident.ValueSource : vs;
        }

        public static void SetBinding(DependencyObject Target, DependencyProperty Property, string VariableName,
            BindingMode Mode = BindingMode.Default, IValueConverter CustomConverter = null)
        {
            var b = new ExpressionBinding(VariableName, Mode) { _customconverter = CustomConverter };

            var bb = b.ProvideValue(Target, Property);

            BindingOperations.SetBinding(Target, Property, (BindingBase)bb);
        }
    }
}
