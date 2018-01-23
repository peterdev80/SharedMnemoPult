using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace fmslstrap.Channel
{
    /// <summary>
    /// Хранит данные об одной конечной точке канала на хосте
    /// </summary>
    internal class EndPointEntry : IComparable<EndPointEntry>, IComparer<EndPointEntry>, IEquatable<EndPointEntry>
    {
        #region Частные данные
        private static UInt32 _uidcnt;

        private readonly UInt32 _uid = _uidcnt++;
        // ReSharper disable once MemberInitializerValueIgnored
        private readonly string _host = "";
        // ReSharper disable once MemberInitializerValueIgnored
        private readonly string _channel = "";
        private readonly IPEndPoint _ipe;
        private long _received, _preceived, _rspeed;
        private long _sended, _psended, _sspeed;

        #endregion

        #region Публичные свойства

        public bool DontSendTo { get; set; }

        public UInt32 UID
        {
            get { return _uid; }
        }

        public bool IsCommandEndPoint
        {
            get { return _channel == "$ADM"; }
        }

        /// <summary>
        /// Имя хоста
        /// </summary>
        public string Host
        {
            get { return _host; }
        }

        /// <summary>
        /// Имя канала
        /// </summary>
        public string Channel
        {
            get { return _channel; }
        }

        /// <summary>
        /// Адрес конечной точки
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return _ipe; }
        }

        /// <summary>
        /// Общий объем данных, принятых от этой конечной точки
        /// </summary>
        public long Received
        {
            get { return Interlocked.Read(ref _received); }
        }

        /// <summary>
        /// Общий объем данных, отправленных этой конечной точке
        /// </summary>
        public long Sended
        {
            get { return Interlocked.Read(ref _sended); }
        }

        public long SendSpeed
        {
            get { return Interlocked.Read(ref _sspeed); }
        }

        public long ReceiveSpeed
        {
            get { return Interlocked.Read(ref _rspeed); }
        }
        #endregion

        #region Конструкторы
        public EndPointEntry(string Host, string Channel, IPEndPoint EndPoint)
        {
            _host = Host;
            _channel = Channel;
            _ipe = EndPoint;
        }
        #endregion

        #region Статистика
        /// <summary>
        /// Увеличение счетчика принятых данных
        /// </summary>
        /// <param name="ReceivedAmount">Прибавляемый объем</param>
        public void AddReceived(long ReceivedAmount)
        {
            Interlocked.Add(ref _received, ReceivedAmount);
        }

        /// <summary>
        /// Увеличение счетчика отправленных данных
        /// </summary>
        /// <param name="SendedAmount">Прибавляемый объем</param>
        public void AddSended(long SendedAmount)
        {
            Interlocked.Add(ref _sended, SendedAmount);
        }

        public void Meter()
        {
            var pr = Interlocked.Read(ref _received);
            var ps = Interlocked.Read(ref _sended);

            _rspeed = (_preceived - pr) * 2;
            _sspeed = (_psended - ps) * 2;

            _preceived = pr;
            _psended = ps;
        }
        #endregion

        #region IComparable, IComparer
        public int CompareTo(EndPointEntry other)
        {
            return Compare(this, other);
        }

        public int Compare(EndPointEntry x, EndPointEntry y)
        {
            if (x == null || y == null)
                return 0;

            return x._channel == y._channel && x._host == y._host ? 0 : String.Compare(x._channel, y._channel, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Compare(this, (EndPointEntry)obj) == 0;
        }

        public override int GetHashCode()
        {
            return _host.GetHashCode() ^ _channel.GetHashCode();
        }
        #endregion

        bool IEquatable<EndPointEntry>.Equals(EndPointEntry other)
        {
            if (other == null)
                return false;

            return _host == other._host && _channel == other._channel;
        }
    }

    internal struct ReceivedEndPoint
    {
        public string Channel;
        public IPEndPoint EndPoint;
        public bool DontSendTo;
    }
}
