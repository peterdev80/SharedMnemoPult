using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;

namespace fmsman.Formats
{
    /// <summary>
    /// Визуализация командной переменной
    /// </summary>
    [DesignTimeVisible(false)]
    public class CMDVarVisual : BaseVarVisual
    {
        #region Частные данные
        private UIElement _exec;
        #endregion

        #region Конструкторы
        static CMDVarVisual()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CMDVarVisual), new FrameworkPropertyMetadata(typeof(CMDVarVisual)));
        }
        #endregion

        #region Перегрузки
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _exec = GetTemplateChild("PART_exec") as UIElement;

            Debug.Assert(_exec != null, "_exec != null");

            _exec.MouseDown += _exec_MouseDown;
        }

        protected override void Reformat()
        {
        }
        #endregion

        #region Обработка событий
        void _exec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                SendAsChanged();
            }
        }
        #endregion
    }
}
