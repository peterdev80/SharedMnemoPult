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
    public class ViewModel_NeptunP2 : ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue Bi { get; private set; }
        public BoolValue Kk { get; private set; }
        public BoolValue Ki { get; private set; }
        public BoolValue PPit { get; private set; }

        public BoolValue _Bi { get; private set; }
        public BoolValue _Kk { get; private set; }
        public BoolValue _Ki { get; private set; }
       

        
        public ViewModel_NeptunP2()
        {
            repos = ModelVariableRepository.Instance;
            Bi = repos.BitValues[1].ValState[14];
            Kk = repos.BitValues[1].ValState[15];
            Ki = repos.BitValues[1].ValState[16];


            PPit = WagoIO.Instance.cLapmPult;//repos.BoolValues[0].ValState;
        }

        #region Command

        private RelayCommand cmdbi, cmdkk, cmdki;
        public ICommand CmdKonserv_down
        {
            get
            {
                return new RelayCommand(p=>WagoIO.Instance.SetSendVar(true,6,0));
            }
        }
        public ICommand CmdKonserv_Up
        {
            get
            {
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(false, 6, 0));
            }
        }
        public ICommand CmdPitVkl_down
        {
            get
            {
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(true,10,1));
            }
        }
        public ICommand CmdPitVkl_up
        {
            get
            {
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(false, 10, 1));
            }
        }
        public ICommand CmdPitOtkl_down
        {
            get
            {
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(true,11,1));
            }
        }
        public ICommand CmdPitOtkl_up
        {
            get
            {
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(false, 11, 1));
            }
        }
        public ICommand CmdBi
        {
            get
            {
               if (cmdbi == null)
                {
                   
                  
                    cmdbi = new RelayCommand(p => repos.getCMD("__VENTIL_BI").SendCommand.Execute(p));
                }
                return cmdbi;
            }
        }
        public ICommand CmdKK
        {
            get
            {
               if (cmdkk == null)
                {

                    cmdbi = new RelayCommand(p => repos.getCMD("__VENTIL_KK").SendCommand.Execute(p));
                }
                return cmdkk;
            }
        }
        public ICommand CmdKi
        {
            get
            {
               if (cmdki == null)
                {

                    cmdbi = new RelayCommand(p => repos.getCMD("__VENTIL_KI").SendCommand.Execute(p));

                }
                return cmdki;
            }

        }

        public ICommand CPusto
        {
            get
            {
                return new RelayCommand(p => p=1+3);
            }
        }



        #endregion
    }
}
