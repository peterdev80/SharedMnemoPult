using System;
using fmslapi.Channel;
using fmslapi.Storage;
using System.Windows.Forms;
using fmslapi.Tasks;
using System.Collections.Generic;

namespace fmslapi
{
    /// <summary>
    /// Интерфейс, представляющий канал передачи данных
    /// </summary>
    public interface IManager : IDisposable
    {
        #region Локальный администратор
        /// <summary>
        /// Флаг, означающий недопустимость отложенного подключения ко всем последующим
        /// каналам
        /// </summary>
        bool HardConnectionCheck { get; set; }

        /// <summary>
        /// Событие происходит при перезагрузке конфигурации
        /// </summary>
        event Action OnConfigReload;
        #endregion

        #region Подключение к обычному каналу
        /// <summary>
        /// Осуществляет подключение к каналу
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Делегат приема данных</param>
        /// <param name="Changed">Делегат изменения состояния подключения канала</param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IChannel JoinChannel(string Channel, DataReceived Received, ChannelStateChanged Changed);

        /// <summary>
        /// Осуществляет подключение к каналу
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Делегат приема данных</param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IChannel JoinChannel(string Channel, DataReceived Received);        
        
        /// <summary>
        /// Осуществляет подключение к каналу
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Делегат приема данных</param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IChannel SafeJoinChannel(string Channel, DataReceived Received);

        /// <summary>
        /// Осуществляет подключение к каналу
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Received">Делегат приема данных</param>
        /// <param name="Changed">Делегат изменения состояния подключения канала</param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IChannel SafeJoinChannel(string Channel, DataReceived Received, ChannelStateChanged Changed);
        #endregion

        #region Подключение к каналу переменных

        /// <summary>
        /// Подключение к каналу обмена переменными
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="VarMap">Имя карты переменных</param>
        /// <param name="StateChanged">Делегат изменения состояния подключения канала</param>
        /// <param name="Changed"></param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IVariablesChannel JoinVariablesChannel(string Channel, string VarMap, ChannelStateChanged StateChanged, VariablesChanged Changed);

        /// <summary>
        /// Подключение к каналу обмена переменными
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="EndPoint">Конечная точка канала</param>
        /// <param name="VarMap">Имя карты переменных</param>
        /// <param name="StateChanged">Делегат изменения состояния подключения канала</param>
        /// <param name="Changed"></param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IVariablesChannel JoinVariablesChannel(string Channel, string EndPoint, string VarMap, ChannelStateChanged StateChanged, VariablesChanged Changed);

        /// <summary>
        /// Подключение к каналу обмена переменными
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="VarMap">Имя карты переменных</param>
        /// <param name="StateChanged">Делегат изменения состояния подключения канала</param>
        /// <param name="Changed"></param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IVariablesChannel SafeJoinVariablesChannel(string Channel, string VarMap, ChannelStateChanged StateChanged, VariablesChanged Changed);

        /// <summary>
        /// Подключение к каналу обмена переменными
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="EndPoint">Конечная точка канала</param>
        /// <param name="VarMap">Имя карты переменных</param>
        /// <param name="StateChanged">Делегат изменения состояния подключения канала</param>
        /// <param name="Changed"></param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IVariablesChannel SafeJoinVariablesChannel(string Channel, string EndPoint, string VarMap, ChannelStateChanged StateChanged, VariablesChanged Changed);
        #endregion

        #region Конфигурация
        /// <summary>
        /// Возвращает конфигурационную секцию
        /// </summary>
        /// <param name="SectionName">Имя секции</param>
        /// <returns>Конфигурационная секция</returns>
        IConfigSection GetSection(string SectionName);

        /// <summary>
        /// Возвращает конфигурационную секцию по умолчанию, заданную для 
        /// текущего запущенного экземпляра компонента
        /// </summary>
        IConfigSection DefaultSection { get; }

        /// <summary>
        /// Событие, происходящее при невозможности соединения с обменом 
        /// для чтения конфигурационных данных
        /// </summary>
        event Func<Action[]> OnFMSLDRNotAvailable;
        #endregion

        /// <summary>
        /// Локальный административный канал
        /// </summary>
        IChannel AdmLocChannel { get; }

        #region Постоянное хранилище
        IPersistStorage PersistStorage { get; }
        #endregion

        #region Управление задачами
        /// <summary>
        /// Запускает локальную задачу
        /// </summary>
        /// <param name="Name">Имя задачи</param>
        ITask StartLocalTask(string Name);

        /// <summary>
        /// Запускает группу задач в домене
        /// </summary>
        /// <param name="Name"></param>
        void StartTasksGroup(string Name);
        #endregion

        #region Всплывающие оповещения

        /// <summary>
        /// Отображает всплывающее сообщение в трее
        /// </summary>
        /// <param name="Duration">Длительность отображения</param>
        /// <param name="Caption">Заголовок сообщения</param>
        /// <param name="Text">Текст сообщения</param>
        /// <param name="Icon">Иконка сообщения</param>
        /// <param name="Force">Игнорировать тихий режим, заданный в конфигурации</param>
        void ShowBalloonTip(TimeSpan Duration, string Caption, string Text, ToolTipIcon Icon, bool Force = false);
        #endregion

        #region Журналирование
        /// <summary>
        /// Запись строки в журнал
        /// </summary>
        /// <param name="Log">Строка журнала</param>
        void WriteLogLine(string Log);

        /// <summary>
        /// Запись строки в журнал
        /// </summary>
        /// <param name="Category">Категория записи</param>
        /// <param name="Log">Строка журнала</param>
        void WriteLogLine(string Category, string Log);

        #endregion

        #region Задачи
        /// <summary>
        /// Возвращает список локальных выполняющихся задач
        /// </summary>
        /// <returns>Cписок локальных выполняющихся задач</returns>
        IList<string> GetLocalTaskNames();
        #endregion

        void ShutdownLdr();

        int StrapAPIVersion { get; }

        int ClientAPIVersion { get; }

        /// <summary>
        /// Список хостов в домене
        /// </summary>
        /// <returns></returns>
        IList<string> GetDomainHostsNames();

        /// <summary>
        /// Проверка наличия хоста в канале
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="Host">Имя хоста</param>
        /// <returns>Признак наличия хоста в канале</returns>
        bool IsHostInChannel(string Channel, string Host);

        /// <summary>
        /// Имя домена, в котором выполняется окружение
        /// </summary>
        string DomainName { get; }
    }

    internal interface IInternalManager
    {
    }
}
