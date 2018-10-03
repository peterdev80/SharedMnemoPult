using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows.Input;
using System.Diagnostics;
using ValueModel.BaseType;
using EWTM.Model;

namespace VirtualPultValves.ViewModel
{
    /****************************************************
     * BitValues[2]-__KLAPAN_4;
     * 14-16 номера битов в переменнной
     ****************************************************/
    public class ViewModel_BAPD : ViewModelBase
   {
       private ModelVariableRepository repos;      
    
       public BoolValue Avtomat {get;set;}
       public BoolValue Osnov {get;set;}
       public BoolValue Dubl {get;set;}
     

     /*  public bool Avtomat
       {
           get 
           {
               if (repos == null) return false;
               return repos.BitValues[2].ValState[14].ValueState;
           }
       }
       public bool Osnov
       {
           get
           {
               if (repos == null) return false;
               return repos.BitValues[2].ValState[15].ValueState;
           }
       }
       public bool Dubl
       {
           get
           {
               if (repos == null) return false;
               return repos.BitValues[2].ValState[16].ValueState;
           }
       }*/

       public ViewModel_BAPD()
       {
           repos = ModelVariableRepository.Instance;
           Avtomat = repos.BitValues[2].ValState[13];
           Osnov = repos.BitValues[2].ValState[14];
           Dubl = repos.BitValues[2].ValState[15];
           
          
          repos.BitValues[2].ValState[13].PropertyChanged += delegate
           {
              Debug.WriteLine(Avtomat.ValueState+"-A");
           };
           repos.BitValues[2].ValState[14].PropertyChanged+=delegate
           {
               Debug.WriteLine(Osnov.ValueState + "-O");
           };
           repos.BitValues[2].ValState[15].PropertyChanged += delegate
           {
               Debug.WriteLine(Dubl.ValueState + "-D");
           };
       }

   #region Command
      
    private RelayCommand _CmdAvtomat;

       public ICommand CmdAvtomat
       {
           get
           {
               if (_CmdAvtomat == null)
               {

                   _CmdAvtomat = new RelayCommand(param => CmdAvtomatSendValue(), param => CmdAvtomatCanValue());
               }
               return _CmdAvtomat;
           }
       }
       private bool CmdAvtomatCanValue()
       {
          
              
           return true;
       }
       private void CmdAvtomatSendValue()
       {
            // repos.KomValues[3].SendCommand.Execute(18);
            LinkInpu.Instance.SetSendVar(true, 18, 3);


       }
       private RelayCommand _CmdOsnov;
       public ICommand CmdOsnov
       {
           get
           {
               if (_CmdOsnov == null)
               {
                   _CmdOsnov = new RelayCommand(param => CmdOsnovSendValue(), param => CmdAvtomatCanValue());
               }
               return _CmdOsnov;
           }
       }      
       private void CmdOsnovSendValue()
       {
            // repos.KomValues[3].SendCommand.Execute(19);
            LinkInpu.Instance.SetSendVar(true, 19, 3);

       }

       private RelayCommand _CmdDubl;
       public ICommand CmdDubl
       {
           get
           {
               if (_CmdDubl == null)
               {
                   _CmdDubl = new RelayCommand(param => CmdDublSendValue(), param => CmdAvtomatCanValue());
               }
               return _CmdDubl;
           }
       }
      
       private void CmdDublSendValue()
       {
            //  repos.KomValues[3].SendCommand.Execute(20);
            LinkInpu.Instance.SetSendVar(true, 20, 3);
       }



       #endregion

   }
}
