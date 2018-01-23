using System;

namespace fmslapi.Channel.Reorder
{
    /// <summary>
    /// Базовый класс механизмов упорядочивания пакетов
    /// </summary>
    public abstract class ReorderBase
    {
        #region Частные данные
        /// <summary>
        /// Присоединенный канал
        /// </summary>
        private Channel _chan;
        #endregion

        internal void AttachChannel(Channel Channel)
        {
            if (_chan != null)
                throw new InvalidOperationException();

            _chan = Channel;
            Start();
        }

        internal void DetachChannel()
        {
            OnClose();
            _chan = null;
        }
        
        protected void EmitPacket(ReceivedMessage Packet)
        {
            _chan?.RoutePacketToReceiver(Packet);
        }

        protected string ChannelName => _chan.ToString();

        protected abstract void Start();

        public abstract void OnNewPacket(ReceivedMessage Packet);
        protected virtual void OnClose() { }
    }
}
