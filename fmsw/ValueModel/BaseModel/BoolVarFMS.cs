using fmslapi.Bindings.WPF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using ValueModel.BaseType;

namespace ValueModel.BaseModel
{
    [UseNamedVariablesContext("inpudc")]
    public class BoolVarFMS : VarClass
    {
        public BoolVarFMS(string varID)
            : base(varID)
        {
            ValState = new BoolValue(false);
        }
        public static readonly DependencyProperty VaRStateBoolPropertyFMS = DependencyProperty.Register(
           "VaRStateBoolFMS", typeof(bool), typeof(BoolVarFMS), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnStatVarUpd));
        public bool VaRStateBoolFMS
        {
            get { return (bool)GetValue(VaRStateBoolPropertyFMS); }
            set { this.SetValue(VaRStateBoolPropertyFMS, value); }
        }
        protected override void SetBinding(string varObmen, string varid)
        {






            VHBinding.StateBinding(this, BoolVarFMS.VaRStateBoolPropertyFMS, varObmen, varid);
            // LoadVariableType=1;
            //var vv = VHFMS.GetBoolVariable(varObmen);
            // BindingOperations.SetBinding(this, BoolVarFMS.VaRStateBoolPropertyFMS, new Binding { Source = vv, Path = new PropertyPath(Variable.ValueProperty) });


        }







        public BoolValue ValState { get; set; }

        private static void OnStatVarUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            BoolVarFMS con = d as BoolVarFMS;
            con.ValState.ValueState = (bool)e.NewValue;


        }

    }
}