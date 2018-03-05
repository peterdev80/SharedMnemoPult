using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace fmslapi
{
    public partial class Channel
    {
        private const string NetIni = "net.ini";

        private static readonly Regex Rx = new Regex("(\\s*) (?<n>\\S+) (\\s*) = (\\s*) ((?<addr>\\S*) (\\s*) : (\\s*))? (?<port>\\d+)",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private static Dictionary<string, IPEndPoint> _endpoints = new Dictionary<string, IPEndPoint>();

        static Channel()
        {
            if (!File.Exists(NetIni))
            {
                MessageBox.Show($"Конфигурация {NetIni} не найдена. Корректная работа невозможна.");
                return;
            }

            using (var rd = File.OpenText(NetIni))
            {
                while (!rd.EndOfStream)
                {
                    var line = rd.ReadLine();

                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                        continue;

                    line = line.Split(new[] { '#' }, 1, StringSplitOptions.None)[0];

                    var m = Rx.Match(line);

                    if (!m.Success)
                        continue;

                    var n = m.Groups["n"].Value;

                    var ep = new IPEndPoint(IPAddress.Any, 0);

                    if (IPAddress.TryParse(m.Groups["addr"].Value, out var ipa))
                        ep.Address = ipa;

                    if (int.TryParse(m.Groups["port"].Value, out var port))
                        ep.Port = port;

                    _endpoints[n] = ep;
                }
            }
        }
    }
}
