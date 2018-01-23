using System;
using System.Windows.Forms;
using System.Windows.Threading;

namespace fmslapi.Channel
{
    internal class ThreadSafeChannel : Channel
    {
        /// <summary>
        /// Делегат обратного вызова для обеспечения потокобезопасности при вызове событий
        /// </summary>
        /// <param name="Target">Вызываемый делегат</param>
        /// <param name="pars">Параметры вызываемого делегата</param>
        private delegate void ThreadSafeExecutor(Delegate Target, object[] pars);

        private readonly ThreadSafeExecutor _tse;

        public ThreadSafeChannel(ChannelParams p)
            : base(p)
        {
            _tse = DelegateInvoker;
        }

        public override void RaiseDelegate(Delegate Target, params object[] pars)
        {
            var sq = _syncqueue;

            if (sq != null)
                lock (sq)
                {
                    sq.Enqueue(pars);

                    return;
                }

            if (Target == null)
                return;

            if (_tse != null)
                _tse(Target, pars);
            else
                Target.DynamicInvoke(pars);
        }

        /// <summary>
        /// Стандартный потокобезопасный вызов делегатов
        /// </summary>
        /// <param name="Target">Вызываемый делегат</param>
        /// <param name="pars">Параметры вызываемого делегата</param>
        /// <remarks>
        /// Для объектов WinForms используется Forms.Invoke
        /// Для объектов WPF используется Dispatcher.Invoke
        /// </remarks>
        private void DelegateInvoker(Delegate Target, object[] pars)
        {
            foreach (var t in Target.GetInvocationList())
            {
                switch (t.Target)
                {
                    case Control tf:                // WinForms
                        if (tf.InvokeRequired)
                            tf.BeginInvoke(t, pars);
                        else
                            t.DynamicInvoke(pars);
                        continue;

                    case DispatcherObject wf:       // WPF
                        wf.Dispatcher.BeginInvoke(t, pars);
                        continue;

                    default:                        // Прямой вызов
                        t.DynamicInvoke(pars);
                        continue;
                }
            }
        }
    }
}
