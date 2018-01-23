using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using fmslstrap.Variables;
using System.Threading;
using System.IO;
using fmslstrap.Variables.VarTypes;

namespace fmslstrap.Pipe
{
    internal partial class PipeManager
    {
        #region Частные данные
        /// <summary>
        /// Зарегистрированные в канале переменные 
        /// </summary>
        private readonly HashSet<Variable> _registeredvariables = new HashSet<Variable>();

        /// <summary>
        /// Потоковая блокировка коллекции _registeredvariables
        /// </summary>
        private readonly ReaderWriterLockSlim _registeredvariableslock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Имя канала переменных
        /// </summary>
        private string _varchanname;

        /// <summary>
        /// Таблица переменных
        /// </summary>
        private VariablesTable _vartable;

        /// <summary>
        /// Имя карты переменных
        /// </summary>
        private string _varmapname;
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Зарегистрированные в канале переменные
        /// </summary>
        public IEnumerable<Variable> RegisteredVariables
        {
            get
            {
                try
                {
                    _registeredvariableslock.EnterReadLock();
                    return _registeredvariables.ToArray();
                }
                finally
                {
                    _registeredvariableslock.ExitReadLock();
                }
            }
        }
        #endregion

        #region Обработчики внешних событий
        /// <summary>
        /// Событие, происходяще после приема пакета переменных
        /// </summary>
        private void OnVarChanged(IEnumerable<Variable> ChangeSet, bool IsInit)
        {
            IEnumerable<Variable> chlist;
            try
            {
                _registeredvariableslock.EnterReadLock();
                chlist = _registeredvariables.Intersect(ChangeSet).ToArray();
            }
            finally
            {
                _registeredvariableslock.ExitReadLock();
            }
            SendVarPackToPipe(chlist, IsInit);
        }

        private void SendVarPackToPipe(IEnumerable<Variable> list, bool IsInit)
        {
            var empty = true;

            var ms = new MemoryStream();
            var iwr = new BinaryWriter(ms);

            iwr.Write((byte)'N');
            iwr.Write(IsInit);
            foreach (var var in list)
            {
                iwr.Write(var.VarNum);
                empty = false;
            }

            iwr.Write(0);

            if (!empty) MessageToClient(ms.ToArray());
        }
        #endregion

        #region Обработка клиентских команд
        /// <summary>
        /// Обработка пакета изменившихся переменных, принятых от локального клиента
        /// </summary>
        private byte[] SendVariablesPack(BinaryReader rdr)
        {
            var count = rdr.ReadInt32();

            var incoming = new HashSet<Variable>();
            var wincoming = new HashSet<Variable>();

            var fb = new HashSet<Variable>();
            for (var i = 0; i < count; i++)
            {
                var vindex = rdr.ReadUInt32();
                var nfb = rdr.ReadBoolean();

                var var = _vartable.GetVariable(vindex);

                if (var == null)
                    continue;

                var wd = var as WVar;

                var afb = true;

                if (wd != null)
                {
                    afb = wd.Reset();
                    if (afb)
                        wincoming.Add(wd);
                }
                else
                    incoming.Add(var);

                if (nfb && afb)
                    fb.Add(var);
            }

            // Возвращаем обратно переменные, затребовавшие этого
            if (fb.Count > 0)
                SendVarPackToPipe(fb, false);

            PipeManager[] pipes;
            try
            {
                ActivePipesLock.EnterReadLock();

                pipes = ActivePipes.ToArray();
            }
            finally
            {
                ActivePipesLock.ExitReadLock();
            }

            var ainc = incoming.Concat(wincoming);

            var cs = ainc as Variable[] ?? ainc.ToArray();

            VariablesManager.RaiseGlobalVarChanged(cs, false);
            _vartable.SendChanges(cs);

            foreach (var pm in pipes.Where(p => p._varmapname == _varmapname && p._instanceid != _instanceid))
            {
                Variable[] rv;
                try
                {
                    pm._registeredvariableslock.EnterReadLock();
                    rv = incoming.Intersect(pm._registeredvariables).ToArray();
                }
                finally
                {
                    pm._registeredvariableslock.ExitReadLock();
                }

                pm.SendVarPackToPipe(rv, false);
            }

            return null;
        }

        private byte[] RegisterVariable(BinaryReader rdr)
        {
            var rvcp = new BinaryReader(rdr.BaseStream, Encoding.GetEncoding(rdr.ReadUInt16()));
            var name = rvcp.ReadString();
            var peek = rvcp.ReadBoolean();

            _joincomplete.WaitOne();

            var var = !peek ? _vartable.GetVariable(name)
                            : (from vt in VariablesManager.GetVariablesMaps()
                               let vv = vt.GetVariable(name)
                               where vv != null
                               select vv).FirstOrDefault();
            
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms, Encoding.UTF8);

            wr.Write('V');              // Команда регистрации

            if (var == null)
            {
                wr.Write(-1);
                wr.Write(name);
                return ms.ToArray();
            }

            if (!peek)
            {
                try
                {
                    _registeredvariableslock.EnterWriteLock();
                    _registeredvariables.Add(var);
                }
                finally
                {
                    _registeredvariableslock.ExitWriteLock();
                }
            }

            wr.Write(var.VarNum);                           // Индекс
            wr.Write(var.Name);                             // Имя
            wr.Write(var.Type[0]);                          // Тип
            wr.Write(VariablesTable.ShMemoryName);          // Имя области памяти
            wr.Write(var.SharedOffset);                     // Смещение в общей памяти
            wr.Write((UInt16)var.ThresholdDigits);          // Количество значимых знаков после запятой

            return ms.ToArray();
        }

        private byte[] GetVarMap(BinaryReader rdr)
        {
            throw new InvalidOperationException("PipeManager.GetVarMap");

            /*var name = rdr.ReadString();

            var vtbl = VariablesManager.GetVariablesMapList(name);
            var bld = new StringBuilder();
            var stringcnt = 0;

            if (vtbl != null)
            {
                foreach (var var in vtbl)
                {
                    // V,<Name>,<Type>,<Category>,<Commentary>
                    bld.AppendFormat(@"V,{0},{1},{2},{3}{4}", var.Name, var.Type, var.Category, var.Commentary, Environment.NewLine);
                    stringcnt++;
                }

                var cms = string.Format(@"Vars,{0}{1}{2}", stringcnt, Environment.NewLine, bld);
                MessageToClient(cms);
            }
            else
            {
                MessageToClient(string.Format(@"Vars,0"));
            }*/
        }

        private byte[] JoinVariableChannel(BinaryReader rdr)
        {
            var codepage = rdr.ReadUInt16();
            var cprd = new BinaryReader(rdr.BaseStream, Encoding.GetEncoding(codepage));
            _endpointname = CheckEndPointName(cprd.ReadString());
            _varchanname = cprd.ReadString();
            _varmapname = cprd.ReadString();
            cprd.ReadInt64();                   // ? hw

            VarChanged vch = OnVarChanged;

            _vartable = VariablesManager.JoinVariableChannel(_varchanname, _varmapname, vch);

            OnCreatePipe?.Invoke(this);

            _joincomplete.Set();

            return null;
        }

        private byte[] GetVariablesMap(BinaryReader rdr)
        {
            var kms = new MemoryStream();
            var kwr = new BinaryWriter(kms);

            var list = VariablesManager.GetVariablesMaps();

            kwr.Write((UInt16)list.Length);

            foreach (var l in list)
                kwr.Write(l.Name);

            return kms.ToArray();
        }

        private byte[] SavePersistentVariable(BinaryReader rdr)
        {
            var vindex = rdr.ReadUInt32();

            var v = (from vm in VariablesManager.GetVariablesMaps()
                     let vv = vm.GetVariable(vindex)
                     where vv != null
                     select vv).FirstOrDefault();

            if (v == null)
                return null;

            if (v.Type == "K")
                return null;

            var ms = new MemoryStream();
            var mwr = new BinaryWriter(ms);
            mwr.Write('I');

            mwr.Write(new byte[16]);                // Ключ транзакции - пустой
            var b = Encoding.UTF8.GetBytes(v.Name);
            mwr.Write((UInt16)b.Length);
            mwr.Write(b);

            b = Encoding.UTF8.GetBytes("pvarstg");
            mwr.Write((UInt16)b.Length);
            mwr.Write(b);

            // ReSharper disable once RedundantCast
            mwr.Write((UInt32)v.ActualSizeOf);
            v.PackValue(mwr);

            Debug.WriteLine(string.Format("Save persistent variable: {0}", v.Name));

            Manager.AdmChannel.SendMessage(ms.ToArray());

            return null;
        }

        private byte[] RemovePersistentVariable(BinaryReader rdr)
        {
            var ms = new MemoryStream();
            var mwr = new BinaryWriter(ms);
            mwr.Write('J');
            rdr.BaseStream.CopyTo(ms);

            Manager.AdmChannel.SendMessage(ms.ToArray());

            return null;
        }

        private byte[] MakeSnapshot(BinaryReader rdr)
        {
            var ms = new MemoryStream();
            var mwr = new BinaryWriter(ms);

            mwr.Write('S');
            rdr.BaseStream.CopyTo(ms);

            Manager.AdmChannel.SendMessage(ms.ToArray());

            return null;
        }

        private byte[] RestoreSnapshot(BinaryReader rdr)
        {
            SnapshotsManager.RestoreSnapshot(rdr, this);

            return null;
        }

        private byte[] ResetWatchDogs(BinaryReader rdr)
        {
            var cnt = rdr.ReadUInt16();

            if (cnt == 0)
            {
                _vartable.ResetWDogs(null);
                return null;
            }

            var l = new List<WVar>();

            for (int i = 0; i < cnt; i++)
            {
                var indx = rdr.ReadUInt16();

                var w = _vartable.GetVariable(indx) as WVar;

                if (w != null)
                    l.Add(w);
            }

            _vartable.ResetWDogs(l);

            return null;
        }
        #endregion
    }
}
