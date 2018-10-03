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
    public class ViewModel_RAP7:ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue Rap { get; private set; }

        public ViewModel_RAP7()
        {
            repos = ModelVariableRepository.Instance;
            Rap = repos.BitValues[2].ValState[6];
        }

        #region Command
        private RelayCommand cmdrap;

        public ICommand CmdRap
        {
            get
            {
                if (cmdrap == null)
                    cmdrap = new RelayCommand(p => SendRap(p));
                return cmdrap;
            }
        }

        private void SendRap(object p)
        {
            if (p.ToString() == "0")
                //repos.KomValues[3].SendCommand.Execute(10);
                LinkInpu.Instance.SetSendVar(true, 10, 3);
            if (p.ToString() == "1")
                //repos.KomValues[3].SendCommand.Execute(11);
                LinkInpu.Instance.SetSendVar(true, 11, 3);
        }

        #endregion
    }
}
