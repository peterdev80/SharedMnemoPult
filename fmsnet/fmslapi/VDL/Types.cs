namespace fmslapi.VDL
{
    /// <summary>
    /// Типы данных исполняемой среды VDL
    /// </summary>
    public enum Types
    {
        /// <summary>
        /// Тип не определен
        /// </summary>
        Undefined,

        /// <summary>
        /// Отсутствие возвращаемого значения
        /// </summary>
        Void,

        /// <summary>
        /// 32 битное знаковое целое
        /// </summary>
        Int32,

        /// <summary>
        /// 16 битное значение с плавающей запятой
        /// </summary>
        Float,

        /// <summary>
        /// 32 битное значение с плавающей запятой
        /// </summary>
        Double,

        /// <summary>
        /// Строка
        /// </summary>
        String,

        /// <summary>
        /// Логическое значение
        /// </summary>
        Boolean,

        /// <summary>
        /// Ключ динамического ресурса WPF
        /// </summary>
        DynamicResource
    }
}
