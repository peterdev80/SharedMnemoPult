using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using System.Management;

namespace FMS
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            var ay = new Func<string, bool>(v => e.Args.Any(x => x.Trim().ToLower() == v));

            var i1 = ay("/inpu1");
            var i2 = ay("/inpu2");

            var scr = 0;
            if (ay("/1")) scr = 1;
            if (ay("/2")) scr = 2;

            if (i2 && scr == 0)
                scr = 2;

            Window mw;

            if (i1 || i2)
            {
                var inpunum = i1 ? 1 : 2;

                mw = new InPU { InpuNum = inpunum, ScreenNo = scr };
                mw.Show();
            }
        }
    }
}
