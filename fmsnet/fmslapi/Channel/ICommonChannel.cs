using System;

namespace fmslapi.Channel
{
    /// <summary>
    /// Общее управление каналом
    /// </summary>
    public interface ICommonChannel
    {
        /// <summary>
        /// Покидает канал
        /// </summary>
        /// <remarks>
        /// После использования этого метода использование любых методов этого
        /// интерфейса будет приводить к исключению
        /// </remarks>
        void Leave();

        void CheckConnect();

        IManager ParentAPIManager { get; }

        /// <summary>
        /// Вызывает делегат
        /// </summary>
        /// <param name="Target">Делегат</param>
        /// <param name="pars">Параметры</param>
        void RaiseDelegate(Delegate Target, params object[] pars);

        /// <summary>
        /// Имя канала
        /// </summary>
        string Name { get; }
    }
}
