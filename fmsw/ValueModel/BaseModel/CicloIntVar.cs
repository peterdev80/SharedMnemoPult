using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ValueModel.BaseType;

namespace ValueModel.BaseModel
{
  
        public class CicloIntVar : VarClass
        {
            public static readonly DependencyProperty VarCicloIntProperty = DependencyProperty.Register(
                "VarCicloInt", typeof(int), typeof(CicloIntVar), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnStatVarUpd));
            public int VarCicloInt
            {
                get { return (int)GetValue(VarCicloIntProperty); }
                set { this.SetValue(VarCicloIntProperty, value); }
            }
            public IntValue ValState { get; set; }
            private int _mask;
            public CicloIntVar(string varID, int mask)
                : base(varID)
            {
                _mask = mask;
                ValState = new IntValue();
            }
            protected override void SetBinding(string varObmen, string varID)
            {

                VHBinding.CicloIntBinding(this, varObmen, varID);
            }
            private static void OnStatVarUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                CicloIntVar con = d as CicloIntVar;
                int v = (int)e.NewValue;
                uint val = (uint)v;


                if (con.ValState != null)
                {
                    con.ValState.ValueState = 0;
                    if (val == con._mask) con.ValState.ValueState = 1;


                   
                }
            }

        }
    }

