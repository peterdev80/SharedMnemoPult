using System.Collections.Generic;
using System.Linq;

namespace fmslapi.Bindings
{
    public class Value : IValue
    {
        private readonly object _val;

        public Value(object Value)
        {
            _val = Value;
        }

        object IValue.Value => _val;
    }

    public class ValueMetadata : Value, IValueMetadata
    {
        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>();

        public string[] MetadataNames => _metadata.Keys.ToArray();

        public object GetMetadata(string Name)
        {
            return !_metadata.TryGetValue(Name, out var v) ? null : v;
        }

        public void SetMetadata(string Name, object Value)
        {
            _metadata[Name] = Value;
        }

        public ValueMetadata(object Value) : base(Value)
        {
        }
    }
}
