using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ValueModel.BaseModelCommand
{
   public class SendKBool : VarSendClass
    {
     /*  protected override void SetBinding(string varObmen, string varID)
        {
            //для комманд не устанавливаем
        }*/
      public SendKBool(string VarID)
            : base(VarID)
        {
        }
        protected override void SendValue(object PARAM)
        {
            VHCommands.ComBoolK(this.VarID, this.VarName, PARAM);

        }
    }
}
