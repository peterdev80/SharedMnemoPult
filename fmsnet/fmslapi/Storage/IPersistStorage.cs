namespace fmslapi.Storage
{
    /// <summary>
    /// Обмен данными с постоянным хранилищем
    /// </summary>
    public interface IPersistStorage
    {
        /// <summary>
        /// Сохраняет объект в постоянном хранилище
        /// </summary>
        /// <param name="Key">Ключ</param>
        /// <param name="Value">Сохраняемый объект</param>
        void Store(string Key, byte[] Value);

        /// <summary>
        /// Извлекает объект из хранилища
        /// </summary>
        /// <param name="Key">Ключ</param>
        /// <returns>Извлеченный объект</returns>
        byte[] Get(string Key);
        
        /// <summary>
        /// Возвращает интерфейс доступа к объекту в хранилище
        /// </summary>
        /// <param name="Key">Ключ</param>
        /// <param name="Index">Ключ индекса</param>
        IKey GetKey(byte[] Key, byte[] Index = null);

        /// <summary>
        /// Возвращает интерфейс доступа к объекту в хранилище
        /// </summary>
        /// <param name="Key">Ключ</param>
        /// <param name="Index">Ключ индекса</param>
        IKey GetKey(string Key, string Index = null);

        /// <summary>
        /// Возвращает интерфейс доступа к индексу хранилища
        /// </summary>
        /// <param name="Index">Ключ индекса</param>
        IIndex GetIndex(byte[] Index);

        /// <summary>
        /// Возвращает интерфейс доступа к индексу хранилища
        /// </summary>
        /// <param name="Index">Ключ индекса</param>
        IIndex GetUniqueContentIndex(byte[] Index);

        /// <summary>
        /// Возвращает интерфейс доступа к индексу хранилища
        /// </summary>
        /// <param name="Index">Ключ индекса</param>
        IIndex GetIndex(string Index);

        /// <summary>
        /// Возвращает интерфейс доступа к индексу хранилища
        /// </summary>
        /// <param name="Index">Ключ индекса</param>
        IIndex GetUniqueContentIndex(string Index);
    }
}
