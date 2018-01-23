using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Glue
{
    /// <summary>
    /// Поддержка связи с задачами на стороне сервера
    /// </summary>
    public class CallbackGlue : GlueBase, fmsldr.IAppDomGlue
    {
        #region Частные данные
        private Action<object, object> _m10;
        #endregion

        public Delegate GetMethod(int N)
        {
            throw new InvalidOperationException();
        }

        void fmsldr.IAppDomGlue.SetMethod(int N, Delegate Method)
        {
            switch (N)
            {
                case 10: _m10 = Method as Action<object, object>; break;

                default:
                    throw new ArgumentException();
            }
        }

        #region Поддержка виртуальных каналов
        public VirtualChannel CreateVirtualChannel(VirtualChannel Glue)
        {
            var vc = new VirtualChannel();

            Debug.Assert(_m10 != null);

            _m10(Glue, vc);

            return vc;
        }
        #endregion
    }
}
