using System.Collections.Generic;
using fmslstrap.Administrator;
using System.IO;
using fmslstrap.Channel;
using fmslstrap.Pipe;

namespace fmslstrap.Variables
{
    /// <summary>
    /// Управление снимками состояния переменных
    /// </summary>
    internal static class SnapshotsManager
    {
        private static readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

        public static void Init(AdmChannel admchan)
        {
            admchan.RegisterAdmCommand('S', MakeSnapshot);
        }

        /// <summary>
        /// Сохранение снимка
        /// </summary>
        private static void MakeSnapshot(Stream Stream, BinaryReader Reader, byte[] Data, ChanConfig Sender)
        {
            var name = Reader.ReadString();             // Имя снимка
            var cnt = Reader.ReadUInt16();              // Количество переменных в снимке

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write(cnt);

            for (var i = 0; i < cnt; i++)
            {
#pragma warning disable 168
                // ReSharper disable once UnusedVariable
                var var = VariablesTable.GlobalGetVariable(Reader.ReadUInt16(), out var vt);
#pragma warning restore 168

                if (var is VarTypes.KVar)       // Команды в снимок не включаются
                    continue;

                var.PackVariable(ms);
            }

            lock (_cache)
            {
                _cache[name] = ms.ToArray();
            }
        }

        /// <summary>
        /// Восстановление переменных из снимка
        /// </summary>
        /// <param name="Reader">Имя снимка</param>
        /// <param name="Sender">Клиент, запросивший восстановление снимка</param>
        public static void RestoreSnapshot(BinaryReader Reader, PipeManager Sender)
        {
            var name = Reader.ReadString();             // Имя снимка
            var sync = Reader.ReadBoolean();            // Синхронный вызов
            var synckey = sync ? Reader.ReadUInt32() : 0;
            var silent = Reader.ReadBoolean();          // Не вызывать события
            
            try
            {
                byte[] snapshot;

                lock (_cache)
                {
                    if (!_cache.TryGetValue(name, out snapshot))
                        return;
                }

                var ms = new MemoryStream(snapshot);
                var rd = new BinaryReader(ms);

                var cnt = rd.ReadUInt16();                  // Количество переменных в снимке

                var dv = new Dictionary<VariablesTable, List<Variable>>();

                for (var i = 0; i < cnt; i++)
                {
                    VariablesTable vt;
                    var var = VariablesTable.GlobalGetVariable(rd.ReadUInt32(), out vt);

                    var.ParseDelta(rd, false);

                    List<Variable> l;

                    if (!dv.TryGetValue(vt, out l))
                    {
                        l = new List<Variable>();
                        dv[vt] = l;
                    }

                    l.Add(var);
                }

                if (!silent)
                    foreach (var vt in dv)
                        vt.Key.RaiseOnVarChanged(vt.Value, false);
            }
            finally
            {
                if (sync)
                    Sender.MessageToClient('K', synckey);
            }
        }
    }
}
