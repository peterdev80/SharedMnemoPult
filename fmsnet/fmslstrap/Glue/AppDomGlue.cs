using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Diagnostics;

namespace Glue
{
    /// <summary>
    /// Связка с управлением задачами
    /// </summary>
    public class AppDomGlue : GlueBase, fmsldr.IAppDomGlue
    {
        #region События
        public event Func<string, byte[][]> OnGetAssembly;
        #endregion

        #region Публичные методы
        /// <summary>
        /// Запрос сборки
        /// </summary>
        /// <param name="Name">Имя сборки</param>
        /// <returns>Тело сборки</returns>
        public byte[][] GetAssembly(string Name)
        {
            return OnGetAssembly(Name);
        }

        /// <summary>
        /// Запуск сборки
        /// </summary>
        /// <param name="AssemblyName">Имя сборки</param>
        /// <param name="StartMethod">Тип, используемый для запуска</param>
        /// <param name="StartType">Метод типа, используемый для запуска</param>
        /// <returns>Сообщение, описывающее результат выполнения</returns>
        public string LaunchAssembly(string AssemblyName, string StartType, string StartMethod, bool WithIManager, IDictionary<string, object> Vals)
        {
            try
            {
                var asm = Assembly.Load(AssemblyName);

                var ep = asm.EntryPoint;

                // Передача в задачу данных
                if (WithIManager)
                {
                    Vals["AppDomGlue"] = _cb;
                    Vals["AppDomCBGlue"] = this;

                    var lapi = Type.GetType("fmslapi.Manager, fmslapi").GetMethod("GetAPI", BindingFlags.Static | BindingFlags.NonPublic);
                    var imant = Type.GetType("fmslapi.IManager, fmslapi");
                    var iman = lapi.Invoke(null, new object[] { Vals });

                    var im = new[] { iman };

                    if (ep == null || (!string.IsNullOrWhiteSpace(StartType) && !string.IsNullOrWhiteSpace(StartMethod)))
                        ep = asm.GetType(StartType).GetMethod(StartMethod, new[] { imant });

                    ep.Invoke(null, im);

                    var id = iman as IDisposable;

                    if (id != null)
                        id.Dispose();
                }
                else
                {
                    if (ep == null || (!string.IsNullOrWhiteSpace(StartType) && !string.IsNullOrWhiteSpace(StartMethod)))
                        ep = asm.GetType(StartType).GetMethod(StartMethod, Type.EmptyTypes);

                    ep.Invoke(null, null);
                }

                return null;
            }
            catch (Exception ex)
            {
                var re = ex;

                var msg = "";

                while (re != null)
                {
                    msg += re.GetType().Name + " at\r\n" + re.StackTrace + "\r\n";
                    msg += re.GetType().Name + "\r\n";
                    msg += re.Message;

                    re = re.InnerException;
                    if (re != null)
                        msg += "\r\n---------------------\r\n";
                }

                return msg;
            }
        }

        public static event Action OnUnloadProcess;

        /// <summary>
        /// Извещение выполняемой сборки о завершении работы
        /// </summary>
        public void RaiseUnloadDomain()
        {
            if (OnUnloadProcess != null)
                OnUnloadProcess();

            try
            {
                var lapi = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("fmslapi"));
                if (lapi != null)
                    lapi.GetType("fmslapi.Manager").InvokeMember("RaiseOnUnloadProcess", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, null);
            }
            catch (RemotingException) { }
        }
        #endregion

        private CallbackGlue _cb;

        Delegate fmsldr.IAppDomGlue.GetMethod(int N)
        {
            switch (N)
            {
                case 0: return new Action<fmsldr.IAppDomGlue>(P);
                case 1: return new Action(RaiseUnloadDomain);
                case 2: return new Func<string, string, string, bool, IDictionary<string, object>, string>(LaunchAssembly);

                default:
                    throw new ArgumentException();
            }
        }

        void fmsldr.IAppDomGlue.SetMethod(int N, Delegate Mehtod)
        {
            throw new InvalidOperationException();
        }

        private void P(fmsldr.IAppDomGlue CB)
        {
            _cb = CB as CallbackGlue;
        }

        public Action<byte[]> CreateVirtualChannel(Action<byte[]> Receive, Action Closed)
        {
            var cg = new VirtualChannel();               // На стороне клиента
            var lg = _cb.CreateVirtualChannel(cg);       // На стороне сервера

            cg.Received += Receive;
            cg.Closed += Closed;

            return lg.Send;
        }
    }
}
