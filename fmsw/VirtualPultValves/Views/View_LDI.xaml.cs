using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для View_LDI.xaml
    /// </summary>
    public partial class View_LDI : UserControl
    {
        private class Inv
        {
            private WeakReference _tgt;
            private MethodInfo _mi;

            public Inv(Delegate Target)
            {
                var il = Target.GetInvocationList();
                if (il.Length != 1)
                    return;

                _tgt = new WeakReference(il[0].Target);
                _mi = il[0].Method;
            }

            public void EventHandler(object sender, KeyEventArgs e)
            {
                var t = _tgt.Target;

                if (t == null)
                    return;

                _mi.Invoke(t, new[] { sender, e });
            }
        }


        DispatcherTimer timer;
        DispatcherTimer timerSS;

        private bool FirstZamer = false;

        int sec = 0;
        int Ssec = 0;

        private ViewModel.ViewModel_LDI VM_Bvk;

        public static readonly RoutedEvent MeasureEvent = EventManager.RegisterRoutedEvent("Measure", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(View_LDI));
        public static readonly RoutedEvent EndMeasureEvent = EventManager.RegisterRoutedEvent("EndMeasure", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(View_LDI));

        public new event RoutedEventHandler Measure
        {
            add { AddHandler(MeasureEvent, value); }
            remove { RemoveHandler(MeasureEvent, value); }
        }

        public event RoutedEventHandler EndMeasure
        {
            add { AddHandler(EndMeasureEvent, value); }
            remove { RemoveHandler(EndMeasureEvent, value); }
        }

        public View_LDI()
        {
            
            InitializeComponent();

            var i = new Inv(new KeyEventHandler(UserControl_KeyDown));
            Application.Current.MainWindow.PreviewKeyDown += i.EventHandler;
           
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1.0);
            timer.Tick += (s, e) => sec++;


            timerSS = new DispatcherTimer();
            timerSS.Interval = TimeSpan.FromSeconds(1.0);
            VM_Bvk = this.ds.DataContext as ViewModel.ViewModel_LDI;
            this.ds.DataContext = null;
            timerSS.Tick += new EventHandler(delegate(object s, EventArgs a)
            {


                Ssec++;
                ShowRast(Ssec);
                


            });
           
        }
        private void ShowRast(int val)
        {
           // var dc = this.ds.DataContext as ViewModel.ViewModel_LDI;

            VM_Bvk.SRasShow(val);


           

        }
        private void Button_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

          /*  FirstZamer = !FirstZamer;
            if (FirstZamer)
                timer.Start();
            else
            {
                timer.Stop();
                //var dc = this.ds.DataContext as ViewModel.ViewModel_LDI;
                VM_Bvk.TimerStop.Execute(sec);
                
                sec = 0;
            }
            
			
        	// TODO: Add event handler implementation here.
			*/
        }

        private void Button_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	// TODO: Add event handler implementation here.

          /*  FirstZamer = !FirstZamer;
            if (FirstZamer)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
               // var dc = this.ds.DataContext as ViewModel.ViewModel_LDI;
                VM_Bvk.TimerStop.Execute(sec);
                sec = 0;
            }*/
            
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	 FirstZamer = !FirstZamer;
            if (FirstZamer)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
               // var dc = this.ds.DataContext as ViewModel.ViewModel_LDI;
                VM_Bvk.TimerStop.Execute(sec);
                sec = 0;


              
            }
        }

       

        private void Button_ClickISX(object sender, System.Windows.RoutedEventArgs e)
        {
			FirstZamer=false;
            this.Tochki_Copy.Content = 1;
            timer.Stop();
            sec = 0; 
            VM_Bvk = new ViewModel.ViewModel_LDI();
          
            
            if ((bool)checkBox1.IsChecked)
            ds.DataContext = VM_Bvk;
           
            timerSS.Stop();
            Ssec = 0;
            
            // TODO: Add event handler implementation here.
        }

        private void mlb(object Sender, MouseButtonEventArgs E)
        {
            if ((bool)checkBox1.IsChecked)
            RaiseEvent(new RoutedEventArgs(MeasureEvent));            
        }

        private void mlbu(object Sender, MouseButtonEventArgs E)
        {
            if ((bool)checkBox1.IsChecked)
            RaiseEvent(new RoutedEventArgs(EndMeasureEvent));
        }
        private void checkBox1_Click(object sender, RoutedEventArgs e)
        {
            timerSS.Stop();
            Ssec = 0;
            timer.Stop();
            sec = 0;

            if ((bool)checkBox1.IsChecked)
            {
                VM_Bvk = new ViewModel.ViewModel_LDI();
                this.ds.DataContext = VM_Bvk;
                //var dc = this.ds.DataContext as ViewModel.ViewModel_LDI;
                VM_Bvk.CmdISX.Execute(0);
            }
            else

                this.ds.DataContext = null;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!FirstZamer)
                timerSS.Start();

        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsVisible)
                return;

            switch (e.Key)
            {
                case Key.NumPad0: VM_Bvk.CmdCif.Execute(0); break;
                case Key.NumPad1: VM_Bvk.CmdCif.Execute(1); break;
                case Key.NumPad2: VM_Bvk.CmdCif.Execute(2); break;
                case Key.NumPad3: VM_Bvk.CmdCif.Execute(3); break;
                case Key.NumPad4: VM_Bvk.CmdCif.Execute(4); break;
                case Key.NumPad5: VM_Bvk.CmdCif.Execute(5); break;
                case Key.NumPad6: VM_Bvk.CmdCif.Execute(6); break;
                case Key.NumPad7: VM_Bvk.CmdCif.Execute(7); break;
                case Key.NumPad8: VM_Bvk.CmdCif.Execute(8); break;
                case Key.NumPad9: VM_Bvk.CmdCif.Execute(9); break;
                case Key.D0: VM_Bvk.CmdCif.Execute(0); break;
                case Key.D1: VM_Bvk.CmdCif.Execute(1); break;
                case Key.D2: VM_Bvk.CmdCif.Execute(2); break;
                case Key.D3: VM_Bvk.CmdCif.Execute(3); break;
                case Key.D4: VM_Bvk.CmdCif.Execute(4); break;
                case Key.D5: VM_Bvk.CmdCif.Execute(5); break;
                case Key.D6: VM_Bvk.CmdCif.Execute(6); break;
                case Key.D7: VM_Bvk.CmdCif.Execute(7); break;
                case Key.D8: VM_Bvk.CmdCif.Execute(8); break;
                case Key.D9: VM_Bvk.CmdCif.Execute(9); break;
                case Key.Escape: { VM_Bvk.CmdISX.Execute(null); Button_ClickISX(this, new RoutedEventArgs()); } break;
                case Key.Back: VM_Bvk.CmdOBN.Execute(null); break;
                case Key.Enter: { VM_Bvk.CmdEnter.Execute(null); Button_Click_1(this, new RoutedEventArgs()); } break;


                    /* case Key.Right: InPUControl.PressNeptKey(NumInpu, 11); break;
                     case Key.Up: InPUControl.PressNeptKey(NumInpu, 14); break;
                     case Key.Down: InPUControl.PressNeptKey(NumInpu, 13); break;
                     case Key.Enter: InPUControl.PressNeptKey(NumInpu, 17); break;
                     case Key.Escape: InPUControl.PressNeptKey(NumInpu, 24); break;*/
            }

            e.Handled = true;
        }
    }
}
