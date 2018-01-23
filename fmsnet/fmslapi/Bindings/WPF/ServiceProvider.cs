using System;
using System.Windows.Markup;
using System.Xaml;

namespace fmslapi.Bindings.WPF
{
    public class ServiceProvider : IServiceProvider, IProvideValueTarget, IRootObjectProvider
    {
        public object TargetProperty { get; internal set; }
        public object TargetObject { get; internal set; }
        public object RootObject { get; internal set; }

        public ServiceProvider(IServiceProvider Source)
        {
            if (Source == null)
                return;

            var ipvt = Source.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            if (ipvt != null)
            {
                TargetObject = ipvt.TargetObject;
                TargetProperty = ipvt.TargetProperty;
            }

            var ro = Source.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;

            if (ro != null)
            {
                RootObject = ro.RootObject;
            }
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
                return this;

            if (serviceType == typeof(IRootObjectProvider))
                return this;

            return null;
        }
    }
}
