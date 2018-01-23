using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace fmslstrap.Configuration
{
    internal delegate void OnConfigChanged(string Path);

    internal class MultiFileWatcher
    {
        private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        public void SetFilesWorWatch(IEnumerable<string> Files)
        {
            lock (this)
            {
                var dels = new HashSet<string>();           // Кандидаты на удаление
                foreach (var k in _watchers.Keys)
                    dels.Add(k);

                foreach (var f in Files)
                {
                    dels.Remove(f);

                    if (_watchers.ContainsKey(f))
                        continue;

                    var w = new FileSystemWatcher
                    {
                        Path = Path.GetDirectoryName(f),
                        NotifyFilter = NotifyFilters.LastWrite,
                        Filter = Path.GetFileName(f)
                    };

                    w.Changed += w_Changed;

                    _watchers.Add(f, w);
                }

                foreach (var d in dels)
                {
                    var dw = _watchers[d];

                    dw.EnableRaisingEvents = false;
                    _watchers.Remove(d);
                }
            }
        }

        void w_Changed(object sender, FileSystemEventArgs e)
        {
            Changed?.Invoke(e.FullPath);
        }

        public event OnConfigChanged Changed;

        public bool EnableRaisingEvents
        {
            set
            {
                lock (this)
                {
                    foreach (var w in _watchers.Values)
                        w.EnableRaisingEvents = value;
                }
            }
        }

        public string[] FileNames
        {
            get
            {
                lock (this)
                {
                    return (from f in _watchers.Keys select f).ToArray();
                }
            }
        }
    }
}
