using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using ValueModel.BaseType;

namespace VirtualPultValves.ViewModel
{
   public class ViewModel_Manov:ViewModelBase
    {
       private ModelVariableRepository repos;
       public IntValue Manov { get; private set; }
      
       public ViewModel_Manov()
       {
           repos = ModelVariableRepository.Instance;
           Manov = repos.IntValues[1].ValState;
          
       }
    }
}
