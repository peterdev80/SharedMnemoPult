using System.Collections.Generic;

namespace fmslstrap.Configuration
{
    internal class ConfigSection
    {
        private readonly Dictionary<string, List<string>> _rs;

        public ConfigSection(Dictionary<string, List<string>> Source)
        {
            _rs = Source;
        }

        public ConfigKey this[string key]
        {
            get
            {
                List<string> l;
                _rs.TryGetValue(key, out l);

                return new ConfigKey(l);
            }
        }

        public bool ContainsKey(string Name)
        {
            return _rs.ContainsKey(Name);
        }
    }
}
