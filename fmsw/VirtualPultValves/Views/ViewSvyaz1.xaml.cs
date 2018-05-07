using System;
using System.Collections.Generic;
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
using VirtualPultValves.ViewModel;

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Логика взаимодействия для ViewSvyaz1.xaml
    /// </summary>
    public partial class ViewSvyaz1 : UserControl
    {
        ViewModel_Svyaz wms;
        public ViewSvyaz1()
        {
            InitializeComponent();
            wms = root.DataContext as ViewModel_Svyaz;
        }

        private void kip1_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)kip1.IsChecked) wms.BSvyz1_dn.Execute(true);
            else
                wms.BSvyz1_up.Execute(false);
        }
    }
}
