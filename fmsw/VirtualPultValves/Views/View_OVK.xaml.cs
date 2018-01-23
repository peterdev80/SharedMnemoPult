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
using VirtualPultValves.Model;

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для View_OVK.xaml
    /// </summary>
    public partial class View_OVK : UserControl
    {
        private ViewModel.ViewModel_OVK vmovk;
        public View_OVK()
        {
            InitializeComponent();
            vmovk = groot.DataContext as ViewModel.ViewModel_OVK;
            
        }
        public static readonly DependencyProperty TextSizeProperty = DependencyProperty.Register(
        "TextSize", typeof(double), typeof(View_OVK), new FrameworkPropertyMetadata(10d,
    FrameworkPropertyMetadataOptions.AffectsRender));
        public double TextSize
        {
            get { return (double)GetValue(TextSizeProperty); }
            set { this.SetValue(TextSizeProperty, value); }
        }

        private void OVK_Label_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ActualHeight > 60) OVK_Label.Visibility = Visibility.Hidden;
            else
                OVK_Label.Visibility = Visibility.Visible;
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button btn = (Button)sender;
            vmovk.OVKDownKey.Execute(btn.CommandParameter);
            Debug.WriteLine(btn.CommandParameter);

        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Button btn = (Button)sender;
            vmovk.OVKUpKey.Execute(btn.CommandParameter);
            Debug.WriteLine("UP");

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
           // LinkInpu.Instance.SetSendVar(true, int.Parse(param.ToString()), 1);
          //  MessageBox.Show("Click");
        }
        
      
      
    }
}
