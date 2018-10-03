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
   public class ViewModel_KVDBO: ViewModelBase
    {
       private ModelVariableRepository repos;
       public BoolValue Otkr { get; private set; }
       public BoolValue Zakr { get; private set; }
       public BoolValue KKCOtkr { get; private set; }
       public BoolValue KKTOtkr { get; private set; }
       public BoolValue KKZakr { get; private set; }

       public ViewModel_KVDBO()
       {
           repos = ModelVariableRepository.Instance;
           Otkr = repos.BitValues[0].ValState[18];
           Zakr = repos.BitValues[0].ValState[19];
           KKCOtkr = repos.BitValues[0].ValState[20];
           KKTOtkr = repos.BitValues[0].ValState[22];
           KKZakr = repos.BitValues[0].ValState[21];

       }
        #region Command
       private RelayCommand cmdotkr, cmdzakr, cmdkks, cmdkkt, cmdkk;
       public ICommand CmdOtkr
       {
           get
           {
               if (cmdotkr == null)
                   cmdotkr = new RelayCommand(param => {

                       // repos.KomValues[0].SendCommand.Execute(20);
                       LinkInpu.Instance.SetSendVar(true, 20, 0);
                   });
               return cmdotkr;
           }
       }
       public ICommand CmdZakr
       {
           get
           {
               if (cmdzakr==null)
                   cmdzakr = new RelayCommand(param => {
                       //repos.KomValues[0].SendCommand.Execute(21);
                       LinkInpu.Instance.SetSendVar(true, 21, 0);
                   });
               return cmdzakr;
           }
       }
       public ICommand CmdKKS
       {
           get
           {
               if (cmdkks==null)
                   cmdkks = new RelayCommand(param => {
                       //repos.KomValues[0].SendCommand.Execute(22); 
                       LinkInpu.Instance.SetSendVar(true, 22, 0);
                   });
               return cmdkks;
           }
       }
       public ICommand CmdsKKT
       {
           get
           {
               if (cmdkkt==null)
                   cmdkkt = new RelayCommand(param => {
                       //repos.KomValues[0].SendCommand.Execute(24);
                       LinkInpu.Instance.SetSendVar(true, 24, 0);
                   });
               return cmdkkt;
           }
       }
       public ICommand CmdKK
       {
           get
           {
               if (cmdkk==null)
                   cmdkk = new RelayCommand(param => {
                       //repos.KomValues[0].SendCommand.Execute(23); 
                       LinkInpu.Instance.SetSendVar(true, 23, 0);
                   });
               return cmdkk;
           }
       }
        #endregion
    }
}
