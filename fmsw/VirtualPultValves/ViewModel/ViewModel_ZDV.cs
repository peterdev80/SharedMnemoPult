using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
   public class ViewModel_ZDV:ViewModelBase
    {
       private ModelVariableRepository repos;

       public BoolValue ZDV1 { get; private set; }
       public BoolValue ZDV2 { get; private set; }

       public ViewModel_ZDV()
       {
           repos = ModelVariableRepository.Instance;
           ZDV1 = repos.BitValues[2].ValState[0];
           ZDV2 = repos.BitValues[2].ValState[1];
       }

        #region Command
       private RelayCommand cmdzdv1o, cmdzdv1c, cmdzdv2o, cmdzdv2c;
       public ICommand CmdZdv1_Open
       {
           get
           {
                if (cmdzdv1o == null)
                    cmdzdv1o = new RelayCommand(p =>
                       //repos.KomValues[3].SendCommand.Execute(0));
                       LinkInpu.Instance.SetSendVar(true, 0, 3));
               return cmdzdv1o;
           }
       }

       public ICommand CmdZdv1_Close
       {
           get
           {
                if (cmdzdv1c == null)
                    cmdzdv1c = new RelayCommand(p =>
                     //repos.KomValues[3].SendCommand.Execute(1));
                     LinkInpu.Instance.SetSendVar(true, 1, 3));
               return cmdzdv1c;
           }
       }

       public ICommand CmdZdv2_Open
       {
           get
           {
                if (cmdzdv2o == null)
                    cmdzdv2o = new RelayCommand(p =>
                     //repos.KomValues[3].SendCommand.Execute(2));
                     LinkInpu.Instance.SetSendVar(true, 2, 3));
               return cmdzdv2o;
           }
       }

       public ICommand CmdZdv2_Close
       {
           get
           {
                if (cmdzdv2c == null)
                    cmdzdv2c = new RelayCommand(p =>
                     //repos.KomValues[3].SendCommand.Execute(3));
                     LinkInpu.Instance.SetSendVar(true, 3, 3));
               return cmdzdv2c;
           }
       }
        #endregion
    }
}
