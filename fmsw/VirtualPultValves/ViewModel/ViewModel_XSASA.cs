using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;
using EWTM.Model;

namespace VirtualPultValves.ViewModel
{
   public class ViewModel_XSASA:ViewModelBase
    {
       private ModelVariableRepository repos;
       public BoolValue XSASA1 { get; private set; }
       public BoolValue XSASA2 { get; private set; }
      

       public ViewModel_XSASA()
       {
           repos = ModelVariableRepository.Instance;
           XSASA1 = repos.BitValues[2].ValState[2];
           XSASA2 = repos.BitValues[2].ValState[3];
       }
        #region Command
       private RelayCommand cmdmax, cmdmin, cmdvykl;

       public ICommand CmdMax
       {
           get
           {
                if (cmdmax == null)
                    cmdmax = new RelayCommand(p =>
                       //repos.KomValues[3].SendCommand.Execute(4));
                       LinkInpu.Instance.SetSendVar(true, 4, 3));
               return cmdmax;
           }
       }

       public ICommand CmdMin
       {
           get
           {
                if (cmdmin == null)
                    cmdmin = new RelayCommand(p =>
                     //repos.KomValues[3].SendCommand.Execute(5));
                     LinkInpu.Instance.SetSendVar(true, 5, 3));
               return cmdmin;
           }
       }

       public ICommand CmdVykl
       {
           get
           {
                if (cmdvykl == null)
                    cmdvykl = new RelayCommand(p =>
                     //repos.KomValues[3].SendCommand.Execute(6));
                     LinkInpu.Instance.SetSendVar(true, 6, 3));
               return cmdvykl;
           }
       }
        #endregion
    }
}
