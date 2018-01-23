using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;

namespace fmsman.Formats
{
    /// <summary>
    /// Базовый класс визуализации значения переменной
    /// </summary>
    [DesignTimeVisible(false)]
    public abstract class BaseVarVisual : Control
    {
        #region Свойства зависимостей
        public static readonly DependencyProperty VariableProperty = DependencyProperty.Register("Variable", typeof(VarEntry), typeof(BaseVarVisual), new PropertyMetadata(Changed));
        #endregion

        #region Частные данные
        private uint _myindex;
        private Storyboard _valueflash;
        #endregion

        #region Конструкторы

        protected BaseVarVisual()
        {
            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }
        #endregion

        #region Публичные свойства
        public VarEntry Variable
        {
            get => (VarEntry)GetValue(VariableProperty);
            set => SetValue(VariableProperty, value);
        }
        #endregion

        #region Перегрузки
        protected abstract void Reformat();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _valueflash = FindResource("valueflash") as Storyboard;
        }
        #endregion

        #region Обработка событий
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalWatcher.Change += Change2;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once DelegateSubtraction
            GlobalWatcher.Change -= Change2;
        }

        private static void Changed(object obj, DependencyPropertyChangedEventArgs e)
        {
            var v = obj as BaseVarVisual;
            var ve = e.NewValue as VarEntry;

            Debug.Assert(v != null, "v != null");
            Debug.Assert(ve != null, "ve != null");

            v._myindex = ve.VarIndex;
        }

        private void Change2(uint VarIndex)
        {
            if (VarIndex != _myindex)
                return;

            Dispatcher.BeginInvoke(new Action<uint>(Flash), VarIndex);
        }

        private void Flash(uint VarIndex)
        {
            Reformat();

            _valueflash?.Begin(this, Template);
        }
        #endregion

        #region Публичные методы
        public void SendAsChanged()
        {
            Variable.Connection.SendVarAsChanged(_myindex);
        }
        #endregion
    }
}
