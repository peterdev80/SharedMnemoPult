using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;

namespace fmsman.Formats
{
    /// <summary>
    /// Визуализация логических переменных
    /// </summary>
    [DesignTimeVisible(false)]
    public class BoolVarVisual : BaseVarVisual
    {
        #region Конструкторы
        static BoolVarVisual()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BoolVarVisual) ,new FrameworkPropertyMetadata(typeof(BoolVarVisual)));
        }
        #endregion

        #region Частные данные
        private TextBlock _txt;

        private UIElement _settrue;
        private UIElement _setfalse;
        private UIElement _vtrue;
        private UIElement _vfalse;
        #endregion

        #region Перегрузки
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _txt = GetTemplateChild("PART_txt") as TextBlock;

            _settrue = GetTemplateChild("PART_true") as UIElement;
            _setfalse = GetTemplateChild("PART_false") as UIElement;
            _vtrue = GetTemplateChild("PART_vtrue") as UIElement;
            _vfalse = GetTemplateChild("PART_vfalse") as UIElement;

            Debug.Assert(_settrue != null, "_settrue != null");
            Debug.Assert(_setfalse != null, "_setfalse != null");

            _settrue.MouseDown += _settrue_MouseDown;
            _setfalse.MouseDown += _setfalse_MouseDown;

            Reformat();
        }

        protected override void Reformat()
        {
            if (_txt == null)
                return;

            var bv = VarEntry.Accessor.ReadBoolean(Variable.ShOffset);

            _vtrue.Visibility = bv ? Visibility.Visible : Visibility.Collapsed;
            _vfalse.Visibility = !bv ? Visibility.Visible : Visibility.Collapsed;

            _txt.Text = bv.ToString();
        }
        #endregion

        #region Обработка событий
        void _setfalse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                VarEntry.Accessor.Write(Variable.ShOffset, false);
                SendAsChanged();
            }
        }

        void _settrue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                VarEntry.Accessor.Write(Variable.ShOffset, true);
                SendAsChanged();
            }
        }
        #endregion
    }
}
