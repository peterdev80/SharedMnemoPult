using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ValueModel.BaseModelCommand
{
    public class SendKVar : VarSendClass
    {
      /*  protected override void SetBinding(string varObmen, string varID)
        {
            //для комманд не устанавливаем
        }*/
        public SendKVar(string VarID)
            : base(VarID)
        {
        }
        protected override void SendValue(object PARAM)
        {
            VHCommands.ComComK(this.VarID, this.VarName, PARAM);

        }
    }
}
