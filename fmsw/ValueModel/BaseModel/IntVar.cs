using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ValueModel.BaseType;

namespace ValueModel.BaseModel
{
    public class IntVar : VarClass
    {
        public static readonly DependencyProperty VaRStateIntProperty = DependencyProperty.Register(
               "VaRStateInt", typeof(int), typeof(IntVar), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnStatVarUpd));
        public int VaRStateInt
        {
            get { return (int)GetValue(VaRStateIntProperty); }
            set { this.SetValue(VaRStateIntProperty, value); }
        }
        public IntVar(string varID)
            : base(varID)
        {
            ValState = new IntValue();
        }

        public IntValue ValState { get; set; }
        protected override void SetBinding(string varObmen, string varID)
        {
            VHBinding.IntBinding(this, varObmen, varID);
        }
        private static void OnStatVarUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            IntVar con = d as IntVar;
            con.ValState.ValueState = int.Parse(e.NewValue.ToString()); ;


        }
    }
}
