using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VirtualPultValves.ViewModel
{
    /// <summary>
    /// Сгрупированное предстовление комманды по признаку Locate
    /// </summary>
    public class GroupCommandViewModel:ViewModelBase
    {
        public List<CommandViewModel> CommandsGroup { get; set; }
        public string NameLocation { get; set; }
        public GroupCommandViewModel(List<CommandViewModel> cg, string name)
        {
            this.CommandsGroup = cg;
            this.NameLocation = name;

        }
    }
}
