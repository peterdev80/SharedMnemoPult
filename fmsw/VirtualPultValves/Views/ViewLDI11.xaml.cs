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
    /// Логика взаимодействия для ViewLDI11.xaml
    /// </summary>
    public partial class ViewLDI11 : UserControl
    {

        #region Event
      
        //Измерить
        
        public static readonly RoutedEvent MeasureEvent = EventManager.RegisterRoutedEvent("Measure", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ViewLDI11));
        public static readonly RoutedEvent EndMeasureEvent = EventManager.RegisterRoutedEvent("EndMeasure", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ViewLDI11));
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

        //Пуск

        public static readonly RoutedEvent StartingEvent = EventManager.RegisterRoutedEvent("Starting", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ViewLDI11));
        public static readonly RoutedEvent EndStartingEvent = EventManager.RegisterRoutedEvent("EndStarting", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ViewLDI11));
        public event RoutedEventHandler Starting
        {
            add { AddHandler(StartingEvent, value); }
            remove { RemoveHandler(StartingEvent, value); }
        }

        public event RoutedEventHandler EndStarting
        {
            add { AddHandler(EndStartingEvent, value); }
            remove { RemoveHandler(EndStartingEvent, value); }
        }

        //Вкл/Выкл 
        public static readonly RoutedEvent SwitchonEvent = EventManager.RegisterRoutedEvent("Switchon", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ViewLDI11));
        public event RoutedEventHandler Switchon
        {
            add { AddHandler(SwitchonEvent, value); }
            remove { RemoveHandler(SwitchonEvent, value); }
        }
        public static readonly RoutedEvent SwitchoffEvent = EventManager.RegisterRoutedEvent("Switchoff", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ViewLDI11));
        public event RoutedEventHandler Switchoff
        {
            add { AddHandler(SwitchoffEvent, value); }
            remove { RemoveHandler(SwitchoffEvent, value); }
        }


        #endregion

        public ViewLDI11()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mlb(object Sender, MouseButtonEventArgs E)
        {
            RaiseEvent(new RoutedEventArgs(MeasureEvent));
        }

        private void mlbu(object Sender, MouseButtonEventArgs E)
        {
            RaiseEvent(new RoutedEventArgs(EndMeasureEvent));
        }
        private void slb(object Sender, MouseButtonEventArgs E)
        {
            RaiseEvent(new RoutedEventArgs(StartingEvent));
        }

        private void slbu(object Sender, MouseButtonEventArgs E)
        {
            RaiseEvent(new RoutedEventArgs(EndStartingEvent));
        }

        private void TurnOn(object Sender, MouseButtonEventArgs E)
        {
            RaiseEvent(new RoutedEventArgs(SwitchonEvent));
        }
        private void TurnOff(object Sender, MouseButtonEventArgs E)
        {
            RaiseEvent(new RoutedEventArgs(SwitchoffEvent));
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void cb_Click(object sender, RoutedEventArgs e)
        {
            if (cb.IsChecked==true) RaiseEvent(new RoutedEventArgs(SwitchonEvent)); else
                RaiseEvent(new RoutedEventArgs(SwitchoffEvent));

           // if (cb.IsChecked == true) Debug.WriteLine("VKL"); else Debug.WriteLine("---");
        }

    }
}
