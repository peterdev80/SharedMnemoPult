using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ValueModel.BaseType;

namespace ValueModel.BaseModel
{
    public class BoolVar : VarClass
    {
        public static readonly DependencyProperty VaRStateBoolProperty = DependencyProperty.Register(
           "VaRStateBool", typeof(bool), typeof(VarClass), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnStatVarUpd));
        public bool VaRStateBool
        {
            get { return (bool)GetValue(VaRStateBoolProperty); }
            set { this.SetValue(VaRStateBoolProperty, value); }
        }
        protected override void SetBinding(string varObmen, string varID)
        {
            VHBinding.BoolBinding(this, varObmen, varID);
        }
        public BoolVar(string varID)
            : base(varID)
        {
            ValState = new BoolValue(false);
        }

        public BoolValue ValState { get; set; }

        private static void OnStatVarUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            BoolVar con = d as BoolVar;
            con.ValState.ValueState = (bool)e.NewValue;


        }

    }
}
