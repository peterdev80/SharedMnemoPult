using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ValueModel.BaseModelCommand
{
    public class SendKInt : VarSendClass
    {
       /* protected override void SetBinding(string varObmen, string varID)
        {
            //для комманд не устанавливаем
        }*/
        public SendKInt(string VarID)
            : base(VarID)
        {
        }
        protected override void SendValue(object PARAM)
        {
            VHCommands.ComIntK(this.VarID, this.VarName, PARAM);
        }
    }
}
