using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;

namespace fmsman.Formats
{
    /// <summary>
    /// Визуализация символьной переменной
    /// </summary>
    [DesignTimeVisible(false)]
    public class CharVarVisual : EditableVarVisual
    {
        #region Конструкторы

        static CharVarVisual()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CharVarVisual),
                new FrameworkPropertyMetadata(typeof(CharVarVisual)));
        }

        #endregion

        #region Частные данные

        private TextBlock _txt;
        private TextBlock _preview;
        private TextBlock _submenu;
        private ContextMenu _menu;
        private Encoding _cp = Encoding.Unicode;

        #endregion

        #region Перегрузки

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _txt = GetTemplateChild("PART_txt") as TextBlock;
            _preview = GetTemplateChild("PART_preview") as TextBlock;
            _submenu = GetTemplateChild("PART_submenu") as TextBlock;

            Debug.Assert(_submenu != null, "_submenu != null");

            _submenu.MouseDown += _submenu_MouseDown;

            _menu = _submenu.FindResource("cpmenu") as ContextMenu;

            Debug.Assert(_menu != null, "_menu != null");

            // ReSharper disable once PossibleNullReferenceException
            (_menu.Items[0] as MenuItem).Click += Unicode;
            // ReSharper disable once PossibleNullReferenceException
            (_menu.Items[1] as MenuItem).Click += cp1251;

            Reformat();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            _preview.Visibility = Visibility.Collapsed;
        }

        protected override void PrepareEditor()
        {
            _editor.Visibility = Visibility.Visible;

            var ve = Variable;

            var c = VarEntry.Accessor.ReadUInt16(ve.ShOffset);
            _editor.Text = $"{c}";

            _editor.Focus();
            _editor.SelectAll();

            _preview.Visibility = Visibility.Visible;
            TextChanged(_editor.Text);
        }

        protected override void AcceptValue()
        {
            _preview.Visibility = Visibility.Collapsed;

            _preview.Text = "";
            _editor.Visibility = Visibility.Collapsed;
            var ve = Variable;

            var v = _editor.Text;

            if (v.Length == 1)
            {
                var b = _cp.GetBytes(new[] { v[0] });
                VarEntry.Accessor.Write(ve.ShOffset, b[0]);
                VarEntry.Accessor.Write(ve.ShOffset + 1, b.Length > 1 ? b[1] : (byte)0);
            }
            else
            {
                if (UInt16.TryParse(_editor.Text, out var code))
                    VarEntry.Accessor.Write(ve.ShOffset, code);
            }

            SendAsChanged();
        }

        protected override void Reformat()
        {
            if (_txt == null)
                return;

            var code = VarEntry.Accessor.ReadUInt16(Variable.ShOffset);
            var b = BitConverter.GetBytes(code);

            _txt.Text = b[1] > 0 ? $"{_cp.GetChars(b)[0]} ({code}) (%{b[0]})" : $"{_cp.GetChars(b)[0]} ({code})";
        }

        protected override void TextChanged(string Text)
        {
            if (_preview == null)
                return;

            if (Text.Length == 1)
                _preview.Text = Text;
            else
            {
                UInt16.TryParse(Text, out var res);
                var b = BitConverter.GetBytes(res);

                _preview.Text = _cp.GetChars(b)[0].ToString(CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region Обработка событий

        private void Unicode(object sender, RoutedEventArgs e)
        {
            _cp = Encoding.Unicode;
            _submenu.Text = "U";
            Reformat();

            TextChanged(_editor.Text);
        }

        private void cp1251(object sender, RoutedEventArgs e)
        {
            _cp = Encoding.GetEncoding(1251);
            _submenu.Text = "W";
            Reformat();

            TextChanged(_editor.Text);
        }

        private void _submenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                e.Handled = true;
                Dispatcher.BeginInvoke(new Action(() => { _menu.IsOpen = true; }), null);
            }
        }

        #endregion
    }
}