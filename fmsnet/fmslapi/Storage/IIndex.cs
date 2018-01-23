using System;
using System.Collections.Generic;

namespace fmslapi.Storage
{
    /// <summary>
    /// Интерфейс доступа к индексу хранилища
    /// </summary>
    public interface IIndex
    {
        /// <summary>
        /// Возвращает полное содержимое индекса в бинарной форме
        /// </summary>
        byte[] GetContent();

        /// <summary>
        /// Асинхронно возвращает полное содержимое индекса в бинарной форме
        /// </summary>
        /// <param name="Callback">Метод, вызываемый по завершению извлечения данных</param>
        void GetContent(Action<IIndex, byte[]> Callback);

        /// <summary>
        /// Возвращает интерфейс доступа к ключу в этом индексе
        /// </summary>
        /// <param name="Key">Ключ</param>
        IKey GetKey(byte[] Key);

        /// <summary>
        /// Возвращает интерфейс доступа к ключу в этом индексе
        /// </summary>
        /// <param name="Key">Ключ</param>
        IKey GetKey(string Key);

        /// <summary>
        /// Удаляет ключ из индекса
        /// </summary>
        /// <param name="Key">Удаляемый ключ</param>
        void Remove(IKey Key);

        /// <summary>
        /// Возвращает список всех ключей индекса
        /// </summary>
        IList<IKey> GetKeys();
    }
}
