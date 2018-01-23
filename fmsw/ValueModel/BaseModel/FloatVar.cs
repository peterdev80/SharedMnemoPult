using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

using ValueModel.BaseType;


namespace ValueModel.BaseModel
{
    public class FloatVar : VarClass
    {
        public static readonly DependencyProperty VaRStatefloatProperty = DependencyProperty.Register(
               "VaRStatefloat", typeof(float), typeof(FloatVar), new FrameworkPropertyMetadata(0f,FrameworkPropertyMetadataOptions.AffectsRender, OnStatVarUpd));
        public float VaRStatefloat
        {
            get { return (float)GetValue(VaRStatefloatProperty); }
            set { this.SetValue(VaRStatefloatProperty, value); }
        }
        public FloatVar(string varID)
            : base(varID)
        {
            ValState = new FloatValue();
        }

        public FloatValue ValState { get; set; }
        protected override void SetBinding(string varObmen, string varID)
        {
            VHBinding.FloatBinding(this, FloatVar.VaRStatefloatProperty, varObmen, varID);
        }
        private static void OnStatVarUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            FloatVar con = d as FloatVar;
            con.ValState.ValueState = float.Parse(e.NewValue.ToString()); ;


        }
    }
}

