using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ValueModel.BaseModel;

namespace ValueModel.BaseModelCommand
{
    public abstract class VarSendClass 
    {
        private string _VarName = String.Empty;
        private string _VarID = String.Empty;
        public int LoadVariableType { get; set; }
        public VarSendClass(string varID)
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
               
            }
        }
        public string VarID
        {
            get { return _VarID; }
        }
        
        #region Command
        private RelayCommand _setValvesValueCommand;

        public ICommand SendCommand
        {
            get
            {
                if (_setValvesValueCommand == null)
                {

                    _setValvesValueCommand = new RelayCommand(param => SendValue(param), param => CanValue());
                }
                return _setValvesValueCommand;
            }
        }
        protected virtual bool CanValue()
        {

            return true;
        }
        protected abstract void SendValue(object PARAM);

        #endregion
    }
}
