using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualPultValves.Model;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Collections.ObjectModel;
using EWTM.Model;

namespace VirtualPultValves.ViewModel
{
   /* public class TCModel : INotifyPropertyChanged
    {

        private string _tcContent;
        private Brush _AColor;
        private Brush _PColor;

        public string TCContent
        {
            get { return _tcContent; }
            set
            {
                _tcContent = value;
                this.OnPropertyChanged("TCContent");
            }
        }
        public Brush AColor
        {
            get { return _AColor; }
            set { _AColor = value; OnPropertyChanged("AColor"); }
        }

        public Brush PColor
        {
            get { return _PColor; }
            set { _PColor = value; OnPropertyChanged("PColor"); }
        }

        public TCModel()
        {
        }
        public TCModel(string _v, Brush _v1, Brush _v2)
        {
            TCContent = _v;
            AColor = _v1;
            PColor = _v2;

        }



        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members

    }*/
    public class ViewModelTC : ViewModelBase
    {
       // private WagoIO wago=WagoIO.Instance.


        private int _Stage;
        public TCModel T1 { get; set; }
        public TCModel T2 { get; set; }
        public TCModel T3 { get; set; }
        public TCModel T4 { get; set; }
        public TCModel T5 { get; set; }
        public TCModel T6 { get; set; }
        public TCModel T7 { get; set; }
        public TCModel T8 { get; set; }
        public TCModel T9 { get; set; }
        public TCModel T10 { get; set; }
        public TCModel T11 { get; set; }
        public TCModel T12 { get; set; }
        public TCModel T13 { get; set; }
        public TCModel T14 { get; set; }
        public TCModel T15 { get; set; }
        public TCModel T16 { get; set; }
        public TCModel T17 { get; set; }
        public TCModel T18 { get; set; }
        public TCModel T19 { get; set; }
        public TCModel T20 { get; set; }
        public TCModel T21 { get; set; }
        public TCModel T22 { get; set; }
        public TCModel T23 { get; set; }
        public TCModel T24 { get; set; }
      
        List<TCModel> lst = new List<TCModel>();

        private string _b1, _b2, _b3, _b4;
        public string Block1 
        {
            get { return _b1; }
            set 
            {
                _b1 = value;
                OnPropertyChanged("Block1");
            }
        }
        public string Block2
        {
            get { return _b2; }
            set
            {
                _b2 = value;
                OnPropertyChanged("Block2");
            }
        }
        public string Block3
        {
            get { return _b3; }
            set
            {
                _b3 = value;
                OnPropertyChanged("Block3");
            }
        }
        public string Block4
        {
            get { return _b4; }
            set
            {
                _b4 = value;
                OnPropertyChanged("Block4");
            }
        }

      


        public int Stage
        {
            get { return _Stage; }
            set
            {
                if (value == _Stage) return;
                _Stage = value;
                StageRev(value);
               
            }



        }

      

        public ViewModelTC()
        {
           
            T1 = new TCModel();
            lst.Add(T1);
            T2 = new TCModel();
            lst.Add(T2);
            T3 = new TCModel();
            lst.Add(T3);
            T4 = new TCModel();
            lst.Add(T4);
            T5 = new TCModel();
            lst.Add(T5);
            T6 = new TCModel();
            lst.Add(T6);
            T7 = new TCModel();
            lst.Add(T7);
            T8 = new TCModel();
            lst.Add(T8);
            T9 = new TCModel();
            lst.Add(T9);
            T10 = new TCModel();
            lst.Add(T10);
            T11 = new TCModel();
            lst.Add(T11);
            T12 = new TCModel();
            lst.Add(T12);
            T13 = new TCModel();
            lst.Add(T13);
            T14 = new TCModel();
            lst.Add(T14);
            T15 = new TCModel();
            lst.Add(T15);
            T16 = new TCModel();
            lst.Add(T16);
            T17 = new TCModel();
            lst.Add(T17);
            T18 = new TCModel();
            lst.Add(T18);
            T19 = new TCModel();
            lst.Add(T19);
            T20 = new TCModel();
            lst.Add(T20);
            T21 = new TCModel();
            lst.Add(T21);
            T22 = new TCModel();
            lst.Add(T22);
            T23 = new TCModel();
            lst.Add(T23);
            T24 = new TCModel();
            lst.Add(T24);

           
            



            StageRev(1);


        }
        private void StageRev( int page)
        {

            WagoIO.Instance.SetListTC(lst, page);

            T1.StageRev("АВАРИЯ^НОСИТЕЛЯ", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T2.StageRev("", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T3.StageRev("Р по^ПАДАЕТ", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T4.StageRev("СРАБОТАЛ^ТД", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T5.StageRev("", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T6.StageRev("ОТКАЗ^БВС", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
           T7.StageRev("Pса^ПАДАЕТ", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T8.StageRev("РАЗГЕРМ^СА", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T9.StageRev("РАЗГЕРМ^КЖО", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T10.StageRev("Р400^мм рт ст", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T11.StageRev("СОСТАВ^ВОЗДУХА", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
            T12.StageRev("РАЗГЕРМ^КНР", new SolidColorBrush(Colors.Red), new SolidColorBrush(Colors.Silver));
           T13.StageRev("PАЗГЕРМ^ПОДГОТ", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
            T14.StageRev("РАЗДЕЛЕН^по ГЦ", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
            T15.StageRev("ОТКРЫТ^КСД БО", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
            T16.StageRev("АВАРИЯ^ЦВМ", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
            T17.StageRev("ПРОГР^РАЗДЕЛЕН", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
            T18.StageRev("ТД^ПОДКЛ", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
            T19.StageRev("ПРИЗНАК^СПУСК", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
            T20.StageRev("СГ^РАЗАРР", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
            T21.StageRev("РАБОТА^ДПО", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
            T22.StageRev("СПУСК^ПО ГЦ", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
            T23.StageRev("ВЫБОР^ДПО", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
            T24.StageRev("РАБОТА^СКД", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
            Block1 = "TC-1";
            Block2 = "TC-2";
            Block3 = "TC-3";
            Block4 = "TC-4";

            if (page == 2)
            {
                T1.StageRev("БС", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T2.StageRev("БСР", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T3.StageRev("закрутка^СА", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T4.StageRev("ВЫЗОВ НА^СВЯЗЬ", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T5.StageRev("разреш^РРСТ", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T6.StageRev("U мало", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                
                T7.StageRev("ввод^ЗСП", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T8.StageRev("отказ^очист СА", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T9.StageRev("ПОСАДКА", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T10.StageRev("РБ^подкл", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T11.StageRev("отказ^очист БО", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                T12.StageRev("останов^КС-020", new SolidColorBrush(Colors.Yellow), new SolidColorBrush(Colors.Silver));
                
                T13.StageRev("БВC^готов", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T14.StageRev("ОСК", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T15.StageRev("ГСО", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T16.StageRev("2 СЕКЦИЯ^НАДДУВА", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T17.StageRev("2 СЕКЦИЯ^КДУ", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T18.StageRev("СА-БО^люк откр", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                
                T19.StageRev("РО АК", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T20.StageRev("ввод^ОСП", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T21.StageRev("подача^O2", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T22.StageRev("РУД", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T23.StageRev("УКВ^передача", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));
                T24.StageRev("МЕХАН^СОЕДИН", new SolidColorBrush(Colors.Green), new SolidColorBrush(Colors.Silver));

                Block1 = "TC-5";
                Block2 = "TC-6";
                Block3 = "TC-7";
                Block4 = "TC-8";
                
            }

        }
        

    }
}
