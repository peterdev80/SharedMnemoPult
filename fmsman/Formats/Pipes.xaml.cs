using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.IO;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace fmsman.Formats
{
    public partial class Pipes
    {
        private readonly MObservableCollection<PipeEntry> _pipes = new MObservableCollection<PipeEntry>();

        DispatcherTimer timer;

        public Pipes(Connection Connection)
        {
            InitializeComponent();

            pipes.DataContext = _pipes;

            Connection.RetreivePipes(OnPipe, OnDeletePipe, Dispatcher);

            timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(100)};
            timer.Tick += (s, e) =>
                {
                    if (timer == null)
                        return;

                    Connection.RetreivePipeStatistics(OnStats);
                };
            timer.IsEnabled = true;
        }

        private void OnPipe(BinaryReader brdr)
        {
            lock (_pipes)
            {
                var pe = new PipeEntry { Instance = brdr.ReadUInt64(), EndPoint = brdr.ReadString() };
                var t = brdr.ReadChar();
                pe.IsVarChan = t == 'V';
                pe.Channel = brdr.ReadString();

                _pipes.Add(pe);

                _pipes.Invalidate(null);
            }
        }

        private void OnDeletePipe(ulong InstanceID)
        {
            lock (_pipes)
            {
                var pd = (from p in _pipes where p.Instance == InstanceID select p).FirstOrDefault();

                Debug.Assert(pd != null, "pd != null");

                pd.OnDelete();
                _pipes.Remove(pd);
                _pipes.Invalidate(null);
            }
        }

        private void OnStats(ulong InstanceID, long Sended, long SendedCnt, long Received, long ReceivedCnt, int VarCnt)
        {
            lock (_pipes)
            {
                var u = (from p in _pipes where p.Instance == InstanceID select p).FirstOrDefault();
                if (u == null)
                    return;

                u.Received = Received;
                u.ReceivedCnt = ReceivedCnt;
                u.Sended = Sended;
                u.SendedCnt = SendedCnt;
                u.VarCount = VarCnt;
            }
        }

        public void Detach()
        {
            timer.Stop();
            timer = null;
        }
    }

    public class PipeEntry : DependencyObject
    {
        public static readonly DependencyProperty ReceivedProperty = DependencyProperty.Register("Received", typeof(long), typeof(PipeEntry));
        public static readonly DependencyProperty SendedProperty = DependencyProperty.Register("Sended", typeof(long), typeof(PipeEntry));
        public static readonly DependencyProperty ReceivedCntProperty = DependencyProperty.Register("ReceivedCnt", typeof(long), typeof(PipeEntry));
        public static readonly DependencyProperty SendedCntProperty = DependencyProperty.Register("SendedCnt", typeof(long), typeof(PipeEntry));
        public static readonly DependencyProperty VarCountProperty = DependencyProperty.Register("VarCount", typeof(int), typeof(PipeEntry));

        public ulong Instance { get; set; }
        public string EndPoint { get; set; }
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

        public long ReceivedCnt
        {
            get => (long)GetValue(ReceivedCntProperty);
            set => SetValue(ReceivedCntProperty, value);
        }

        public long SendedCnt
        {
            get => (long)GetValue(SendedCntProperty);
            set => SetValue(SendedCntProperty, value);
        }

        public bool IsVarChan { get; set; }

        public int VarCount
        {
            get => (int)GetValue(VarCountProperty);
            set => SetValue(VarCountProperty, value);
        }

        public event EventHandler Delete;

        public void OnDelete()
        {
            Delete?.Invoke(null, null);
        }
    }
}
