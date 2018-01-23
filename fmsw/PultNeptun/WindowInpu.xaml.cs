using AVIAKOM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VirtualPultValves.Views;

namespace PultNeptun
{
    /// <summary>
    /// Логика взаимодействия для WindowInpu.xaml
    /// </summary>
    public partial class WindowInpu : UserControl
    {
        private InPUWin32View _inwin1 = new InPUWin32View();


        public InpuPresenter InPUControl;

       


        public WindowInpu()
        {
            InitializeComponent();
         
           
            _inwin1.InpuNum = 1;
            WinPult.DataContext = _inwin1;
        }
        private void PultGlassButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void PultGlassButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {



        }
    }
}
