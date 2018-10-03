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
    public class ViewModel_BRUB : ViewModelBase
    {

        private ModelVariableRepository repos;
        public BoolValue Osn { get; set; }
        public BoolValue Rez { get; set; }
        public BoolValue Vbo { get; set; }
        public BoolValue Asu { get; set; }
        public BoolValue Sro { get; set; }
        public BoolValue VV { get; set; }
       

        public ViewModel_BRUB()
        {
            repos = ModelVariableRepository.Instance;
            Osn = repos.BitValues[0].ValState[24];
            Rez = repos.BitValues[0].ValState[25];
            Vbo = repos.BitValues[0].ValState[26];
            Asu = repos.BitValues[0].ValState[27];
            Sro = repos.BitValues[0].ValState[28];
            VV = repos.BitValues[0].ValState[29];

        }
        #region Command
        private RelayCommand cmdosn,  cmdrez, cmdvbo, cmdasu, cmdsro,v;


        public ICommand CmdOsn
        {
            get 
            {
                if (cmdosn==null)
                    cmdosn=new RelayCommand(param=>osnSend(param));
                return cmdosn;
            }
        }
        private void osnSend(object param)
        {
            if (param.ToString() == "0")
                // repos.KomValues[2].SendCommand.Execute(18);
                LinkInpu.Instance.SetSendVar(true, 18, 2);
            if (param.ToString() == "1")
                //repos.KomValues[2].SendCommand.Execute(19);
                LinkInpu.Instance.SetSendVar(true, 19, 2);
        }

        public ICommand CmdRez
        {
            get
            {
                if (cmdrez == null)
                    cmdrez = new RelayCommand(param=>rezSend(param));
                return cmdrez;
            }
        }
        private void rezSend(object Param)
        {
            if (Param.ToString() == "2")
                //repos.KomValues[2].SendCommand.Execute(20);
                LinkInpu.Instance.SetSendVar(true, 20, 2);
            if (Param.ToString() == "3")
                //repos.KomValues[2].SendCommand.Execute(21);
                LinkInpu.Instance.SetSendVar(true, 21, 2);
        }

        public ICommand CmdVbo
        {
            get
            {
                if (cmdvbo == null) cmdvbo = new RelayCommand(param => vboSend(param));
                return cmdvbo;
            }
        }
        private void vboSend(object Param)
        {
            if (Param.ToString() == "4")
                //repos.KomValues[2].SendCommand.Execute(22);
                LinkInpu.Instance.SetSendVar(true, 22, 2);
            if (Param.ToString() == "5")
                //repos.KomValues[2].SendCommand.Execute(23);
                LinkInpu.Instance.SetSendVar(true, 23, 2);
        }

        public ICommand CmdAsu
        {
            get
            {
                if (cmdasu == null)
                    cmdasu = new RelayCommand(param => asuSend(param));
                return cmdasu;
            }
        }
        private void asuSend(object param)
        {
            if (param.ToString() == "6")
                // repos.KomValues[2].SendCommand.Execute(24);
                LinkInpu.Instance.SetSendVar(true, 24, 2);
            if (param.ToString() == "7")
                // repos.KomValues[2].SendCommand.Execute(25);
                LinkInpu.Instance.SetSendVar(true, 25, 2);
        }

        public ICommand CmdSro
        {
            get
            {
                if (cmdsro == null) cmdsro = new RelayCommand(param => sroSend(param));
                return cmdsro;
            }
        }
        private void sroSend(object param)
        {
            if (param.ToString() == "0")
                //repos.KomValues[1].SendCommand.Execute(28);
                LinkInpu.Instance.SetSendVar(true, 28, 1);
            if (param.ToString() == "2")
                // repos.KomValues[1].SendCommand.Execute(30);
                LinkInpu.Instance.SetSendVar(true, 30, 1);
        }

        public ICommand CmdV
        {
            get
            {
                if (v == null)
                    v = new RelayCommand(param => vSend(param));
                return v;
            }
        }
        private void vSend(object param)
        {
            if (param.ToString() == "1")
                //repos.KomValues[1].SendCommand.Execute(29);
                LinkInpu.Instance.SetSendVar(true, 29, 1);
            if (param.ToString() == "3")
                // repos.KomValues[1].SendCommand.Execute(31);
                LinkInpu.Instance.SetSendVar(true, 31, 1);
        }
        #endregion
    }
}
