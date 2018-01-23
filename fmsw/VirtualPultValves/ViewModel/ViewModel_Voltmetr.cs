using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValueModel.BaseType;
using VirtualPultValves.Model;

namespace VirtualPultValves.ViewModel
{
   public class ViewModel_Voltmetr:ViewModelBase
    {
       private ModelVariableRepository repos;
       public IntValue Voltag { get; private set; }
       public double MaxValue
       {
           get
           {
               return 40.6d;
           }
       }
       public double SmalChange
       {
           get
           {
               return 1.0d;
           }
       }
       public ViewModel_Voltmetr()
       {
           repos = ModelVariableRepository.Instance;
           Voltag = repos.IntValues[2].ValState;
       }

    }
}
