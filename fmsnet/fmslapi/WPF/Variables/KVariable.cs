using System;
using System.Windows.Markup;
using System.ComponentModel;

namespace fmslapi.WPF.Variables
{
    /// <summary>
    /// Командная переменная
    /// </summary>
    [ContentProperty]                   // KVariable не имеет контентных свойств
    public class KVariable : Variable
    {
        private IKVariable _kv;

        internal new IVariable NativeVariable
        {
            get => _kv;
            set 
            {
                _kv = value as IKVariable;
                if (_kv == null)
                    throw new InvalidOperationException("Ошибка назначения переменной");
            }
        }

        [Browsable(false)]
        public new object Value
        {
            get
            {
#if DEBUG
                throw new InvalidOperationException("У переменной K типа не может быть значения");
#else
                return null;
#endif
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
#if DEBUG
                throw new InvalidOperationException("У переменной K типа не может быть значения");
#endif

            }
        }

        /// <summary>
        /// Активизирует команду
        /// </summary>
        /// <remarks>
        /// Непосредственная активизация осуществляется в общем порядке при отправке изменений
        /// </remarks>
        public void Set()
        {
            _kv.Set();

            if (AutoSend)
                Manager.SendChanges(this);

            var ev = new VariableChangedEventArgs(VariableChangedEvent, this);
            RaiseEvent(ev);
        }

        /// <summary>
        /// Команда активирована
        /// </summary>
        /// <remarks>
        /// Чтение обнуляет флаг активации
        /// </remarks>
        public bool IsFired => _kv.IsFired;

        public void Reset()
        {
#if !DEBUG
            if (_kv == null)
                return;
#endif
            _kv.Reset();
        }

        public void WaitOne()
        {
#if !DEBUG
            if (_kv == null)
                return;
#endif
            _kv.WaitOne();
        }

        public void WaitOne(TimeSpan TimeSpan)
        {
#if !DEBUG
            if (_kv == null)
                return;
#endif
            _kv.WaitOne(TimeSpan);
        }

        public bool WaitOne(int Milliseconds)
        {
            return _kv.WaitOne(Milliseconds);
        }
    }
}
