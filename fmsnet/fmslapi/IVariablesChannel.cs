using System;
using System.Collections.Generic;
using fmslapi.Channel;

namespace fmslapi
{
    /// <summary>
    /// Внутренние методы поддержки канала переменных
    /// </summary>
    internal interface IVariablesChannelSupport : ICommonChannel
    {
        /// <summary>
        /// Регистрирует переменную для приема данных
        /// </summary>
        /// <param name="variable">Переменная</param>
        void RegisterVariable(Variable variable);

        /// <summary>
        /// Завершает регистрацию переменных на сервере и разрешает прием данных
        /// </summary>
        /// <param name="variable">Переменная</param>
        void AddChangedVariable(Variable variable);

        /// <summary>
        /// Сохраняет значение переменной в постоянном хранилище
        /// </summary>
        void SavePersistentVariable(IVariable Variable);

        /// <summary>
        /// Сохраняет снимок значений переменных
        /// </summary>
        /// <param name="SnapshotName">Имя снимка</param>
        /// <param name="Variables">Набор переменных для сохранения снимка</param>
        void MakeSnapshot(string SnapshotName, IVariable[] Variables);

        /// <summary>
        /// Восстанавливает значения переменных из снимка
        /// </summary>
        /// <param name="SnapshotName">Имя снимка</param>
        void RestoreSnapshot(string SnapshotName);
    }

    public interface IVariablesChannelExtensions
    {
        void AddRawSubscriber(Delegate Receiver);

        unsafe void SendRawMessage(void* Buffer, UInt32 Length);
    }

    /// <summary>
    /// Представляет канал обмена переменными
    /// </summary>
    public interface IVariablesChannel : ICommonChannel
    {
        #region Методы GetVariable
        /// <summary>
        /// Регистрирует переменную типа String
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IStringVariable GetStringVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа Int32
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IIntVariable GetIntVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа Int64
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        ILongVariable GetLongVariable(string Name);

        /// <summary>
        /// Регистрирует переменную 
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IKVariable GetKVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа Char
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        ICharVariable GetCharVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа Boolean
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IBoolVariable GetBoolVariable(string Name);

        /// <summary>
        /// Регистрирует переменную триггер
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        ITriggerVariable GetTriggerVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа Single
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IFloatVariable GetFloatVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа Double
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IDoubleVariable GetDoubleVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа ByteArray
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IByteArrayVariable GetByteArrayVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа сторожевого таймера
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IWatchDogVariable GetWatchDogVariable(string Name);

        /// <summary>
        /// Регистрирует переменную типа T
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <typeparam name="T">Тип переменной</typeparam>
        /// <returns>Интерфейс переменной</returns>
        T GetVariable<T>(string Name) where T : class, IVariable;

        /// <summary>
        /// Регистрирует переменную без указания типа
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <returns>Интерфейс переменной</returns>
        IVariable GetVariable(string Name);

        /// <summary>
        /// Повторно регистрирует переменную, например после переподключения к fmsldr
        /// </summary>
        /// <param name="Variable">Переменная для повторной регистрации</param>
        void RegetVariable(IVariable Variable);
        #endregion

        /// <summary>
        /// Отсылает изменившиеся переменные в канал
        /// </summary>
        void SendChanges();

        /// <summary>
        /// Отправляет изменившуюся переменную
        /// </summary>
        /// <remarks>
        /// Только если она действительно была изменена
        /// </remarks>
        void SendChanges(IVariable Variable);

        /// <summary>
        /// Отправляет ограниченный пакет изменившихся переменных
        /// </summary>
        /// <param name="Source">
        /// Отправляются только переменные, присутствующие в этом списке
        /// </param>
        void SendChanges(IEnumerable<IVariable> Source);

        /// <summary>
        /// Сброс в неактивное состояние всех сторожевых переменных выбранной таблицы
        /// </summary>
        void ResetWatchDogs();

        /// <summary>
        /// Сброс в неактивное состояние всех сторожевых переменных выбранной таблицы
        /// </summary>
        void ResetWatchDogs(IEnumerable<IWatchDogVariable> Variables);

        /// <summary>
        /// Создает снимок состояния набора переменных
        /// </summary>
        /// <param name="SnapshotName">Имя снимка значений набора переменных</param>
        /// <returns>Объект снимка</returns>
        VariablesSnapshot CreateSnapshot(string SnapshotName);

        /// <summary>
        /// Создает снимок состояния набора переменных
        /// </summary>
        /// <param name="SnapshotName">Имя снимка значений набора переменных</param>
        /// <param name="Source">Набор переменных в снимке</param>
        /// <returns>Объект снимка</returns>
        VariablesSnapshot CreateSnapshot(string SnapshotName, IEnumerable<IVariable> Source);
    }
}
