using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
//using AVIAKOM;
using f = System.Windows.Forms;

namespace FMS
{
    /// <summary>
    /// Автономное окно ИнПУ
    /// </summary>
    [DesignTimeVisible(false)]
    public partial class InPU : Window
    {
        private int _inum;

        public InPU()
        {
            InitializeComponent();
            Loaded += (s, e) =>
                {
                    WindowState = WindowState.Maximized;
                    Show();
                    Focus();
                };
        }

        public int InpuNum
        {
            set
            {
                _inum = value;
                g.Child = new AVIAKOM.InpuPresenter(800, 600, value, null, null) { HideCursor = true };

                Title = string.Format("InPU-{0}", value);
            }
        }

        public int ScreenNo
        {
            set
            {
                // Разворачиваем ИнПУ на весь экран на соответствующем дисплее (если он есть)
                var display = value - 1;
                if (f.SystemInformation.MonitorCount < value)
                    display = 0;

                if (display < 0)
                    display = 0;

                WindowStartupLocation = WindowStartupLocation.Manual;

                var workingArea = f.Screen.AllScreens[display].WorkingArea;
                Left = workingArea.Left;
                Top = workingArea.Top;
                Width = workingArea.Width;
                Height = workingArea.Height;
                WindowStyle = WindowStyle.None;
                Topmost = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            var h = g.Child as AVIAKOM.InpuPresenter;

            if (h == null)
                return;

            switch (e.Key)
            {
                case Key.Escape:
                    g.Child = null;
                    UpdateLayout();
                    Close();
                    return;

                case Key.Left: h.PressNeptKey(_inum, 12); break;
                case Key.Right: h.PressNeptKey(_inum, 11); break;
                case Key.Up: h.PressNeptKey(_inum, 14); break;
                case Key.Down: h.PressNeptKey(_inum, 13); break;

                case Key.Enter: h.PressNeptKey(_inum, 17); break;

                default:
                    break;
            }
        }
    }
}
