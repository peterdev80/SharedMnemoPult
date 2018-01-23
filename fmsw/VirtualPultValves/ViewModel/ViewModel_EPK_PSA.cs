using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    class ViewModel_EPK_PSA : ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue OtkrEPKPSA { get; set; }
        public BoolValue ZakrEPKPSA { get; set; }
        public BoolValue MalyiRashod { get; set; }

        public ViewModel_EPK_PSA()
        {
            repos = ModelVariableRepository.Instance;
            OtkrEPKPSA = repos.BitValues[0].ValState[5];
            MalyiRashod = repos.BitValues[0].ValState[4];
            ZakrEPKPSA = repos.BitValues[0].ValState[3];
        }

        #region Command
        private RelayCommand cmdOtk, cmdzakr, cmdmmr;

        public ICommand CmdOtk
        {
            get
            {
                if (cmdOtk == null)
                    cmdOtk = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(5);
                        LinkInpu.Instance.SetSendVar(true, 5, 0);
                    });
                return cmdOtk;
            }
        }

        public ICommand CmdZakr
        {
            get
            {
                if (cmdzakr == null)
                    cmdzakr = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(3); 
                        LinkInpu.Instance.SetSendVar(true, 3, 0);
                    });
                return cmdzakr;
            }
        }

        public ICommand CmdMmr
        {
            get
            {
                if (cmdmmr == null)
                    cmdmmr = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(4);
                        LinkInpu.Instance.SetSendVar(true, 4, 0);
                    });
                return cmdmmr;
            }
        }
        #endregion
    }
}
