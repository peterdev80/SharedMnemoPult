using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ValueModel.BaseModel
{
    public abstract class VarClass : DependencyObject //, INotifyPropertyChanged
    {

        private string _VarName = String.Empty;
        private string _VarID = String.Empty;
        public int LoadVariableType { get; set; }
        public VarClass(string varID)
        {
            LoadVariableType = 0;
            _VarID = varID;
        }


        /// <summary>
        /// Имя переменной обмена
        /// </summary>
        public string VarName
        {
            get { return _VarName; }
            set
            {
                if (_VarName == value) return;
                _VarName = value;
              ///SetBinding(value, _VarID);/// Удаленно как устаревшое !!!!!!!!!!!!!!!!!!!
            }
        }
        public string VarID
        {
            get { return _VarID; }
        }

        protected abstract void SetBinding(string varObmen, string varID);


    /*    #region INotifyPropertyChanged Members

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
        */


    }
}
