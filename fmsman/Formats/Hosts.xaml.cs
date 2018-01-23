using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.IO;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace fmsman.Formats
{
    public partial class Hosts
    {
        private readonly MObservableCollection<HostEntry> _hosts = new MObservableCollection<HostEntry>();

        DispatcherTimer _timer;

        public Hosts(Connection Connection)
        {
            InitializeComponent();

            Connection.RetreiveHosts(Dispatcher.CurrentDispatcher, OnHosts);

            hosts.DataContext = _hosts;

            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(250)};
            _timer.Tick += (s, e) =>
            {
                if (_timer == null)
                    return; 

                Connection.RetreiveHosts();
            };

            _timer.IsEnabled = true;
        }

        public void Detach()
        {
            _timer.Stop();
            _timer = null;
        }

        private void OnHosts(BinaryReader brdr)
        {
            lock (_hosts)
            {
                var iv = false;

                var cnt = brdr.ReadInt32();

                var linst = new List<Int32>();

                for (var i = 0; i < cnt; i++)
                {
                    var iid = brdr.ReadInt32();

                    var h = _hosts.FirstOrDefault(x => x.Instance == iid) ?? new HostEntry { Instance = iid };

                    h.Host = brdr.ReadString();
                    h.Channel = brdr.ReadString();
                    h.EndPoint = brdr.ReadString();
                    h.Received = brdr.ReadInt64();
                    h.Sended = brdr.ReadInt64();
                    h.SendSpeed = brdr.ReadUInt32();
                    h.ReceiveSpeed = brdr.ReadUInt32();
                    h.DontSendTo = brdr.ReadBoolean();

                    linst.Add(h.Instance);

                    if (!_hosts.Any(x => x.Instance == h.Instance))
                    {
                        iv = true;
                        _hosts.Add(h);
                    }
                }

                foreach (var l in _hosts.Where(x => !linst.Contains(x.Instance)).ToArray())
                {
                    _hosts.Remove(l);
                    iv = true;
                }

                if (iv)
                    _hosts.Invalidate(null);
            }
        }
    }

    public class HostEntry : DependencyObject
    {
        public static readonly DependencyProperty ReceivedProperty = DependencyProperty.Register("Received", typeof(long), typeof(HostEntry));
        public static readonly DependencyProperty SendedProperty = DependencyProperty.Register("Sended", typeof(long), typeof(HostEntry));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", typeof(string), typeof(HostEntry));

        public static readonly DependencyProperty SendSpeedProperty = DependencyProperty.Register("SendSpeed", typeof(UInt32), typeof(HostEntry));
        public static readonly DependencyProperty ReceiveSpeedProperty = DependencyProperty.Register("ReceiveSpeed", typeof(UInt32), typeof(HostEntry));

        public static readonly DependencyProperty DontSendToProperty = DependencyProperty.Register("DontSendTo", typeof(bool), typeof(HostEntry));

        public int Instance { get; set; }
        public string Host { get; set; }

        public bool DontSendTo
        {
            get => (bool)GetValue(DontSendToProperty);
            set => SetValue(DontSendToProperty, value);
        }
        
        public string EndPoint
        {
            get => (string)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        public string Channel { get; set; }
        public long Received
        {
            get => (long)GetValue(ReceivedProperty);
            set => SetValue(ReceivedProperty, value);
        }

        public long Sended 
        {
            get => (long)GetValue(SendedProperty);
            set => SetValue(SendedProperty, value);
        }

        public UInt32 SendSpeed
        {
            get => (UInt32)GetValue(SendSpeedProperty);
            set => SetValue(SendSpeedProperty, value);
        }

        public UInt32 ReceiveSpeed
        {
            get => (UInt32)GetValue(ReceiveSpeedProperty);
            set => SetValue(ReceiveSpeedProperty, value);
        }
    }
}
