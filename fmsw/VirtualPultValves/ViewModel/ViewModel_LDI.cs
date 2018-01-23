using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ValueModel.BaseType;
using VirtualPultValves.Model;

namespace VirtualPultValves.ViewModel
{
    public class ViewModel_LDI : ViewModelBase
    {
       //счетчик разряда числа
        int step = 5;
        int sec = 0;
        public IntValue l1 { get; set; }
        public IntValue l2 { get; set; }
        public IntValue l3 { get; set; }
        public IntValue l4 { get; set; }
        public IntValue l5 { get; set; }

        public IntValue ls1 { get; set; }
        public IntValue ls2 { get; set; }
        public IntValue ls3 { get; set; }
        public IntValue ls4 { get; set; }

        public BoolValue lamp1 { get; set; }
        public BoolValue lamp2 { get; set; }
        public IntValue Indicator { get; private set; }
        public IntValue Razryad { get; private set; }
        public DoubleValue Speed { get; set; }
        private int curValue1;
        private int curValue2;
        private int curValue3;
        double  rspeed;


        private string curValue;

        public ViewModel_LDI()
        {
            l1 = new IntValue();
            l1.ValueState = -1;
            l2 = new IntValue();
            l2.ValueState = -1;
            l3 = new IntValue();
            l3.ValueState = -1;
            l4 = new IntValue();
            l4.ValueState = -1;
            l5 = new IntValue();
            l5.ValueState = -1;


            ls1 = new IntValue();
            ls1.ValueState = -1;
            ls2 = new IntValue();
            ls2.ValueState = -1;
            ls3 = new IntValue();
            ls3.ValueState = -1;
            ls4 = new IntValue();
            ls4.ValueState = -1;

            lamp1 = new BoolValue();
            lamp1.ValueState = false;
            lamp2 = new BoolValue();
            lamp2.ValueState = false;
           

            Indicator = new IntValue();
            Indicator.ValueState = 0;
            Razryad = new IntValue();
            Razryad.ValueState = 4;
            Speed = new DoubleValue();
            Speed.ValueState = 0d;


            curValue3 = 0;
            rspeed = 0;
        }

        #region Command
        private RelayCommand cmdCif,cmdTimerStop, cmdOBN, cmdISX, cmdI;

        public ICommand CmdCif
        {
            get
            {
                if (cmdCif == null)
                    cmdCif = new RelayCommand(param => osnSend(param));
                return cmdCif;
            }
        }

        private void osnSend(object param)
        {
           
            
            
            if (step<4)
            { 
               
                step++; 
                Razryad.ValueState = step;
            curValue=curValue+param.ToString();
            if (step == 1) l2.ValueState = Int32.Parse(param.ToString());
            if (step == 2) l3.ValueState = Int32.Parse(param.ToString());
            if (step == 3) l4.ValueState = Int32.Parse(param.ToString());
            if (step == 4) l5.ValueState = Int32.Parse(param.ToString());
           // if (step == 5) l5.ValueState = Int32.Parse(param.ToString());
            }

        }


        public ICommand CmdISX
        {
            get
            {
                if (cmdISX == null)
                    cmdISX = new RelayCommand(param => osnSendISX(param));
                return cmdISX;
            }
        }

        private void osnSendISX(object param)
        {
            obnul();
            curValue1 = 0;
            curValue2 = 0;
            curValue3 = 0;
            Indicator.ValueState = 0;
            Speed.ValueState = 0d;
            SpeedShow(Speed.ValueState);
            lampSetup();




        }


        public ICommand CmdOBN
        {
            get
            {
                if (cmdOBN == null)
                    cmdOBN = new RelayCommand(param => osnSendOBN(param));
                return cmdOBN;
            }
        }

        private void osnSendOBN(object param)
        {
            obnul();

        }


        public ICommand TimerStop
        {
            get
            {
                if (cmdTimerStop==null)
                    cmdTimerStop=new RelayCommand(param=>onTime(param));
                return cmdTimerStop;
            }
        }

        private void onTime(object param)
        {
            sec = int.Parse(param.ToString());
           /// 
        }

        public ICommand CmdEnter
        {
            get
            {
                if (cmdI == null)
                    cmdI = new RelayCommand(param => onEnter(param));
                return cmdI;
            }
        }

        private void onEnter(object param)
        {

            
            
            if (Indicator.ValueState<2)
            Indicator.ValueState++;
            lampSetup();
            if ((curValue != null)&&(curValue!=""))
            {
                if (Indicator.ValueState == 1) curValue1 = int.Parse(curValue);
                if (Indicator.ValueState == 2)
                {
                    curValue2 = int.Parse(curValue);
                    int metr = curValue1 - curValue2;
                    if (sec != 0)
                    {

                       
                        Speed.ValueState = Math.Round((double)metr / (double)sec, 1);

                    }
                   
                    SpeedShow(Speed.ValueState);

                    curValue3 = curValue2;
                    
                }
            }

            obnul();

          

        }

        


        #endregion
        private void obnul()
        {
           
            l1.ValueState = 0;
            
            l2.ValueState = 0;
           
            l3.ValueState = 0;
           
            l4.ValueState = 0;
           
            l5.ValueState = 0;     
           
           
            Razryad.ValueState =0;
            step = 0;
            curValue = "";

        }

        private void SpeedShow(double var_s)
        {
            if (var_s == 0)
            {
                ls1.ValueState = -1;

                ls2.ValueState = 0;

                ls3.ValueState = 0;

                ls4.ValueState = 0;

                
            }
            //первый разряд сближения расхлждения
            if (var_s < 0) ls1.ValueState = -1;
            if (var_s>0) ls1.ValueState=10;
            if (var_s != 0)
            {

                var_s = Math.Abs(var_s);
                //первый десятичный разряд
                double i1 = Math.Truncate(var_s / 10);

                double i2 = Math.Truncate(var_s - i1 * 10);

                double i3 = Math.Truncate((var_s - i1 * 10 - i2) * 10);


                ls2.ValueState = (int)i1;

                ls3.ValueState = (int)i2;

                ls4.ValueState = (int)i3;
             
            }


        }

        public void SRasShow(int timsec)
        {
            rspeed = -1 * Speed.ValueState;


            double metr = rspeed * timsec + curValue3;

            double i1 = Math.Truncate(metr / 1000);

            double i2 = Math.Truncate((metr - i1 * 1000)/100);

            double i3 = Math.Truncate((metr - i1 * 1000 - i2*100) / 10);
            double i4= Math.Truncate((metr - i1 * 1000 - i2 * 100-i3*10));

            l2.ValueState = (int)i1;
            l3.ValueState = (int)i2;
            l4.ValueState = (int)i3;
            l5.ValueState = (int)i4;
            Razryad.ValueState = 4;


        }

        private void lampSetup()
        {
            lamp1.ValueState = false;
            lamp2.ValueState = false;

            if (Indicator.ValueState == 1) lamp1.ValueState = true;
            if (Indicator.ValueState == 2) lamp2.ValueState = true;
        }
    }

}
