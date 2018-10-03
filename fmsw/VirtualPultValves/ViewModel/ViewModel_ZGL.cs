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
   public class ViewModel_ZGL:ViewModelBase
    {
       private ModelVariableRepository repos;
       public BoolValue Zgl { get; private set; }

       public ViewModel_ZGL()
       {
           repos = ModelVariableRepository.Instance;
           Zgl = repos.BitValues[1].ValState[20];
       }

        #region Command
       private RelayCommand cmdZgl;

       public ICommand CmdZGL
       {
           get
           {
               if (cmdZgl == null)
                   cmdZgl = new RelayCommand(p => SendZgl(p));
               return cmdZgl ;
               ;
           }
       }
       private void SendZgl(object p)
       {
            if (p.ToString() == "0")
                LinkInpu.Instance.SetSendVar(true, 8, 2); //repos.KomValues[2].SendCommand.Execute(8);
            if (p.ToString() == "1")
                //repos.KomValues[2].SendCommand.Execute(9);
                LinkInpu.Instance.SetSendVar(true, 9, 2);

       }

     

        #endregion
    }
}
