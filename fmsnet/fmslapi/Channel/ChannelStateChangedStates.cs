namespace fmslapi.Channel
{
    /// <summary>
    /// Состояние подключения канала
    /// </summary>
    public enum ChannelStateChangedStates
    {
        /// <summary>
        /// Первоначальное успешное подключение
        /// </summary>
        FirstConnect,

        /// <summary>
        /// Первоначальное неуспешное подключение
        /// </summary>
        CantConnect,

        /// <summary>
        /// Подключение восстановлено
        /// </summary>
        Connected,

        /// <summary>
        /// Подключение утеряно
        /// </summary>
        Disconnected
    }
}
