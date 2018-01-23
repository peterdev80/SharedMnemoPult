using System.ComponentModel;
using fmslapi.Annotations;

namespace fmslapi.Bindings.WPF
{
    public partial class BaseValueBinding
    {
        private sealed class Bridge : INotifyPropertyChanged
        {
            private object _value;

            public object Value
            {
                // ReSharper disable once UnusedMember.Local
                get => _value;
                set
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged(string PropertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}
