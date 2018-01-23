using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using fmslstrap.Administrator;
using fmslstrap.Interface;
using System.Windows.Forms;
using fmslstrap.Configuration;

namespace fmslstrap.Tasks
{
    /// <summary>
    /// Задача выполняемая в отдельном домене приложений в текущем процессе
    /// </summary>
    internal class AppDomainTask : Task
    {
        #region Частные данные
        /// <summary>
        /// Поток, в котором выполняется задача
        /// </summary>
        private readonly Thread _thread;

        /// <summary>
        /// Конфигурационные данные задачи
        /// </summary>
        private readonly ConfigSection _taskconfig;

        /// <summary>
        /// Объект междоменной связи с задачей
        /// </summary>
        private AppDomGlue _glue;

        /// <summary>
        /// Административные канал
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private readonly AdmChannel _admchan;

        /// <summary>
        /// Имя задачи
        /// </summary>
        private readonly string _taskname;
        #endregion

        #region Конструкторы
        internal AppDomainTask(string TaskName, ConfigSection TaskConfig, AdmChannel AdmChan)
        {
            _taskconfig = TaskConfig;
            _admchan = AdmChan;
            _taskname = TaskName;

            _thread = new Thread(PrepareTask);
            _thread.SetApartmentState(ApartmentState.STA);
        }

        private void PrepareTask()
        {
            try
            {
                TryPrepareTask();
            }
            catch (Exception)
            {
                Thread.Sleep(1000);

                try
                {
                    TryPrepareTask();
                }

                catch (Exception ex)
                {
                    Exception x = ex;
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format("Ошибка при подготовке задачи к выполнению: {0}", _taskname));

                    while (x != null)
                    {
                        sb.AppendLine(x.ToString());
                        sb.AppendLine(x.Message);
                        sb.AppendLine(x.StackTrace);
                        sb.AppendLine("-----------------\r\n\r\n\r\n\r\n");
                        x = x.InnerException;
                    }

                    var msg = sb.ToString();

                    try
                    {
                        File.AppendAllText(Environment.ExpandEnvironmentVariables(@"%AllUsersProfile%\FMS700\excpts.txt"), msg);
                    }
                    catch (IOException) { }

                    if (!Debugger.IsAttached)
                        MessageBox.Show(msg);
                    else
                        throw;

                    //InterfaceManager.ShowBalloonTip(2500, "Task Exception in " + assembly, excpt, System.Windows.Forms.ToolTipIcon.Error, true);
                }
            }
        }

        private void TryPrepareTask()
        {
            try
            {
                var assembly = _taskconfig["assembly"].Value;

                Debug.WriteLine($"{DateTime.Now}: StartInitTask assembly={assembly}");

                var appdomain = _taskconfig["appdomain"].Value;
                var deps = GetValues(_taskconfig["dependency"].Values).ToArray();

                _glue = new AppDomGlue(appdomain);

                var st = _taskconfig["starttype"].Value;
                var sm = _taskconfig["startmethod"].Value;

                var wimanager = deps.Contains("fmslapi");

                Debug.WriteLine($"{DateTime.Now}: LaunchTask assembly={assembly}");

                var vals = new Dictionary<string, object>();

                vals["TaskName"] = _taskconfig["componentname"].Value ?? _taskname;

                try
                {
                    Guid.TryParse(_taskconfig["componentid"].Value, out var g);
                    vals["ComponentID"] = g;
                }
                catch (SystemException) { }

                var excpt = _glue.LaunchAssembly(assembly, st, sm, wimanager, vals);
                if (excpt != null)
                {
                    Logger.WriteLine("Tasks", $"Исключение в задаче {_taskname}{Environment.NewLine}{excpt}");
                    InterfaceManager.ShowBalloonTip(2500, "Task Exception in " + assembly, excpt, ToolTipIcon.Error, true);

                    Trace.WriteLine($"AppDomainTask Exception: {excpt}");
                }

                _glue.Close();

                _glue = null;

                OnTaskClosed?.Invoke(this);
            }
            finally
            {
                _glue?.Close();

                _glue = null;
            }
        }


        #endregion

        #region Управление задачей
        public override void StartTask()
        {
            _thread.Start();
        }

        public override void StopTask()
        {
            _glue?.RaiseUnloadDomain();

            /*if (OnTaskClosed != null)
                OnTaskClosed(this);*/
        }
        #endregion
    }
}
