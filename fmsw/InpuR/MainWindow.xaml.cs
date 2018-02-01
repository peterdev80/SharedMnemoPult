using System;
using System.Collections.Generic;
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
using fmslapi.Bindings.WPF;

namespace InpuR
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var rc = App.SetRootContext(this);

            var cc = VariablesDataContext.GetNamedContext("Klapany");
            cc.Manager = rc.Manager;
            cc.VariablesChannelName = "VarNeptun";

            InitializeComponent();
        }
    }
}
