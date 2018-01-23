using System.Collections.Generic;

namespace fmslstrap.Configuration
{
    internal class ConfigKey
    {
        private readonly IList<string> _k;

        public ConfigKey(IList<string> Keys)
        {
            _k = Keys;
        }

        public bool IsExists
        {
            get 
            {
                if (_k == null)
                    return false;

                if (_k.Count < 1)
                    return false;

                return true;
            }
        }

        public string Value
        {
            get 
            {
                if (!IsExists)
                    return null;

                return _k[0];
            }
        }

        public IList<string> Values
        {
            get
            {
                if (!IsExists)
                    return null;

                return _k;
            }
        }
    }
}
