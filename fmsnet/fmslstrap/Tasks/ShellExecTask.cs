using System;
using System.Diagnostics;
using fmslstrap.Configuration;

namespace fmslstrap.Tasks
{
    /// <summary>
    /// Задача, запускаемая как команда оболочки
    /// </summary>
    internal class ShellExecTask : ProcTaskBase
    {
        #region Частные данные
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly string _cmd;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly string _cmdpars;
        #endregion

        #region Конструкторы
        public ShellExecTask(ConfigSection TaskConfig)
        {
            _cmd = TaskConfig["cmdline"].Value;
            _cmdpars = TaskConfig["cmdparams"].Value;
            var wstyle = TaskConfig["windowstyle"].Value;

            _psi = new ProcessStartInfo(_cmd, _cmdpars) {UseShellExecute = true};

            ProcessWindowStyle pws;
            if (!Enum.TryParse(wstyle, true, out pws))
                pws = ProcessWindowStyle.Normal;

            _psi.WindowStyle = pws;
        }
        #endregion
    }
}
