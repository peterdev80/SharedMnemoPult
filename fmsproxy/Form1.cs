using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using fmslapi;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace fmsproxy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            _termination = false;

            fmslapi.Manager.OnUnloadProcess += () =>
                {
                    _termination = true;
                    Close();
                };

            InitializeComponent();
        }

        #region Частные данные
        private IManager _man;
        private int _sectionhash;
        private List<ModelProxy> _proxies = new List<ModelProxy>();
        private IConfigSection _conf;
        private bool _termination;
        private bool _working;
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            _man = Manager.GetAPI("PeerProxy", new Guid("{57D60194-6B12-4A4B-B542-C9C2D0EA5843}"));
            var stray = tray.Text;

            Action availnow = () => { tray.Text = stray; };
            Action ontry = () => { Application.DoEvents(); if (_termination) throw new ConnectionAbortException(); };

            _man.OnFMSLDRNotAvailable += () =>
                {
                    tray.Text = "Ожидание соединения с обменом.";
                    if ((_conf == null) || (_conf != null && !_conf.GetPrefixed(null).GetBool("silent")))
                        tray.ShowBalloonTip(2500, "Ожидание", "Ожидание соединения с обменом. Работа не может быть продолжена.", ToolTipIcon.Warning);
                    return new[] { availnow, ontry };
                };

            _man.OnConfigReload += () =>
            {
                Action a = () =>
                {
                    _conf = _man.DefaultSection;
                    var th = _conf.GetHashCode();
                    if (th == _sectionhash)
                        return;

                    _sectionhash = th;

                    foreach (var p in _proxies)
                        p.CloseProxy();

                    ModelProxy.LocalPoints.Clear();
                    _proxies.Clear();
                    tlp.Controls.Clear();

                    LoadProxies();

                    tray.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("fmsproxy.WarnIcon.ico"));
                    if (_conf != null && !_conf.GetPrefixed(null).GetBool("silent"))
                        tray.ShowBalloonTip(2000, "Данные конфигурации изменились.", "Данные конфигурации изменились и были перезагружены.", ToolTipIcon.Info);
                };
                BeginInvoke(a);
                //var ccn = _man.DefaultSection;
            };

            _conf = _man.DefaultSection;
            _sectionhash = _conf.GetHashCode();

            LoadProxies();

            _working = true;
        }

        private void LoadProxies()
        {
            var tlpcnt = 0;

            var pxs = _conf.GetPrefixed(null).AsWordsArray("proxy.start");

            if (pxs == null || pxs.Length == 0)
                Application.Exit();

            foreach(var us in pxs)
            {
                _conf = _conf.GetPrefixed(x => string.Format("{1}.{0}", x, us));

                var ts = "typename";
                if (!_conf.HasKey(ts))
                    continue;

                var tn = Type.GetType(_conf[ts]);
                var pr = Activator.CreateInstance(tn) as ModelProxy;
                if (pr == null)
                    continue;

                pr.Config = _conf;
                pr.Manager = _man;
                
                var cpt = _conf["tlp.caption"];
                if (!string.IsNullOrEmpty(cpt))
                {
                    var lbl = new Label {AutoSize = true, Text = cpt};
                    tlp.Controls.Add(lbl, 0, tlpcnt);

                    var stlbl = new Label {Text = @"OK", AutoSize = true};
                    pr.OnConnected += (s2, e2) =>
                    {
                        Action ac = () =>
                            {
                                stlbl.Text = @"OK";
                                stlbl.BackColor = Color.LightGreen;
                            };

                        BeginInvoke(ac);
                    };

                    pr.OnDisconnected += (s3, e3) =>
                    {
                        Action ad = () =>
                            {
                                stlbl.Text = "Отсутствует";
                                stlbl.BackColor = Color.Red;
                            };

                        BeginInvoke(ad);
                    };
                    tlp.Controls.Add(stlbl, 1, tlpcnt);

                    var clbl = new Label { AutoSize = true, Text = "/" };
                    tlp.Controls.Add(clbl, 2, tlpcnt);
                    pr.CountIndicator = clbl;

                    tlpcnt++;
                }

                pr.Load();
                _proxies.Add(pr);
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _termination = true;
            Close();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!_working)
                return;

            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShowInTaskbar = false;
            Hide();
            WindowState = FormWindowState.Minimized;
            e.Cancel = e.CloseReason == CloseReason.UserClosing && !_termination;
        }

        private void tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            toolStripMenuItem1_Click(sender, e);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (var p in _proxies)
                p.CountIndicator.Text = string.Format("{0} / {1}", Interlocked.Read(ref p.ReceivedPackets), Interlocked.Read(ref p.SendedPackets));
        }
    }
}
