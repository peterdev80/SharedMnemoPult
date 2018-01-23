using System;
using System.ComponentModel;
using System.Windows.Forms;
// ReSharper disable All

namespace fmslapi.Components
{
    public delegate void VariableChanged(object Sender, EventArgs e);

    [DesignTimeVisible(false)]
    public class FMSVariable : Component
    {
        #region Частные данные
        private FMSLVarManager _manager;
        private string _name;
        protected IVariable _nativevariable;
        protected bool _autosend;
        protected bool _needlocalfeedback;
        #endregion

        #region Общие свойства
        public FMSLVarManager Manager 
        {
            get { return _manager; }
            set
            {
                _manager = value;
                value.RegisterVariableInstance(this);
            }
        }

        /// <summary>
        /// Имя переменной
        /// </summary>
        public string VarName
        {
            get { return _name; }
            set
            {
                if (!string.IsNullOrEmpty(_name))
                    throw new InvalidOperationException(@"Попытка переназначения имени переменной");

                _name = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        public bool AutoSend
        {
            get
            {
                return _autosend;
            }
            set
            {
                _autosend = value;
                if (_nativevariable != null)
                    _nativevariable.AutoSend = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        public bool NeedLocalFeeback
        {
            get
            {
                return _needlocalfeedback;
            }
            set
            {
                _needlocalfeedback = value;
                if (_nativevariable != null)
                    _nativevariable.NeedLocalFeedback = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public IVariable NativeVariable
        {
            get { return _nativevariable; }
            set
            {
                if (_nativevariable != null)
                    throw new InvalidOperationException(@"Попытка переназначения интерфейса переменной");

                _nativevariable = value;
                _nativevariable.AutoSend = _autosend;
                _nativevariable.NeedLocalFeedback = _needlocalfeedback;
                _nativevariable.VariableChanged += RaiseVariableChanged;
            }
        }
        #endregion

        #region Общие методы
        public void Unregister()
        {
            if (_manager != null)
            {
                _manager.UnregisterVariableInstance(this);
            }
        }
        #endregion

        #region Перегруженные методы
        protected override void Dispose(bool disposing)
        {
            Unregister();
            base.Dispose(disposing);
        }
        #endregion

        #region События
        private event VariableChanged _varchanged;
        private readonly object varchangedlock = new object();

        public event VariableChanged VariableChanged
        {
            add
            {
                lock (varchangedlock)
                {
                    _varchanged += value;

                    if (_varchanged.GetInvocationList().Length == 1)
                    {
                        _manager.IHaveChangeEvent(this);
                    }
                }
            }
            remove
            {
                lock (varchangedlock)
                {
                    _varchanged -= value;

                    if (_varchanged.GetInvocationList().Length == 0)
                    {
                        _manager.IDontHaveChangeEvent(this);
                    }
                }
            }
        }

        private void RaiseVariableChanged(IVariable Sender, bool IsInit)
        {
            if (_varchanged == null)
                return;

            var form = _varchanged.Target as Form;
            if (form != null)
            {
                form.BeginInvoke(_varchanged, this, EventArgs.Empty);
            }
            else
            {
                _varchanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Внутренние методы
        /// <summary>
        /// Отключает от объектов внутренних переменных
        /// </summary>
        internal void Detach()
        {
            _nativevariable.VariableChanged -= RaiseVariableChanged;
            _nativevariable = null;
        }
        #endregion
    }

    #region Уточнения для типов переменных

    #region BVar
    public class BVar : FMSVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool Value
        {
            get 
            {
                try
                {
                    return ((IBoolVariable)_nativevariable).Value;
                }
                catch (InvalidCastException) { return false; }
                catch (NullReferenceException) { return false; }
            }
            set { ((IBoolVariable)_nativevariable).Value = value; }
        }
    }
    #endregion

    #region SVar
    public class SVar : FMSVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string Value
        {
            get 
            {
                try
                {
                    return ((IStringVariable)_nativevariable).Value;
                }
                catch (InvalidCastException) { return ""; }
                catch (NullReferenceException) { return ""; }
            }
            set { ((IStringVariable)_nativevariable).Value = value; }
        }
    }
    #endregion

    #region IVar
    public class IVar : FMSVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int Value
        {
            get 
            {
                try
                {
                    return ((IIntVariable)_nativevariable).Value;
                }
                catch (InvalidCastException) { return 0; }
                catch (NullReferenceException) { return 0; }
            }
            set { ((IIntVariable)_nativevariable).Value = value; }
        }

        public static implicit operator float(IVar v)
        {
            return v.Value;
        }

        public static implicit operator decimal(IVar v)
        {
            return v.Value;
        }

        public static implicit operator double(IVar v)
        {
            return v.Value;
        }

        public static implicit operator int(IVar v)
        {
            return v.Value;
        }
    }
    #endregion

    #region FVar
    public class FVar : FMSVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public float Value
        {
            get 
            {
                try
                {
                    return ((IFloatVariable)_nativevariable).Value;
                }
                catch (NullReferenceException) { return 0F; }
                catch (InvalidCastException) { return 0F; }
            }
            set { ((IFloatVariable)_nativevariable).Value = value; }
        }

        public static implicit operator float(FVar v)
        {
            return v.Value;
        }

        public static implicit operator decimal(FVar v)
        {
            return (decimal)v.Value;
        }

        public static implicit operator double(FVar v)
        {
            return (double)v.Value;
        }

        public static implicit operator int(FVar v)
        {
            return (int)v.Value;
        }
    }
    #endregion

    #region DVar
    public class DVar : FMSVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public double Value
        {
            get
            {
                try
                {
                    return ((IDoubleVariable)_nativevariable).Value;
                }
                catch (InvalidCastException) { return 0D; }
                catch (NullReferenceException) { return 0D; }
            }
            set { ((IDoubleVariable)_nativevariable).Value = value; }
        }

        public static implicit operator float(DVar v)
        {
            return (float)v.Value;
        }

        public static implicit operator decimal(DVar v)
        {
            return (decimal)v.Value;
        }

        public static implicit operator double(DVar v)
        {
            return v.Value;
        }

        public static implicit operator int(DVar v)
        {
            return (int)v.Value;
        }
    }
    #endregion

    #region CVar
    public class CVar : FMSVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public char Value
        {
            get 
            {
                try
                {
                    return ((ICharVariable)_nativevariable).Value;
                }
                catch (InvalidCastException) { return ' '; }
                catch (NullReferenceException) { return ' '; }
            }
            set { ((ICharVariable)_nativevariable).Value = value; }
        }

        public static implicit operator int(CVar v)
        {
            return v.Value;
        }

        public static implicit operator string(CVar v)
        {
            return new string(v.Value, 1);
        }
    }
    #endregion

    #region KVar
    public class KVar : FMSVariable
    {
        /// <summary>
        /// Активизирует команду
        /// </summary>
        public void Set()
        {
            ((IKVariable)_nativevariable).Set();
        }

        [Browsable(false)]
        new public event VariableChanged VariableChanged
        {
            add
            {
                throw new InvalidOperationException("В классе KVar событие VariableChanged не поддерживается. Используйте CommandInvoked.");
            }
// ReSharper disable ValueParameterNotUsed
            remove { }
// ReSharper restore ValueParameterNotUsed
        }

        private EventHandler _evthnd;

        [Browsable(true)]
        public event EventHandler CommandInvoked
        {
            add
            {
                base.VariableChanged += KVar_VariableChanged;
                _evthnd += value;
            }
            remove
            {
                base.VariableChanged -= KVar_VariableChanged;
                _evthnd -= value;
            }
        }

        private void KVar_VariableChanged(object Sender, EventArgs e)
        {
            if (_evthnd == null)
                return;

            foreach (var _invoke in _evthnd.GetInvocationList())
            {
                var frm = _invoke.Target as Form;
                if (frm != null)
                {
                    frm.BeginInvoke(_invoke, Sender, e);
                }
                else
                {
                    _invoke.DynamicInvoke(Sender, e);
                }
            }
        }
    }
    #endregion

    #endregion
}
