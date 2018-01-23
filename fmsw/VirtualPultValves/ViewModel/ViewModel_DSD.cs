using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_DSD: ViewModelBase
    {
        private ModelVariableRepository repos;
        public IntValue Dsd { get; set; }
        public ViewModel_DSD()
        {
            repos = ModelVariableRepository.Instance;
            Dsd = repos.IntValues[0].ValState;
           // CmdDsd = repos.KomValues[4].SendCommand;
        }

        #region Command
        private RelayCommand cmd;
        public ICommand CmdDsd {
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
            LinkInpu.Instance.SetSendVar( int.Parse(param.ToString()), 4);
        }
        #endregion
    }
}
