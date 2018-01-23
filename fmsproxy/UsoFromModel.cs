using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fmslapi;
using System.IO;

namespace fmsproxy
{
    /// <summary>
    /// Соединение потока данных от модели в УСО пульта
    /// </summary>
    public class UsoFromModel : ModelVarProxy
    {
        public UsoFromModel()
        {
            UsoToModel.SoundOff += soundoff;
        }

        private byte[] _cooffcmd;
        private IBoolVariable _coffvar;

        protected override void CustomPostRegistration(IEnumerable<IVariable> Variables)
        {
            _coffvar = Variables.Where(v => v.VariableName == "__SET_CENTR_OGON").FirstOrDefault() as IBoolVariable;
            if (_coffvar == null)
                return;

            _coffvar.AutoSend = true;
            var colightoffindex = (UInt16)_pvars[_coffvar];

            var rootconf = _config.GetPrefixed(null);
            var forceco = rootconf.GetBool("force.co.switchoff");

            if (!forceco)
                return;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            // Команда выключения ЦО на пульте
            wr.Write((UInt16)15);
            wr.Write((UInt16)colightoffindex);
            wr.Write((byte)0);
            wr.Write((UInt16)2000);
            wr.Write((long)0);

            _cooffcmd = ms.ToArray();
        }

        private void soundoff()
        {
            if (_cooffcmd == null)
                return;

            base.ProcessIncomingUDP(null, _cooffcmd);
        }

        public override void CloseProxy()
        {
            UsoToModel.SoundOff -= soundoff;
            base.CloseProxy();
        }
    }
}
