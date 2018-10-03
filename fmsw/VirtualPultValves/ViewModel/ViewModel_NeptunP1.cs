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
    public class ViewModel_NeptunP1:ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue BdusV1 { get; private set; }
        public BoolValue BdusV2 { get; private set; }
        public IntValue Voltag { get; private set; }
        public double MaxValue
        {
            get
            {
                return 40.6d;
            }
        }
        public double SmalChange
        {
            get
            {
                return 1.0d;
            }
        }

        public ViewModel_NeptunP1()
        {
            repos = ModelVariableRepository.Instance;
            BdusV1 = repos.BitValues[1].ValState[23];
            BdusV2 = repos.BitValues[1].ValState[24];
            Voltag = repos.IntValues[2].ValState;

            if (BdusV1.ValueState && !BdusV2.ValueState) CmdBdus1.Execute(0);
            if (!BdusV1.ValueState && BdusV2.ValueState) CmdBdus2.Execute(0);
            if (BdusV1.ValueState && BdusV2.ValueState) CmdBdus3.Execute(0);

        }

        #region Command
        private RelayCommand cmdbdus1, cmdbdus2, cmdbdus3;

        public ICommand CmdKontVPFalse
        {
            get
            {
                return new RelayCommand(p=>repos.getCMD("__KONTROL_IP").SendCommand.Execute(0));
            }
        }
        public ICommand CmdKontVPTrue
        {
            get
            {
                return new RelayCommand(p => repos.getCMD("__KONTROL_IP").SendCommand.Execute(1));
            }
        }
      
        public ICommand CmdBdus1
        {
            get
            {
                if (cmdbdus1 == null)
               
                    cmdbdus1 = new RelayCommand(p =>
                        {
                            
                            WagoIO.Instance.SetSendVar(true, 12, 1);
                            WagoIO.Instance.SetSendVar(false, 13, 1);
                            WagoIO.Instance.SetSendVar(false, 14, 1);
                          //  repos.KomValues[8].SendCommand.Execute(0);
                        }

                );
                
                return cmdbdus1;
            }
        }
        public ICommand CmdBdus2
        {
            get
            {
                if (cmdbdus2 == null)
                    cmdbdus2 = new RelayCommand(p =>
                        {
                            WagoIO.Instance.SetSendVar(false, 12, 1);
                            WagoIO.Instance.SetSendVar(true, 13, 1);
                            WagoIO.Instance.SetSendVar(false, 14, 1);
                           // repos.KomValues[9].SendCommand.Execute(0);

                        });
                return cmdbdus2;
            }
        }
        public ICommand CmdBdus3
        {
            get
            {
                if (cmdbdus3 == null)
                    cmdbdus3 = new RelayCommand(p =>
                        {
                            WagoIO.Instance.SetSendVar(false, 12, 1);
                            WagoIO.Instance.SetSendVar(false, 13, 1);
                            WagoIO.Instance.SetSendVar(true, 14, 1);
                           // repos.KomValues[10].SendCommand.Execute(0);
                        });
                return cmdbdus3;
            }
        }

        public ICommand CmdBdusDown
        {
            get
            {
                return new RelayCommand(p =>  WagoIO.Instance.SetSendVar(true, 6, 1));
            }
        }
        public ICommand CmdBdusUP
        {
            get
            {
                return new RelayCommand(p =>  WagoIO.Instance.SetSendVar(false, 6, 1));
            }
        }

        public ICommand CmdSbrosAvarSign_Down
        {
            get
            {
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(true, 9, 0));
            }
        }
        public ICommand CmdSbrosAvarSign_Up 
        {
            get
            {
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(false, 9, 0));
            }
        }

        public ICommand CmdKontrTCFalse
        {
            get 
            {
                //  return new RelayCommand(p => WagoIO.Instance.SetSendVar(false, 12, 6)); 
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(false, 15, 1));
            }
        }

        public ICommand CmdKontrTCTrue
        {
            get 
            {
                // return new RelayCommand(p => WagoIO.Instance.SetSendVar(true, 12, 6)); 
                return new RelayCommand(p => WagoIO.Instance.SetSendVar(true, 15, 1));
            }
        }
        #endregion

    }
}
