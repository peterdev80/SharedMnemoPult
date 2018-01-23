namespace fmslapi.VDL
{
    /// <summary>
    /// Интерфейс доступа к свойствам VDL
    /// </summary>
    public interface IPropertyHost
    {
        /// <summary>
        /// Возвращает объект доступа к свойству
        /// </summary>
        /// <param name="Property">Имя свойства</param>
        /// <param name="Target">Целевой объект-владелец свойства</param>
        /// <returns>Объект доступа к свойству</returns>
        IProperty GetProperty(string Property, object Target);
    }

    /// <summary>
    /// Интерфейс доступа к переменной VDL
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// Возвращает значение свойства
        /// </summary>
        /// <returns>Значение свойства</returns>
        object GetValue();

        /// <summary>
        /// Устанавливает значение свойства
        /// </summary>
        /// <param name="value">Значение свойства</param>
        void SetValue(object value);
    }
}
