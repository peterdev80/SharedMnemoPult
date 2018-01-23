using fmslapi.Bindings.WPF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ValueModel.BaseType;


namespace ValueModel.BaseModelCommand
{
    public class VHCommands
    {
       
        public static void ComBoolK(string varID, string VarName, object PARAM)
        {
            int i = 0;
            bool val = false;
            if (!int.TryParse(PARAM.ToString(), out i)) throw new Exception("В качестве парамет параметра Комманды должен быть инт");
            if (i != 0) val = true;
             // var VaRSend = VHClass.Instance.GetVHPC(varID).VHCurr.GetBoolVariable(VarName);
            //   VaRSend.AutoSend = true;
          //  VaRSend.Value = val;
         //   int pos = StructVarModel.PosElInStruct(varID, VarName);
        //    if (pos != -1) { }

           
            var v = VariablesDataContext.GetNamedContext(varID).VariablesChannel.GetBoolVariable(VarName);
            v.AutoSend = true;
            v.Value = val;
        }

        public static void ComIntK(string varID, string VarName, object PARAM)
        {
            int i = 0;
            if (!int.TryParse(PARAM.ToString(), out i)) throw new Exception("В качестве парамет параметра Комманды должен быть инт");
            /*  var VaRSend = VHClass.Instance.GetVHPC(varID).VHCurr.GetIntVariable(VarName);
              VaRSend.AutoSend = true;
              VaRSend.Value = i;*/
            var v = VariablesDataContext.GetNamedContext(varID).VariablesChannel.GetIntVariable(VarName);
            v.AutoSend = true;
            v.Value = i;
        }
        public static void ComComK(string varID, string VarName, object PARAM)
        {
            int i = 0;
            if (!int.TryParse(PARAM.ToString(), out i)) throw new Exception("В качестве парамет параметра Комманды должен быть инт");
            /*  var VaRSend = VHClass.Instance.GetVHPC(varID).VHCurr.GetKVariable(VarName);
              VaRSend.AutoSend = true;
              VaRSend.Set();*/
            var v = VariablesDataContext.GetNamedContext(varID).VariablesChannel.GetKVariable(VarName);
            v.AutoSend = true;
            v.Set();
        }
        public static void ComBitK(string varID, string VarName, object PARAM, List<BitPosValue> KeyBit)
        {
            int i = 0;
            if (!int.TryParse(PARAM.ToString(), out i)) throw new Exception("В качестве парамет параметра Комманды должен быть инт");
            BitPosValue _val = KeyBit[i];
            /* var VaRSend = VHClass.Instance.GetVHPC(varID).VHCurr.GetIntVariable(VarName);
             VaRSend.AutoSend = true;
             VaRSend.Value = (int)_val;*/
            var v = VariablesDataContext.GetNamedContext(varID).VariablesChannel.GetIntVariable(VarName);
            v.AutoSend = true;
            v.Value = (int)_val;
        }
    }
}
