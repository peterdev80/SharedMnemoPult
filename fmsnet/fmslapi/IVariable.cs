using System;
using fmslapi.UpdateTriggers;

namespace fmslapi
{
    /// <summary>
    /// Известные типы переменных
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// Тип переменной не задан
        /// </summary>
        Unknown,

        /// <summary>
        /// Логическая переменная
        /// </summary>
        Boolean,

        /// <summary>
        /// 32 битное целое знаковое значение
        /// </summary>
        Int32,

        /// <summary>
        /// 16 битное значение с плавающей точкой
        /// </summary>
        Single,
        
        /// <summary>
        /// 32 битное значение с плавающей точкой
        /// </summary>
        Double,

        /// <summary>
        /// Символ Unicode
        /// </summary>
        Char,

        /// <summary>
        /// Команда
        /// </summary>
        KMD,

        /// <summary>
        /// Строка Unicode
        /// </summary>
        String,

        /// <summary>
        /// Байтовый массив
        /// </summary>
        ByteArray,

        /// <summary>
        /// Сторожевой таймер
        /// </summary>
        WatchDog,

        /// <summary>
        /// 64 битное целое знаковое значение
        /// </summary>
        Long,

        /// <summary>
        /// Триггер
        /// </summary>
        Trigger
    }

    /// <summary>
    /// Представляет переменную
    /// </summary>
    public interface IVariable
    {
        /// <summary>
        /// Событие, происходящее при внешнем изменении переменной
        /// </summary>
        event VariableChanged VariableChanged;

        /// <summary>
        /// Код типа переменной
        /// </summary>
        VariableType VariableType { get; }

        /// <summary>
        /// Имя переменной
        /// </summary>
        string VariableName { get; }

        /// <summary>
        /// Уникальное значение, идентифицирующее переменную
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Значение переменной
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Автоматическая отправка всего пакета изменившихся переменных при изменении этой переменной
        /// </summary>
        bool AutoSend { get; set; }

        /// <summary>
        /// Сразу присылать событие об изменении переменной обратно отправителю
        /// </summary>
        bool NeedLocalFeedback { get; set; }

        /// <summary>
        /// Проверять и игнорировать при отправке одно и то же значение
        /// </summary>
        bool CheckDups { get; set; }

        /// <summary>
        /// Признак изменения переменной внешним источником
        /// </summary>
        /// <remarks>
        /// При чтении обнуляется
        /// </remarks>
        bool IsChanged { get; }

        /// <summary>
        /// Блокирует текущий поток до получения уведомления об изменении переменной
        /// </summary>
        /// <returns>true</returns>
        bool WaitOne();

        /// <summary>
        /// Блокирует текущий поток до получения уведомления об изменении переменной
        /// </summary>
        /// <param name="MillisecondsTimeout">Время ожидания в миллисекундах</param>
        /// <returns>true в случае изменения переменной</returns>
        bool WaitOne(int MillisecondsTimeout);

        /// <summary>
        /// Блокирует текущий поток до получения уведомления об изменении переменной
        /// </summary>
        /// <param name="Timeout">Время ожидания</param>
        /// <returns>true в случае изменения переменной</returns>
        bool WaitOne(TimeSpan Timeout);

        /// <summary>
        /// Сбрасывает событие изменения переменной
        /// </summary>
        void Reset();

        /// <summary>
        /// Сохраняет значение переменной в постоянном хранилище
        /// </summary>
        void SavePersistent();

        TriggerBase UpdateTrigger { get; set; }

        /// <summary>
        /// Установка значения переменной, если она была изменена извне
        /// </summary>
        /// <param name="Value">Устанавливаемое значение</param>
        /// <returns>Признак того, что переменная была изменена извне</returns>
        /// <remarks>Операция проверки и установки выполняется атомарно</remarks>
        bool SetIfChanged(object Value);

        /// <summary>
        /// Установка значения переменной, если она не была изменена извне
        /// </summary>
        /// <param name="Value">Устанавливаемое значение</param>
        /// <returns>Признак того, что переменная не была изменена извне</returns>
        /// <remarks>Операция проверки и установки выполняется атомарно</remarks>
        bool SetIfNotChanged(object Value);
    }

    #region Типизированые переменные
    /// <summary>
    /// Представляет переменную типа Boolean
    /// </summary>
    public interface IBoolVariable : IVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        new bool Value { get; set; }

        /// <summary>
        /// Инвертирует значение логической переменной
        /// </summary>
        void Toggle();
    }

    /// <summary>
    /// Представляет переменную триггер
    /// </summary>
    public interface ITriggerVariable : IVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        new bool Value { get; set; }

        /// <summary>
        /// Сброс значения триггера
        /// </summary>
        void ResetTrigger();
    }

    /// <summary>
    /// Представляет переменную типа Int32
    /// </summary>
    public interface IIntVariable : IVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        new int Value { get; set; }
    }

    /// <summary>
    /// Представляет переменную типа Int64
    /// </summary>
    public interface ILongVariable : IVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        new Int64 Value { get; set; }
    }

    /// <summary>
    /// Представляет переменную типа Single
    /// </summary>
    public interface IFloatVariable : IVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        new float Value { get; set; }
    }

    /// <summary>
    /// Представляет переменную типа Double
    /// </summary>
    public interface IDoubleVariable : IVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        new double Value { get; set; }
    }

    /// <summary>
    /// Представляет переменную типа Char
    /// </summary>
    public interface ICharVariable : IVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        new char Value { get; set; }
    }

    public interface IKVariable : IVariable
    {
        /// <summary>
        /// Активирует команду
        /// </summary>
        void Set();

        /// <summary>
        /// Команда активирована
        /// </summary>
        /// <remarks>
        /// Чтение обнуляет флаг активации
        /// </remarks>
        bool IsFired { get; }
    }

    /// <summary>
    /// Представляет переменную типа String
    /// </summary>
    public interface IStringVariable : IVariable
    {
        /// <summary>
        /// Значение переменной
        /// </summary>
        new string Value { get; set; }

        unsafe void SetFromAnsi(void* Value);
    }

    /// <summary>
    /// Переменная представляет байтовый массив
    /// </summary>
    public interface IByteArrayVariable : IVariable
    {
        new byte[] Value { get; set; }
    }

    /// <summary>
    /// Переменная представляет сторожевой таймер
    /// </summary>
    public interface IWatchDogVariable : IVariable
    {
        new bool Value { get; }

        /// <summary>
        /// Сброс счетчика сторожевого таймера
        /// </summary>
        new void Reset();

        /// <summary>
        /// Сброс счетчика сторожевого таймера на указанное значение
        /// </summary>
        /// <param name="Value">Новое значение сторожевого таймера</param>
        /// <remarks>
        /// Значение счетчика устанавливается единоразово. 
        /// Базовое значение не изменяется.
        /// </remarks>
        void Reset(UInt16 Value);

        bool Locked { get; set; }
    }
    #endregion
}
