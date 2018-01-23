using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO.MemoryMappedFiles;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace fmsman.Formats
{
    public partial class VariablesMap
    {
        private readonly object _data;
        private readonly Connection _connection;
        private readonly MObservableCollection<VarEntry> _vars = new MObservableCollection<VarEntry>();
        private event Action<VarEntry[]> OnVarsAvail;
        private readonly int _token;
        private static readonly Random rnd = new Random();
        private readonly DispatcherTimer _timer;
        private readonly FilteredVarMap _flt;

        public bool IsCustom
        {
            get;
            set;
        }

        public VariablesMap()
        {
            InitializeComponent();
        }

        public VariablesMap(object data)
        {
            InitializeComponent();

            _token = rnd.Next(int.MaxValue);
            _data = data;
            _connection = Application.Current.Properties["admconn"] as Connection;

            _flt = new FilteredVarMap(_vars, "");
            var ldr = new SmoothVarLoader(_flt);
            variables.DataContext = ldr;

            OnVarsAvail += VarsAvail;

            if (!(_data is PipeEntry)) 
                return;

            var pipe = _data as PipeEntry;

            Debug.Assert(_connection != null, "_connection != null");

            _connection.RetreivePipeVars(_token, OnVars, pipe.Instance);
                
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(333)};
            _timer.Tick += (s, e) => _connection.RetreivePipeVars(_token, pipe.Instance);
            _timer.IsEnabled = true;
        }

        private void OnVars(VarEntry[] vars)
        {
            Debug.Assert(OnVarsAvail != null, "OnVarsAvail != null");

            Dispatcher.BeginInvoke(OnVarsAvail, new object[] { vars });
        }

        private void VarsAvail(VarEntry[] vars)
        {
            lock (_vars)
            {
                foreach (var v in vars)
                    _vars.Add(v);

                TextBox_TextChanged(null, null);
            }
        }

        public void VariableDragged(VarEntry Var)
        {
            lock (_vars)
            {
                if (!_vars.Contains(Var))
                    _vars.Add(Var);

                TextBox_TextChanged(null, null);
            }
        }

        public void Close()
        {
            _connection.DetachVarMap(_token);
            _timer.IsEnabled = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _flt.Refilter(tbfilt.Text);
            stats.Text = $"Переменных в списке {_flt.Count}/{_vars.Count}";
        }

        private Point _startPoint;
        private bool _dragprep;

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _dragprep = true;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            var mousePos = e.GetPosition(null);
            var diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance &&
                _dragprep)
            {
                var cur = variables.CurrentItem;
                if (cur != null)
                {
                    _dragprep = false;
                    var dragData = new DataObject("VarEntry", cur);
                    DragDrop.DoDragDrop(this, dragData, DragDropEffects.Move);
                }
            } 
        }

        private void variables_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragprep = false;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tbfilt.Text = "";
            TextBox_TextChanged(sender, null);
        }
    }

    public class SmoothVarLoader : IEnumerable<VarEntry>, INotifyCollectionChanged
    {
        private readonly FilteredVarMap _map;
        private readonly DispatcherTimer _timer;

        private readonly List<VarEntry> _current = new List<VarEntry>();

        public SmoothVarLoader(FilteredVarMap Map)
        {
            _map = Map;

            _timer = new DispatcherTimer(DispatcherPriority.DataBind);
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += _timer_Tick;

            _timer.IsEnabled = true;
        }

        public void _timer_Tick(object sender, EventArgs e)
        {
            if (CollectionChanged == null)
                return;

            var adds = _map.Except(_current).Take(5).ToList();
            var dels = _current.Except(_map).ToList();

            if (adds.Count > 0)
            {
                foreach(var a in adds)
                {
                    var index = _map.GetList().IndexOf(a);
                    _current.Insert(index, a);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { a }.ToList(), index));
                }
            }

            if (dels.Count > 0)
            {
                foreach (var d in dels)
                {
                    var index = _current.IndexOf(d);
                    _current.Remove(d);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, d, index));
                }
            }

            if (_map.Except(_current).Take(1).Any())
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => _timer_Tick(null, null)), DispatcherPriority.DataBind);
        }

        #region Члены INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region Члены IEnumerable<VarEntry>

        public IEnumerator<VarEntry> GetEnumerator()
        {
            return _current.GetEnumerator();
        }

        #endregion

        #region Члены IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_current).GetEnumerator();
        }

        #endregion
    }

    public class FilteredVarMap : IEnumerable<VarEntry>
    {
        private readonly List<VarEntry> _internal = new List<VarEntry>();
        private readonly MObservableCollection<VarEntry> _src;

        public FilteredVarMap(MObservableCollection<VarEntry> Src, string Filter)
        {
            _src = Src;
            Refilter(Filter);
        }

        public int Count => _internal.Count;

        public void Refilter(string Filter)
        {
            var lf = Filter.ToLower();

            var newlist = (from v in _src
                           where v.VarName.ToLower().Contains(lf) || v.Comment.ToLower().Contains(lf)
                           select v).ToArray();

            /*
            var dels = _internal.Except(newlist).ToArray();
            var news = newlist.Except(_internal).ToArray();

            foreach (var d in dels)
            {
                var i = _internal.IndexOf(d);
                _internal.Remove(d);
                if (CollectionChanged != null)
                    CollectionChanged(_internal, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, d, i));
            }

            foreach (var n in news)
            {
                _internal.Add(n);
                if (CollectionChanged != null)
                    CollectionChanged(_internal, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, n));
            }*/

            _internal.Clear();
            _internal.AddRange(newlist);
        }

        #region Члены IEnumerable<VarEntry>

        public IEnumerator<VarEntry> GetEnumerator()
        {
            return (_internal as IEnumerable<VarEntry>).GetEnumerator();
        }

        public IList<VarEntry> GetList()
        {
            return _internal;
        }
        #endregion

        #region Члены IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internal.GetEnumerator();
        }

        #endregion
    }

    public class VarEntry
    {
        private static MemoryMappedFile _file;
        private static MemoryMappedViewAccessor _accessor;

        public uint VarIndex { get; set; }
        public string VarName { get; set; }
        public string VarType { get; set; }
        public uint ShOffset { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int VarSize { get; set; }
        public string Comment { get; set; }
        public string VarMap { get; set; }
        public Connection Connection { get; set; }

        public static MemoryMappedViewAccessor Accessor
        {
            get => _accessor;
            set
            {
                Debug.Assert(_accessor == null);

                _accessor = value;
            }
        }

        public static MemoryMappedFile File
        {
            get => _file;
            set
            {
                Debug.Assert(_file == null);
                _file = value;
            }
        }
    }
}
