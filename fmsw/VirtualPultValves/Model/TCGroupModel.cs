using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VirtualPultValves.Model
{
    public class TCGroupModel
    {
        private ReadOnlyCollection<TransporantModel> _blockTc;
        private List<TransporantModel> _lstTc;


        public string TCGroupName { get; set; }
        public ReadOnlyCollection<TransporantModel> BlockList
        {
            get
            {

                if (_lstTc == null) throw new ApplicationException("Неверно инициализированн TCGroupModel: " + TCGroupName); ;

                if (_blockTc == null)
                    _blockTc = new ReadOnlyCollection<TransporantModel>(_lstTc);
                return _blockTc;
            }
        }
        public TCGroupModel(string groupname, List<TransporantModel> lstTc)
        {
            _lstTc = lstTc;
            TCGroupName = groupname;
        }


    }
}
