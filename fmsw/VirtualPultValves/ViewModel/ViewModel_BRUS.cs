using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;
using System.Diagnostics;
using EWTM.Model;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_BRUS : ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue Podsvet { get; set; }
        public BoolValue Vent { get; set; }
        public BoolValue Ilym { get; set; }
        public BoolValue BoaSaOsn { get; set; }
        public BoolValue BoaSaRez { get; set; }
        public BoolValue Vsa { get; set; }
        public BoolValue PitEpkrdOsn { get; set; }
        public BoolValue PitEpkrdRez { get; set; }
        public BoolValue RazreshRrst { get; set; }
        public BoolValue Pvk { get; set; }
        public BoolValue Pbk { get; set; }
        public BoolValue Vp1 { get; set; }
        public BoolValue Vp2 { get; set; }
        public BoolValue Sro { get; set; }
       

        public ViewModel_BRUS()
        {
             repos = ModelVariableRepository.Instance;
            Podsvet=repos.BitValues[1].ValState[0];
                Vent=repos.BitValues[1].ValState[1];
                Ilym=repos.BitValues[1].ValState[2];
                BoaSaOsn=repos.BitValues[1].ValState[3];
                BoaSaRez=repos.BitValues[1].ValState[4];
                Vsa=repos.BitValues[1].ValState[5];
                PitEpkrdOsn=repos.BitValues[1].ValState[6];
                PitEpkrdRez=repos.BitValues[1].ValState[7];
                RazreshRrst=repos.BitValues[1].ValState[8];
                Pvk=repos.BitValues[1].ValState[9];
                Pbk=repos.BitValues[1].ValState[10];
                Vp1=repos.BitValues[1].ValState[11];
                Vp2=repos.BitValues[1].ValState[12];
                Sro = repos.BitValues[1].ValState[13];




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
            //  repos.KomValues[1].SendCommand.Execute(param);
            Debug.WriteLine("cmdSend=" + param.ToString());
            LinkInpu.Instance.SetSendVar(true, int.Parse(param.ToString()), 1);
        }
        #endregion



    }
}
