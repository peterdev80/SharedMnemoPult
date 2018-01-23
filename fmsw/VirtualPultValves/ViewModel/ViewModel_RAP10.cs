using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_RAP10:ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue AVT { get; private set; }
        public BoolValue VIN { get; private set; }

        public ViewModel_RAP10()
        {
            repos = ModelVariableRepository.Instance;
            AVT = repos.BitValues[2].ValState[11];
            VIN = repos.BitValues[2].ValState[12];
        }

        #region Command
        private RelayCommand cmdavt, cmdvin;
        public ICommand CmdAVT
        {
            get
            {
                if (cmdavt == null)
                    cmdavt = new RelayCommand(p => {
                        // repos.KomValues[3].SendCommand.Execute(15);
                        LinkInpu.Instance.SetSendVar(true, 15, 3);
                    });
                return cmdavt;
            }
        }

        public ICommand CmdVIN
        {
            get
            {
                if (cmdvin == null)
                    cmdvin = new RelayCommand(p => {
                        // repos.KomValues[3].SendCommand.Execute(16);
                        LinkInpu.Instance.SetSendVar(true, 16, 3);
                    });
                return cmdvin;
            }
        }
        #endregion
    }
}
