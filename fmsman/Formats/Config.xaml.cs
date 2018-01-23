using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.IO;

namespace fmsman.Formats
{
    [DesignTimeVisible(false)]
    public partial class Config
    {
        private readonly Connection _con;

        public Config(Connection con)
        {
            _con = con;

            _con.OnConfig += OnConfig;

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            ReloadConfig(null, null);
        }

        public void Detach()
        {
            Dispatcher.Invoke(() =>
                              {
                                  Loaded -= OnLoaded;
                                  _con.OnConfig -= OnConfig;
                              });
        }

        private void OnConfig(IList<string> files, string cfg)
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateConfig(files, cfg)));
        }

        private void UpdateConfig(IList<string> files, string cfg)
        {
            var wr = new StringWriter();

            foreach (var file in files)
                wr.WriteLine($"## {file}");

            var rd = new StringReader(cfg);

            string l;

            while((l = rd.ReadLine()) != null)
            {
                if (l.StartsWith("#$"))
                    break;

                if (l.StartsWith("##"))
                {
                    wr.WriteLine();

                    wr.WriteLine($"[{l.Replace("##", "")}]");

                    continue;
                }

                string ll;

                while ((ll = rd.ReadLine()) != "#!")
                    wr.WriteLine($"{l} = {ll}");
            }

            tb.Text = wr.ToString();
        }

        private void ReloadConfig(object sender, RoutedEventArgs e)
        {
            _con.RetreiveConfig();
        }
    }
}
