using System;
using System.IO;
using fmslstrap.Variables.VarTypes;
// ReSharper disable RedundantNameQualifier

namespace fmslstrap.Variables
{
    /// <summary>
    /// Базовый класс переменной
    /// </summary>
    public abstract class Variable : IComparable
    {
        #region Частные данные
        /// <summary>
        /// Глобальный счетчик для присвоения индексов переменным
        /// </summary>
        /// <remarks>
        /// Значения меньше 8 служебные
        /// 0 : Маркер конца списка переменных
        /// </remarks>
        private static uint _varcnt = 8;

        private static uint _globoffset = 64;
        private string _name;

        protected VariablesTable _vartable;

        /// <summary>
        /// Блокировка одновременного доступа к значению переменной
        /// </summary>
        protected CrossProcessReaderWriterLock _lock;

        #endregion

        #region Конструкторы
        protected void AlignOffset(ref uint Offset)
        {
            var al = this is SVar ? 4 : SizeOf;

            switch (al)
            {
                case 0:
                case 1:
                    break;

                case 2:
                    Offset = (Offset + 1) & 0xfffffffe;
                    break;

                case 3:
                case 4:
                    Offset = (Offset + 3) & 0xfffffffc;
                    break;

                case 5:
                case 6:
                case 7:
                case 8:
                    Offset = (Offset + 7) & 0xfffffff8;
                    break;

                default:
                    Offset = (Offset + 3) & 0xfffffffc;
                    break;
            }
        }

        public static Variable New(VariablesTable VarTable, uint? VarNum, string Name, char Type, int Par1, int Par2, int Par3, string Comment, string Category)
        {
            Variable rvar;

            switch (Type)
            {
                case 'B':
                    rvar = new VarTypes.BVar(ref _globoffset);
                    break;                
                    
                case 'T':
                    rvar = new VarTypes.TVar(ref _globoffset);
                    break;

                case 'K':
                    rvar = new VarTypes.KVar(ref _globoffset);
                    break;

                case 'I':
                    rvar = new VarTypes.IVar(ref _globoffset);
                    break;

                case 'L':
                    rvar = new VarTypes.LVar(ref _globoffset);
                    break;

                case 'F':
                    rvar = new VarTypes.FVar(ref _globoffset);
                    break;

                case 'C':
                    rvar = new VarTypes.CVar(ref _globoffset);
                    break;

                case 'S':
                    rvar = new VarTypes.SVar(ref _globoffset, Par1);
                    break;

                case 'D':
                    rvar = new VarTypes.DVar(ref _globoffset);
                    break;
                
                case 'A':
                    rvar = new VarTypes.AVar(ref _globoffset, Par1);
                    break;

                case 'W':
                    rvar = new VarTypes.WVar(ref _globoffset, (UInt16)Par1, (UInt16)Par2, (UInt16)Par3);
                    break;

                default:
                    throw new Exception("Обнаружен неизвестный тип переменной");
            }

            _globoffset += rvar.SizeOf;

            rvar._vartable = VarTable;

            rvar.Category = Category;
            rvar.Name = Name;
            rvar.Comment = Comment;
            rvar.VarNum = VarNum ?? _varcnt++;

            rvar.ThresholdDigits = Par1;

            return rvar;
        }
        #endregion

        #region Общие свойства
      
        /// <summary>
        /// Идентификатор переменной
        /// </summary>
        public uint VarNum;

        /// <summary>
        /// Смещение в файле отображенной памяти
        /// </summary>
        public uint SharedOffset;

        /// <summary>
        /// Указатель на значение переменной в отображенной памяти
        /// </summary>
        public unsafe void* SharedPointer;

        /// <summary>
        /// Имя переменной
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comment;

        /// <summary>
        /// Категория
        /// </summary>
        public string Category;

        /// <summary>
        /// Размер значения переменной
        /// </summary>
        public abstract uint SizeOf { get; }

        /// <summary>
        /// Актуальный размер переменной в потоке сериализации
        /// </summary>
        public virtual uint ActualSizeOf => SizeOf;

        public int ThresholdDigits
        {
            get;
            private set;
        }
        #endregion

        #region Работа с переменными

        #region Абстрактные методы
        /// <summary>
        /// Упаковывает значение переменной для формирование дельта-пакета
        /// </summary>
        /// <param name="writer">Писатель потока</param>
        public abstract void PackValue(BinaryWriter writer);

        /// <summary>
        /// Парсинг значения переменных из потока
        /// </summary>
        /// <param name="Reader">Читатель потока</param>
        /// <param name="SkipOnly">Только пропустить упакованное значение в потоке</param>
        public abstract void ParseDelta(BinaryReader Reader, bool SkipOnly);
        #endregion

        /// <summary>
        /// Упаковывает переменную в поток для передачи
        /// </summary>
        /// <param name="stream">Поток для передачи</param>
        public void PackVariable(Stream stream)
        {
            var wrtr = new BinaryWriter(stream);

            wrtr.Write((Int32)VarNum);
            PackValue(wrtr);
        }

        /// <summary>
        /// Устанавливает указатель на значение переменной в разделяемой области
        /// </summary>
        protected unsafe void AssingValuePointer()
        {
            byte* origin = null;

            VariablesTable.SharedAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref origin);
            origin += SharedOffset;
            SharedPointer = origin;
        }
        #endregion

        #region Свойства для грида
        /// <summary>
        /// Текстовое представление типа переменной в виде "[x]var"
        /// </summary>
        public string Type => GetType().Name;

        /// <summary>
        /// Комментарий к переменной из таблицы описания
        /// </summary>
        public string Commentary => Comment;

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            var v = obj as Variable;
            if (v == null) throw new ArgumentException();

            return (int)(VarNum - v.VarNum);
        }
        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            var v = obj as Variable;
            if (v == null)
            {
                throw new ArgumentException();
            } 
            
            return VarNum == v.VarNum;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return (int)VarNum;
        }

        #endregion
    }
}
