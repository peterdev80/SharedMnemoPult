using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_EPKRD : ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue RDOpen { get; set; }
        public BoolValue RDClose { get; set; }
        public BoolValue RDProduvka { get; set; }

        public ViewModel_EPKRD()
        {
            repos = ModelVariableRepository.Instance;
            RDOpen = repos.BitValues[0].ValState[12];
            RDClose = repos.BitValues[0].ValState[13];
            RDProduvka = repos.BitValues[0].ValState[14];

        }
        #region Command
        private RelayCommand cmdopen,cmdclose,cmdprdOp,cmdprdCl;

        public ICommand CmdOpen
        {
            get
            {
                if (cmdopen == null)
                    cmdopen = new RelayCommand(param => {
                        //repos.KomValues[0].SendCommand.Execute(14);
                        LinkInpu.Instance.SetSendVar(true, 14, 0);
                    });
                return cmdopen;

            }
        }

        public ICommand CmdClose
        {
            get
            {
                if (cmdclose == null)
                    cmdclose = new RelayCommand(param => ComClose());
                return cmdclose;
            }
        }
        private void ComClose()
        {
            // repos.KomValues[2].SendCommand.Execute(1);
            LinkInpu.Instance.SetSendVar(true, 1, 2);
            //  repos.KomValues[0].SendCommand.Execute(15);
            LinkInpu.Instance.SetSendVar(true, 15, 0);

        }

        public ICommand CmdPrdOpen
        {
            get
            {
                if (cmdprdOp == null)
                    cmdprdOp = new RelayCommand(param => {
                        //repos.KomValues[2].SendCommand.Execute(0);
                        LinkInpu.Instance.SetSendVar(true, 0, 2);
                    }, param => CanCmdPrdO());
                return cmdprdOp;
            }
        }
        private bool CanCmdPrdO()
        {

           // if (RDClose.ValueState) return false;
            return true;

        }
        public ICommand CmdPrdClose
        {
            get
            {
                if (cmdprdCl == null)
                    cmdprdCl = new RelayCommand(param => {
                        // repos.KomValues[2].SendCommand.Execute(1);
                        LinkInpu.Instance.SetSendVar(true, 1, 2);
                    },param=>CanClose());
                return cmdprdCl;
            }
        }
        private bool CanClose()
        {
           // if (!RDProduvka.ValueState) return false;
            return true;
        }
        #endregion
    }
}
