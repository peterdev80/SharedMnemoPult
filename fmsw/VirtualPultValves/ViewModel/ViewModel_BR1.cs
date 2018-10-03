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
    class ViewModel_BR1 : ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue Otkaz12 { get; set; }
        public BoolValue Otkaz13 { get; set; }
        public BoolValue Norm { get; set; }
        public ViewModel_BR1()
        {
             repos = ModelVariableRepository.Instance;          
            Otkaz12=repos.BitValues[2].ValState[7];
            Otkaz13=repos.BitValues[2].ValState[9];
            Norm = repos.BitValues[2].ValState[8];

        }
        #region Command
        private RelayCommand cmdotkaz12;
        public ICommand CmdOtkaz12
        {
            get
            {
                if (cmdotkaz12 == null)
                {
                    cmdotkaz12 = new RelayCommand(param => CmdOtkaz12Send());
                }
                return cmdotkaz12;
            }
        }
        private void CmdOtkaz12Send()
        {
            // repos.KomValues[3].SendCommand.Execute(12);
            LinkInpu.Instance.SetSendVar(true, 12, 3);
        }

        private RelayCommand cmdotkaz13;
        public ICommand CmdOtkaz13
        {
            get
            {
                if (cmdotkaz13 == null)
                    cmdotkaz13 = new RelayCommand(param => CmdOtkaz13Send());
                return cmdotkaz13;
            }
        }
        private void CmdOtkaz13Send()
        {
            // repos.KomValues[3].SendCommand.Execute(14);
            LinkInpu.Instance.SetSendVar(true, 14, 3);
        }

        private RelayCommand cmdnorm;
        public ICommand CmdNorm
        {
            get
            {
                if (cmdnorm == null)
                    cmdnorm = new RelayCommand(param => CmdNormSend());
                return cmdnorm;
            }
        }
        private void CmdNormSend()
        {
            //  repos.KomValues[3].SendCommand.Execute(13);
            LinkInpu.Instance.SetSendVar(true, 13, 3);

        }
        #endregion
    }
}
