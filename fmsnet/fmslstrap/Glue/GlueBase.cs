using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Lifetime;
using System.Diagnostics;

namespace Glue
{
    public abstract class GlueBase : MarshalByRefObject
    {
        #region Время жизни связующего объекта
#if DEBUG
        [DebuggerStepThrough]
#endif
        public override Object InitializeLifetimeService()
        {
            var lease = (ILease)base.InitializeLifetimeService();

            if (lease.CurrentState == LeaseState.Initial)
            {
                Debug.WriteLine(string.Format("{0}: AppDomGlue. Lifetime service initialized in domain \"{1}\"", DateTime.Now, AppDomain.CurrentDomain.FriendlyName));

                lease.InitialLeaseTime = TimeSpan.Zero;
            }
            return lease;
        }
        #endregion
    }
}
