using System;
using System.IO;
using System.Diagnostics;
using fmslstrap.Configuration;

namespace fmslstrap.Tasks
{
    /// <summary>
    /// Задача выполняемая в отдельном процессе
    /// </summary>
    internal class SeparateProcessTask : ProcTaskBase
    {
        #region Конструкторы
        public SeparateProcessTask(ConfigSection TaskConfig)
        {
            var procn = Path.Combine(Config.CodeBase, TaskConfig["process"].Value);

            _psi = new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, procn))
                {
                    UseShellExecute = false,
                    // ReSharper disable once AssignNullToNotNullAttribute
                    WorkingDirectory = Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(procn))
                };

            var args = TaskConfig["arguments"];

            if (args.IsExists)
                _psi.Arguments = args.Value;
        }
        #endregion
    }
}
