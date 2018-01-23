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

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для View_NeptunP2.xaml
    /// </summary>
    public partial class View_NeptunP2 : UserControl
    {
        private ViewModel.ViewModel_NeptunP1 vmp1;
        private ViewModel.ViewModel_NeptunP2 vmp2;


      
        public View_NeptunP2()
        {
            InitializeComponent();
            vmp2 = croot.DataContext as ViewModel.ViewModel_NeptunP2;
        }

        public ViewModel.ViewModel_NeptunP1 VM
        {
            get
            {
                if (vmp1 == null)
                    try
                    {
                        vmp1 = this.FindResource("vP1") as ViewModel.ViewModel_NeptunP1;
                    }
                    catch (ResourceReferenceKeyNotFoundException e)
                    {
                        Debug.WriteLine(e.Message);
                        vmp1 = new ViewModel.ViewModel_NeptunP1();
                    }

                return vmp1;
            }

        }




        private void kip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VM.CmdKontVPTrue.Execute(0);



        }
        private void kip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VM.CmdKontVPFalse.Execute(0);

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            kip.MouseLeftButtonDown += kip_MouseLeftButtonDown;
            kip.MouseLeftButtonUp += kip_MouseLeftButtonUp;
            kip.AddHandler(UIElement.MouseLeftButtonDownEvent,
          (MouseButtonEventHandler)kip_MouseLeftButtonDown, true);
            kip.AddHandler(UIElement.MouseLeftButtonUpEvent,
           (MouseButtonEventHandler)kip_MouseLeftButtonUp, true);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            kip.MouseLeftButtonDown -= kip_MouseLeftButtonDown;
            kip.MouseLeftButtonUp -= kip_MouseLeftButtonUp;
        }

        private void PultLampButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PultLampButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            vmp2.CmdPitVkl_down.Execute(0);
            
        }

        private void PultLampButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            vmp2.CmdPitVkl_up.Execute(0);
        }

        private void PultBigButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            vmp2.CmdPitOtkl_up.Execute(0);
        }

        private void PultBigButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            vmp2.CmdPitOtkl_down.Execute(0);
        }

        private void PultBigButton_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            vmp2.CmdKonserv_down.Execute(0);
        }

        private void PultBigButton_PreviewMouseUp_1(object sender, MouseButtonEventArgs e)
        {
            vmp2.CmdKonserv_Up.Execute(0);
        }
        private void CheckBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
           // vmp2.CmdBi.Execute(0);

        }

        private void CheckBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
           

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox vl = sender as CheckBox;
            int param = 0;
            ICommand cmd;
            cmd = vmp2.CPusto;
            if (vl.Content.ToString() == "Bi")
            {
                if (vmp2.Bi.ValueState) param = 1;
                cmd = vmp2.CmdBi;
               
            }
            // if ((bool)vl.IsChecked) param = 0;
            
         
          
           
            if (vl.Content.ToString() == "KK")
            {
                cmd = vmp2.CmdKK;
                if (vmp2.Kk.ValueState) param = 1;
                
               
            }
            if (vl.Content.ToString() == "Ki")
            {
                cmd = vmp2.CmdKi;
                if (vmp2.Ki.ValueState) param =1;
               
            }
           
            cmd.Execute(param);
           // MessageBox.Show(vl.Content.ToString() + "param=" + param.ToString());

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("checked");

        }
    }
}
