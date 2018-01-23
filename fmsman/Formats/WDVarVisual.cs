using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace fmsman.Formats
{
    /// <summary>
    /// Визуализация сторожевых переменных
    /// </summary>
    [DesignTimeVisible(false)]
    public class WDVarVisual : BaseVarVisual
    {
        #region Конструкторы
        static WDVarVisual()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WDVarVisual), new FrameworkPropertyMetadata(typeof(WDVarVisual)));
        }
        #endregion

        #region Частные данные
        private TextBlock _txt;
        private UIElement _rst;
        private DispatcherTimer _dt;
        private UIElement _vt, _vf;
        #endregion

        #region Перегрузки
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _txt = GetTemplateChild("PART_txt") as TextBlock;
            _rst = GetTemplateChild("PART_rst") as UIElement;

            _vt = GetTemplateChild("PART_vtrue") as UIElement;
            _vf = GetTemplateChild("PART_vfalse") as UIElement;

            Debug.Assert(_rst != null, "_rst != null");

            _rst.MouseDown += _rst_MouseDown;

            Loaded += WDVarVisual_Loaded;
            Unloaded += WDVarVisual_Unloaded;

            Reformat();
        }

        protected override void Reformat()
        {
            if (_txt == null)
                return;

            var bv = VarEntry.Accessor.ReadUInt16(Variable.ShOffset);

            if (bv == 0)
            {
                _txt.Text = "False";
                _vt.Visibility = Visibility.Collapsed;
                _vf.Visibility = Visibility.Visible;
            }
            else
            {
                _txt.Text = $"{bv != 0} ({bv})";
                _vf.Visibility = Visibility.Collapsed;
                _vt.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Обработка событий
        void _rst_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                SendAsChanged();
            }
        }

        void WDVarVisual_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_dt == null)
                return;

            _dt.IsEnabled = false;
            _dt = null;
        }

        void WDVarVisual_Loaded(object sender, RoutedEventArgs e)
        {
            _dt = new DispatcherTimer();
            _dt.Interval = TimeSpan.FromMilliseconds(200);
            _dt.Tick += _dt_Tick;
            _dt.IsEnabled = true;
        }

        void _dt_Tick(object sender, EventArgs e)
        {
            Reformat();
        }
        #endregion
    }
}
