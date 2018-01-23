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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using AVIAKOM;

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для InPUWin32View.xaml
    /// </summary>
    [DesignTimeVisible(false)]
    public partial class InPUWin32View : UserControl
    {
        public InpuPresenter InPUControl;

        private int _InpuNum;
        

        public int InpuNum
        {
            set
            {
                _InpuNum = value;
                InPUControl = new InpuPresenter(600, 800, _InpuNum, FindResource("loading") as UIElement, new InpuFailed());
                ControlHostElement.Child = InPUControl;
#if DEBUG
                Debug.WriteLine("InPU: Create Control Host " + _InpuNum.ToString());
#endif
              
            }
            get
            {
                return _InpuNum;
            }
        }

        public InPUWin32View()
        {
            InitializeComponent();
        }
    }
}
