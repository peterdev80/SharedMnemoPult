using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;

namespace VirtualPultValves.Model
{
    public class TCModel : INotifyPropertyChanged
    {
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


            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members
        private string _tn = String.Empty;
        private Brush _PassiveColor;
        private Brush _ActiveColor;
        private int _ts = 12;
        private bool _ValTC;
        private Brush _TCColor;
        public TCModel()
        {
            _PassiveColor = new SolidColorBrush(Colors.Silver);
            _ActiveColor = new SolidColorBrush(Colors.Red);
            TextSize = 11;
        }
        #region Public Property
        public int TextSize
        {
            get { return _ts; }
            set
            {
                if (value != _ts)
                    _ts = value;
                this.OnPropertyChanged("TextSize");
            }

        }
        /// <summary>
        /// Надпись на транспоранте
        /// </summary>
        public string TransporantName
        {
            get { return ((string)_tn).Replace("^", Environment.NewLine); }
            set
            {
                if (value != _tn)
                {
                    _tn = value;
                    OnPropertyChanged("TransporantName");
                }
            }
        }
        /// <summary>
        /// Цвет активного транспоранта
        /// </summary>
        public Brush ActiveColor
        {
            get { return _ActiveColor; }
            set { if (value == _ActiveColor) return;
            _ActiveColor = value;
            if (ValTC) TCColor = _ActiveColor;
            }
        }

        /// <summary>
        /// Цвет пасивного транспоранта
        /// </summary>
        public Brush PassiveColor
        {
            get { return _PassiveColor; }
            set
            {
                if (value == _PassiveColor) return;
                _PassiveColor = value;
                if (!ValTC) TCColor = PassiveColor;
            }
        }


        public Boolean ValTC
        {
            get { return _ValTC; }
            set 
            {
                _ValTC = value;
                if (_ValTC)
                    TCColor = ActiveColor;
                else
                    TCColor = PassiveColor;
                
                
            }
        }

        public Brush TCColor
        {
            get { return _TCColor; }
            set { _TCColor = value; OnPropertyChanged("TCColor"); }
        }

        #endregion


        public void StageRev(string txt, Brush _acolor, Brush _pcolor)
        {
            this.ActiveColor = _acolor;
            this.PassiveColor = _pcolor;
            this.TransporantName = txt;

        }
    }
}
