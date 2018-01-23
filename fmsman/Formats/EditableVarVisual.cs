using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

// ReSharper disable MemberCanBePrivate.Global

namespace fmsman.Formats
{
    public abstract class EditableVarVisual : BaseVarVisual
    {
        protected UIElement _pen;

        protected TextBox _editor;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _pen = GetTemplateChild("PART_pencil") as UIElement;
            _editor = GetTemplateChild("PART_editor") as TextBox;

            Debug.Assert(_pen != null, "_pen != null");
            Debug.Assert(_editor != null, "_editor != null");

            _pen.MouseDown += _pen_MouseDown;
            _editor.KeyDown += _editor_KeyDown;
            _editor.LostFocus += _editor_LostFocus;
            _editor.TextChanged += _editor_TextChanged;
        }

        void _editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged(_editor.Text);
        }

        void _editor_LostFocus(object sender, RoutedEventArgs e)
        {
            _editor.Visibility = Visibility.Collapsed;
            
            if (e != null)
                OnLostFocus(e);
        }

        void _editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                AcceptValue();
            }

            if (e.Key == Key.Escape)
            {
                _editor_LostFocus(sender, null);
            }
        }

        protected abstract void PrepareEditor();
        protected abstract void AcceptValue();
        protected virtual void TextChanged(string Text) { }
        //protected virtual void LostFocus() { }

        void _pen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
                PrepareEditor();
        }
    }
}
