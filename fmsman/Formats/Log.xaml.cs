using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace fmsman.Formats
{
    public partial class Log
    {
        private class lco : ObservableCollection<LogEntry>
        {
            public void Refilter()
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        #region Частные данные

        private readonly lco _log = new lco();
        private readonly CollectionViewSource _cvs = new CollectionViewSource();
        private readonly List<string> _senders = new List<string>();
        private List<string> _showsenders = new List<string>();

        private LogEntry _prevlogline;
        private int _prevloglinecnt;

        private event Action<DateTime, string, string> OnLogLineAvail;
        #endregion

        #region Конструкторы
        public Log(Connection Connection)
        {
            InitializeComponent();

            _cvs.Filter += cvs_Filter;

            _cvs.Source = _log;
            logg.DataContext = _cvs;

            OnLogLineAvail += AddLog;

            Connection.RetreiveLog(OnLog);
        }

        void cvs_Filter(object sender, FilterEventArgs e)
        {
            var l = e.Item as LogEntry;

            // ReSharper disable once PossibleNullReferenceException
            e.Accepted = _showsenders.Contains(l.LogSender);
        }
        #endregion

        /// <summary>
        /// Событие происходит при приеме очередной записи журнала
        /// </summary>
        /// <param name="Timestamp">Временная метка записи журнала</param>
        /// <param name="LogMsg">Сообщение</param>
        /// <param name="LogSender">Отправитель сообщения</param>
        private void OnLog(DateTime Timestamp, string LogMsg, string LogSender)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Dispatcher.BeginInvoke(OnLogLineAvail, Timestamp, LogMsg, LogSender);
        }

        /// <summary>
        /// Добавляет запись журнала в элемент отображения
        /// </summary>
        /// <param name="Timestamp">Временная метка записи журнала</param>
        /// <param name="LogMsg">Сообщение</param>
        /// <param name="LogSender">Отправитель сообщения</param>
        private void AddLog(DateTime Timestamp, string LogMsg, string LogSender)
        {
            lock (this)
            {
                var le = new LogEntry { LogTime = Timestamp, LogString = LogMsg, LogSender = LogSender };

                if (le.Equals(_prevlogline))
                {
                    _prevloglinecnt++;
                    return;
                }

                if (!_senders.Contains(LogSender))
                {
                    _senders.Add(LogSender);
                    var cb = new CheckBox { Content = LogSender, IsChecked = true };
                    cb.Click += shownetwork_Click;
                    _showsenders.Add(LogSender);
                    ftb.Items.Add(cb);
                }

                if (_prevloglinecnt > 0)
                {
                    _log.Add(new LogEntry
                    {
                        LogSender = "",
                        HideTime = true,
                        LogString = $"Предыдущая строка была повторена {_prevloglinecnt} раз"
                    });
                }

                _log.Add(le);

                logg.ScrollIntoView(le);

                _prevlogline = le;
                _prevloglinecnt = 0;
            }
        }

        private void shownetwork_Click(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            _showsenders = ftb.Items.OfType<CheckBox>().Where(b => b.IsChecked.Value).Select(b => b.Content as string).ToList();

            _log.Refilter();
        }
    }

    public class LogEntry : INotifyPropertyChanged
    {
        private bool _hidetime;
        
        public DateTime LogTime { get; set; }
        public string LogString { get; set; }
        public string LogSender { get; set; }
        
        public bool HideTime
        {
            get => _hidetime;
            set
            {
                _hidetime = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HideTime"));
            }
        }

        public bool Equals(LogEntry o)
        {
            if (o == null)
                return false;

            return (int)LogTime.TimeOfDay.TotalSeconds == (int)o.LogTime.TimeOfDay.TotalSeconds && LogString == o.LogString && LogSender == o.LogSender;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
