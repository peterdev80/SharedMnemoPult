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
using VirtualPultValves.Model;

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для View_TC.xaml
    /// </summary>
    public partial class View_TC : UserControl
    {
        private ViewModel.ViewModelTC vmtc;
      
        public View_TC()
        {
            InitializeComponent();
            vmtc = new ViewModel.ViewModelTC();
           // WagoIO.Instance.TC = vmtc;
            this.DataContext = vmtc;
        }

        public static DependencyProperty RMTypeProperty =
            DependencyProperty.Register("RMType", typeof(int), typeof(View_TC), new FrameworkPropertyMetadata(0,FrameworkPropertyMetadataOptions.AffectsRender, OnChangeRM));

        private static void OnChangeRM(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as View_TC;
            if (e.NewValue.ToString() == "1")  obj.vmtc.Stage = 1;
            if (e.NewValue.ToString() == "2") obj.vmtc.Stage = 2;           

        }
        public int RMType
        {
            get { return (int)GetValue(RMTypeProperty); }
            set { SetValue(RMTypeProperty,value); }
        }
    }
}
