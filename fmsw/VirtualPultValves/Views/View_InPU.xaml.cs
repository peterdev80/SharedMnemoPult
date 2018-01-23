using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using AVIAKOM;
using VirtualPultValves.PultControl;

namespace VirtualPultValves.Views
{
    /// <summary>
    ///     Логика взаимодействия для View_InPU.xaml
    /// </summary>
    public partial class View_InPU : UserControl
    {
        private InpuPresenter InPUControl;
        private InPUWin32View com;
        private int NumInpu;
        private ViewModel.ViewModel_InPU vminpu;

        public View_InPU()
        {
            InitializeComponent();
            vminpu = roo.DataContext as ViewModel.ViewModel_InPU;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            com = WinPult.Content as InPUWin32View;
            if (com != null)
            {
                InPUControl = com.InPUControl;
                NumInpu = com.InpuNum;
            }

            Debug.Assert(InPUControl != null, "НЕ создан win32 Control");

            var vm = FindResource("vInPU") as ViewModel.ViewModel_InPU;

            Debug.Assert(vm != null, "vm != null");

            if (vm.RMNum == 0) vm.RMNum = NumInpu;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:   InPUControl.PressNeptKey(NumInpu, 12); break;
                case Key.Right:  InPUControl.PressNeptKey(NumInpu, 11); break;
                case Key.Up:     InPUControl.PressNeptKey(NumInpu, 14); break;
                case Key.Down:   InPUControl.PressNeptKey(NumInpu, 13); break;
                case Key.Enter:  InPUControl.PressNeptKey(NumInpu, 17); break;
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

            if (btn.CommandParameter.ToString()=="1")
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