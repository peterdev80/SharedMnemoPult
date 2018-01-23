using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_RPV:ViewModelBase
    {
        private ModelVariableRepository repos;
        public BoolValue RPV1_1 { get; private set; }
        public BoolValue RPV1_2 { get; private set; }
        public BoolValue RPV2_1 { get; private set; }
        public BoolValue RPV2_2 { get; private set; }

        public ViewModel_RPV()
        {
            repos = ModelVariableRepository.Instance;
            RPV1_1 = repos.BitValues[0].ValState[6];
            RPV1_2 = repos.BitValues[0].ValState[7];
            RPV2_1 = repos.BitValues[0].ValState[8];
            RPV2_2 = repos.BitValues[0].ValState[9];
        }

        #region Command
        private RelayCommand cmd1o, cmd1c, cmd1s, cmd2o, cmd2c, cmd2s;

        public ICommand Cmd1o
        {
            get
            {
                if (cmd1o == null)
                    cmd1o = new RelayCommand(p => {
                        //repos.KomValues[0].SendCommand.Execute(6); 
                        LinkInpu.Instance.SetSendVar(true, 6, 0);
                    });
                return cmd1o;
            }
        }

        public ICommand Cmd1c
        {
            get
            {
                if (cmd1c == null)
                    cmd1c = new RelayCommand(p => {
                        //repos.KomValues[0].SendCommand.Execute(7); 
                        LinkInpu.Instance.SetSendVar(true, 7, 0);
                    });
                return cmd1c;
            }
        }

        public ICommand Cmd1S
        {
            get
            {
                if (cmd1s == null)
                    cmd1s = new RelayCommand(p => {
                        ///repos.KomValues[0].SendCommand.Execute(8);
                        LinkInpu.Instance.SetSendVar(true, 8, 0);
                    });
                return cmd1s;
            }
        }

        public ICommand Cmd2o
        {
            get
            {
                if (cmd2o == null)
                    cmd2o = new RelayCommand(p => {
                        //repos.KomValues[0].SendCommand.Execute(9);
                        LinkInpu.Instance.SetSendVar(true, 9, 0);
                    });
                return cmd2o;
            }
        }

        public ICommand Cmd2c
        {
            get
            {
                if (cmd2c == null)
                    cmd2c = new RelayCommand(p => {
                        //repos.KomValues[0].SendCommand.Execute(10); 
                        LinkInpu.Instance.SetSendVar(true, 10, 0);
                    });
                return cmd2c;
            }
        }

        public ICommand Cmd2S
        {
            get
            {
                if (cmd2s == null)
                    cmd2s = new RelayCommand(p =>
                    //repos.KomValues[0].SendCommand.Execute(11));
                    LinkInpu.Instance.SetSendVar(true, 11, 0));
                return cmd2s;
            }
        }
         

        #endregion
    }
}
