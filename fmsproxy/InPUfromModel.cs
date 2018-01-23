using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using fmslapi;

namespace fmsproxy
{
    public class InPUfromModel : ModelVarProxy
    {
        private IBoolVariable[] zerolist;
        private IFloatVariable[] fzlist;
        private IDoubleVariable[] fdlist;
        private IIntVariable[] filist;
        private IBoolVariable _bs_real;

        protected override void CustomPostRegistration(IEnumerable<IVariable> Variables)
        {
            _bs_real = _varchan.GetBoolVariable("__BS_REAL");

            zerolist = (from v in Variables
                        where v.VariableType == VariableType.Boolean
                        select v as IBoolVariable).Union(new[] { _bs_real }).ToArray();

            fzlist = (from v in Variables
                        where v.VariableType == VariableType.Single
                        select v as IFloatVariable).ToArray();

            fdlist = (from v in Variables
                      where v.VariableType == VariableType.Double
                      select v as IDoubleVariable).ToArray();
            
            filist = (from v in Variables
                      where v.VariableType == VariableType.Int32
                      select v as IIntVariable).ToArray();
        }

        protected override void IndividualVarProcess(UInt16 index, IVariable Variable, object NewValue, ref bool UndoVar)
        {
            if (Variable.VariableName == "__Var_To_Init")
            {
                foreach (var v in zerolist) v.Value = false;
                foreach (var v in fzlist) v.Value = 0;
                foreach (var v in fdlist) v.Value = 0;
                foreach (var v in filist) v.Value = 0;

                if (_varchan != null)
                    _varchan.SendChanges();
            }

            if (Variable.VariableName == "__SOOB_N_INPU1" || Variable.VariableName == "__SOOB_N_INPU2")
            {
                if ((Int32)NewValue == 5 || (Int32)NewValue == 6)
                    _bs_real.Value = true;

                if ((Int32)NewValue == 0)
                    UndoVar = true;
            }
        }
    }
}
