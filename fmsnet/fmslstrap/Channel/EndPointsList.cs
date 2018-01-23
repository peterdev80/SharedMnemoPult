using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System;

namespace fmslstrap.Channel
{
    /// <summary>
    /// Управляет конечными точками сокетов каналов
    /// </summary>
    internal static class EndPointsList
    {
        #region Частные данные
        /// <summary>
        /// Коллекция конечных точек
        /// </summary>
        private static readonly List<EndPointEntry> _endpoints = new List<EndPointEntry>();

        /// <summary>
        /// Потоковая блокировка доступа к коллекции
        /// </summary>
        private static readonly ReaderWriterLockSlim _lockobj = new ReaderWriterLockSlim();

        // ReSharper disable once NotAccessedField.Local
        private static Timer _timer;

        /// <summary>
        /// Представление сгруппированное по каналам
        /// </summary>
        private static readonly Dictionary<string, EndPointEntry[]> _bychannelview = new Dictionary<string, EndPointEntry[]>();

        private static readonly EndPointEntry[] _emptylst = new EndPointEntry[0];
        #endregion

        #region Конструкторы
        static EndPointsList()
        {
            _timer = new Timer(TrafficMeter, null, 500, 500);
        }
        #endregion

        #region Коллекции
        /// <summary>
        /// Возвращает полный список конечных точек
        /// </summary>
        internal static EndPointEntry[] GetAll()
        {
            try
            {
                _lockobj.EnterReadLock();

                return _endpoints.ToArray();
            }
            finally
            {
                _lockobj.ExitReadLock();
            }
        }

        /// <summary>
        /// Возвращает коллекцию конечных точек каналов на указанном хосте
        /// </summary>
        /// <param name="Host">Хост</param>
        /// <returns>Коллекция конечных точек каналов</returns>
        public static EndPointEntry[] GetByHost(string Host)
        {
            try
            {
                _lockobj.EnterReadLock();

                return _endpoints.Where(a => a.Host == Host).ToArray();
            }
            finally
            {
                _lockobj.ExitReadLock();
            }
        }

        /// <summary>
        /// Возвращает коллекцию конечных точек указанного канала
        /// </summary>
        /// <param name="Channel">Канал</param>
        /// <returns>Коллекция конечных точек каналов</returns>
        public static EndPointEntry[] GetByChannel(string Channel)
        {
            try
            {
                _lockobj.EnterReadLock();

                EndPointEntry[] r;
                _bychannelview.TryGetValue(Channel, out r);

                return r ?? _emptylst;
            }
            finally
            {
                _lockobj.ExitReadLock();
            }
        }

        /// <summary>
        /// Возвращает конечную точку обмена по сетевой конечной точке
        /// </summary>
        /// <param name="EndPoint">Сетевая конечная точка</param>
        /// <returns>Конечная точка обмена</returns>
        public static EndPointEntry GetByEndPoint(IPEndPoint EndPoint)
        {
            try
            {
                _lockobj.EnterReadLock();

                return _endpoints.FirstOrDefault(h => h.EndPoint.Equals(EndPoint));
            }
            finally
            {
                _lockobj.ExitReadLock();
            }
        }

        public static string[] GetHosts()
        {
            try
            {
                _lockobj.EnterReadLock();

                return _endpoints.Select(x => x.Host).Distinct().ToArray();
            }
            finally
            {
                _lockobj.ExitReadLock();
            }
        }

        public static IPEndPoint[] GetHostsEndPoints()
        {
            try
            {
                _lockobj.EnterReadLock();

                return _endpoints.Where(x => x.IsCommandEndPoint).Select(x => x.EndPoint).ToArray();
            }
            finally
            {
                _lockobj.ExitReadLock();
            }
        }

        public static EndPointEntry[] GetEndPoint(string Host, string Channel)
        {
            try
            {
                _lockobj.EnterReadLock();

                return _endpoints.Where(x => x.Host == Host && x.Channel == Channel).ToArray();
            }
            finally
            {
                _lockobj.ExitReadLock();
            }
        }
        #endregion

        #region Манипуляции
        public static void AddHostEndpoints(string Host, Tuple<string, IPEndPoint>[] Channels)
        {
            try
            {
                _lockobj.EnterWriteLock();

                // Теперь переносим обратно и обновляем (или создаем новые) те, которые есть в Channels
                foreach (var c in Channels)
                {
                    var cc = _endpoints.FirstOrDefault(x => x.Channel == c.Item1 && x.EndPoint.Equals(c.Item2));

                    if (cc != null)
                    {
                        cc.EndPoint.Address = c.Item2.Address;
                        cc.EndPoint.Port = c.Item2.Port;
                    }
                    else
                        _endpoints.Add(new EndPointEntry(Host, c.Item1, c.Item2));
                }

                UpdateViews();

                // В im остались конечные точки которых удаленный хост больше не имеет
                // игнорируем их
            }
            finally
            {
                _lockobj.ExitWriteLock();
            }
        }

        /// <summary>
        /// Обновляет данных о конечных точках хоста
        /// </summary>
        /// <param name="Host">Имя хоста</param>
        /// <param name="Channels">Массив данных о конечных точках для обновления</param>
        /// <param name="AllowNewHostEvent">Генерировать события изменения состава хостов в канале</param>
        public static void UpdateHostEndpoints(string Host, ReceivedEndPoint[] Channels, bool AllowNewHostEvent)
        {
            var tn = new List<EndPointEntry>();     // Список _новых_ конечных точек

            try
            {
                _lockobj.EnterWriteLock();

                var im = new List<EndPointEntry>();

                // Сначала изымаем из списка все конечные точки удаленного хоста (Host)
                foreach (var e in _endpoints.ToArray().Where(e => e.Host == Host))
                {
                    _endpoints.Remove(e);
                    im.Add(e);
                }

                // Теперь переносим обратно и обновляем (или создаем новые) те, которые есть в Channels
                foreach (var c in Channels)
                {
                    var cc = im.FirstOrDefault(x => x.Channel == c.Channel && x.EndPoint.Equals(c.EndPoint));

                    if (cc != null)
                    {
                        im.Remove(cc);
                        _endpoints.Add(cc);

                        cc.EndPoint.Address = c.EndPoint.Address;
                        cc.EndPoint.Port = c.EndPoint.Port;
                        cc.DontSendTo = c.DontSendTo;
                    }
                    else
                    {
                        var nepe = new EndPointEntry(Host, c.Channel, c.EndPoint) { DontSendTo = c.DontSendTo };
                        _endpoints.Add(nepe);
                        tn.Add(nepe);
                    }
                }

                UpdateViews();

                // В im остались конечные точки которых удаленный хост больше не имеет
                // игнорируем их
            }
            finally
            {
                _lockobj.ExitWriteLock();
            }

            if (!AllowNewHostEvent || tn.Count <= 0) 
                return;

            foreach (var ch in tn.Select(c => Manager.GetChannel(c.Channel)).Where(ch => ch != null))
                ch.RaiseNewHostInChannel(Host);
        }

        public static void RemoveHostFromChannel(string Host, string Channel)
        {
            try
            {
                _lockobj.EnterWriteLock();

                _endpoints.RemoveAll(x => x.Host == Host && x.Channel == Channel);

                UpdateViews();
            }
            finally
            {
                _lockobj.ExitWriteLock();
            }
        }

        private static void UpdateViews()
        {
            _bychannelview.Clear();

            foreach (var c in _endpoints.Select(c => c.Channel).Distinct())
                _bychannelview.Add(c, _endpoints.Where(a => a.Channel == c).ToArray());
        }

        #endregion

        #region Подсчет скорости
        /// <summary>
        /// Подсчет скорости передачи данных
        /// </summary>
        private static void TrafficMeter(object state)
        {
            EndPointEntry[] epes;

            try
            {
                _lockobj.EnterReadLock();

                epes = _endpoints.ToArray();
            }
            finally
            {
                _lockobj.ExitReadLock();
            }

            foreach (var epe in epes)
                epe.Meter();
        }
        #endregion
    }
}
