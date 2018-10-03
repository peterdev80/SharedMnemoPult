using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.Windows;
using System.Windows.Input;
using VirtualPultValves.Views;
using ValueModel.BaseType;
using EWTM.Model;

namespace VirtualPultValves.ViewModel
{
   public class ViewModel_InPU:ViewModelBase
    {
      

       private ModelVariableRepository repos;
       private int _rmnum = 0;
       public string TextInPU { get; private set; }
      

       public BoolValue DejRegim { get; private set; }
       public BoolValue CentOgon { get; private set; }
       public BoolValue clInpu { get; private set; }
       public Visibility InPU1Visible { get; private set; }
       public Visibility InPU2Visible { get; private set; }
       public IntValue COTransparant { get; private set; }
       public int RMNum
       {
           get { return _rmnum; }
           set
           {
               if (value == _rmnum) return;
               _rmnum = value;
               if (_rmnum == 1)
               {
                   TextInPU = "ИнПУ 1";
                   InPU1Visible = Visibility.Visible;
                   InPU2Visible = Visibility.Hidden;
                   DejRegim = WagoIO.Instance.DejRegim1;//repos.BoolValues[2].ValState;
                   clInpu = WagoIO.Instance.clInpu_1;
               }
               else
               {
                   if (_rmnum == 2)
                   {
                       TextInPU = "ИнПУ 2";
                       InPU2Visible = Visibility.Visible;
                       InPU1Visible = Visibility.Hidden;
                       DejRegim = WagoIO.Instance.DejRegim2;// repos.BoolValues[3].ValState;
                       clInpu = WagoIO.Instance.clInpu_2;
                   }
               }

               this.OnPropertyChanged("TextInPU");
               this.OnPropertyChanged("InPU1Visible");
               this.OnPropertyChanged("InPU2Visible");
               this.OnPropertyChanged("RMNum");
               this.OnPropertyChanged("DejRegim");
               this.OnPropertyChanged("clInpu");

           }
       }


       public ViewModel_InPU()
       {
           repos = ModelVariableRepository.Instance;
          // DejRegim1 = new BoolValue();
         //  DejRegim2 = new BoolValue();
           //RMNum = 1;
           CentOgon = WagoIO.Instance.CentOgon;//repos.BoolValues[1].ValState;
         // COTransparant = repos.IntValues[3].ValState;
          clInpu = new BoolValue();
          clInpu = WagoIO.Instance.clInpu_1;
       }

        #region Command
       private RelayCommand cmdInpu1select, cmdinpu2select, cmdinpuchangeselect;
       private RelayCommand cmdbat, cmdzvuk, cmdvkl1, cmdotkl1, cmdvkl2, cmdotkl2;
       public ICommand CmdInPU1Select
       {
           get
           {
               if (cmdInpu1select==null)
                   cmdInpu1select = new RelayCommand(p => { RMNum = 1; }, p => Can1());
               return cmdInpu1select;
           }
       }
       private Boolean Can1()
       {
           if (RMNum==1) return false;
           return true;
       }
       public ICommand CmdInPU2Select
       {
           get
           {
               if (cmdinpu2select == null)
                   cmdinpu2select = new RelayCommand(p => { RMNum = 2; }, p => Can2());
               return cmdinpu2select;
           }
       }
       private Boolean Can2()
       {
           if (RMNum==1) return true;
           return false;
       }
       public ICommand CmdInPUChange
       {
           get
           {
               if (cmdinpuchangeselect == null)
                   cmdinpuchangeselect = new RelayCommand(p => { RMNum = 0; });
               return cmdinpuchangeselect;
           }
       }

       public ICommand CmdVKL_REZ_BATAR_DOWN
       {
           get
           {
               if (cmdbat == null)
                   cmdbat = new RelayCommand(p => 
                       {
                           int i=Int32.Parse(p.ToString());
                           if (i==1) WagoIO.Instance.SetSendVar(true,7,1);
                           else
                               WagoIO.Instance.SetSendVar(false,7,1);
                       });
               return cmdbat;
           }
       }
       
       public ICommand CmdOTBOI_ZVUKA_DOWN
       {
           get
           {
               if (cmdzvuk == null)
                   cmdzvuk = new RelayCommand(p => 
                   {
                       int i = Int32.Parse(p.ToString());
                       if (i == 1) WagoIO.Instance.SetSendVar(true, 9, 1);
                       else
                       WagoIO.Instance.SetSendVar(false, 9, 1);
                       View_InPU.BtnCmd.Execute(27, null);
                   });
               return cmdzvuk;
           }
       }
       public ICommand CmdVKLInpu1
       {
           get
           {
               if (cmdvkl1 == null)
                   cmdvkl1 = new RelayCommand(p =>
                   {
                       int i = Int32.Parse(p.ToString());
                       if (i == 1) WagoIO.Instance.SetSendVar(true, 0, 7);
                       else
                           WagoIO.Instance.SetSendVar(false, 0, 7);
                       
                   });
               return cmdvkl1;
           }
       }
       public ICommand CmdOTKLInpu1
       {
           get
           {
               if (cmdotkl1 == null)
                   cmdotkl1 = new RelayCommand(p =>
                   {
                       int i = Int32.Parse(p.ToString());
                       if (i == 1) WagoIO.Instance.SetSendVar(true, 2, 7);
                       else
                           WagoIO.Instance.SetSendVar(false, 2, 7);

                   });
               return cmdotkl1;
           }
       }

       public ICommand CmdVKLInpu2
       {
           get
           {
               if (cmdvkl2 == null)
                   cmdvkl2 = new RelayCommand(p =>
                   {
                       int i = Int32.Parse(p.ToString());
                       if (i == 1) WagoIO.Instance.SetSendVar(true, 4, 7);
                       else
                           WagoIO.Instance.SetSendVar(false, 4, 7);

                   });
               return cmdvkl2;
           }
       }
       public ICommand CmdOTKLInpu2
       {
           get
           {
               if (cmdotkl2 == null)
                   cmdotkl2 = new RelayCommand(p =>
                   {
                       int i = Int32.Parse(p.ToString());
                       if (i == 1) WagoIO.Instance.SetSendVar(true, 6, 7);
                       else
                           WagoIO.Instance.SetSendVar(false, 6, 7);

                   });
               return cmdotkl2;
           }
       }


     
        #endregion
    }
}
