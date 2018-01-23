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
    /// Логика взаимодействия для View_NeptunP1.xaml
    /// </summary>
    public partial class View_NeptunP1 : UserControl
    {
        private ViewModel.ViewModel_NeptunP1 vmp1;

        public View_NeptunP1()
        {
            InitializeComponent();

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

        #region EventDelegateMouseFunction

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
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            kip.MouseLeftButtonDown -= kip_MouseLeftButtonDown;
            kip.MouseLeftButtonUp -= kip_MouseLeftButtonUp;
            kts.MouseLeftButtonDown -= kts_MouseLeftButtonDown;
            kts.MouseLeftButtonUp -= kts_MouseLeftButtonUp;

        }
        private bool _PultVisual = true;
        public bool PultVisual
        {
            set
            {
                _PultVisual = value;
                if (!_PultVisual)
                {
                    grPK.Visibility = Visibility.Hidden;
                    brdTen.Visibility = Visibility.Hidden;
                }
            }
        }

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {

            kip.MouseLeftButtonDown += kip_MouseLeftButtonDown;
            kip.MouseLeftButtonUp += kip_MouseLeftButtonUp;
            kts.MouseLeftButtonDown += kts_MouseLeftButtonDown;
            kts.MouseLeftButtonUp += kts_MouseLeftButtonUp;

            kip.AddHandler(UIElement.MouseLeftButtonDownEvent,
            (MouseButtonEventHandler)kip_MouseLeftButtonDown, true);
            kip.AddHandler(UIElement.MouseLeftButtonUpEvent,
           (MouseButtonEventHandler)kip_MouseLeftButtonUp, true);

            kts.AddHandler(UIElement.MouseLeftButtonDownEvent,
          (MouseButtonEventHandler)kts_MouseLeftButtonDown, true);
            kts.AddHandler(UIElement.MouseLeftButtonUpEvent,
           (MouseButtonEventHandler)kts_MouseLeftButtonUp, true);

        }

        private void PultBigButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VM.CmdSbrosAvarSign_Down.Execute(0);
        }

        private void PultBigButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
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



    }
}

