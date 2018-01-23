using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace fmsman
{
    public class MObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Invalidate(object Changed)
        {
            CollectionChanged?.Invoke(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
