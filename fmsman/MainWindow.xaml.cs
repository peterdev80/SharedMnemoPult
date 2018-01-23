using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using fmsman.Formats;
using System.Windows.Threading;

namespace fmsman
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly bool _embedded;

        public MainWindow()
        {
            InitializeComponent();

            Application.Current.Properties["Tabs"] = tabs;

            // ReSharper disable once UnusedVariable
            var close = new Action(() => Dispatcher.Invoke(Close));

            var ldr = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("fmsldr"));
            if (ldr == null)
                return;

            // Запущено в процессе fmsldr
            _Connect.Visibility = Visibility.Collapsed;
            _Disconnect.Visibility = Visibility.Collapsed;

            Dispatcher.BeginInvoke(new Action(() => MenuItem_Click(null, null)));

            _embedded = true;
        }

        private DispatcherTimer _timer;

        private Log _log;
        private Hosts _hosts;
        private Pipes _pipes;
        private static ModelConsoleLog _modellog;
        private Config _config;

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var con = new Connection();
            if (!con.Connect())
                return;

            _Disconnect.IsEnabled = true;

            con.Disconnected += con_Disconnected;

            con.AttachToGlobalChangesList();

            _Connect.IsEnabled = false;

            Application.Current.Properties["admconn"] = con;

            _timer = new DispatcherTimer 
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += (s2, e2) => con.RetreiveGlobalChangesList();
            _timer.IsEnabled = true;

            _log = new Log(con);

            var t = new TabItem {Header = "Лог", Content = _log};
            tabs.Items.Add(t);

            t = new TabItem { Header = "Контроль", Content = new AdmLoc(con) };
            tabs.Items.Add(t);

            _pipes = new Pipes(con);
            t = new TabItem {Header = "Клиенты", Content = _pipes};
            tabs.Items.Add(t);

            t.IsSelected = true;

            _hosts = new Hosts(con);
            t = new TabItem { Header = "Хосты", Content = _hosts };
            tabs.Items.Add(t);

            if (_modellog == null)
                _modellog = new ModelConsoleLog();

            t = new TabItem { Header = "Модель", Content = _modellog };
            t.SetBinding(VisibilityProperty, new Binding { Source = _modellog, Path = new PropertyPath(VisibilityProperty) });

            tabs.Items.Add(t);

            _config = new Config(con);
            t = new TabItem { Header = "Конфигурация", Content = _config };
            tabs.Items.Add(t);
        }

        void con_Disconnected()
        {
            _pipes.Detach();
            _hosts.Detach();
            _config.Detach();

            void Cl()
            {
                tabs.Items.Clear();
                _Connect.IsEnabled = true;
            }

            Dispatcher.BeginInvoke((Action)Cl);

            if (_embedded)
                Dispatcher.Invoke(Close);
        }

        private void _Disconnect_Click(object sender, RoutedEventArgs e)
        {
            var con = Application.Current.Properties["admconn"] as Connection;

            if (con == null)
                return;

            con.Disconnect();

            _Disconnect.IsEnabled = false;
        }

        private void AddWorkset(object sender, RoutedEventArgs e)
        {
            var mi = new MenuItem {Header = "Набор переменных"};
            mi.Click += WorksetClick;

            worksets.Items.Insert(0, mi);
            WorksetClick(mi, e);
        }

        private void WorksetClick(object sender, RoutedEventArgs e)
        {
            var m = sender as MenuItem;
            Debug.Assert(m != null, "m != null");

            var vm = m.Tag as VariablesMap;

            if (vm == null)
            {
                vm = new VariablesMap(null) {IsCustom = true};
                m.Tag = vm;
            }

            var t = vm.Tag as CloseableTabItem;

            if (t == null)
            {
                t = new CloseableTabItem {Header = m.Header.ToString()};

                t.MouseDoubleClick += (s3, e3) =>
                    {
                        //if (e3.Key == Key.F2)
                        {
                            var ti = s3 as TabItem;
                            // ReSharper disable once PossibleNullReferenceException
                            var tb = new TextBox {Text = ti.Header.ToString()};
                            tb.LostKeyboardFocus += (s4, e4) =>
                                {
                                    var tb4 = s4 as TextBox;
                                    // ReSharper disable once PossibleNullReferenceException
                                    ti.Header = tb4.Text;

                                    m.Header = tb4.Text;
                                };


                            ti.Header = tb;

                            tb.Focusable = true;
                            FocusManager.SetFocusedElement(ti, tb);
                            Keyboard.Focus(tb);
                            tb.Select(0, 0);
                        }
                    };

                t.CloseTab += (s2, e2) =>
                {
                    t.Content = null;

                    (from a in worksets.Items.Cast<MenuItem>()
                     let vml = a.Tag as VariablesMap
                     where a != null
                     // ReSharper disable once PossibleUnintendedReferenceComparison
                     where vml == vm
                     select vml).First().Tag = null;

                    tabs.Items.Remove(t);
                };

                t.Content = vm;

                vm.Tag = t;
                t.Tag = vm;

                tabs.Items.Add(t);
            }

            t.IsSelected = true;
        }

        private TabItem _psel;

        private void tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 && e.RemovedItems.Count == 1)
            {
                tabs.SelectedItem = _psel;
                return;
            }

            if (e.RemovedItems.Count == 1)
                _psel = e.RemovedItems[0] as TabItem;
        }
    }
}
