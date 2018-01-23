using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;

namespace fmsman.Formats
{
    public class VariableVisualizer2 : ContentControl
    {
        public static readonly DependencyProperty VariableProperty =
            DependencyProperty.Register("Variable", typeof(VarEntry), typeof(VariableVisualizer2), new PropertyMetadata(Changed));

        public VarEntry Variable
        {
            get => (VarEntry)GetValue(VariableProperty);
            set => SetValue(VariableProperty, value);
        }

        private static void Changed(object obj, DependencyPropertyChangedEventArgs e)
        {
            var v = obj as VariableVisualizer2;
            var ve = e.NewValue as VarEntry;

            Debug.Assert(ve != null, "ve != null");

            var vt = ve.VarType;

            if (vt.StartsWith("B") || vt.StartsWith("T"))
            {
                Debug.Assert(v != null, "v != null");

                v.Content = new BoolVarVisual { Variable = ve };
                return;
            }

            if (vt.StartsWith("K"))
            {
                Debug.Assert(v != null, "v != null");

                v.Content = new CMDVarVisual { Variable = ve };
                return;
            }

            if (vt.StartsWith("I") || vt.StartsWith("F") || vt.StartsWith("D") || vt.StartsWith("L"))
            {
                Debug.Assert(v != null, "v != null");

                v.Content = new NumericVarVisual { Variable = ve };
                return;
            } 
            
            if (vt.StartsWith("C"))
            {
                Debug.Assert(v != null, "v != null");

                v.Content = new CharVarVisual { Variable = ve };
                return;
            }

            if (vt.StartsWith("W"))
            {
                Debug.Assert(v != null, "v != null");

                v.Content = new WDVarVisual { Variable = ve };
                return;
            }

            Debug.Assert(v != null, "v != null");

            v.Content = new VariableVisualizer { Variable = ve };
        }
    }
}
