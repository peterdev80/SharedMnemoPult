using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace fmslapi.VDL
{
    /// <summary>
    /// Общая исполняемая среда VDL
    /// </summary>
    public static class VDLRuntime
    {
        #region Частные данные
        private static readonly Dictionary<string, VDLScript> _scripts = new Dictionary<string, VDLScript>();
        private static readonly HashSet<int> _loadedassemblies = new HashSet<int>();
        #endregion

        #region Загрузка двоичного образа VDL

        static VDLRuntime()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (s, e) => CheckVDLLoaded(e.LoadedAssembly);

            foreach (var f in AppDomain.CurrentDomain.GetAssemblies())
                CheckVDLLoaded(f);
        }

        private static void CheckVDLLoaded(Assembly Source)
        {
            if (Source.IsDynamic)
                return;

            var h = Source.GetHashCode();

            if (_loadedassemblies.Contains(h)) 
                return;

            _loadedassemblies.Add(h);
            LoadVDL(Source);
        }

        public static void LoadVDL(Assembly Source)
        {
            var s = Source.GetManifestResourceStream("VDL");
            if (s != null)
                LoadVDL(s);
        }

        public static void LoadVDL(Stream Source)
        {
            var gz = new GZipStream(Source, CompressionMode.Decompress);
            var rdr = new BinaryReader(gz);

            var count = rdr.ReadUInt16();                       // Количество скриптов

            // Таблица строк
            var sc = rdr.ReadUInt16();                          // Количество строк в таблице
            var strings = new string[sc];
            for (var i = 0; i < sc; i++)
                strings[i] = rdr.ReadString();                 // Очередная строка в таблице

            var scd = new Dictionary<int, VDLScript>();        // Таблица ссылок из скрипта на другие скрипты в пределах сборки

            for (var i = 0; i < count; i++)
            {
                var sname = rdr.ReadString();                   // Имя скрипта
                var s = new VDLScript(sname, (Types)rdr.ReadUInt16(), strings, scd);    // Тип возвращаемого значения

                var pc = rdr.ReadByte();                        // Количество входных параметров скрипта
                for (var j = 0; j < pc; j++)
                {
                    var pn = strings[rdr.ReadUInt16()];

                    var a1 = strings[rdr.ReadUInt16()].Replace('.', '/');
                    if (string.IsNullOrWhiteSpace(a1))
                        a1 = null;

                    var a2 = strings[rdr.ReadUInt16()].Replace('.', '/');
                    if (string.IsNullOrWhiteSpace(a2))
                        a2 = null;

                    s.AddParameter(pn, Types.Undefined, a1, a2);
                }

                s.UpdateInterval = TimeSpan.FromMilliseconds(rdr.ReadUInt16());
                s.UpdateLatch = TimeSpan.FromMilliseconds(rdr.ReadUInt16());

                var codesize = rdr.ReadInt16();                 // Размер кода скрипта в байтах

                var startupadr = rdr.ReadUInt16();              // Адрес инициализации статических переменных
                var staticcnt = rdr.ReadByte();                 // Количество статических переменных

                var code = rdr.ReadBytes(codesize);             // Код скрипта

                s.StaticCount = staticcnt;
                s.AssignCode(code);

                _scripts.Add(sname, s);
                scd.Add(scd.Count, s);

                if (startupadr != 0)
                    s.Execute(startupadr, null);
            }
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Возвращает исполняемую среду скрипта
        /// </summary>
        /// <param name="Name">Имя скрипта</param>
        /// <returns>Исполняемая среда</returns>
        public static VDLScript GetScript(string Name)
        {
            _scripts.TryGetValue(Name, out var rv);

            if (rv == null)
                _scripts.TryGetValue(Name, out rv);

            return rv;
        }

        /// <summary>
        /// Проверяет наличие скрипта с указанным именем
        /// </summary>
        /// <param name="Name">Имя скрипта</param>
        /// <returns>Признак наличия скрипта</returns>
        public static bool ScriptExists(string Name)
        {
            return _scripts.ContainsKey(Name);
        }
        #endregion
    }
}
