using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using fmslapi;
using fmslapi.Bindings.WPF;

namespace InpuR
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static VariablesDataContext SetRootContext(UIElement Target)
        {
            var vdc = VariablesDataContext.GetRootContext(Target, "iwks");

            vdc.Manager = Manager.GetAPI("fms", new Guid("B9B9B67E-3571-4038-A1DA-73FC6FE99583"));
            vdc.VariablesChannelName = "VarControl";
            vdc.FormatString = "0.000";

            return vdc;
        }

    }
}
