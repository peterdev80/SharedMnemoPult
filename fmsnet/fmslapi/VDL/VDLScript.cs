using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

// ReSharper disable RedundantCast
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace fmslapi.VDL
{
    /// <summary>
    /// Исполняемая среда VDL скрипта
    /// </summary>
#if DEBUG
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
#endif
    public class VDLScript
    {
        #region Частные данные
        /// <summary>
        /// Имя скрипта
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Возвращаемый тип
        /// </summary>
        private readonly Types _returntype;

        /// <summary>
        /// Типы параметров скрипта
        /// </summary>
        private readonly Dictionary<string, Types> _params = new Dictionary<string, Types>();        
        
        /// <summary>
        /// Вспомогательные значения параметров скрипта
        /// </summary>
        private readonly Dictionary<string, Tuple<string, string>> _adparams = new Dictionary<string, Tuple<string, string>>();

        /// <summary>
        /// Код скрипта
        /// </summary>
        private byte[] _code;

        /// <summary>
        /// Коллекция значений статических переменных скрипта
        /// </summary>
        private Dictionary<object, object>[] _statics;

        /// <summary>
        /// Коллекция строк
        /// </summary>
        private readonly string[] _strings;

        /// <summary>
        /// Коллекция откомпилированных регулярных выражений
        /// </summary>
        private static readonly Dictionary<string, Regex> _regexes = new Dictionary<string, Regex>();

        [ThreadStatic]
        private static Stack _stack;

        /// <summary>
        /// Список скриптов, загруженных из той же самой сборки
        /// </summary>
        private readonly IDictionary<int, VDLScript> _domain;
        #endregion

        #region Публичные свойства

        public string Name => _name;

        /// <summary>
        /// Интервал обновления результата скрипта
        /// </summary>
        public TimeSpan UpdateInterval { get; set; }

        /// <summary>
        /// Интервал времени, во время которого повторное обновление запрещено
        /// </summary>
        /// <remarks>
        /// По истечению интервала будет выполнен одиночный цикл обновления, если было
        /// запрошено хотя бы один раз
        /// </remarks>
        public TimeSpan UpdateLatch { get; set; }

        /// <summary>
        /// Количество статических переменных
        /// </summary>
        public byte StaticCount
        {
            set 
            { 
                _statics = new Dictionary<object, object>[value];
                for (var i = 0; i < _statics.Length; i++)
                    _statics[i] = new Dictionary<object, object> {{0, null}};
            }
        }

        /// <summary>
        /// Количество параметров
        /// </summary>
        public byte ParamsCount => (byte)_params.Count;

        /// <summary>
        /// Тип возвращаемого значения
        /// </summary>
        public Types ReturnType => _returntype;

        #endregion

        #region Конструкторы

        /// <summary>
        /// Создает новый экземпляр исполняемой среды
        /// </summary>
        /// <param name="Name">Имя скрипта</param>
        /// <param name="RetType">Тип возвращаемого параметра</param>
        /// <param name="StringTable">Коллекция общих строк</param>
        /// <param name="Domain"></param>
        public VDLScript(string Name, Types RetType, string[] StringTable, IDictionary<int, VDLScript> Domain)
        {
            _strings = StringTable;
            _name = Name;
            _returntype = RetType;
            _domain = Domain;
        }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Добавляет параметр в коллекцию параметров
        /// </summary>
        /// <param name="Name">Имя параметра</param>
        /// <param name="Type">Тип параметра</param>
        /// <param name="Addit1">Вспомогательное значение параметра</param>
        /// <param name="Addit2">Вспомогательное значение параметра</param>
        // ReSharper disable once ParameterHidesMember
        internal void AddParameter(string Name, Types Type, string Addit1, string Addit2)
        {
            _params.Add(Name, Type);
            _adparams.Add(Name, new Tuple<string, string>(Addit1, Addit2));
        }

        /// <summary>
        /// Добавляет исполняемый код скрипта
        /// </summary>
        /// <param name="Code">Исполняемый код</param>
        internal void AssignCode(byte[] Code)
        {
            _code = Code;
        }

        /// <summary>
        /// Коллекция имен параметров скрипта
        /// </summary>
        /// <returns>Коллекция имен параметров скрипта</returns>
        public string[] GetParamNames()
        {
            return _params.Keys.ToArray();
        }

        /// <summary>
        /// Возвращает дополнительные значения параметра скрипта
        /// </summary>
        /// <param name="Name">Имя параметра</param>
        /// <param name="A1">Дополнительное значение 1</param>
        /// <param name="A2">Дополнительное значение 2</param>
        // ReSharper disable once ParameterHidesMember
        public void GetParamAdditional(string Name, out string A1, out string A2)
        {
            var t = _adparams[Name];

            A1 = t.Item1;
            A2 = t.Item2;
        }

        /// <summary>
        /// Исполняет скрипт
        /// </summary>
        /// <param name="Parameters">Входные данные скрипта</param>
        /// <param name="PropertyHost">Хост свойств VDL</param>
        /// <returns>Результат работы скрипта</returns>
        public object Execute(object[] Parameters, IPropertyHost PropertyHost)
        {
            if (_stack == null)
                _stack = new Stack();

            if (Parameters != null)
                foreach (var p in Parameters)
                    _stack.Push(p);

            return Execute(0, PropertyHost);
        }
        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Исполняет скрипт
        /// </summary>
        /// <param name="PC">Точка входа</param>
        /// <param name="PropertyHost">Хост свойств VDL</param>
        /// <returns>Результат работы скрипта</returns>
        internal object Execute(UInt16 PC, IPropertyHost PropertyHost)
        {
            if (_stack == null)
                _stack = new Stack();

            var s = _stack;
            var bp = 0;
            var pc = (int)PC;

            var statkeys = new object[_statics.Length];
            for (var i = 0; i < _statics.Length; i++)
                statkeys[i] = 0;

            var props = new Dictionary<int, IProperty>();

            var pb = 0;

            while (true)
            {
                var op = (OpCodes)_code[pc++];
                switch (op)
                {
                    #region Исполнение инструкций

                    #region ENTER
                    case OpCodes.ENTER:
                        var v = _code[pc++];         // Количество локальных переменных
                        var pars = _code[pc++];
                        pb = s.SP - pars;
                        bp = s.SP;
                        s.SP += v;
                        break;
                    #endregion

                    #region LOADG
                    case OpCodes.LOADG0: s.Push(s.At(pb + 0)); break;
                    case OpCodes.LOADG1: s.Push(s.At(pb + 1)); break;
                    case OpCodes.LOADG2: s.Push(s.At(pb + 2)); break;
                    case OpCodes.LOADG3: s.Push(s.At(pb + 3)); break;
                    case OpCodes.LOADG4: s.Push(s.At(pb + 4)); break;
                    case OpCodes.LOADG: s.Push(s.At(pb + _code[pc++])); break;
                    #endregion

                    #region STOG
                    case OpCodes.STOG0: s.SetAt(pb + 0, s.Pop()); break;
                    case OpCodes.STOG1: s.SetAt(pb + 1, s.Pop()); break;
                    case OpCodes.STOG2: s.SetAt(pb + 2, s.Pop()); break;
                    case OpCodes.STOG3: s.SetAt(pb + 3, s.Pop()); break;
                    case OpCodes.STOG4: s.SetAt(pb + 4, s.Pop()); break;
                    case OpCodes.STOG: s.SetAt(pb + _code[pc++], s.Pop()); break;
                    #endregion

                    #region STOG.P
                    case OpCodes.STOG0_P: s.SetAt(pb + 0, s.Peek()); break;
                    case OpCodes.STOG1_P: s.SetAt(pb + 1, s.Peek()); break;
                    case OpCodes.STOG2_P: s.SetAt(pb + 2, s.Peek()); break;
                    case OpCodes.STOG3_P: s.SetAt(pb + 3, s.Peek()); break;
                    case OpCodes.STOG4_P: s.SetAt(pb + 4, s.Peek()); break;
                    case OpCodes.STOG_P: s.SetAt(pb + _code[pc++], s.Peek()); break;
                    #endregion

                    #region LOAD
                    case OpCodes.LOAD0: s.Push(s.At(bp + 0)); break;
                    case OpCodes.LOAD1: s.Push(s.At(bp + 1)); break;
                    case OpCodes.LOAD2: s.Push(s.At(bp + 2)); break;
                    case OpCodes.LOAD3: s.Push(s.At(bp + 3)); break;
                    case OpCodes.LOAD4: s.Push(s.At(bp + 4)); break;
                    case OpCodes.LOAD: s.Push(s.At(bp + _code[pc++])); break;
                    #endregion

                    #region LOAD_S, STO_S
                    case OpCodes.LOAD_S: var sn = (int)_code[pc++]; s.Push(_statics[sn][statkeys[sn]]); break;
                    case OpCodes.STO_S: sn = (int)(_code[pc++]); _statics[sn][statkeys[sn]] = s.Pop(); break;
                    #endregion

                    #region STATREF
                    case OpCodes.STATREF:
                        sn = (int)(_code[pc++]);
                        var snk = s.Pop();
                        if (!_statics[sn].ContainsKey(snk))
                            _statics[sn].Add(snk, null);
                        statkeys[sn] = snk;
                        break;
                    #endregion

                    #region STO
                    case OpCodes.STO0: s.SetAt(bp + 0, s.Pop()); break;
                    case OpCodes.STO1: s.SetAt(bp + 1, s.Pop()); break;
                    case OpCodes.STO2: s.SetAt(bp + 2, s.Pop()); break;
                    case OpCodes.STO3: s.SetAt(bp + 3, s.Pop()); break;
                    case OpCodes.STO4: s.SetAt(bp + 4, s.Pop()); break;
                    case OpCodes.STO: s.SetAt(bp + _code[pc++], s.Pop()); break;
                    #endregion

                    #region STO.P
                    case OpCodes.STO0_P: s.SetAt(bp + 0, s.Peek()); break;
                    case OpCodes.STO1_P: s.SetAt(bp + 1, s.Peek()); break;
                    case OpCodes.STO2_P: s.SetAt(bp + 2, s.Peek()); break;
                    case OpCodes.STO3_P: s.SetAt(bp + 3, s.Peek()); break;
                    case OpCodes.STO4_P: s.SetAt(bp + 4, s.Peek()); break;
                    case OpCodes.STO_P: s.SetAt(bp + _code[pc++], s.Peek()); break;
                    #endregion

                    #region CONST.UNSET
                    case OpCodes.CONST_UNSET: s.Push(UnsetValue.Value); break;
                    #endregion

                    #region CONSTI32
                    case OpCodes.CONSTI32_0: s.Push((int)0); break;
                    case OpCodes.CONSTI32_1: s.Push((int)1); break;
                    case OpCodes.CONSTI32: s.Push(BitConverter.ToInt32(_code, pc)); pc += sizeof(int); break;
                    case OpCodes.CONSTI32_SHORT: s.Push((int)(sbyte)_code[pc++]); break;
                    #endregion

                    #region CONSTF
                    case OpCodes.CONST_F0: s.Push((float)0); break;
                    case OpCodes.CONST_F1: s.Push((float)1); break;
                    case OpCodes.CONSTF: s.Push(BitConverter.ToSingle(_code, pc)); pc += sizeof(float); break;
                    #endregion

                    #region CONSTD
                    case OpCodes.CONST_D0: s.Push((double)0); break;
                    case OpCodes.CONST_D1: s.Push((double)1); break;
                    case OpCodes.CONSTD: s.Push(BitConverter.ToDouble(_code, pc)); pc += sizeof(double); break;

                    case OpCodes.CONSTD_PI: s.Push(Math.PI); break;
                    case OpCodes.CONSTD_2PI: s.Push(2D * Math.PI); break;
                    #endregion

                    #region CONSTSTRING
                    case OpCodes.CONSTSTRING16:
                        var iv = ((int)_code[pc++]) + ((int)_code[pc++] << 8);
                        s.Push(_strings[iv]);
                        break;
                    case OpCodes.CONSTSTRING8: s.Push(_strings[(int)_code[pc++]]); break;
                    #endregion

                    #region CONSTNULL
                    case OpCodes.CONST_NULL: s.Push(null); break;
                    #endregion

                    #region CONSTTRUE, CONSTFALSE
                    case OpCodes.CONSTTRUE: s.Push(true); break;
                    case OpCodes.CONSTFALSE: s.Push(false); break;
                    #endregion

                    #region LSS, GTR, EQU, NEQU
                    case OpCodes.MATCH:
                        var so1 = s.Pop() as string; var so2 = s.Pop() as string;
                        if ((so1 == null) || (so2 == null)) { s.Push(false); break; }

                        var r = new Regex(so1, RegexOptions.Compiled);
                        _regexes[so1] = r;

                        _code[pc - 1] = (byte)OpCodes.MATCH_OPT;

                        s.Push(r.IsMatch(so2));
                        break;

                    case OpCodes.MATCH_OPT:
                        so1 = s.Pop() as string; so2 = s.Pop() as string;
                        if ((so1 == null) || (so2 == null)) { s.Push(false); break; }

                        _regexes.TryGetValue(so1, out r);

                        Debug.Assert(r != null);

                        s.Push(r.IsMatch(so2));
                        break;

                    case OpCodes.LSS:
                        var o1 = s.Pop(); var o2 = s.Pop();
                        if (o1 is int && o2 is int)
                        {
                            s.Push((int)o2 < (int)o1);
                            break;
                        }

                        s.Push(Convert.ToDouble(o2) < Convert.ToDouble(o1));
                        break;

                    case OpCodes.GTR:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is int && o2 is int)
                        {
                            s.Push((int)o2 > (int)o1);
                            break;
                        }

                        s.Push(Convert.ToDouble(o2) > Convert.ToDouble(o1));
                        break;

                    case OpCodes.LSS_EQU:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is int && o2 is int)
                        {
                            s.Push((int)o2 <= (int)o1);
                            break;
                        }

                        s.Push(Convert.ToDouble(o2) <= Convert.ToDouble(o1));
                        break;

                    case OpCodes.GTR_EQU:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is int && o2 is int)
                        {
                            s.Push((int)o2 >= (int)o1);
                            break;
                        }

                        s.Push(Convert.ToDouble(o2) >= Convert.ToDouble(o1));
                        break;

                    case OpCodes.EQU:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 == null && o2 == null) { s.Push(true); break; }
                        if (o1 == null || o2 == null) { s.Push(false); break; }
                        if (o1 is bool && o2 is bool) { s.Push((bool)o2 == (bool)o1); break; }
                        if (o1 is string && o2 is string) { s.Push((string)o2 == (string)o1); break; }
                        s.Push(Convert.ToDouble(o2) == Convert.ToDouble(o1)); break;

                    case OpCodes.NEQU: 
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is string && o2 is string) { s.Push((string)o2 != (string)o1); break; }
                        s.Push(Convert.ToDouble(o1) != Convert.ToDouble(o2)); 
                        break;

                    case OpCodes.EQU_I32_Z:
                        o1 = s.Pop(); o2 = (int)0;
                        if (o1 is bool && o2 is bool) { s.Push((bool)o2 == (bool)o1); break; }
                        s.Push(Convert.ToDouble(o2) == Convert.ToDouble(o1)); break;

                    case OpCodes.NEQU_I32_Z:
                        o1 = s.Pop(); o2 = (int)0;
                        s.Push(Convert.ToDouble(o1) != Convert.ToDouble(o2));
                        break;

                    case OpCodes.EQU_F_Z:
                        o1 = s.Pop(); o2 = (float)0;
                        if (o1 is bool && o2 is bool) { s.Push((bool)o2 == (bool)o1); break; }
                        s.Push(Convert.ToDouble(o2) == Convert.ToDouble(o1)); break;

                    case OpCodes.NEQU_F_Z:
                        o1 = s.Pop(); o2 = (float)0;
                        s.Push(Convert.ToDouble(o1) != Convert.ToDouble(o2));
                        break;

                    case OpCodes.EQU_TRUE:
                        o1 = s.Pop(); o2 = (bool)true;
                        if (o1 is bool && o2 is bool) { s.Push((bool)o2 == (bool)o1); break; }
                        s.Push(Convert.ToDouble(o2) == Convert.ToDouble(o1)); break;

                    case OpCodes.NEQU_TRUE:
                        o1 = s.Pop(); o2 = (bool)true;
                        if (o1 is bool && o2 is bool) { s.Push((bool)o2 != (bool)o1); break; }
                        s.Push(Convert.ToDouble(o2) != Convert.ToDouble(o1)); break;

                    case OpCodes.EQU_FALSE:
                        o1 = s.Pop(); o2 = (bool)false;
                        if (o1 is bool && o2 is bool) { s.Push((bool)o2 == (bool)o1); break; }
                        s.Push(Convert.ToDouble(o2) == Convert.ToDouble(o1)); break;

                    case OpCodes.NEQU_FALSE:
                        o1 = s.Pop(); o2 = (bool)false;
                        if (o1 is bool && o2 is bool) { s.Push((bool)o2 != (bool)o1); break; }
                        s.Push(Convert.ToDouble(o2) != Convert.ToDouble(o1)); break;
                    #endregion

                    #region LSS_Z, GTR_Z
                    case OpCodes.LSS_Z:
                        o1 = s.Pop();
                        if (o1 is int) { s.Push((int)o1 < (int)0); break; }
                        if (o1 is float) { s.Push((float)o1 < (float)0); break; }
                        if (o1 is double) { s.Push((double)o1 < (double)0); break; }

#if DEBUG
                        throw new InvalidOperationException("Операция не может быть выполнена");
#else
                        s.Push(false);
                        break;
#endif

                    case OpCodes.GTR_Z:
                        o1 = s.Pop();
                        if (o1 is int) { s.Push((int)o1 > (int)0); break; }
                        if (o1 is float) { s.Push((float)o1 > (float)0); break; }
                        if (o1 is double) { s.Push((double)o1 > (double)0); break; }

#if DEBUG
                        throw new InvalidOperationException("Операция не может быть выполнена");
#else
                        s.Push(false);
                        break;
#endif

                    case OpCodes.LSS_EQU_Z:
                        o1 = s.Pop();
                        if (o1 is int) { s.Push((int)o1 <= (int)0); break; }
                        if (o1 is float) { s.Push((float)o1 <= (float)0); break; }
                        if (o1 is double) { s.Push((double)o1 <= (double)0); break; }

#if DEBUG
                        throw new InvalidOperationException("Операция не может быть выполнена");
#else
                        s.Push(false);
                        break;
#endif

                    case OpCodes.GTR_EQU_Z:
                        o1 = s.Pop();
                        if (o1 is int) { s.Push((int)o1 >= (int)0); break; }
                        if (o1 is float) { s.Push((float)o1 >= (float)0); break; }
                        if (o1 is double) { s.Push((double)o1 >= (double)0); break; }

                        #if DEBUG
                        throw new InvalidOperationException("Операция не может быть выполнена");
#else
                        s.Push(false);
                        break;
#endif

                    #endregion

                    #region OR, AND
                    case OpCodes.OR: 
                        o2 = s.Pop(); o1 = s.Pop();
                        if (o1 is int && o2 is int) { s.Push((int)o1 | (int)o2); break; }
                        s.Push((bool)o1 || (bool)o2);
                        break;

                    case OpCodes.AND:
                        o2 = s.Pop(); o1 =s.Pop();
                        if (o1 is int && o2 is int) { s.Push((int)o1 & (int)o2); break; }
                        s.Push((bool)o1 && (bool)o2); 
                        break;
                    #endregion

                    #region JMP, FJMP
                    case OpCodes.JMP: pc = ((int)_code[pc++]) + ((int)_code[pc] << 8); break;
                    case OpCodes.FJMP: if (!(bool)s.Pop()) pc = ((int)_code[pc++]) + ((int)_code[pc] << 8); else pc += 2; break;
                    #endregion

                    #region DUP
                    case OpCodes.DUP: s.Push(s.Peek()); break;
                    #endregion

                    #region POP
                    case OpCodes.POP: s.Pop(); break;
                    #endregion

                    #region SWAP
                    case OpCodes.SWAP: s.Swap(); break;
                    #endregion

                    #region ADD, SUB, MUL, DIV, RMN
                    case OpCodes.ADD:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is int && o2 is int) { s.Push((int)o2 + (int)o1); break; }
                        if (o1 is float && o2 is float) { s.Push((float)o2 + (float)o1); break; }
                        if (o1 is string && o2 is string) { s.Push((string)o2 + (string)o1); break; }

                        s.Push(Convert.ToDouble(o2) + Convert.ToDouble(o1));
                        break;

                    case OpCodes.SUB:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is int && o2 is int) { s.Push((int)o2 - (int)o1); break; }
                        if (o1 is float && o2 is float) { s.Push((float)o2 - (float)o1); break; }
                        if (o1 is float && o2 is float) { s.Push((float)o2 - (float)o1); break; }

                        s.Push(Convert.ToDouble(o2) - Convert.ToDouble(o1));
                        break;

                    case OpCodes.MUL:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is int && o2 is int)
                        {
                            s.Push((int)o2 * (int)o1);
                            break;
                        }

                        if (o2 is float && o1 is float)
                        {
                            s.Push((float)o2 * (float)o1);
                            break;
                        }

                        s.Push(Convert.ToDouble(o2) * Convert.ToDouble(o1));
                        break;

                    case OpCodes.DIV:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is int && o2 is int) { s.Push((int)o2 / (int)o1); break; }
                        if (o1 is float && o2 is float) { s.Push((float)o2 / (float)o1); break; }

                        s.Push(Convert.ToDouble(o2) / Convert.ToDouble(o1));
                        break;

                    case OpCodes.RMN:
                        o1 = s.Pop(); o2 = s.Pop();
                        if (o1 is int && o2 is int) { s.Push((int)o2 % (int)o1); break; }
                        if (o1 is float && o2 is float) { s.Push((float)o2 % (float)o1); break; }

                        s.Push(Convert.ToDouble(o2) % Convert.ToDouble(o1));
                        break;
                    #endregion

                    #region NEG
                    case OpCodes.NEG:
                        o1 = s.Pop();
                        if (o1 is int) { s.Push(-(int)o1); break; }
                        if (o1 is float) { s.Push(-(float)o1); break; }
                        if (o1 is double) { s.Push(-(double)o1); break; }
                        if (o1 is bool) { s.Push(!(bool)o1); break; }
#if DEBUG
                        throw new InvalidOperationException("Неверный контекст OpCodes.NEG");
#else
                        s.Push(false);
                        break;
#endif
                    #endregion

                    #region NOT
                    case OpCodes.NOT: s.Push(!(bool)s.Pop()); break;
                    #endregion

                    #region EXIT
                    case OpCodes.EXIT:
                        if (PC != 0)
                        {
                            // Вызов инициализации статических переменных.
                            // Ничего не возвращает
                            s.SP = pb;
                            return null;
                        }

                        if (_returntype != Types.Void)
                        {
                            var res = s.Pop();     // Результат
                            s.SP = pb;

                            if (_returntype == Types.DynamicResource) return res;
                            if (_returntype == Types.Boolean && (res is bool || res == null)) return res;
                            if (_returntype == Types.Double && (res is double || res == null)) return res;
                            if (_returntype == Types.Float && (res is float || res == null)) return res;
                            if (_returntype == Types.Int32 && (res is Int32 || res == null)) return res;
                            if (_returntype == Types.String && (res is string || res == null)) return res;

#if DEBUG
                            throw new InvalidOperationException("Возвращаемое значение имеет неверный тип");
#else
                            // Релизная версия лучше пусть работает неправильно
                            // нежели упадет с исключением

                            switch (_returntype)
                            {
                                case Types.Int32: return 0;
                                case Types.Float: return 0f;
                                case Types.Double: return 0d;
                                case Types.String: return "";
                                case Types.Boolean: return false;

                                default: return null;
                            }
#endif
                        }
                        
                        s.SP = pb;
                        return null;

                        #endregion

                    #region FORMAT
                    case OpCodes.FORMAT:
                        v = _code[pc++];
                        var objs = s.PopReverse(v);
                        var fs = (string)s.Pop();
                        s.Push(string.Format(fs, objs).Replace("\\n", "\n"));
                        break;
                    #endregion

                    #region PROPGET, PROPSET
                    case OpCodes.PROPGET:
                        iv = ((int)_code[pc++]) + ((int)_code[pc++] << 8);
                        IProperty prop;
                        if (props.TryGetValue(iv, out prop))
                        {
                            s.Push(prop.GetValue());
                            break;
                        }
                        prop = PropertyHost.GetProperty(_strings[iv], s.Pop());
                        props.Add(iv, prop);
                        s.Push(prop.GetValue());
                        break;

                    case OpCodes.PROPSET:
                        iv = ((int)_code[pc++]) + ((int)_code[pc++] << 8);
                        var val = s.Pop();
                        if (props.TryGetValue(iv, out prop))
                        {
                            prop.SetValue(val);
                            break;
                        }
                        prop = PropertyHost.GetProperty(_strings[iv], s.Pop());
                        props.Add(iv, prop);
                        prop.SetValue(val);
                        break;
                    #endregion

                    #region CONVERTTO
                    case OpCodes.CONVERTTO:
                        var totype = (Types)_code[pc++];
                        var obj = s.Pop();
                        switch (totype)
	                    {
                            case Types.Undefined: throw new InvalidCastException(string.Format("Невозможно преобразовать к типу {0}", totype));

                            case Types.Int32: s.Push(Convert.ToInt32(obj, CultureInfo.InvariantCulture)); break;
                            case Types.Float: s.Push(Convert.ToSingle(obj, CultureInfo.InvariantCulture)); break;
                            case Types.Double: s.Push(Convert.ToDouble(obj, CultureInfo.InvariantCulture)); break;
                            case Types.String: s.Push(obj.ToString()); break;
                            case Types.Boolean: if (obj is bool) s.Push(obj); else s.Push(Convert.ToInt32(obj, CultureInfo.InvariantCulture) != 0); break;
                            case Types.DynamicResource:
                                s.Push(new System.Windows.DynamicResourceExtension { ResourceKey = obj }.ProvideValue(null));
                                break;
	                    }
                        break;
                    #endregion

                    #region CALL
                    case OpCodes.CALL_8:
                        var cs = _domain[_code[pc++]];
                        s.Push(cs.Execute(0, PropertyHost));
                        break;

                    case OpCodes.CALL_16:
                        cs = _domain[_code[BitConverter.ToUInt16(_code, pc)]];
                        pc += 2;
                        s.Push(cs.Execute(0, PropertyHost));
                        break;

                    #endregion

                    #region Тригонометрические функции
                    case OpCodes.SIN:
                    case OpCodes.COS:
                    case OpCodes.TAN:
                    case OpCodes.ASIN:
                    case OpCodes.ACOS:
                    case OpCodes.ATAN:
                        o1 = s.Pop();

                        if (!((o1 is double) || (o1 is float)))
#if DEBUG
                            throw new InvalidOperationException("Тригонометрические функции разрешены только для типов single и double");
#else
                            break;
#endif

                        var dbo = Convert.ToDouble(o1);

                        switch (op)
                        {
                            case OpCodes.SIN: s.Push(Math.Sin(dbo)); break;
                            case OpCodes.COS: s.Push(Math.Cos(dbo)); break;
                            case OpCodes.TAN: s.Push(Math.Tan(dbo)); break;
                            case OpCodes.ASIN: s.Push(Math.Asin(dbo)); break;
                            case OpCodes.ACOS: s.Push(Math.Acos(dbo)); break;
                            case OpCodes.ATAN: s.Push(Math.Atan(dbo)); break;
                        }
                        break;
                    #endregion

                    #region Арифметические сдвиги

                    case OpCodes.LSHIFT:
                        o1 = s.Pop();
                        o2 = s.Pop();

                        s.Push(Convert.ToInt32(o2) << Convert.ToInt32(o1));

                        break;

                    #endregion

                    #region Локализация

                    case OpCodes.LOCALIZE:
                        break;

                    #endregion

                    #endregion

                    default:
                        throw new InvalidOperationException("Неправильный OpCode");
                }
            }
        }
        #endregion

        #region Визуализация отладки
#if DEBUG
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => string.Format("{0} {1}({2})", _returntype, _name, string.Join(", ", _params.Keys));
#endif
        #endregion
    }

    /// <summary>
    /// Маркер значения DependencyProperty.UnsetValue или аналогичного
    /// </summary>
    internal sealed class UnsetValue
    {
        static UnsetValue()
        {
            Value = new UnsetValue();
        }

        public static UnsetValue Value { get; }
    }
}
