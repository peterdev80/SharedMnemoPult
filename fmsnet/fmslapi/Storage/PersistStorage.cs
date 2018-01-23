using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using fmslapi.Channel;

namespace fmslapi.Storage
{
    /// <summary>
    /// Работа с постоянным хранилищем
    /// </summary>
    internal partial class PersistStorage : IPersistStorage
    {
        #region Частные данные
        /// <summary>
        /// Канал обмена с fmsldr
        /// </summary>
        private readonly IChannel _chan;

        /// <summary>
        /// Событие приема новых данные
        /// </summary>
        private readonly EventWaitHandle _evt = new AutoResetEvent(false);

        /// <summary>
        /// Кеш принятых данных
        /// </summary>
        private readonly Dictionary<kw, byte[]> _dic = new Dictionary<kw, byte[]>();
        #endregion

        #region Конструкторы
        public PersistStorage(IChannel Transport)
        {
            _chan = Transport;
            _chan.Received += _chan_Received;
        }
        #endregion

        #region Внутренние методы
        /// <summary>
        /// Обработка пакета, принятого от fmsldr
        /// </summary>
        private void _chan_Received(ISenderChannel Sender, ReceivedMessage Message)
        {
            var ms = new MemoryStream(Message.Data);
            var rd = new BinaryReader(ms);

            var cmd = rd.ReadChar();

            if (cmd == 'P')
            {
                var k = new kw(rd.ReadBytes(16));

                if (k.IsEmpty)
                    return;

                lock (_dic)
                    _dic[k] = new byte[0];

                _evt.Set();

                return;
            }

            if (cmd != 'L' && cmd != 'O')
                return;

            var kl = rd.ReadUInt16();               // Длина ключа
            var key = rd.ReadBytes(kl);             // Ключ
            
            var len = rd.ReadInt32();

            var data = len == 0 ? new byte[0] : rd.ReadBytes(len);

            lock (_dic)
                _dic[new kw(key)] = data;

            _evt.Set();
        }

        /// <summary>
        /// Ожидание подтверждения транзакции
        /// </summary>
        /// <param name="Key">Ключ транзакции</param>
        private byte[] WaitKey(byte[] Key)
        {
            var kw = new kw(Key);

            while (true)
            {
                _evt.WaitOne(20);

                lock (_dic)
                {
                    if (!_dic.TryGetValue(kw, out var rv))
                        continue;

                    _dic.Remove(kw);
                    return rv;
                }
            }
        }

        private void ClearKey(byte[] Key)
        {
            var kw = new kw(Key);

            lock (_dic)
            {
                _dic.Remove(kw);
            }
        }
        #endregion

        public IKey GetKey(byte[] Key, byte[] Index = null)
        {
            if (_chan == null)
                return null;

            return new Key(Key, Index, this);
        }

        public IKey GetKey(string Key, string Index = null)
        {
            var k = Encoding.UTF8.GetBytes(Key);
            var s = Index == null ? null : Encoding.UTF8.GetBytes(Index);

            return GetKey(k, s);
        }

        public IIndex GetIndex(byte[] Index)
        {
            if (_chan == null)
                return null;

            return new Index(Index, this);
        }

        public IIndex GetIndex(string Index)
        {
            return GetIndex(Encoding.UTF8.GetBytes(Index));
        }

        public IIndex GetUniqueContentIndex(byte[] Index)
        {
            if (_chan == null)
                return null;

            return new Index(Index, this, true);
        }

        public IIndex GetUniqueContentIndex(string Index)
        {
            return GetUniqueContentIndex(Encoding.UTF8.GetBytes(Index));
        }

        public void Store(string Key, byte[] Value)
        {
            GetKey(Key).Store(Value);
        }

        public byte[] Get(string Key)
        {
            return GetKey(Key).Get();
        }
    }
}
