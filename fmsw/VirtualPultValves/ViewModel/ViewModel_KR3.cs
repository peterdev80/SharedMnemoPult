using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_KR3:ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue Kr { get; private set; }
        public ViewModel_KR3()
        {
            repos = ModelVariableRepository.Instance;
            Kr = repos.BitValues[1].ValState[17];

        }

        #region Command
        private RelayCommand cmdBo, cmdSa;
        public ICommand CmdBo
        {
            get
            {
                if (cmdBo == null)
                    cmdBo = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(29);
                        LinkInpu.Instance.SetSendVar(true, 29, 0);
                    });
                return cmdBo;
            }
        }
        public ICommand CmdSa
        {
            get
            {
                if (cmdSa == null)
                    cmdSa = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(30);
                        LinkInpu.Instance.SetSendVar(true, 30, 0);
                    });
                return cmdSa;
            }
        }
        #endregion
    }
}
