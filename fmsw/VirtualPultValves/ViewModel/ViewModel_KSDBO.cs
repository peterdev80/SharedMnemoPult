using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_KSDBO:ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue EUPR { get; private set; }
        public BoolValue OTKR { get; private set; }
        public BoolValue ZAKR { get; private set; }
        public ViewModel_KSDBO()
        {
            repos = ModelVariableRepository.Instance;
            EUPR = repos.BitValues[0].ValState[2];
            OTKR = repos.BitValues[0].ValState[1];
            ZAKR = repos.BitValues[0].ValState[0];
        }

        #region Command
        private RelayCommand cmdeupr, cmdotkr, cmdzakr;
        public ICommand CmdEUPR
        {
            get
            {
                if (cmdeupr == null)
                    cmdeupr = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(2);
                        LinkInpu.Instance.SetSendVar(true, 2, 0);
                    });
                return cmdeupr;
            }
        }
        public ICommand CmdOTKR
        {
            get
            {
                if (cmdotkr == null)
                    cmdotkr = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(1); 
                        LinkInpu.Instance.SetSendVar(true, 1, 0);
                    });
                return cmdotkr;
            }
        }
        public ICommand CmdZAKR
        {
            get
            {
                if (cmdzakr == null)
                    cmdzakr = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(0);
                        LinkInpu.Instance.SetSendVar(true, 0, 0);
                    });
                return cmdzakr;
            }
        }
        #endregion
    }
}
