using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace ValueModel.BaseType
{
    public class Valuer<T> : INotifyPropertyChanged where T : struct
    {
        protected T _ValueState;
        public T ValueState
        {
            get { return ras(); }
            set
            {
               // if (value == _ValueState) return;
               if (value.Equals(_ValueState)) return;
                _ValueState = value;
                OnPropertyChanged("ValueState");
            }
        }
        
        protected virtual T ras()
        {
            return _ValueState;
        }

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

    }
}
