using System;

namespace fmslapi.Tasks
{
    public interface ITask
    {
        /// <summary>
        /// Добавляет пользовательскую команду в меню fmsldr задачи
        /// </summary>
        /// <param name="Caption"></param>
        /// <param name="Callback"></param>
        void AddCommand(string Caption, Action Callback);
    }
}
