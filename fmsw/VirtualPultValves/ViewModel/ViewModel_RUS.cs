using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using EWTM.Model;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_RUS:ViewModelBase
    {
        private ModelVariableRepository repos;
     //   public ICommand CmdRus { get; private set; }

        public ViewModel_RUS()
        {
            repos = ModelVariableRepository.Instance;
           // CmdRus = repos.KomValues[5].SendCommand;
        }

        private RelayCommand cmd;
        public ICommand CmdRus
        {
            get
            {
                if (cmd == null)
                    cmd = new RelayCommand(param => cmdSend(param));
                return cmd;
            }
        }
        private void cmdSend(object param)
        {
            //  repos.KomValues[1].SendCommand.Execute(param);
            //LinkInpu.Instance.SetSendVar((int)param, 5);
            LinkInpu.Instance.SetSendVar(Convert.ToInt32(param), 5);
        }

    }
}
