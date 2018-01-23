using System;
using System.IO;

namespace fmslapi.Channel
{
    /// <summary>
    /// Принятое из канала сообщение
    /// </summary>
    public class ReceivedMessage : IComparable<ReceivedMessage>, IComparable
    {
        #region Частные данные
        /// <summary>
        /// Порядковый номер принятого сообщения
        /// </summary>
        private readonly UInt32 _order;

        /// <summary>
        /// Имя хоста отправителя сообщения
        /// </summary>
        private readonly string _sender;

        /// <summary>
        /// Байтовая посылка
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Идентификатор отправителя сообщения
        /// </summary>
        private readonly UInt32 _senderid;

        /// <summary>
        /// Поток
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// Было обращение к потоку
        /// </summary>
        private bool _swa;                          // Поток был затронут
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Порядковый номер сообщения
        /// </summary>
        public UInt32 OrderID => _order;

        /// <summary>
        /// Имя хоста отправителя сообщения
        /// </summary>
        public string Sender => _sender;

        /// <summary>
        /// Байтовая посылка
        /// </summary>
        public byte[] Data
        {
            get 
            {
                lock (this)
                {
                    if (_data == null && _stream != null)
                    {
                        var ms = new MemoryStream();
                        _stream.CopyTo(ms);

                        _swa = true;

                        _data = ms.ToArray();
                    }
                }

                return _data; 
            }
        }

        /// <summary>
        /// Поток данных
        /// </summary>
        public Stream Stream
        {
            get 
            {
                lock (this)
                {
                    if (_stream == null && _data != null)
                        return new MemoryStream(_data);
                        
                    _swa = true;
                }

                return _stream; 
            }
        }

        /// <summary>
        /// Поток был затронут
        /// </summary>
        internal bool StreamWasAffected => _swa;

        /// <summary>
        /// Уникальный идентификатор отправителя сообщения
        /// </summary>
        public UInt32 SenderID => _senderid;

        #endregion

        #region Конструкторы
        public ReceivedMessage(byte[] Data)
        {
            _data = Data;
        }

        public ReceivedMessage(UInt32 OrderID, string Sender, byte[] Data, UInt32 SenderID)
        {
            _order = OrderID;
            _sender = Sender;
            _data = Data;
            _senderid = SenderID;
        }

        public ReceivedMessage(UInt32 OrderID, string Sender, ChannelDataStream Stream, UInt32 SenderID)
        {
            _order = OrderID;
            _sender = Sender;
            _stream = Stream;
            _senderid = SenderID;
        }
        #endregion

        #region Члены IComparable<ChannelPacket>

        int IComparable<ReceivedMessage>.CompareTo(ReceivedMessage other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (_sender == other._sender && _order == other._order && _senderid == other._senderid)
                return 0;

            return _order < other._order ? -1 : 1;
        }

        #endregion

        #region Перегрузки
        public override bool Equals(object obj)
        {
            return obj is ReceivedMessage o && (_sender == o._sender && _order == o._order && _senderid == o._senderid);
        }

        public override int GetHashCode()
        {
            return _order.GetHashCode() ^ _sender.GetHashCode() ^ _senderid.GetHashCode();
        }
        #endregion

        #region Члены IComparable

        int IComparable.CompareTo(object obj)
        {
            return ((IComparable<ReceivedMessage>)this).CompareTo(obj as ReceivedMessage);
        }

        #endregion
    }
}
