namespace fmslstrap.Channel
{
    /// <summary>
    /// Тип канала
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// Обычный канал для обмена байтовыми посылками
        /// </summary>
        Regular,

        /// <summary>
        /// Выскокоуровневый канал обмена таблицами переменных
        /// </summary>
        Variables,

        /// <summary>
        /// Локальный канал обмена в пределах одного хоста
        /// </summary>
        Local
    }
}
