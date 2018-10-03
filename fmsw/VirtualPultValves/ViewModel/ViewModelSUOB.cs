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
    public class ViewModelSUOB : ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue SUOB { get; set; }
        public ViewModelSUOB()
        {
            repos = ModelVariableRepository.Instance;
            SUOB = repos.BitValues[1].ValState[18];
        }
        private RelayCommand _cmdop, _cmdcl;
        public ICommand cmdOp
        {
            get
            {
                if (_cmdop == null) _cmdop = new RelayCommand(param => {
                    //repos.KomValues[2].SendCommand.Execute(4); 
                    LinkInpu.Instance.SetSendVar(true, 4, 2);
                });
                return _cmdop;
            }
        }
        public ICommand cmdCl
        {
            get
            {
                if (_cmdcl == null) _cmdcl = new RelayCommand(param => {
                  //  repos.KomValues[2].SendCommand.Execute(5);
                    LinkInpu.Instance.SetSendVar(true, 5, 2);
                });
                return _cmdcl;
            }
        }
    }
}
