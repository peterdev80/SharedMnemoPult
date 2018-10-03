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

       
        #endregion
    }
}
