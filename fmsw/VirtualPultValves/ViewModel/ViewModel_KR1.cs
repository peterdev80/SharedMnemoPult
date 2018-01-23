using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
  public  class ViewModel_KR1: ViewModelBase
    {
      private ModelVariableRepository repos;
      public BoolValue Xsa1 { get; private set; }
      public BoolValue Xsa2 { get; private set; }
    

      public ViewModel_KR1()
      {
          repos = ModelVariableRepository.Instance;
          Xsa1 = repos.BitValues[2].ValState[4];
          Xsa2 = repos.BitValues[2].ValState[5];
         
      }

        #region Command
      private RelayCommand cmdmax, cmdmin, cmdvykl;
      public ICommand CmdMax
      {
          get
          {
              if (cmdmax == null)
                  cmdmax = new RelayCommand(param => {
                      // repos.KomValues[3].SendCommand.Execute(7);
                      LinkInpu.Instance.SetSendVar(true, 7, 3);
                  });
              return cmdmax;
          }

      }
      public ICommand CmdMin
      {
          get
          {
              if (cmdmin == null)
                  cmdmin = new RelayCommand(param => {
                      //repos.KomValues[3].SendCommand.Execute(8);
                      LinkInpu.Instance.SetSendVar(true, 8, 3);
                  });
              return cmdmin;
          }
      }
      public ICommand CmdVykl
      {
          get
          {
              if (cmdvykl == null)
                  cmdvykl = new RelayCommand(param => {
                      // repos.KomValues[3].SendCommand.Execute(9); 
                      LinkInpu.Instance.SetSendVar(true, 9, 3);
                  });
              return cmdvykl;
          }
      }

        #endregion
    }
}
