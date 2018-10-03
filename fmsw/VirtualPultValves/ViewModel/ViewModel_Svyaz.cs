using EWTM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ValueModel.BaseType;
using VirtualPultValves.Model;

namespace VirtualPultValves.ViewModel
{
   public class ViewModel_Svyaz : ViewModelBase
    {
        public BoolValue LSvyz1 { get; private set; }
        public BoolValue LSvyz2 { get; private set; }
        public BoolValue LSvyz3 { get; private set; }

        public ViewModel_Svyaz()
        {
            LSvyz1= WagoIO.Instance.LampSvyaz1;
            LSvyz2 = WagoIO.Instance.LampSvyaz2;
            LSvyz3 = WagoIO.Instance.LampSvyaz3;

        }


        private RelayCommand _BSvyz1_up, _BSvyz2_up, _BSvyz3_up;
        private RelayCommand _BSvyz1_dn, _BSvyz2_dn, _BSvyz3_dn;

        public ICommand BSvyz1_dn
        {
            get
            {
                if (_BSvyz1_dn == null)

                    _BSvyz1_dn = new RelayCommand(p =>{  WagoIO.Instance.SetSendVar(true, 1, 8);} );

                return _BSvyz1_dn;
            }
        }
        public ICommand BSvyz2_dn
        {
            get
            {
                if (_BSvyz2_dn == null) _BSvyz2_dn = new RelayCommand(p =>{WagoIO.Instance.SetSendVar(true, 3, 8); });

                return _BSvyz2_dn;
            }
        }
        public ICommand BSvyz3_dn
        {
            get
            {
                if (_BSvyz3_dn == null) _BSvyz3_dn = new RelayCommand(p => { WagoIO.Instance.SetSendVar(true, 5, 8); });

                return _BSvyz3_dn;
            }
        }



        public ICommand BSvyz1_up
        {
            get
            {
                if (_BSvyz1_up == null)

                    _BSvyz1_up = new RelayCommand(p => { WagoIO.Instance.SetSendVar(false, 1, 8); });

                return _BSvyz1_up;
            }
        }
        public ICommand BSvyz2_up
        {
            get
            {
                if (_BSvyz2_up == null) _BSvyz2_up = new RelayCommand(p => { WagoIO.Instance.SetSendVar(false, 3, 8); });

                return _BSvyz2_up;
            }
        }
        public ICommand BSvyz3_up
        {
            get
            {
                if (_BSvyz3_up == null) _BSvyz3_up = new RelayCommand(p => { WagoIO.Instance.SetSendVar(false, 5, 8); });

                return _BSvyz3_up;
            }
        }



    }
}
