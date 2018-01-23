using System;
using System.Windows;

namespace fmslapi.Bindings.WPF
{
    public class BindingOptions : DependencyObject
    {
        #region Свойства зависимостей

        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.RegisterAttached(
            "FormatString", typeof(string), typeof(BindingOptions), new FrameworkPropertyMetadata(fs_changed));

        public static readonly DependencyProperty VariablesTableProperty = DependencyProperty.RegisterAttached(
            "VariablesTable", typeof(string), typeof(BindingOptions), new PropertyMetadata(default(string), vt_changed));

        #endregion

        public static void SetVariablesTable(DependencyObject element, string value)
        {
            element.SetValue(VariablesTableProperty, value);
        }

        public static string GetVariablesTable(DependencyObject element)
        {
            return (string)element.GetValue(VariablesTableProperty);
        }

        public static void SetFormatString(DependencyObject element, string value)
        {
            element.SetValue(FormatStringProperty, value);
        }

        public static string GetFormatString(DependencyObject element)
        {
            return (string)element.GetValue(FormatStringProperty);
        }
        
        private static void fs_changed(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var c = VariablesDataContext.GetVariablesDataContext(D);

            c = c != null ? c.Clone(D) : new VariablesDataContext(D);

            c.FormatString = E.NewValue.ToString();

            VariablesDataContext.SetVariablesDataContext(D, c);
        }

        private static void vt_changed(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var c = VariablesDataContext.GetVariablesDataContext(D);

            c = c != null ? c.Clone(D) : new VariablesDataContext(D);

            c.VariablesChannelName = E.NewValue.ToString();

            VariablesDataContext.SetVariablesDataContext(D, c);
        }
    }
}
