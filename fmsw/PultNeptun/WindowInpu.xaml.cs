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
using ValueModel.BaseType;
using VirtualPultValves.Model;
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
        private static int NumInpu = 1;
        private VirtualPultValves.ViewModel.ViewModel_InPU vminpu;
      



        public WindowInpu()
        {
            InitializeComponent();

          
            _inwin1.InpuNum = NumInpu;
            InPUControl = _inwin1.InPUControl;
            WinPult.DataContext = _inwin1;
            vminpu.RMNum = 1; //переключение дежурного режима из ViewModel на InPu1
            vminpu = roo.DataContext as VirtualPultValves.ViewModel.ViewModel_InPU;
           
            WagoIO.Instance.SenderType = 8; //Инпу1
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left: InPUControl.PressNeptKey(NumInpu, 12); break;
                case Key.Right: InPUControl.PressNeptKey(NumInpu, 11); break;
                case Key.Up: InPUControl.PressNeptKey(NumInpu, 14); break;
                case Key.Down: InPUControl.PressNeptKey(NumInpu, 13); break;
                case Key.Enter: InPUControl.PressNeptKey(NumInpu, 17); break;
                case Key.Escape: InPUControl.PressNeptKey(NumInpu, 24); break;
            }

            e.Handled = true;
        }
        #region Commanda BtnClick

        //Команда для кнопок 

        public static RoutedCommand BtnCmd = new RoutedCommand();

        private void BtnCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var par = Int32.Parse(e.Parameter.ToString());
            InPUControl.PressNeptKey(NumInpu, par);
        }

        private void BtnCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion

        private void PultGlassButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button btn = (Button)sender;

            if (btn.CommandParameter.ToString() == "1")
                vminpu.CmdVKL_REZ_BATAR_DOWN.Execute(1);
            if (btn.CommandParameter.ToString() == "2")
                vminpu.CmdOTBOI_ZVUKA_DOWN.Execute(1);

            if (btn.CommandParameter.ToString() == "3")
            {
                if (vminpu.RMNum == 1) vminpu.CmdVKLInpu1.Execute(1);
                if (vminpu.RMNum == 2) vminpu.CmdVKLInpu2.Execute(1);
            }

            if (btn.CommandParameter.ToString() == "4")
            {
                // MessageBox.Show("->");
                if (vminpu.RMNum == 1)
                {
                    // MessageBox.Show("lll");
                    vminpu.CmdOTKLInpu1.Execute(1);
                }

                if (vminpu.RMNum == 2) vminpu.CmdOTKLInpu2.Execute(1);
            }
        }

        private void PultGlassButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

            Button btn = (Button)sender;
            if (btn.CommandParameter.ToString() == "1")
                vminpu.CmdVKL_REZ_BATAR_DOWN.Execute(0);
            if (btn.CommandParameter.ToString() == "2")
                vminpu.CmdOTBOI_ZVUKA_DOWN.Execute(0);

            if (btn.CommandParameter.ToString() == "3")
            {
                if (NumInpu == 1) vminpu.CmdVKLInpu1.Execute(0);
                if (NumInpu == 2) vminpu.CmdVKLInpu2.Execute(0);
            }
            if (btn.CommandParameter.ToString() == "4")
            {
                if (NumInpu == 1) vminpu.CmdOTKLInpu1.Execute(0);
                if (NumInpu == 2) vminpu.CmdOTKLInpu2.Execute(0);
            }

        }
    }
}

