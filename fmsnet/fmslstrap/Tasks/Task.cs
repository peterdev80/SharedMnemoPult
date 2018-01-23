using System;
using System.Collections.Generic;
using System.Linq;
using fmslstrap.Configuration;
using fmslstrap.Administrator;
using fmslstrap.Interface;

namespace fmslstrap.Tasks
{
    /// <summary>
    /// Базовый класс задач
    /// </summary>
    internal abstract class Task
    {
        #region Частные данные
        protected MenuItem _rootmenuitem;
        #endregion

        #region Публичные данные
        public string Title { get; set; }

        public Action<Task> OnTaskClosed;

        public MenuItem RootMenuItem
        {
            get { return _rootmenuitem; }
            set { _rootmenuitem = value; }
        }
        #endregion

        #region Инициализация
        public static Task InitTask(string TaskName, ConfigSection TaskConfig, AdmChannel AdmChan)
        {
            var mode = TaskConfig["mode"].Value.ToLower();
            var title = TaskConfig["title"].Value;

            if (mode == "appdomain")
                return new AppDomainTask(TaskName, TaskConfig, AdmChan) { Title = title };

            if (mode == "process")
                return new SeparateProcessTask(TaskConfig) { Title = title };

            if (mode == "shell")
                return new ShellExecTask(TaskConfig) { Title = title };

            return null;
        }
        #endregion

        #region Публичные вспомогательные методы
        public static IEnumerable<string> GetValues(IEnumerable<string> Values)
        {
            return from l in Values
                   let le = l.Split(' ')
                   from ll in le
                   let ple = ll.Trim()
                   where !string.IsNullOrEmpty(ple)
                   select ple;
        }
        #endregion

        #region Абстрактные методы
        /// <summary>
        /// Запуск задачи на выполнение
        /// </summary>
        public abstract void StartTask();

        /// <summary>
        /// Останов задачи
        /// </summary>
        public abstract void StopTask();
        #endregion
    }
}
