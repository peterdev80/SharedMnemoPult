using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections;
// ReSharper disable All

namespace fmslapi.Components
{
    public class LVar
    {
        public bool Check { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Commentary { get; set; }
    }

    //[Serializable, Editor(typeof(LVarMapTypeEditor), typeof(UITypeEditor))]
    [Browsable(false)]
    public class LVarMap : List<LVar>
    {
        public LVarMap(IEnumerable<LVar> src)
            : base(src)
        {
        }

        public LVarMap()
        {
        }
    }

    public class LVarMapSorter : IEnumerable
    {
        private readonly LVarMap _src;
        private string _filter;
        private string _category;

        public LVarMapSorter(LVarMap source, string filter, string category)
        {
            _src = source;
            _filter =  filter == null ? null : filter.ToLower();
            _category = category;
        }

        public void ApplyFilter(string filter, string category)
        {
            _filter = filter == null ? null : filter.ToLower();
            _category = category;
        }

        public IEnumerator GetEnumerator()
        {
            if (string.IsNullOrEmpty(_filter) && string.IsNullOrEmpty(_category))
                return (from v in _src select v).GetEnumerator();

            return string.IsNullOrEmpty(_category)
                       ?
                         (from v in _src where v.Name.ToLower().Contains(_filter) select v).GetEnumerator()
                       : (from v in _src where v.Name.ToLower().Contains(_filter) && v.Category == _category select v).GetEnumerator();
        }
    }
}
