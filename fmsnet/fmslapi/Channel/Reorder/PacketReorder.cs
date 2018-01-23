using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace fmslapi.Channel.Reorder
{
    /// <summary>
    /// Восстановление исходного порядка пакетов
    /// </summary>
    public class PacketReorder : ReorderBase
    {
        private EventWaitHandle _reorderevent;
        private readonly SortedSet<ReceivedMessage> _reordercache = new SortedSet<ReceivedMessage>();
        private readonly Dictionary<UInt32, UInt32> _lastorderids = new Dictionary<UInt32, UInt32>();
        private bool _exit, _exited;

        protected override void Start()
        {
            _exit = _exited = false;

            _reorderevent = new AutoResetEvent(false);
            ThreadPool.QueueUserWorkItem(ReorderWorker);
        }

        /// <summary>
        /// Сортировщик пакетов
        /// </summary>
        private void ReorderWorker(object state)
        {
            ReceivedMessage rp = null;
            var avoidwait = false;
            var tc = 0;

            while (!_exit)
            {
                EventWaitHandle evt;
                lock (this)
                {
                    // Если _reorderevent стал null - выходим из потока упорядочивания
                    evt = _reorderevent;
                    if (evt == null)
                        break;
                }

                if (rp != null)
                {
                    EmitPacket(rp);

                    rp = null;
                    tc = 0;
                }
                else
                    if (!avoidwait)
                        if (!evt.WaitOne(50))
                            tc++;

                avoidwait = false;

                lock (this)
                {
                    if (_reordercache.Count == 0)
                        continue;

                    var fe = _reordercache.FirstOrDefault();
                    if (fe == null)
                        continue;

                    var porder = fe.OrderID;           // OrderID первого в очереди пакета
                    var sender = fe.Sender;            // Отправитель пакета
                    var sid = fe.SenderID;             // Метка отправителя пакета

                    UInt32 lordfromsender;             // Последний принятый OrderID от этого отправителя
                    if (!_lastorderids.TryGetValue(sid, out lordfromsender))
                    {
                        // Самый первый пакет в потоке.
                        // Просто передаем дальше с сохранением OrderID начала потока
                        _lastorderids[sid] = porder;
                        _reordercache.Remove(fe);
                        rp = fe;
                        continue;
                    }

                    if (porder == 0)
                    {
                        // Перезапуск последовательности
                        _lastorderids[sid] = porder;
                        _reordercache.RemoveWhere(p => p.Sender == sender);
                        rp = fe;
                        continue;
                    }

                    if (porder <= lordfromsender)
                    {
                        // Пакет-дубликат игнорируем
                        // Пакет пришедший слишком поздно тоже
                        _reordercache.Remove(fe);
                        avoidwait = true;
                        continue;
                    }

                    if (porder == lordfromsender + 1)
                    {
                        // Ожидаемый пакет
                        _lastorderids[sid] = porder;
                        _reordercache.Remove(fe);
                        rp = fe;
                        continue;
                    }

                    
                    if (tc < 2) 
                        continue;

                    // Если ждали более 100мс
#if DEBUG
                    Trace.WriteLine(string.Format("ChannelReorder: Lost packet. Channel={0}, OldLorder={1}, NewLorder={2}, Sender={3}, SenderID={4}",
                                                  ChannelName, lordfromsender, porder, sender, sid));
#endif

                    // Похоже пакет пропал совсем
                    _lastorderids[sid] = porder;
                    _reordercache.Remove(fe);
                    rp = fe;
                    avoidwait = true;
                }
            }

            _reorderevent.Close();
            _reorderevent = null;

            _exited = true;
        }

        public override void OnNewPacket(ReceivedMessage Packet)
        {
            lock (this)
            {
                if (_exit)
                    return;

                _reordercache.Add(Packet);

                _reorderevent?.Set();
            }
        }

        protected override void OnClose()
        {
            lock (this)
                _exit = true;

            while (!_exited)
                Thread.Sleep(1);
        }
    }
}
