using System;

namespace fmslapi
{
    /// <summary>
    /// Конфигурационная секция
    /// </summary>
    public interface IConfigSection
    {
        /// <summary>
        /// Возвращает значение ключа
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Значение</returns>
        /// <remarks>
        /// В случае множественного значения возвращается первое
        /// </remarks>
        string this[string key]
        {
            get;
        }

        /// <summary>
        /// Возвращает коллекцию всех значений ключа
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Значение</returns>
        string[] AsArray(string key);

        /// <summary>
        /// Возвращает целочисленное значение ключа
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Значение</returns>
        int GetInt(string key);        
        
        /// <summary>
        /// Возвращает значение ключа плавающей точкой повышенной точности
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Значение</returns>
        double GetDouble(string key);

        /// <summary>
        /// Возвращает логическое значение ключа
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Значение</returns>
        /// <remarks>
        /// За истину принимаются значения
        /// 1, yes, true, on
        /// </remarks>
        bool GetBool(string key);

        /// <summary>
        /// Возвращает наличие указанного ключа
        /// </summary>
        /// <param name="Key">Имя ключа</param>
        /// <returns>Флаг наличия ключа</returns>
        bool HasKey(string Key);

        /// <summary>
        /// Возвращает уникальный код идентифицирующий содержимое раздела
        /// </summary>
        /// <remarks>
        /// Изменение порядка следования ключей в секции не приводит к изменению отпечатка.
        /// Изменение порядка следования значений в ключе приводит к изменению отпечатка.
        /// </remarks>
        int GetHashCode();

        /// <summary>
        /// Доступ к секции с настраиваемым модификатором ключа
        /// </summary>
        /// <param name="Format">Форматер значения ключа</param>
        /// <returns>Секция с настраиваемым модификатором ключа</returns>
        IConfigSection GetPrefixed(Func<string, string> Format);

        /// <summary>
        /// Возвращает коллекцию всех слов значения
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Коллекция слов значения</returns>
        string[] AsWordsArray(string key);
    }
}
