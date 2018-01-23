using System.Collections.Generic;

namespace fmslapi.VDL
{
    /// <summary>
    /// Стек исполняемой среды VDL
    /// </summary>
    internal class Stack
    {
        #region Частные данные
        /// <summary>
        /// Объекты в стеке
        /// </summary>
        private readonly List<object> _stack = new List<object>();
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Указатель следующей свободной позиции в стеке
        /// </summary>
        public int SP { get; set; }
        #endregion

        #region Публичные методы
        /// <summary>
        /// Помещает объект в стек
        /// </summary>
        /// <param name="value">Объект</param>
        public void Push(object value)
        {
            var add = SP - _stack.Count + 1;
            if (add > 0)
                _stack.AddRange(new object[add]);
            _stack[SP++] = value;
        }

        /// <summary>
        /// Извлекает объект и стека
        /// </summary>
        /// <returns>Объект</returns>
        public object Pop()
        {
            return _stack[--SP];
        }

        /// <summary>
        /// Возвращает некоторое количество элементов с вершины стека в порядке обратном занесению
        /// </summary>
        /// <param name="Length">Количество элементов</param>
        /// <returns>Массив элементов</returns>
        public object[] PopReverse(int Length)
        {
            var o = new object[Length];
            for (var i = Length - 1; i >= 0; i--)
                o[i] = Pop();

            return o;
        }

        /// <summary>
        /// Возвращает объект с вершины стека не извлекая его оттуда
        /// </summary>
        /// <returns>Объект</returns>
        public object Peek()
        {
            return _stack[SP - 1];
        }

        /// <summary>
        /// Объект в позиции, указанной индексом
        /// </summary>
        /// <param name="Index">Индекс объекта</param>
        /// <returns>Объект</returns>
        public object At(int Index)
        {
            return _stack[Index];
        }

        /// <summary>
        /// Замена объекта в стеке по индексу
        /// </summary>
        /// <param name="Index">Индекс</param>
        /// <param name="value">Значение</param>
        public void SetAt(int Index, object value)
        {
            _stack[Index] = value;
        }

        /// <summary>
        /// Обмен двух значений на вершине стека
        /// </summary>
        public void Swap()
        {
            var t = _stack[SP - 1];
            _stack[SP - 1] = _stack[SP - 2];
            _stack[SP - 2] = t;
        }
        #endregion
    }
}
