using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ValueModel.BaseType;


namespace ValueModel.BaseModel
{
    public class DoubleVar : VarClass
    {
        public DoubleVar(string VarID)
            : base(VarID)
        {
            ValState = new DoubleValue();
        }
        public static readonly DependencyProperty VaRStateDoubleProperty = DependencyProperty.Register(
          "VaRStateDouble", typeof(Double), typeof(DoubleVar), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, OnStatVarUpd));
        public Double VaRStateDouble
        {
            get { return (Double)GetValue(VaRStateDoubleProperty); }
            set { this.SetValue(VaRStateDoubleProperty, value); }
        }
        public DoubleValue ValState { get; set; }
        protected override void SetBinding(string varObmen, string varID)
        {
            VHBinding.DoubleBinding(this, varObmen, varID);
        }
        private static void OnStatVarUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            DoubleVar con = d as DoubleVar;
            con.ValState.ValueState = Double.Parse(e.NewValue.ToString()); ;


        }
    }
}
