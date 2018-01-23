using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

// ReSharper disable InconsistentNaming

namespace fmsman.Formats
{
    /// <summary>
    /// Логика взаимодействия для VariableVisualizer.xaml
    /// </summary>
    public partial class VariableVisualizer
    {
        public VariableVisualizer()
        {
            InitializeComponent();

            OnReformat += Reformat;
        }

        private readonly Action<uint> OnReformat;

        private uint _myindex;

        public static readonly DependencyProperty VariableProperty = DependencyProperty.Register("Variable", typeof(VarEntry), typeof(VariableVisualizer), new PropertyMetadata(Changed));

        public static readonly DependencyProperty VariableColorProperty = DependencyProperty.Register("VariableColor", typeof(Color), typeof(VariableVisualizer), new PropertyMetadata(VarColorChanged));

        public static readonly RoutedEvent StartFlashEvent = EventManager.RegisterRoutedEvent("StartFlash", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(VariableVisualizer));
        public event RoutedEventHandler StartFlash
        {
            add => AddHandler(StartFlashEvent, value);
            remove => RemoveHandler(StartFlashEvent, value);
        }

        private readonly VariableFormatter _fmt = new VariableFormatter();

        public VarEntry Variable
        {
            get => (VarEntry)GetValue(VariableProperty);
            set => SetValue(VariableProperty, value);
        }

        private static void Changed(object obj, DependencyPropertyChangedEventArgs e)
        {
            var v = obj as VariableVisualizer;
            var ve = e.NewValue as VarEntry;

            Debug.Assert(v != null, "v != null");
            Debug.Assert(ve != null, "ve != null");

            v.txt.Text = v._fmt.Convert(ve, null, null, null)?.ToString();
            v._myindex = ve.VarIndex;
        }

        private static void VarColorChanged(object obj, DependencyPropertyChangedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            (obj as VariableVisualizer).txt.Foreground = new SolidColorBrush((Color)e.NewValue);
        }

        private void Change2(uint VarIndex)
        {
            if (VarIndex != _myindex)
                return; 

            Dispatcher.BeginInvoke(OnReformat, VarIndex);
        }

        private void Reformat(uint VarIndex)
        {
            var ve = Variable;

            txt.Text = _fmt.Convert(ve, null, null, null)?.ToString();

            if (ve.VarType.StartsWith("K"))
                ((Storyboard)FindResource("flash")).Begin(this);
            else
                ((Storyboard)FindResource("valueflash")).Begin(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalWatcher.Change += Change2;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once DelegateSubtraction
            GlobalWatcher.Change -= Change2;
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            txt.Foreground = Foreground;
        }
    }
}
