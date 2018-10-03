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
    public class ViewModel_EPKP:ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue EPKPOpen { get; set; }
        public BoolValue EPKPClose { get; set; }

        public ViewModel_EPKP()
        {
            repos = ModelVariableRepository.Instance;
            EPKPOpen = repos.BitValues[0].ValState[10];
            EPKPClose = repos.BitValues[0].ValState[11];
        }

        #region Command
        private RelayCommand cmdopen, cmdclose;
        public ICommand CmdOpen
        {
            get
            {
                if (cmdopen == null)
                    cmdopen = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(12);
                        LinkInpu.Instance.SetSendVar(true, 12, 0);
                    });
                return cmdopen;
            }
        }
        public ICommand CmdClose
        {
            get
            {
                if (cmdclose == null)
                    cmdclose = new RelayCommand(param => {
                        // repos.KomValues[0].SendCommand.Execute(13);
                        LinkInpu.Instance.SetSendVar(true, 13, 0);
                    });
                return cmdclose;
            }
        }

        #endregion 

    }
}
