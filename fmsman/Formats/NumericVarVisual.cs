using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Input;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable RedundantCast

namespace fmsman.Formats
{
    public class NumericVarVisual : EditableVarVisual
    {
        protected UIElement _up;
        protected UIElement _down;

        static NumericVarVisual()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericVarVisual), new FrameworkPropertyMetadata(typeof(NumericVarVisual)));
        }

        private TextBlock _txt;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _up = GetTemplateChild("PART_up") as UIElement;
            _down = GetTemplateChild("PART_down") as UIElement;

            Debug.Assert(_up != null, "_up != null");
            Debug.Assert(_down != null, "_down != null");

            _up.MouseDown += _up_MouseDown;
            _down.MouseDown += _down_MouseDown;

            _txt = GetTemplateChild("PART_txt") as TextBlock;

            Reformat();
        }

        void _down_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                if (Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.LeftShift))
                    mult(0.5);
                else
                    step(-1);
            }
        }

        void _up_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                if (Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.LeftShift))
                    mult(2);
                else
                    step(1);
            }
        }

        private void step(int step)
        {
            var ve = Variable;

            try
            {
                var vt = ve.VarType;
                var vac = VarEntry.Accessor;

                if (vt.StartsWith("I"))
                    vac.Write(ve.ShOffset, vac.ReadInt32(Variable.ShOffset) + step);

                if (vt.StartsWith("L"))
                    vac.Write(ve.ShOffset, vac.ReadInt64(Variable.ShOffset) + step);

                if (vt.StartsWith("F"))
                    vac.Write(ve.ShOffset, vac.ReadSingle(Variable.ShOffset) + (float)step);

                if (vt.StartsWith("D"))
                    vac.Write(ve.ShOffset, vac.ReadDouble(Variable.ShOffset) + (double)step);

                SendAsChanged();
            }
            catch (FormatException) { }
            catch (OverflowException) { }
        }

        private void mult(double step)
        {
            var ve = Variable;

            try
            {
                var vt = ve.VarType.StartsWith("I");
                var vac = VarEntry.Accessor;

                if (vt)
                    vac.Write(ve.ShOffset, (Int32)(vac.ReadInt32(Variable.ShOffset) * step));

                if (ve.VarType.StartsWith("L"))
                    vac.Write(ve.ShOffset, (Int64)(vac.ReadInt64(Variable.ShOffset) * step));

                if (ve.VarType.StartsWith("F"))
                    vac.Write(ve.ShOffset, (Single)(vac.ReadSingle(Variable.ShOffset) * (float)step));

                if (ve.VarType.StartsWith("D"))
                    vac.Write(ve.ShOffset, (Double)(vac.ReadDouble(Variable.ShOffset) * (double)step));

                SendAsChanged();
            }
            catch (FormatException) { }
            catch (OverflowException) { }
        }

        protected override void Reformat()
        {
            if (_txt == null)
                return;

            var vt = Variable.VarType[0];
            var vac = VarEntry.Accessor;

            switch (vt)
            {
                case 'I':
                    _txt.Text = vac.ReadInt32(Variable.ShOffset).ToString(CultureInfo.InvariantCulture);
                    break;

                case 'L':
                    _txt.Text = vac.ReadInt64(Variable.ShOffset).ToString(CultureInfo.InvariantCulture);
                    break;

                case 'F':
                    _txt.Text = vac.ReadSingle(Variable.ShOffset).ToString(CultureInfo.InvariantCulture);
                    break;

                case 'D':
                    _txt.Text = vac.ReadDouble(Variable.ShOffset).ToString(CultureInfo.InvariantCulture);
                    break;

                case 'C':
                    var c = vac.ReadChar(Variable.ShOffset);
                    _txt.Text = $"{c} ({(int)c})";
                    break;
            }
        }

        protected override void PrepareEditor()
        {
            _editor.Visibility = Visibility.Visible;

            var ve = Variable;
            var vt = ve.VarType;
            var vac = VarEntry.Accessor;

            if (vt.StartsWith("I"))
                _editor.Text = vac.ReadInt32(ve.ShOffset).ToString(CultureInfo.InvariantCulture);

            if (vt.StartsWith("L"))
                _editor.Text = vac.ReadInt64(ve.ShOffset).ToString(CultureInfo.InvariantCulture);

            if (vt.StartsWith("F"))
                _editor.Text = vac.ReadSingle(ve.ShOffset).ToString(CultureInfo.InvariantCulture);

            if (vt.StartsWith("D"))
                _editor.Text = vac.ReadDouble(ve.ShOffset).ToString(CultureInfo.InvariantCulture);

            _editor.Focus();
            _editor.SelectAll();
        }

        protected override void AcceptValue()
        {
            _editor.Visibility = Visibility.Collapsed;

            var ve = Variable;

            try
            {
                var vt = ve.VarType;
                var vac = VarEntry.Accessor;

                if (vt.StartsWith("I"))
                    vac.Write(ve.ShOffset, Int32.Parse(_editor.Text, CultureInfo.InvariantCulture));

                if (vt.StartsWith("L"))
                    vac.Write(ve.ShOffset, Int64.Parse(_editor.Text, CultureInfo.InvariantCulture));

                if (vt.StartsWith("F"))
                    vac.Write(ve.ShOffset, Single.Parse(_editor.Text.Replace(",", "."), CultureInfo.InvariantCulture));

                if (vt.StartsWith("D"))
                    vac.Write(ve.ShOffset, Double.Parse(_editor.Text.Replace(",", "."), CultureInfo.InvariantCulture));

                SendAsChanged();
            }
            catch (FormatException) { }
            catch (OverflowException) { }
        }
    }
}
