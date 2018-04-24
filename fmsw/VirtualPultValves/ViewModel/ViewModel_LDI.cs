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

        int step = 5;//счетчик разряда числа
        int sec = 0;//время замера
        private int curValue1; //замер 1
        private int curValue2; //замер 2
        private int curValue3; //буфер замера
        double rspeed; //расчетная скорость
        private string curValue;//значение текущего замера

        public IntValue l1 { get; set; }// Первый сегмент ввода
        public IntValue l2 { get; set; }//сегмент ввода
        public IntValue l3 { get; set; }//сегмент ввода
        public IntValue l4 { get; set; }//сегмент ввода
        public IntValue l5 { get; set; }//пятый сегмент ввода

        public IntValue ls1 { get; set; }//первый сегмент выывода
        public IntValue ls2 { get; set; }//сегмент вывода
        public IntValue ls3 { get; set; }//сегмент вывода
        public IntValue ls4 { get; set; }//сегмент вывода


        public BoolValue lamp1 { get; set; } //Лампа первого замера
        public BoolValue lamp2 { get; set; } //Лампа второго замера
        public IntValue Indicator { get; private set; } // Индикатор состояния прибора
        public IntValue Razryad { get; private set; }//точка разряда
        public DoubleValue Speed { get; set; } //расчитанная скорость

        /// <summary>
        /// Конструкор, инициализация состояний
        /// </summary>
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
        private RelayCommand cmdCif, cmdTimerStop, cmdOBN, cmdISX, cmdI;
        /// <summary>
        ///Комманда нажатия цифры
        /// </summary>
        public ICommand CmdCif
        {
            get
            {
                if (cmdCif == null)
                    cmdCif = new RelayCommand(param => osnSend(param));
                return cmdCif;
            }
        }
        //обработка цифр
        private void osnSend(object param)
        {
            //определяем последовательность и разряд цифр
            if (step < 4)
            {
                step++;
                Razryad.ValueState = step;
                curValue = curValue + param.ToString();
                //В зависимости от порядка ввода заполняем индикаторы
                if (step == 1) l2.ValueState = Int32.Parse(param.ToString());
                if (step == 2) l3.ValueState = Int32.Parse(param.ToString());
                if (step == 3) l4.ValueState = Int32.Parse(param.ToString());
                if (step == 4) l5.ValueState = Int32.Parse(param.ToString());

            }

        }


        /// <summary>
        /// Комманда приведение в исходное состояние
        /// </summary>
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


        /// <summary>
        /// Комманда обнуление ввода
        /// </summary>
        public ICommand CmdOBN
        {
            get
            {
                if (cmdOBN == null)
                    cmdOBN = new RelayCommand(param => { obnul(); });
                return cmdOBN;
            }
        }



        /// <summary>
        /// Команда остановки таймера
        /// </summary>
        public ICommand TimerStop
        {
            get
            {
                if (cmdTimerStop == null)
                    cmdTimerStop = new RelayCommand(param => { sec = int.Parse(param.ToString()); });
                return cmdTimerStop;
            }
        }


        /// <summary>
        /// Комманда принятия данных
        /// </summary>
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


            //определяем какой замер 1,2
            if (Indicator.ValueState < 2) Indicator.ValueState++;
            //Установка значения выбброной лампы в зависимости от замера
            lampSetup();
            //проверяем коректность состояния 
            if ((curValue != null) && (curValue != ""))
            {
                //при первом замере сохраняем значение растояния
                if (Indicator.ValueState == 1) curValue1 = int.Parse(curValue);
                //Если второй замер расчитываем скорость
                if (Indicator.ValueState == 2)
                {
                    curValue2 = int.Parse(curValue);
                    int metr = curValue1 - curValue2;
                    if (sec != 0)
                    {


                        Speed.ValueState = Math.Round((double)metr / (double)sec, 1);

                    }
                    //вывод скорости на индикаторы
                    SpeedShow(Speed.ValueState);

                    curValue3 = curValue2;

                }
            }

            obnul();



        }



        //обнуление полей ввода
        private void obnul()
        {
            l1.ValueState = 0;
            l2.ValueState = 0;
            l3.ValueState = 0;
            l4.ValueState = 0;
            l5.ValueState = 0;
            Razryad.ValueState = 0;
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
            //первый разряд сближения расхождения 
            if (var_s < 0) ls1.ValueState = -1;//сближение
            if (var_s > 0) ls1.ValueState = 10;//расхождение
            if (var_s != 0)
            {
                var_s = Math.Abs(var_s);
                //первый десятичный разряд
                double i1 = Math.Truncate(var_s / 10);
                //второй десятичный разряд
                double i2 = Math.Truncate(var_s - i1 * 10);
                //третий десятичный разряд
                double i3 = Math.Truncate((var_s - i1 * 10 - i2) * 10);
                //Разряды скорости 2,3,4
                ls2.ValueState = (int)i1;
                ls3.ValueState = (int)i2;
                ls4.ValueState = (int)i3;

            }


        }
        //Установка ламп в зависимости от измерения
        private void lampSetup()
        {
            lamp1.ValueState = false;
            lamp2.ValueState = false;

            if (Indicator.ValueState == 1) lamp1.ValueState = true; //первое измерение
            if (Indicator.ValueState == 2) lamp2.ValueState = true; //второе измерение
        }


        #endregion


        /// <summary>
        /// отображение прогноза растояние в зависимости от расчета скорости
        /// </summary>
        /// <param name="timsec"></param>
        public void SRasShow(int timsec)        {
            rspeed = -1 * Speed.ValueState;
            double metr = rspeed * timsec + curValue3; //расчет расстоянмя

            //разложение расстояние по индикаторам
            double i1 = Math.Truncate(metr / 1000);
            double i2 = Math.Truncate((metr - i1 * 1000) / 100);
            double i3 = Math.Truncate((metr - i1 * 1000 - i2 * 100) / 10);
            double i4 = Math.Truncate((metr - i1 * 1000 - i2 * 100 - i3 * 10));
            l2.ValueState = (int)i1;
            l3.ValueState = (int)i2;
            l4.ValueState = (int)i3;
            l5.ValueState = (int)i4;

            Razryad.ValueState = 4; //убрать точку


        }

    }

}
