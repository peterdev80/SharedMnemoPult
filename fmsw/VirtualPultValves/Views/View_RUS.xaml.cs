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

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для View_RUS.xaml
    /// </summary>
    public partial class View_RUS : UserControl
    {
        public static bool VKLPit = false;

        Brush brUp;
        Brush brDwn;
        public View_RUS()
        {
            InitializeComponent();
            brUp = btnPit.Background;
            brDwn = btnPit.Foreground;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            View_RUS.VKLPit = !View_RUS.VKLPit;
            if (VKLPit)
                btnPit.Background = brUp;
            else
                btnPit.Background = brDwn;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (VKLPit) btnPit.Background = brUp;
            else
                btnPit.Background = brDwn;
        }
    }
}

