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
   public class ViewModel_ValvesfA:ViewModelBase
   {
       private ModelVariableRepository repos;
       public BoolValue BOSU { get; private set; }
       public BoolValue OBSU { get; private set; }
       public BoolValue KSDSU { get; private set;}

       public ViewModel_ValvesfA()
       {
           repos = ModelVariableRepository.Instance;
           BOSU = repos.BitValues[0].ValState[16];
           OBSU = repos.BitValues[0].ValState[17];
           KSDSU = repos.BitValues[1].ValState[19];
       }
       #region Command
       private RelayCommand cmdbosuO, cmdbosuC, cmdobsuO, cmdobsuC;
       private RelayCommand cmdksdsuo, cmdksdsuc;
       public ICommand CmdBoSuO
       {
           get
           {
                if (cmdbosuO == null)
                    cmdbosuO = new RelayCommand(p =>
                     //repos.KomValues[0].SendCommand.Execute(18));
                     LinkInpu.Instance.SetSendVar(true, 18, 0));
               return cmdbosuO;
           }
       }

       public ICommand CmdBoSUC
       {
           get
           {
                if (cmdbosuC == null)
                    cmdbosuC = new RelayCommand(p =>
                     //repos.KomValues[0].SendCommand.Execute(19));
                     LinkInpu.Instance.SetSendVar(true, 19, 0));
               return cmdbosuC;
           }
       }

       public ICommand CmdObSuO
       {
           get
           {
               if (cmdobsuO == null)
                   cmdobsuO = new RelayCommand(p =>
                    //repos.KomValues[2].SendCommand.Execute(10)
                    LinkInpu.Instance.SetSendVar(true,10,2));
               return cmdobsuO;
           }
       }

       public ICommand CmdObSuC
       {
           get
           {
               if (cmdobsuC == null)
                   cmdobsuC = new RelayCommand(p =>
                    //repos.KomValues[2].SendCommand.Execute(11)
                    LinkInpu.Instance.SetSendVar(true,11,2));
               return cmdobsuC;
           }
       }

       public ICommand CmdKsdSuO
       {
           get
           {
               if (cmdksdsuo == null)
                   cmdksdsuo = new RelayCommand(p =>
                   //repos.KomValues[2].SendCommand.Execute(6)
                    LinkInpu.Instance.SetSendVar(true,6,2));
               return cmdksdsuo;
           }

       }

       public ICommand CmdKsdSuC
       {
           get
           {
                if (cmdksdsuc == null)
                    cmdksdsuc = new RelayCommand(p =>
                     //repos.KomValues[2].SendCommand.Execute(7)
                     LinkInpu.Instance.SetSendVar(true, 7, 2)
                   );
               return cmdksdsuc;
           }
       }
       #endregion
   }
}
