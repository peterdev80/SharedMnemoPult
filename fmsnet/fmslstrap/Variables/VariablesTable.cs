using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections;
using fmslstrap.Channel;
using System.Diagnostics;
using v = fmslstrap.Variables.VarTypes;
using fmslstrap.Variables.VarTypes;

namespace fmslstrap.Variables
{
    /// <summary>
    /// Таблица переменных
    /// </summary>
    public partial class VariablesTable : IEnumerable<Variable>
    {
        #region Конструкторы
        /// <summary>
        /// Создает новую таблицу переменных
        /// </summary>
        /// <param name="Name"></param>
        private VariablesTable(string Name)
        {
            this.Name = Name;
            _varlist = new Dictionary<string, Variable>();
            _varlistn = new Dictionary<uint, Variable>();
            _alltables.Add(this);
        }

        /// <summary>
        /// Создает объединенную таблицу переменных
        /// </summary>
        /// <param name="Map">Каталог таблиц переменных</param>
        /// <param name="Reader">Исходный ресурсный поток данных</param>
        public static void MergeVariablesTable(IDictionary<string, VariablesTable> Map, BinaryReader Reader)
        {
            VariablesTable tbl = null;

            try
            {
                var c = Reader.ReadByte();      // Количество категорий
                var name = Reader.ReadString(); // Имя таблицы

                tbl = new VariablesTable(name);
                tbl._vlock.EnterWriteLock();
                Map.Add(name, tbl);

                for (var i = 0; i < c; i++)
                {
                    var vc = Reader.ReadUInt16();       // Кол-во переменных
                    var cn = Reader.ReadString();       // Имя категории
                    tbl._categories.Add(cn);

                    for (var j = 0; j < vc; j++)
                    {
                        var vname = Reader.ReadString();

                        var var = Variable.New(tbl, null, vname,
                                               (char)Reader.ReadByte(),
                                               Reader.ReadUInt16(),
                                               Reader.ReadUInt16(),
                                               Reader.ReadUInt16(),
                                               Reader.ReadString(), cn);

                        tbl._varlist.Add(vname, var);
                        tbl._varlistn.Add(var.VarNum, var);
                        tbl._weakinit_vars.Add(var.VarNum, 0);
                    }
                }

                // Внутренний список сторожевых переменных таблицы
                // ReSharper disable once RedundantNameQualifier
                tbl._watchdogvars = tbl._varlist.Values.OfType<VarTypes.WVar>().ToArray();
            }
            finally
            {
                tbl?._vlock.ExitWriteLock();
            }
        }
        #endregion

        #region Работа с переменными
        /// <summary>
        /// Возвращает объект переменной
        /// </summary>
        /// <param name="Variable">Имя переменной</param>
        /// <returns>Переменная</returns>
        public Variable GetVariable(string Variable)
        {
            try
            {
                _vlock.EnterReadLock();

                _varlist.TryGetValue(Variable, out var v);

                return v;
            }
            finally 
            { 
                _vlock.ExitReadLock(); 
            }
        }

        /// <summary>
        /// Возвращает объект переменной
        /// </summary>
        /// <returns>Переменная</returns>
        public Variable GetVariable(uint Index)
        {
            try
            {
                _vlock.EnterReadLock();

                _varlistn.TryGetValue(Index, out var v);

                return v;
            }
            finally 
            { 
                _vlock.ExitReadLock(); 
            }
        }

        /// <summary>
        /// Поиск переменной в глобальном каталоге
        /// </summary>
        /// <param name="Index">Индекс переменной</param>
        /// <param name="Table">Таблица, в которой находится переменная</param>
        /// <returns>Переменная с заданным глобальным индексом</returns>
        public static Variable GlobalGetVariable(uint Index, out VariablesTable Table)
        {
            var z = (from tbl in _alltables 
                     let vn = tbl._varlistn
                     where vn.ContainsKey(Index)
                     select new { Table = tbl, Variable = vn[Index] }).FirstOrDefault();

            if (z == null)
            {
                Table = null;
                return null;
            }

            Table = z.Table;
            return z.Variable;
        }

        /// <summary>
        /// Установка значения переменной по умолчанию
        /// </summary>
        /// <param name="Name">Имя переменной</param>
        /// <param name="Input">Поток со значением по умолчанию</param>
        public static void SetDefaultValue(string Name, BinaryReader Input)
        {
            var z = (from tbl in _alltables
                     let vn = tbl._varlist
                     where vn.ContainsKey(Name)
                     select new { Var = vn[Name], Table = tbl }).FirstOrDefault();

            if (z == null)
                return;

            lock (z.Table._weakinit_vars)
                z.Var.ParseDelta(Input, !z.Table._weakinit_vars.ContainsKey(z.Var.VarNum));
        }
        #endregion

        #region Прием дельта пакетов
        /// <summary>
        /// Событие происходит после приема и обработки дельта пакета
        /// </summary>
        public event VarChanged OnVarChanged;

        /// <summary>
        /// Генерирует событие изменения пакета переменных
        /// и глобальное событие изменения переменных
        /// </summary>
        /// <param name="ChangeSet">Пакет измененных переменных</param>
        /// <param name="IsInit">Признак пакета начальной инициализации значений переменных</param>
        public void RaiseOnVarChanged(IEnumerable<Variable> ChangeSet, bool IsInit)
        {
            var cs = ChangeSet as Variable[] ?? ChangeSet.ToArray();

            OnVarChanged?.Invoke(cs, IsInit);

            VariablesManager.RaiseGlobalVarChanged(cs, IsInit);
        }

        /// <summary>
        /// Чтение пакета с данными канала
        /// </summary>
        private void VarChanDataReceived(ChanConfig Channel, DataPacket Packet)
        {
            using (var rdr = new BinaryReader(new MemoryStream(Packet.Data)))
            {
                var hash = rdr.ReadUInt16();

                // Только пакеты, сформированные на основе таблицы с хеш кодом, совпадающим
                // с нашим, могут быть корректно обработаны
                if (hash != VariablesManager.GlobalVariablesHash)
                {
                    Manager.InvalidVariablesSignature();
                    return;
                }

                var type = rdr.ReadByte();
                switch (type)
                {
                    case 1:
                        // Пакет с изменениями
                        ParseVarChangeChunks(rdr);
                        break;

                    case 2:
                        // Инициализация данных
                        ParseVarInitChunks(rdr);
                        break;
                }
            }
        }

        /// <summary>
        /// Разбор дельта пакета
        /// </summary>
        /// <param name="rdr">Данные дельта пакета</param>
        private void ParseVarChangeChunks(BinaryReader rdr)
        {
            var chlist = new List<Variable>();

            try
            {
                _vlock.EnterReadLock();

                while (true)
                {
                    try
                    {
                        var index = (uint)rdr.ReadInt32();

                        if (index == 0) 
                            break;

                        lock (_weakinit_vars)
                            _weakinit_vars.Remove(index);

                        var var = _varlistn[index];

                        var.ParseDelta(rdr, false);

                        if (!(var is WVar))
                            chlist.Add(var);
                    }
                    catch (EndOfStreamException) { break; }
                }
            }
            finally
            {
                _vlock.ExitReadLock();
            }

            if (chlist.Count > 0)
                RaiseOnVarChanged(chlist, false);
        }

        private void ParseVarInitChunks(BinaryReader rdr)
        {
            Debug.WriteLine(@"InitVars received. Channel={0}", Name);
       
            var chlist = new List<Variable>();

            // uptime - время, прошедшее от старта fmsldr.exe
            var uptime = rdr.ReadUInt32();

            try
            {
                _vlock.EnterReadLock();

                while (true)
                {
                    try
                    {
                        var index = (uint)rdr.ReadInt32();

                        if (index == 0)
                            break;

                        var so = false;

                        lock (_weakinit_vars)
                        {
                            if (_weakinit_vars.TryGetValue(index, out var cupt))
                            {
                                if (cupt >= uptime)
                                    // Принята переменная от хоста с меньшим временем работы чем существующая
                                    so = true;
                                else
                                    // Принята переменная от хоста с большим временем работы
                                    _weakinit_vars[index] = uptime;
                            }
                            else
                                // Уже есть определенное значение переменной
                                so = true;
                        }

                        var var = _varlistn[index];

                        var.ParseDelta(rdr, so);

                        chlist.Add(var);
                    }
                    catch (EndOfStreamException) { break; }
                }
            }
            finally
            {
                _vlock.ExitReadLock();
            }

            if (chlist.Count > 0)
                RaiseOnVarChanged(chlist, true);
        }
        #endregion

        #region Обработка событий
        /// <summary>
        /// Обработка изменения состояния несущего канала
        /// </summary>
        /// <param name="Channel">Несущий канал</param>
        /// <param name="e">Данные состояния</param>
        private void BaseChannelChanged(ChanConfig Channel, ChannelChangeEventArgs e)
        {
            Debug.WriteLine(@"OnChannelChange. Channel={0}; NewHost={1}", Channel.Name, e.NewHost);

            switch (e.ChangeType)
            {
                case ChannelChangeType.AddHost:
                    if (Channel.ChanType == ChannelType.Variables)
                    {
                        var rhost = e.NewHost;
                        ThreadPool.QueueUserWorkItem(x => SendInits(Channel, rhost));
                    }
                    break;
            }
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// При необходимости подключается к событиям несущего канала
        /// </summary>
        public void CheckOnline()
        {
            if (_subscribercnt++ != 0) 
                return;

            _associatedchannel = Manager.SubscribeToChannel(Name, VarChanDataReceived, ChannelType.Variables);
            _associatedchannel.OnChannelChange += BaseChannelChanged;
        }

        /// <summary>
        /// При необходимости отключается от событий несущего канала переменных
        /// </summary>
        public void CheckOffline()
        {
            if (--_subscribercnt != 0) 
                return;

            _associatedchannel.OnChannelChange -= BaseChannelChanged;
            Manager.UnSubscribeFromChannel(_associatedchannel.Name, VarChanDataReceived);
            _associatedchannel = null;
            
            lock(_weakinit_vars)
            {
                foreach (var v in _varlistn)
                    _weakinit_vars[v.Key] = 0;
            }
        }

        public IEnumerable<Variable> SelectOwnVars(IEnumerable<Variable> Source)
        {
            return Source.Intersect(_varlist.Values).ToArray();
        }

        /// <summary>
        /// Сброс в неактивное состояние всех сторожевых переменных
        /// </summary>
        // ReSharper disable once RedundantNameQualifier
        public void ResetWDogs(IEnumerable<v.WVar> Vars)
        {
            var src = Vars == null ? _watchdogvars : _watchdogvars.Intersect(Vars);

            // ReSharper disable once RedundantNameQualifier
            var wvs = src as v.WVar[] ?? src.ToArray();
            foreach (var v in wvs)
                v.TotalReset();

            RaiseOnVarChanged(wvs, false);
        }
        #endregion

        #region Общие свойства
        /// <summary>
        /// Имя канала
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Список всех категорий
        /// </summary>
        public string[] Categories
        {
            get { return _categories.ToArray(); }
        }
        #endregion

        #region Списки изменений
        /// <summary>
        /// Отсылает изменившиеся переменные в канал
        /// </summary>
        /// <param name="Subset">Ограничивает отсылаемые параметры имеющимися в этом наборе</param>
        public void SendChanges(IEnumerable<Variable> Subset)
        {
            var vars = new Variable[0];

            if (Subset != null)
                vars = _varlistn.Values.Intersect(Subset).ToArray();

            if (vars.Length == 0)
                return;
            
            lock(_weakinit_vars)
            {
                foreach (var v in vars)
                    _weakinit_vars.Remove(v.VarNum);
            }

            if (_associatedchannel == null)
                return;

            BinaryWriter lwr = null;

            var stream = new MemoryStream();
            var wr = new BinaryWriter(stream);
            wr.Write(VariablesManager.GlobalVariablesHash);
            stream.WriteByte(1);

            foreach (var var in vars)
            {
                // Большие переменные пакуются отдельно
                if (var.ActualSizeOf > 1000)
                {
                    if (lwr == null)
                    {
                        lwr = new BinaryWriter(new MemoryStream());
                        lwr.Write(VariablesManager.GlobalVariablesHash);
                        lwr.Write((byte)1);
                    }

                    var.PackVariable(lwr.BaseStream);

                    continue;
                }

                var.PackVariable(stream);

                if (stream.Length <= 1250) 
                    continue;

                wr.Write(0);
                _associatedchannel.SendMessage(stream.ToArray(), Name);

                stream.SetLength(0);
                stream.Position = 0;
                wr.Write(VariablesManager.GlobalVariablesHash);
                stream.WriteByte(1);
            }

            if (stream.Position >= 6)
            {
                wr.Write(0);

                _associatedchannel.SendMessage(stream.ToArray(), Name);
            }

            // При необходимости отправляем все большие переменные разом (наверняка через TCP)
            if (lwr == null)
                return;

            wr.Write(0);

            _associatedchannel.SendMessage(((MemoryStream)lwr.BaseStream).ToArray(), Name);
        }

        /// <summary>
        /// Отправляет пакет инициализации значений переменных хосту
        /// </summary>
        /// <param name="Channel">Несущий канал</param>
        /// <param name="ToHost">Имя хоста-цели</param>
        private void SendInits(ChanConfig Channel, string ToHost)
        {
            // Передаются значения имеющие гарантированное значение
            // Т.е. которые после старта были либо установлены клиентским приложением
            // либо получены обычным порядком. В конечном итоге - те, которые
            // отсутствуют в словаре _weakinit_vars.
            // Также не передаются переменные типа K т.к. они не имеют значения, а
            // фактом передачи инициируют действие.

            var stream = new MemoryStream();
            var wr = new BinaryWriter(stream);

            // uptime - время, прошедшее от старта fmsldr.exe
            var uptime = (uint)(DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMilliseconds;

            wr.Write(VariablesManager.GlobalVariablesHash);
            stream.WriteByte(2);
            wr.Write(uptime);

            byte[] vi;
            IEnumerable<Variable> sends;
            lock (_weakinit_vars)
                sends = _varlist.Values.Where(v => !v.Type.StartsWith("K") && !v.Type.StartsWith("W") && !_weakinit_vars.ContainsKey(v.VarNum)).ToArray();

            foreach (var var in sends)
            {
                var.PackVariable(stream);

                if (stream.Length <= 1250)
                    continue;

                wr.Write(0);

                vi = stream.ToArray();
                Channel.SendMessage(vi, ToHost);
                Channel.SendMessage(vi, ToHost);

                stream.SetLength(0);
                stream.Position = 0;
                wr.Write(VariablesManager.GlobalVariablesHash);
                stream.WriteByte(2);
                wr.Write(uptime);
            }

            if (stream.Position < 8)
                return;

            wr.Write(0);

            vi = stream.ToArray();
            Channel.SendMessage(vi, ToHost);
            Channel.SendMessage(vi, ToHost);

            Debug.WriteLine(@"VarInits send. Channel={1}; ToHost={0}", ToHost, Name);
        }
        #endregion

        #region IEnumerable Members

        IEnumerator<Variable> IEnumerable<Variable>.GetEnumerator()
        {
            try
            {
                _vlock.EnterReadLock();

                return _varlistn.Values.GetEnumerator();
            }
            finally
            {
                _vlock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Variable>)this).GetEnumerator();
        }
        
        #endregion
    }
}
