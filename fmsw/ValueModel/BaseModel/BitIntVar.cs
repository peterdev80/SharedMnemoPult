using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ValueModel.BaseType;

namespace ValueModel.BaseModel
{
    public class BitIntVar : VarClass
    {
        public static readonly DependencyProperty VarBitIntProperty = DependencyProperty.Register(
              "VarBitInt", typeof(int), typeof(BitIntVar), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnStatVarUpd));
        public int VarBitInt
        {
            get { return (int)GetValue(VarBitIntProperty); }
            set { this.SetValue(VarBitIntProperty, value); }
        }


        public ObservableCollection<IntValue> ValState { get; set; }

        private List<int> _mask;
        public BitIntVar(string varID, List<int> mask)
            : base(varID)
        {

            _mask = mask;
            ValState = new ObservableCollection<IntValue>();
            for (int i = 0; i < _mask.Count; i++)
            {
                ValState.Add(new IntValue());
            }
        }
        protected override void SetBinding(string varObmen, string varID)
        {

            VHBinding.BitIntBinding(this, varObmen, varID);
        }

        private static void OnStatVarUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BitIntVar con = d as BitIntVar;
            int v = (int)e.NewValue;
            uint val = (uint)v;

            int position = 0;
            if (con.ValState != null)
            {
                for (int i = 0; i < con.ValState.Count; i++)
                {
                    int lenght = con._mask[i];

                    int sd = (1 << lenght) - 1;
                    // int sd = (int)Math.Pow((double)2, (double)lenght) - 1;
                    var _val = (val >> position) & sd;
                    con.ValState[i].ValueState = (int)_val;
                   
                    position = position + con._mask[i];
                }
            }
        }

    }
}
