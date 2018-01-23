using System;
using fmslapi.Bindings.WPF;

namespace fmslapi.Bindings
{
    /// <summary>
    /// Тип события изменения значения источника привязки
    /// </summary>
    /// <param name="Value"></param>
    public delegate void SourceValueChanged(IValue Value);

    public interface IValue
    {
        /// <summary>
        /// Значение источника привязки
        /// </summary>
        object Value { get; }
    }

    public interface IValueMetadata : IValue
    {
        /// <summary>
        /// Коллекция имен метаданных значения
        /// </summary>
        string[] MetadataNames { get; }

        /// <summary>
        /// Возвращает значение метеданных
        /// </summary>
        /// <param name="Name">Имя метаданных</param>
        /// <returns>Значение метаданных</returns>
        object GetMetadata(string Name);
    }

    /// <summary>
    /// Источник данных для привязок
    /// </summary>
    public interface IValueSource
    {
        /// <summary>
        /// Инициализация привязки
        /// </summary>
        /// <param name="AttachedTo">Цель привязки</param>
        /// <param name="DataContext">Контекст источника данных</param>
        void Init(object AttachedTo, VariablesDataContext DataContext);

        /// <summary>
        /// Событие изменения значения источника привязки
        /// </summary>
        event SourceValueChanged ValueChanged;

        /// <summary>
        /// Принудительное обновление значения на цели привязки
        /// </summary>
        void UpdateTarget();
        
        /// <summary>
        /// Передача значения обратно источнику привязки
        /// </summary>
        /// <param name="NewValue">Новое значение</param>
        void UpdateSource(object NewValue);

        /// <summary>
        /// Значение источника привязки
        /// </summary>
        IValue Value { get; }

        /// <summary>
        /// Тип значения источника привязки
        /// </summary>
        Type ValueType { get; }
    }

}
