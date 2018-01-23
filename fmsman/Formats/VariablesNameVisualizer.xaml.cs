using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace fmsman.Formats
{
    public partial class VariablesNameVisualizer
    {
        public VariablesNameVisualizer()
        {
            InitializeComponent();
        }

        private void cpy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                Clipboard.SetData(DataFormats.Text, txt.Text);
                // ReSharper disable once AssignNullToNotNullAttribute
                BeginStoryboard(FindResource("fcp") as Storyboard);
            }
        }
    }
}
