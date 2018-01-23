using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using fmslstrap.Channel;
using System;
using System.IO.Compression;
using System.IO;

namespace fmslstrap.Variables
{
    #region Делегаты
    public delegate void VarChanged(IEnumerable<Variable> ChangedSet, bool IsInit);
    #endregion

    public class VariablesManager
    {
        #region Частные данные
        private static readonly Dictionary<string, VariablesTable> _vartables = new Dictionary<string, VariablesTable>();
        private static readonly Dictionary<string, VariablesTable> _varmaps = new Dictionary<string, VariablesTable>();
        private static readonly ReaderWriterLockSlim _vlock = new ReaderWriterLockSlim();
        #endregion

        public static event VarChanged GlobalVarChanged;

        public static void RaiseGlobalVarChanged(IEnumerable<Variable> ChangeSet, bool IsInit)
        {
            GlobalVarChanged?.Invoke(ChangeSet, IsInit);
        }

        #region Конструкторы
        public VariablesManager()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var gz = new GZipStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("vartables"), CompressionMode.Decompress);
            var rd = new BinaryReader(gz, Encoding.UTF8);

            var hash = rd.ReadUInt32();
            UInt16 hash16 = 0;

            // Преобразование 32 битного значения в 16 битное
            // путем вырезания нечетных битов
            for (int i = 0; i < 16; i++)
            {
                hash16 |= (UInt16)(hash & 1);
                hash >>= 1;
                hash16 ^= (UInt16)(hash & 1);
                hash >>= 1;
                hash16 <<= 1;
            }

            GlobalVariablesHash = hash16;

            var c = rd.ReadByte();

            for (var i = 0; i < c; i++)
                VariablesTable.MergeVariablesTable(_varmaps, rd);

            var dc = rd.ReadUInt16();
            for (var i = 0; i < dc; i++)
                VariablesTable.SetDefaultValue(rd.ReadString(), rd);

            Manager.AdmChannel.RegisterAdmCommand('T', RestoreFromStorage);
        }
        #endregion

        private static void RestoreFromStorage(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            var cnt = Reader.ReadUInt32();

            var tables = new HashSet<VariablesTable>();
            var vars = new HashSet<Variable>();

            for (int i = 0; i < cnt; i++)
            {
                var nl = Reader.ReadUInt16();
                var name = Encoding.UTF8.GetString(Reader.ReadBytes(nl));

                // ReSharper disable once ArrangeStaticMemberQualifier
                var vl = (from vm in VariablesManager.GetVariablesMaps()
                          let vv = vm.GetVariable(name)
                          where vv != null
                          select new { map = vm, var = vv }).FirstOrDefault();

                var ds = Reader.ReadUInt32();

                if (vl == null)
                {
                    if (ds == 0)
                        break;
                    else
                    {
                        Reader.ReadBytes((int)ds);   // Пропускаем нужное количество байт
                        continue;
                    }
                }

                if (!vl.var.Type.StartsWith("S") && vl.var.SizeOf != ds)
                    break;      // Скорей всего структура потока разрушена
                // Поэтому break; т.к. восстановить последующие значения всеравно
                // не получится

                vl.var.ParseDelta(Reader, false);

                tables.Add(vl.map);
                vars.Add(vl.var);
            }

            foreach (var t in tables)
            {
                t.SendChanges(vars);
                t.RaiseOnVarChanged(t.SelectOwnVars(vars), false);
            }
        }

        /// <summary>
        /// Хеш, рассчитанный из всех переменных всех таблиц
        /// </summary>
        /// <remarks>
        /// Передается в каждом пакете, служит гарантией соответствия индексов переменных
        /// в пакете и таблицах хоста
        /// </remarks>
        public static UInt16 GlobalVariablesHash
        {
            get;
            private set;
        }

        public static IEnumerable<Variable> GetVariablesMapList(string VarTable)
        {
            try
            {
                return _varmaps[VarTable];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public static VariablesTable[] GetVariablesMaps()
        {
            return _varmaps.Values.ToArray();
        }
        
        /// <summary>
        /// Запрашивает подключение к каналу обмена переменными
        /// </summary>
        /// <param name="Channel">Имя канала</param>
        /// <param name="VarMap">Имя карты переменных</param>
        /// <param name="Changed">Событие, происходящее при внешнем изменении переменных</param>
        public static VariablesTable JoinVariableChannel(string Channel, string VarMap, VarChanged Changed)
        {
            VariablesTable table;
            
            try
            {
                _vlock.EnterWriteLock();

                if (!_varmaps.ContainsKey(VarMap))
                    throw new InvalidOperationException(@"Попытка создания канала на основе несуществующей таблицы переменных");

                var varsrc = _varmaps[VarMap];

                if (!_vartables.TryGetValue(Channel, out table))
                    _vartables[Channel] = table = varsrc;

                table.OnVarChanged += Changed;
            }
            finally
            {
                _vlock.ExitWriteLock();
            }

            table.CheckOnline();
            return table;
        }

        /// <summary>
        /// Покидает канал переменных
        /// </summary>
        /// <param name="Channel">Имя канала переменных</param>
        /// <param name="Changed">Событие, происходящее при внешнем изменении переменных</param>
        public static void LeaveVariableChannel(string Channel, VarChanged Changed)
        {
            try
            {
                _vlock.EnterWriteLock();

                if (!_vartables.ContainsKey(Channel))
                    throw new InvalidOperationException(@"Попытка закрытия канала, который не был открыт");

                var table = _vartables[Channel];
                table.OnVarChanged -= Changed;

                table.CheckOffline();
            }
            finally
            {
                _vlock.ExitWriteLock();
            }
        }
    }
}
