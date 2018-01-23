using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_OVK:ViewModelBase
    {
        private ModelVariableRepository repos;
        public byte[] wagoDin = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0,0,0,0,0,0,0 ,0,0,0,0,0,0,0,0,0};
        public BitPosValue wagobit=0;
        public BoolValue KO{ get; set; }

        public ViewModel_OVK()
        {
           repos = ModelVariableRepository.Instance;
            KO= repos.BitValues[2].ValState[16];


        }

        #region Command
        private RelayCommand cmd;
        public ICommand Cmd
        {
            get
            {
                if (cmd == null)
                    cmd = new RelayCommand(param => cmdSend(param));
                return cmd;
            }
        }
        private void cmdSend(object param)
        {
           /* int i = Int32.Parse(param.ToString());
            if (i==23) { LinkInpu.Instance.SetSendVar(true, 23, 3); LinkInpu.Instance.SetSendVar(false, 24, 3); } else
            { LinkInpu.Instance.SetSendVar(false, 23, 3); LinkInpu.Instance.SetSendVar(true, 24, 3); }*/
            //  int i = Int32.Parse(param.ToString());
            //  repos.KomValues[1].SendCommand.Execute(param);
            LinkInpu.Instance.SetSendVar(true, int.Parse(param.ToString()), 3);
            //  System.Windows.MessageBox.Show("Click="+(param.ToString()));
        }
        private RelayCommand _OVKUpKey, _OVKDownKey;
        public ICommand OVKUpKey
        {
            get
            {
                if (_OVKUpKey==null)
                    _OVKUpKey=new RelayCommand(param=>CMDUpKey(param));
                return _OVKUpKey;
            }
        }
        private void CMDUpKey(object val)
        {
            int i = Int32.Parse(val.ToString());
            if (i < 16) WagoIO.Instance.SetSendVar(false, i, 0); else
            WagoIO.Instance.SetSendVar(false, i-16, 1); 
           // wagoDin[i] = 0;
        }


        public ICommand OVKDownKey
        {
            get
            {
                if (_OVKDownKey == null)
                    _OVKDownKey = new RelayCommand(param => CMDDownKey(param));
                return _OVKDownKey;
            }
        }

        private void CMDDownKey(object val)
        {
            int i = Int32.Parse(val.ToString());
            if (i < 16) WagoIO.Instance.SetSendVar(true, i, 0);
            else
                WagoIO.Instance.SetSendVar(true, i - 16, 1); 
            
        }

        public ICommand OVK_ODR
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(19));
            }

        }
        public ICommand OVK_OB_SEKC_NADD
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(7));
            }

        }
        public ICommand OVK_OTSTREL_BO
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(16));
            }

        }
        public ICommand OVK_OTSTREL_BO2
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(16));///!!!!!!!!!!
            }

        }
        public ICommand OVK_TD_PODKL
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(5));
            }

        }
        public ICommand OVK_TD_OTKL
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(4));
            }

        }
        public ICommand OVK_KO_2
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(18));
            }

        }
        public ICommand OVK_KO
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(17));
            }

        }
        public ICommand OVK_VKL_SKD
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(2));
            }

        }
        public ICommand OVK_RAZDEL
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(8));
            }

        }
        public ICommand OVK_OTKL_SKD
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(3));
            }

        }
        public ICommand OVK_OTSTREL_ST_MEX
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(12));
            }

        }

        public ICommand OVK_VUBOR_DPOB_NASPUSK
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(1));
            }

        }
        public ICommand OVK_PEZERV_RASSTYK
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(13));
            }

        }
        public ICommand OVK_PODGOT_RADEL
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_PRIZNAK_SPUSK
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(10));
            }

        }
        public ICommand OVK_OTKR_KSDBO
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(11));
            }

        }
        public ICommand OVK_RUD
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(21));
            }

        }
        public ICommand OVK_RO_AK
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(20));
            }

        }
        public ICommand OVK_RAZGERM
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(15));
            }

        }
        public ICommand OVK_PODG_RAZGERM
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(14));
            }

        }
        public ICommand OVK_AVARVKLPIT
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[43].SendCommand.Execute(24));
            }

        }
      /*  public ICommand OVK_ODR
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[16].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_OB_SEKC_NADD
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[17].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_OTSTREL_BO
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[18].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_OTSTREL_BO2
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[19].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_TD_PODKL
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[20].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_TD_OTKL
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[21].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_KO_2
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[22].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_KO
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[23].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_VKL_SKD
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[24].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_RAZDEL
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[25].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_OTKL_SKD
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[26].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_OTSTREL_ST_MEX
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[27].SendCommand.Execute(0));
            }

        }

        public ICommand OVK_VUBOR_DPOB_NASPUSK
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[28].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_PEZERV_RASSTYK
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[29].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_PODGOT_RADEL
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[30].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_PRIZNAK_SPUSK
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[31].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_OTKR_KSDBO
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[32].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_RUD
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[33].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_RO_AK
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[34].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_RAZGERM
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[35].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_PODG_RAZGERM
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[36].SendCommand.Execute(0));
            }

        }
        public ICommand OVK_AVARVKLPIT
        {
            get
            {
                return new RelayCommand(p => repos.KomValues[37].SendCommand.Execute(0));
            }

        }*/
        #endregion
    }
}
