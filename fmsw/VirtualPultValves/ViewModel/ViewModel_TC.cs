using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using VirtualPultValves.Model;
using VirtualPultValves.DataAccess;

namespace VirtualPultValves.ViewModel
{
    
    public class ViewModel_TC : ViewModelBase
    {

        ReadOnlyCollection<TCGroupModel> _TCList;

        private string _tcname;
        private TCReader tcr=new TCReader();

        public ViewModel_TC()
        {
            TCName = "TC1List";

        }
        public string TCName
        {
            get { return _tcname; }
            set
            {
                _tcname = value;
                _TCList = new ReadOnlyCollection<TCGroupModel>(tcr.CreateGroupModelList(_tcname));
                base.OnPropertyChanged("TCList");
            }
        }



        public ReadOnlyCollection<TCGroupModel> TCList
        {
            get
            {                
                return _TCList;
            }
        }





    }
}
