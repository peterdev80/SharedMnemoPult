using System;

namespace fmslapi.Bindings.WPF
{
    public class TimeSpanValueBinding : ExpressionBinding
    {
        private IValueSource _vs;
        private string _fs;

        private class tsc : BaseChainConverter
        {
            private readonly string _fs;

            public tsc(IValueSource Source, string FormatString) : base(Source)
            {
                _fs = FormatString.Replace(":", @"\:");
            }

            protected override IValue Convert(IValue Val)
            {
                var ts = TimeSpan.FromSeconds(System.Convert.ToDouble(Val?.Value));

                return new Value(ts.ToString(_fs));
            }
        }

        public TimeSpanValueBinding(string VariableName) : base(VariableName)
        {
            _fs = @"hh:mm:ss";
        }

        public override IValueSource GetSource()
        {
            _vs = new tsc(base.GetSource(), _fs);
            return _vs;
        }

        public string FormatString
        {
            get => _fs;
            set => _fs = value;
        }
    }
}