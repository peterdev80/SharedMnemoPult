using System;
using System.Collections.Generic;
using System.Linq;
using fmslstrap.Configuration;
using System.Threading;
using fmslstrap.Channel;
using System.IO;
using System.Diagnostics;
using fmslstrap.Administrator;
using fmslstrap.Interface;
// ReSharper disable RedundantNameQualifier

namespace fmslstrap.Tasks
{
    /// <summary>
    /// Управление задачами
    /// </summary>
    internal class TasksManager
    {
        #region Частные данные
        private static AdmChannel _admchan;

        private static readonly Dictionary<string, Task> _tasks = new Dictionary<string, Task>();

        // ReSharper disable once UnusedMember.Local
        private static readonly HashSet<Guid> _globalassmeblylist = new HashSet<Guid>();

        private static Interface.MenuItem _tasksmenuitem;
        #endregion

        #region Инициализация
        public static void Init(AdmChannel admchan)
        {
            _tasksmenuitem = new Interface.MenuItem { Caption = "Задачи", IsSubmenu = true };

            InterfaceManager.InsertMenuItem(_tasksmenuitem, 1);
            InterfaceManager.InsertMenuItem(new MenuItemSeparator(), 1);

            _admchan = admchan;

            admchan.RegisterAdmCommand('E', StartGroup);
            admchan.RegisterAdmCommand('F', StopGroup);
            admchan.RegisterAdmCommand('G', StartRemote);
            admchan.RegisterAdmCommand('Y', KillAll);
            admchan.RegisterAdmCommand('M', BeginpStgReplication);

            var astr = ConfigurationManager.GetSection("autostarter");
            var skk = string.Format("start.{0}", Config.WorkstationName).ToLower();

            var l = astr[skk].Values ?? astr["start"].Values;

            if (l != null)
            {
                foreach (var ts in GetValues(l))
                {
                    var tsect = ConfigurationManager.GetSection(string.Format("task.{0}", ts));
                    if (tsect == null)
                        continue;

                    StartTask(ts);
                }
            }

            var sgl = astr["startgroup"].Values;

            if (sgl != null)
                foreach (var sg in GetValues(sgl))
                    StartGroup(sg);
        }
        #endregion

        #region Частные вспомогательные методы
        private static IEnumerable<string> GetValues(IEnumerable<string> Values)
        {
            return from l in Values
                   let le = l.Split(new[] { ' ' })
                   from ll in le
                   let ple = ll.Trim()
                   where !string.IsNullOrEmpty(ple)
                   select ple;
        }
        #endregion

        #region Сетевой обмен

        #region Запуск группы
        private static void StartGroup(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            StartGroup(Reader.ReadString());
        }
        
        private static void StartGroup(string GroupName)
        {
            Logger.WriteLine($"Запуск группы задач {GroupName}");

            var gsect = ConfigurationManager.GetSection("task.groups");
            var tk = $"group.{Config.WorkstationName}.{GroupName}".ToLower();
            if (!gsect.ContainsKey(tk))
            {
                tk = $"group.{GroupName}".ToLower();
                if (!gsect.ContainsKey(tk))
                    return;
            }

            var tasks = GetValues(gsect[tk].Values);

            foreach (var t in tasks)
                StartTask(t);
        }
        #endregion

        #region Останов группы
        private static void StopGroup(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            var stop = Reader.ReadString();
            
            Logger.WriteLine(string.Format("Останов группы задач {0}", stop));
        }
        #endregion

        #region Удаленный запуск задачи
        private static void StartRemote(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            var rhost = Reader.ReadString().ToLower();
            if (rhost != Config.WorkstationName.ToLower() && rhost != "*")
                return;

            var rstart = Reader.ReadString();
            StartTask(rstart);
        }
        #endregion

        #region KILLALL
        private static void KillAll(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            Process.GetCurrentProcess().Kill();
        }
        #endregion

        #endregion

        #region Управление задачами
        /// <summary>
        /// Запуск задачи с указанным именем
        /// </summary>
        /// <param name="Name">Имя задачи</param>
        public static void StartTask(string Name)
        {
            // ReSharper disable once UnusedVariable
            var tt = Name.ToLowerInvariant();

            if (tt == "logconsolewriter")
            {
                LogConsoleWriter.Start();
                return;
            }

            if (tt == "consoleredirector")
            {
                ConsoleRedirector.Start();
                return;
            }

            var tsect = ConfigurationManager.GetSection(string.Format("task.{0}", Name));
            if (tsect == null)
            {
                Logger.WriteLine("tasks", string.Format("Не найдена конфигурация для задачи {0}", Name));

                return;
            }

            var sig = Name;

            var ssig = tsect["signature"];
            if (ssig.IsExists)
                sig = ssig.Value;

            var before = tsect["before"];
            if(before.IsExists)
                CheckBefore(before.Values);

            Task task;
            lock (_tasks)
            {
                if (_tasks.ContainsKey(sig))
                    return;

                Logger.WriteLine("tasks", string.Format("Запуск задачи {0}", Name));

                task = Task.InitTask(Name, tsect, _admchan);
                _tasks.Add(sig, task);
                
            }

            var rmi = new Interface.MenuItem { Caption = task.Title, Parent = _tasksmenuitem, IsSubmenu = true };
            var miclose = new Interface.MenuItem { Caption = "Завершение", Parent = rmi };

            task.RootMenuItem = rmi;

            task.RootMenuItem.Parent.RaiseOnChanged();

            miclose.OnInvoke += () => task.StopTask();

            task.OnTaskClosed += (rt) =>
                {
                    lock (_tasks) { _tasks.Remove(sig); }
                    rmi.Parent = null;

                    Logger.WriteLine("tasks", string.Format("Задача {0} завершена", Name));
                };

            var dly = tsect["delay"].Value;

            if (dly != null)
                Thread.Sleep(int.Parse(dly));

            task.StartTask();
        }

        private static void CheckBefore(IList<String> Conditions)
        {
            foreach (var c in Conditions)
            {
                var cl = c.ToLowerInvariant();
                switch (cl)
                {
                    case "stopalltasks": StopAllTasks(); break;
                }
            }
        }

        private static void StopAllTasks()
        {
            // ReSharper disable once RedundantAssignment
            Task[] sts = null;
            lock (_tasks)
                sts = _tasks.Values.ToArray();

            foreach (var ct in sts)
                ct.StopTask();

            while (true)
            {
                lock (_tasks)
                    if (_tasks.Count == 0)
                        return;

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Заканчивает все задачи
        /// </summary>
        public static void ShutdownAllTasks()
        {
            lock (_tasks)
            {
                foreach (var t in _tasks.ToArray())
                    t.Value.StopTask();
            }
        }
        #endregion

        #region Поддержка репликации постоянного хранилища
        private static void BeginpStgReplication(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            // Запуск клиента репликации, но только если на этом хосте
            // не запущена уже задача постоянного хранилища

            lock (_tasks)
            {
                if (_tasks.Keys.Contains("db"))
                    return;
            }

            StartTask("pstoragereplicationclient");
        }
        #endregion

        public static IList<string> GetExecutingTasks()
        {
            lock (_tasks)
            {
                return _tasks.Select(x => x.Key).ToArray();
            }
        }
    }
}
