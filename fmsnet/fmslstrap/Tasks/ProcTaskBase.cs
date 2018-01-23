using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using fmslstrap.Interface;

namespace fmslstrap.Tasks
{
    /// <summary>
    /// Задачи в отдельном процессе
    /// </summary>
    internal abstract class ProcTaskBase : Task
    {
        #region Частные данные
        protected ProcessStartInfo _psi;
        protected Process Proc;
        #endregion

        #region Управление задачей
        public override void StartTask()
        {
            try
            {
                Proc = Process.Start(_psi);
            }
            catch (Win32Exception ex)
            {
                InterfaceManager.ShowBalloonTip(2500, "Ошибка запуска задачи " + Title, ex.Message, ToolTipIcon.Error, true);

                Proc = null;

                OnTaskClosed?.Invoke(this);

                return;
            }

            Debug.Assert(Proc != null, "Proc != null");

            Proc.Exited += (s, e) => { OnTaskClosed?.Invoke(this); };
            Proc.EnableRaisingEvents = true;
        }

        public override void StopTask()
        {
            Proc.Kill();

            OnTaskClosed?.Invoke(this);
        }
        #endregion
    }
}
