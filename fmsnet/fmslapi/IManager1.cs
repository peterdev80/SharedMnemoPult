using System;
using System.Collections.Generic;
using System.Windows.Forms;
using fmslapi.Channel;

namespace fmslapi
{
    /// <summary>
    /// Фиксированный интерфейс IManager версии 1
    /// </summary>
    public interface IManager1
    {
        /// <summary>
        /// Осуществляет подключение к каналу
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <returns>Интерфейс канала</returns>
        /// <remarks>
        /// События канала могут быть (и будут!) вызваны в контексте потока, отличном от
        /// потока осуществившего подключение
        /// </remarks>
        IChannel1 JoinChannel(string Channel);

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
        IChannel1 JoinChannel(string Channel, DataReceived Received); 

        /// <summary>
        /// Локальный административный канал
        /// </summary>
        IChannel1 AdmLocChannel { get; }

        /// <summary>
        /// Запускает группу задач в домене
        /// </summary>
        /// <param name="Name"></param>
        void StartTasksGroup(string Name);

        /// <summary>
        /// Отображает всплывающее сообщение в трее
        /// </summary>
        /// <param name="Duration">Длительность отображения</param>
        /// <param name="Caption">Заголовок сообщения</param>
        /// <param name="Text">Текст сообщения</param>
        /// <param name="Icon">Иконка сообщения</param>
        /// <param name="Force">Игнорировать тихий режим, заданный в конфигурации</param>
        void ShowBalloonTip(TimeSpan Duration, string Caption, string Text, ToolTipIcon Icon, bool Force = false);

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

        int StrapAPIVersion { get; }
        
        int ClientAPIVersion { get; }

        /// <summary>
        /// Список хостов в домене
        /// </summary>
        /// <returns></returns>
        IList<string> GetDomainHostsNames();

        string DomainName { get; }

        string VersionString { get; }
    }
}
