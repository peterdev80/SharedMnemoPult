using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace fmsman.Formats
{
    /// <summary>
    /// Логика взаимодействия для ChannelNameVisualizer.xaml
    /// </summary>
    public partial class ChannelNameVisualizer
    {
        public ChannelNameVisualizer()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ChannelNameProperty = DependencyProperty.Register("ChannelName", typeof(string), typeof(ChannelNameVisualizer));
        public static readonly DependencyProperty IsVarChanProperty = DependencyProperty.Register("IsVarChan", typeof(bool), typeof(ChannelNameVisualizer));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", typeof(string), typeof(ChannelNameVisualizer));

        private static readonly Dictionary<PipeEntry, CloseableTabItem> _opentabs = new Dictionary<PipeEntry, CloseableTabItem>();

        public string ChannelName
        {
            get => (string)GetValue(ChannelNameProperty);
            set => SetValue(ChannelNameProperty, value);
        }

        public string EndPoint
        {
            get => (string)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        public bool IsVarChan
        {
            get => (bool)GetValue(IsVarChanProperty);
            set => SetValue(IsVarChanProperty, value);
        }

        private void iam_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tabs = Application.Current.Properties["Tabs"] as TabControl;

            var d = DataContext as PipeEntry;
            if (d == null)
                return;

            if (!d.IsVarChan)
                return;

            _opentabs.TryGetValue(d, out var ct);
            if (ct != null)
            {
                ct.IsSelected = true;
                return;
            }

            var vm = new VariablesMap(d);

            var t = new CloseableTabItem();
            _opentabs.Add(d, t);


            var ep = EndPoint;
            if (ep.Length > 16)
                ep = ep.Substring(0, 12) + "..." + ep.Last();

            t.Header = $"[{ep}]/{ChannelName}";
            t.Content = vm;

            Debug.Assert(tabs != null, "tabs != null");

            tabs.Items.Add(t);

            t.IsSelected = true;

            d.Delete += (s3, e3) =>
                {
                    if (t.Pinned)
                        return;

                    vm.Close();

                    tabs.Items.Remove(t);
                    _opentabs.Remove(d);
                };

            t.CloseTab += (s2, e2) =>
                {
                    vm.Close();

                    tabs.Items.Remove(t);
                    _opentabs.Remove(d);
                };
        }
    }
}
