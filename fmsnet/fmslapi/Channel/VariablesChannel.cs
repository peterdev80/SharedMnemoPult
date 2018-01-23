using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace fmslapi.Channel
{
    /// <summary>
    /// Клиентский объект, представляющий подключение к каналу
    /// </summary>
    internal partial class Channel : IVariablesChannel, IVariablesChannelSupport, IVariablesChannelExtensions
    {
        #region Частные данные
        /// <summary>
        /// Список объектов ожидания для получения данных переменной
        /// </summary>
        private readonly Dictionary<string, EventWaitHandle> _waitingvars = new Dictionary<string, EventWaitHandle>();

        /// <summary>
        /// Список зарегистрированных переменных канала по ключу - имени переменной
        /// </summary>
        private readonly Dictionary<string, Variable> _varlist = new Dictionary<string, Variable>();

        /// <summary>
        /// Список зарегистрированных переменных канала по ключу - индексу переменной
        /// </summary>
        private readonly Dictionary<int, Variable> _varlistn = new Dictionary<int, Variable>();

        /// <summary>
        /// Список изменившихся с последней отсылки изменений переменных
        /// </summary>
        private readonly HashSet<Variable> _changedvars = new HashSet<Variable>();

        /// <summary>
        /// Блокировка доступа к списку переменных
        /// </summary>
        private readonly ReaderWriterLockSlim _varlistnlock = new ReaderWriterLockSlim();

        /// <summary>
        /// Имя карты переменных
        /// </summary>
        private readonly string _varmap;
        #endregion

        #region IVariablesChannel
        /// <summary>
        /// Отправляет изменившуюся переменную
        /// </summary>
        /// <remarks>
        /// Только если она действительно была изменена
        /// </remarks>
        public void SendChanges(IVariable Variable)
        {
            if (Variable is Variable v)
                SendChanges(new[] { v });
        }

        /// <summary>
        /// Отправляет ограниченный пакет изменившихся переменных
        /// </summary>
        public void SendChanges()
        {
            SendChanges((IEnumerable<Variable>)null);
        }

        /// <summary>
        /// Отправляет изменившиеся переменные в канал
        /// </summary>
        /// <param name="Source">
        /// Отправляются только переменные, присутствующие в этом списке
        /// </param>
        public void SendChanges(IEnumerable<IVariable> Source)
        {
            SendChanges(Source.OfType<Variable>());
        }

        /// <summary>
        /// Отправляет изменившиеся переменные в канал
        /// </summary>
        /// <param name="Source">
        /// Отправляются только переменные, присутствующие в этом списке
        /// </param>
        private void SendChanges(IEnumerable<Variable> Source)
        {
            Variable[] sendlist;

            if (!_active)
                return;

            lock (_changedvars)
            {
                if (Source == null)
                {
                    sendlist = _changedvars.ToArray();
                    _changedvars.Clear();
                }
                else
                {
                    sendlist = _changedvars.Intersect(Source).ToArray();
                    foreach (var ve in sendlist)
                        _changedvars.Remove(ve);
                }
            }

            if (sendlist.Length == 0)
                return;

            var ms = new MemoryStream();
            var bwrtr = new BinaryWriter(ms);

            bwrtr.Write('C');
            bwrtr.Write(sendlist.Length);

            try
            {
                foreach (var var in sendlist)
                {
                    bwrtr.Write(var.Index);
                    bwrtr.Write(var.NeedLocalFeedback);
                }

                MessageToServer(ms.ToArray());
            }
            catch (InvalidOperationException) { }
        }
        #endregion

        #region Методы GetVariable
        // ReSharper disable ParameterHidesMember
        public IByteArrayVariable GetByteArrayVariable(string Name)
        {
            return GetVariable<IByteArrayVariable>(Name);
        }

        public IStringVariable GetStringVariable(string Name)
        {
            return GetVariable<IStringVariable>(Name);
        }

        public IIntVariable GetIntVariable(string Name)
        {
            return GetVariable<IIntVariable>(Name);
        }

        public ILongVariable GetLongVariable(string Name)
        {
            return GetVariable<ILongVariable>(Name);
        }

        public IKVariable GetKVariable(string Name)
        {
            return GetVariable<IKVariable>(Name);
        }

        public ICharVariable GetCharVariable(string Name)
        {
            return GetVariable<ICharVariable>(Name);
        }

        public IBoolVariable GetBoolVariable(string Name)
        {
            return GetVariable<IBoolVariable>(Name);
        }

        public ITriggerVariable GetTriggerVariable(string Name)
        {
            return GetVariable<ITriggerVariable>(Name);
        }

        public IFloatVariable GetFloatVariable(string Name)
        {
            return GetVariable<IFloatVariable>(Name);
        }

        public IDoubleVariable GetDoubleVariable(string Name)
        {
            return GetVariable<IDoubleVariable>(Name);
        }

        public IWatchDogVariable GetWatchDogVariable(string Name)
        {
            return GetVariable<IWatchDogVariable>(Name);
        }

        public T GetVariable<T>(string Name) where T : class, IVariable
        {
            try
            {
                _varlistnlock.EnterReadLock();

                if (_varlist.ContainsKey(Name))
                    return _varlist[Name] as T;
            }
            finally
            {
                _varlistnlock.ExitReadLock();
            }
            
            return new Variable(this, Name) as T;
        }

        public IVariable GetVariable(string Name)
        {
            return GetVariable<IVariable>(Name);
        }

        public void RegetVariable(IVariable Variable)
        {
            RegisterVariable(Variable as Variable);
        }
        // ReSharper restore ParameterHidesMember
        #endregion

        #region IVariablesChannelSupport Members
        /// <summary>
        /// Добавляет переменную в список измененных переменных
        /// </summary>
        /// <param name="Variable">Переменная</param>
        public void AddChangedVariable(Variable Variable)
        {
            lock (_changedvars)
                _changedvars.Add(Variable);
        }
        
        /// <summary>
        /// Регистрирует переменную для получения данных
        /// </summary>
        /// <param name="Variable">Переменная</param>
        public void RegisterVariable(Variable Variable)
        {
            try
            {
                _varlistnlock.EnterWriteLock();

                _varlist[Variable.VariableName] = Variable;
            }
            finally
            {
                _varlistnlock.ExitWriteLock();
            }


            if (!_active)
                return;

            EventWaitHandle evt;

            lock (_waitingvars)
            {
                var name = Variable.VariableName;

                if (!_waitingvars.TryGetValue(name, out evt))
                {
                    evt = new EventWaitHandle(false, EventResetMode.ManualReset);
                    _waitingvars[name] = evt;
                }
            }

            MessageToServer('D', (UInt16)Encoding.UTF8.CodePage, Variable.VariableName, false);

            // Ожидание ответа с действительным типом и индексом переменной
            evt.WaitOne();

            evt.Close();

            lock (_waitingvars)
                _waitingvars.Remove(Variable.VariableName);

            // Если сигнал сработал по завершению канала
            // ничего делать не надо
            if (!_active)
                return;

            // Если индекс равен -1 - запрошена несуществующая переменная
            if (Variable.Index == -1)
                return;

            try
            {
                _varlistnlock.EnterWriteLock();

                _varlistn[Variable.Index] = Variable;
            }
            finally
            {
                _varlistnlock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Сохраняет значение переменной в постоянном хранилище
        /// </summary>
        public void SavePersistentVariable(IVariable Variable)
        {
            if (Variable is Variable v)
                MessageToServer('L', v.Index);
        }
        #endregion

        #region Обработка событий
        /// <summary>
        /// Обработка изменения состояния канала
        /// </summary>
        /// <param name="state">Новое состояние канала</param>
        private void VarStateChanged(ChannelStateChangedStates state)
        {
            switch (state)
            {
                case ChannelStateChangedStates.CantConnect:
                    RaiseDelegate(_vstatechanged, ChannelStateChangedStates.CantConnect);
                    break;
                
                case ChannelStateChangedStates.Connected:

                    // Повторная регистрация в случае появления соединения с сервером
                    Variable[] arr;
                    try
                    {
                        _varlistnlock.EnterReadLock();

                        arr = _varlist.Values.ToArray();
                    }
                    finally
                    {
                        _varlistnlock.ExitReadLock();
                    }

                    foreach (var v in arr)
                    {
                        RegetVariable(v);
                    }

                    RaiseDelegate(_vstatechanged, ChannelStateChangedStates.Connected);
                    break;

                case ChannelStateChangedStates.Disconnected:
                    RaiseDelegate(_vstatechanged, ChannelStateChangedStates.Disconnected);
                    break;

                case ChannelStateChangedStates.FirstConnect:
                    RaiseDelegate(_vstatechanged, ChannelStateChangedStates.FirstConnect);
                    break;
            }
        }
        #endregion

        unsafe void IVariablesChannelExtensions.SendRawMessage(void* Buffer, UInt32 Length)
        {
            var bf = new byte[Length];
            Marshal.Copy(new IntPtr(Buffer), bf, 0, (int)Length);

            MessageToServer(bf);
        }

        unsafe void IVariablesChannelExtensions.AddRawSubscriber(Delegate Receiver)
        {
            _additionalsubscribers = data =>
                {
                    fixed (byte* ptr = &data[0])
                    {
                        Receiver.DynamicInvoke(new IntPtr(ptr), (UInt32)data.Length);
                    }
                };
        }

        #region Поддержка снимков

        public VariablesSnapshot CreateSnapshot(string SnapshotName)
        {
            return CreateSnapshot(SnapshotName, null);
        }

        public VariablesSnapshot CreateSnapshot(string SnapshotName, IEnumerable<IVariable> Source)
        {
            if (string.IsNullOrWhiteSpace(SnapshotName))
                throw new InvalidOperationException("Имя снимка не может быть пустым");

            var vs = new VariablesSnapshot(Source, this, SnapshotName);
            
            if (Source != null)
                vs.MakeSnapshot();

            return vs;
        }

        public void MakeSnapshot(string SnapshotName, IVariable[] Variables)
        {
            if (string.IsNullOrWhiteSpace(SnapshotName))
                throw new InvalidOperationException("Имя снимка не может быть пустым");

            if (Variables == null)
                return;

            MessageToServer('N', SnapshotName, (UInt16)Variables.Length, Variables);
        }

        public void RestoreSnapshot(string SnapshotName)
        {
            if (string.IsNullOrWhiteSpace(SnapshotName))
                throw new InvalidOperationException("Имя снимка не может быть пустым");

            MessageToServer('O', SnapshotName, false, false);
        }

        #endregion
    }
}
