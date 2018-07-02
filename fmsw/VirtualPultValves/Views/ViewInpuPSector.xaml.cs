using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для ViewInpuPSector.xaml
    /// </summary>
    public partial class ViewInpuPSector : UserControl
    {
        private ViewModel.ViewModel_NeptunP1 vmp1;
        private ViewModel.ViewModel_NeptunP2 vmp2;
        private ViewModel.ViewModel_Svyaz vms;
        public ViewInpuPSector()
        {
            InitializeComponent();
            vmp2 = croot.DataContext as ViewModel.ViewModel_NeptunP2;
            vms = sroot.DataContext as ViewModel.ViewModel_Svyaz;
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

        private void kts_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VM.CmdKontrTCTrue.Execute(0);


        }
        private void kts_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VM.CmdKontrTCFalse.Execute(0);

        }
       
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        kip.MouseLeftButtonDown += kip_MouseLeftButtonDown;
        kip.MouseLeftButtonUp += kip_MouseLeftButtonUp;
        kip.AddHandler(UIElement.MouseLeftButtonDownEvent,
      (MouseButtonEventHandler)kip_MouseLeftButtonDown, true);
        kip.AddHandler(UIElement.MouseLeftButtonUpEvent,
       (MouseButtonEventHandler)kip_MouseLeftButtonUp, true);


            kts.MouseLeftButtonDown += kts_MouseLeftButtonDown;
            kts.MouseLeftButtonUp += kts_MouseLeftButtonUp;
            kts.AddHandler(UIElement.MouseLeftButtonDownEvent,
         (MouseButtonEventHandler)kts_MouseLeftButtonDown, true);
            kts.AddHandler(UIElement.MouseLeftButtonUpEvent,
           (MouseButtonEventHandler)kts_MouseLeftButtonUp, true);
        }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        kip.MouseLeftButtonDown -= kip_MouseLeftButtonDown;
        kip.MouseLeftButtonUp -= kip_MouseLeftButtonUp;
            kts.MouseLeftButtonDown -= kts_MouseLeftButtonDown;
            kts.MouseLeftButtonUp -= kts_MouseLeftButtonUp;
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
            ///!не реализованно через вагу
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
            if (vmp2.Ki.ValueState) param = 1;

        }

        cmd.Execute(param);
        // MessageBox.Show(vl.Content.ToString() + "param=" + param.ToString());

    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        //MessageBox.Show("checked");

    }

        private void PultBigButton_SBrosASig(object sender, MouseButtonEventArgs e)
        {
            VM.CmdSbrosAvarSign_Down.Execute(0);
        }

       

        private void PultBigButton_PMUpSBrosASig(object sender, MouseButtonEventArgs e)
        {
            VM.CmdSbrosAvarSign_Up.Execute(0);
        }

        private void PultGlassButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            VM.CmdBdusUP.Execute(0);

        }

        private void PultGlassButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VM.CmdBdusDown.Execute(0);

        }

        private void kip1_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)kip1.IsChecked) vms.BSvyz2_dn.Execute(1);
            else
                vms.BSvyz2_up.Execute(1);
        }

        private void kip2_Click(object sender, RoutedEventArgs e)
        {

            if ((bool)kip2.IsChecked) vms.BSvyz3_dn.Execute(1);
            else
                vms.BSvyz3_up.Execute(1);

        }
    }
}

