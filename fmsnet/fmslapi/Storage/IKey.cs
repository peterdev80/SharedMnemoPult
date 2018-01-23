using System;

namespace fmslapi.Storage
{
    /// <summary>
    /// Интерфейс доступа к объекту в хранилище
    /// </summary>
    public interface IKey
    {
        /// <summary>
        /// Сохраняет объект в хранилище
        /// </summary>
        /// <param name="Value">Сохраняемый объект</param>
        /// <remarks>
        /// Выполнение блокируется до завершения операции хранилищем
        /// </remarks>
        void Store(byte[] Value);

        /// <summary>
        /// Сохраняет объект в хранилище
        /// </summary>
        /// <param name="Value">Сохраняемый объект</param>
        /// <param name="Sync">Ожидать завершения операции</param>
        void Store(string Value, bool Sync = false);

        /// <summary>
        /// Сохраняет объект в хранилище
        /// </summary>
        /// <param name="Value">Сохраняемый объект</param>
        /// <param name="Sync">Ожидать завершения операции</param>
        void Store<T>(T Value, bool Sync = false);
        
        /// <summary>
        /// Сохраняет объект в хранилище
        /// </summary>
        /// <param name="Value">Сохраняемый объект</param>
        /// <param name="Sync">Ожидать завершения операции</param>
        void Store(byte[] Value, bool Sync);

        /// <summary>
        /// Удаляет объект из хранилища
        /// </summary>
        void Remove();

        /// <summary>
        /// Возвращает объект из хранилища
        /// </summary>
        byte[] Get();

        /// <summary>
        /// Возвращает объект из хранилища
        /// </summary>
        T Get<T>() where T : struct;

        /// <summary>
        /// Асинхронно возвращает объект из хранилища
        /// </summary>
        void Get(Action<IKey, byte[]> Callback);

        /// <summary>
        /// Ключ объекта в двоичном виде
        /// </summary>
        byte[] Key
        {
            get;
        }
    }
}
