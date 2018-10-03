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
   public class ViewModel_Lyk:ViewModelBase
    {
       private ModelVariableRepository repos;
       public BoolValue Stopor { get; private set; }
       public BoolValue Klapan { get; private set; }
       public BoolValue Lyk { get; private set; }

       public ViewModel_Lyk()
       {
           repos = ModelVariableRepository.Instance;
           Stopor = repos.BitValues[0].ValState[30];
           Klapan = repos.BitValues[0].ValState[23];
           Lyk = repos.BitValues[0].ValState[15];
       }
        #region Command
       private RelayCommand cmdStopor, cmdKlapan, cmdLykOpen,cmdLykClose;
       public ICommand CmdStopor
       {
           get
           {
               if (cmdStopor == null)
                   cmdStopor = new RelayCommand(param => SendStopor(param));
               return cmdStopor;
           }
       }
       private void SendStopor(object param)
       {
            if (param.ToString() == "0")
                //repos.KomValues[3].SendCommand.Execute(21);
                LinkInpu.Instance.SetSendVar(true, 21, 3);
            if (param.ToString() == "1")
                //repos.KomValues[3].SendCommand.Execute(22);

                LinkInpu.Instance.SetSendVar(true, 22, 3);
       }

       public ICommand CmdKlapan
       {
           get
           {
               if (cmdKlapan == null)
                   cmdKlapan = new RelayCommand(param => SendKlapan(param));
               return cmdKlapan;
           }
       }
       private void SendKlapan(object param)
       {
            if (param.ToString() == "0")
                //repos.KomValues[2].SendCommand.Execute(2);
                LinkInpu.Instance.SetSendVar(true, 2, 2);
            if (param.ToString() == "1")
                //repos.KomValues[2].SendCommand.Execute(3);
                LinkInpu.Instance.SetSendVar(true, 3, 2);
       }

       public ICommand CmdLykOpen
       {
           get
           {
               if (cmdLykOpen == null)
                   cmdLykOpen = new RelayCommand(param => {
                       //repos.KomValues[0].SendCommand.Execute(16);
                       LinkInpu.Instance.SetSendVar(true, 16, 0);
                   });
               return cmdLykOpen;
           }
       }
       public ICommand CmdLykClose
       {
           get
           {
               if (cmdLykClose == null)
                   cmdLykClose = new RelayCommand(param => {
                       //repos.KomValues[0].SendCommand.Execute(17); 
                       LinkInpu.Instance.SetSendVar(true, 17, 0);
                   });
               return cmdLykClose;
           }
       }

        #endregion


    }
}
