using System.Collections.Generic;
using System.Threading;
using System.IO.MemoryMappedFiles;
using fmslstrap.Channel;

namespace fmslstrap.Variables
{
    public partial class VariablesTable
    {
        /// <summary>
        /// Словарь переменных по ключу - имени переменной
        /// </summary>
        private readonly Dictionary<string, Variable> _varlist;

        /// <summary>
        /// Словарь переменных по ключу - индексу переменной
        /// </summary>
        private readonly Dictionary<uint, Variable> _varlistn;

        /// <summary>
        /// Потоковая блокировка доступа к коллекции переменных таблицы
        /// </summary>
        private readonly ReaderWriterLockSlim _vlock = new ReaderWriterLockSlim();

        /// <summary>
        /// Список всех категорий
        /// </summary>
        private readonly HashSet<string> _categories = new HashSet<string>();

        /// <summary>
        /// Имя отображенного файла
        /// </summary>
        public const string ShMemoryName = "fmsldr_shared_space"; //string.Format("{0}.shmem", Guid.NewGuid().ToString());

        /// <summary>
        /// Отображенный файл для обмена переменными с клиентами
        /// </summary>
        private static readonly MemoryMappedFile _shmemory = MemoryMappedFile.CreateOrOpen(ShMemoryName, 1024 * 1024 * 16);

        /// <summary>
        /// Объект доступа к разделяемой памяти значений переменных
        /// </summary>
        public static readonly MemoryMappedViewAccessor SharedAccessor = _shmemory.CreateViewAccessor();

        /// <summary>
        /// Количество пользователей карты переменных
        /// </summary>
        private int _subscribercnt;

        /// <summary>
        /// Глобальный каталог таблиц переменных
        /// </summary>
        private static readonly List<VariablesTable> _alltables = new List<VariablesTable>();

        /// <summary>
        /// Мягкоинициализированные переменные
        /// </summary>
        /// <remarks>
        /// После подключения к каналу остальные участники присылают свои значения переменных 
        /// для инициализации. Т.к. невозможно гарантировать актуальность их данных
        /// принимается значение от хоста с максимальным временем работы, передаваемым 
        /// как часть посылки. Соответствие переменной и текущего времени работы хоста, явившегося источником
        /// начального значения хранится здесь. После получения значения переменной
        /// обычным порядком - запись из этой таблицы удаляется => инициализация на эту переменную более не действует.
        /// </remarks>
        private readonly Dictionary<uint, uint> _weakinit_vars = new Dictionary<uint, uint>();

        /// <summary>
        /// Несущий сетевой канал для обмена значениями переменных
        /// </summary>
        private ChanConfig _associatedchannel;

        /// <summary>
        /// Список сторожевых переменных
        /// </summary>
        private VarTypes.WVar[] _watchdogvars;
    }
}
