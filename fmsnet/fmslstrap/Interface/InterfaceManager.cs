using System;
using System.Collections.Generic;
using System.Threading;
using fmslstrap.Tasks;
using System.Windows.Forms;
using fmslstrap.Pipe;

namespace fmslstrap.Interface
{
    internal static class InterfaceManager
    {
        private class ToolTipInfo
        {
            public int Duration;
            public string Caption;
            public string Text;
            public ToolTipIcon Icon;
        }

        private static fmsldr.FWaiting _wf;
        private static GlobalState _gstate;
        private static readonly List<MenuItem> _menuitems = new List<MenuItem>();
        private static ToolTipInfo _tooltippending;
        private static bool _forceexit;

        public static void Start(fmsldr.FWaiting wForm)
        {
            Application.ApplicationExit += (s, a) =>
            {
                TasksManager.ShutdownAllTasks();
                CommandSocket.CommandSocket.DisableCommandProcessing();
                PipeManager.ShutdownAllPipes();
                Manager.LeaveAllChannels();
            };

            _wf = wForm;

            _wf.CreateHandle(); 

            var sman = new MenuItem { Caption = "Открыть", IsBold = true };
            sman.OnInvoke += () => TasksManager.StartTask("fmsman");

            var exitbut = new MenuItem { Caption = "Выход" };

            exitbut.OnInvoke += PressExit;

            InsertMenuItem(sman, 0);
            InsertMenuItem(new MenuItemSeparator(), 1);
            InsertMenuItem(exitbut, 2);

            _wf.tray.MouseDoubleClick += (s, e) => TasksManager.StartTask("fmsman");

            var closeevt = new EventWaitHandle(false, EventResetMode.ManualReset, "fmslapicloseall");
            ThreadPool.RegisterWaitForSingleObject(closeevt, (State, Out) => { PressExit(); }, null,
                Timeout.Infinite, true);
        }

        public static void PressExit()
        {
            _forceexit = true;

            ForceUpdate();
        }

        public static GlobalState GlobalState
        {
            get
            {
                return _gstate;
            }
            set
            {
                _gstate = value;

                ForceUpdate();
            }
        }

        private static void Update()
        {
            if (_forceexit)
            {
                _wf.tray.Visible = false;
                _wf.Close();
                Application.Exit();
                return;
            }

            switch (_gstate)
            {
                case GlobalState.Initialization:
                    _wf.tray.Text = @"Инициализация...";
                    break;

                case GlobalState.ConfigPending:
                    _wf.tray.Text = @"Ожидание конфигурации...";
                    break;

                case GlobalState.Active:
                    _wf.tray.Text = @"Активно";
                    break;
            }

            if (_tooltippending != null)
            {
                _wf.tray.ShowBalloonTip(_tooltippending.Duration, _tooltippending.Caption, _tooltippending.Text, _tooltippending.Icon);

                _tooltippending = null;
            }

            if (_wf.tray.ContextMenuStrip == null)
                _wf.tray.ContextMenuStrip = new ContextMenuStrip();

            var si = _wf.tray.ContextMenuStrip.Items; 
            
            si.Clear();

            IEnumerable<MenuItem> mis;

            lock (_menuitems)
                mis = _menuitems.ToArray();

            foreach (var mi in mis)
                si.Add(cvt(mi));
        }

        private static ToolStripItem cvt(MenuItem Item)
        {
            if (Item is MenuItemSeparator)
                return new ToolStripSeparator();

            var tsmi = new ToolStripMenuItem { Text = Item.Caption };
            tsmi.Click += tsmi_Click;

            if (Item.IsBold)
                tsmi.Font = new System.Drawing.Font(tsmi.Font, System.Drawing.FontStyle.Bold);

            if (Item.IsSubmenu)
            {
                foreach (var mi in Item.Submenu)
                {
                    var si = cvt(mi);

                    tsmi.DropDownItems.Add(si);
                }
            }
            else
                tsmi.Tag = Item;

            return tsmi;
        }

        private static void tsmi_Click(object sender, EventArgs e)
        {
            var mi = (sender as ToolStripMenuItem)?.Tag as MenuItem;

            mi?.RaiseOnInvoke();
        }

        /// <summary>
        /// Отображает всплывающее сообщение в трее
        /// </summary>
        /// <param name="Duration">Длительность отображения</param>
        /// <param name="Caption">Заголовок сообщения</param>
        /// <param name="Text">Текст сообщения</param>
        /// <param name="Icon">Иконка сообщения</param>
        /// <param name="Force">Игнорировать параметр Silent в главном конфигурационном файле</param>
        public static void ShowBalloonTip(int Duration, string Caption, string Text, ToolTipIcon Icon, bool Force = false)
        {
            if (!Config.Silent || Force)
            {
                _tooltippending = new ToolTipInfo { Caption = Caption, Duration = Duration, Icon = Icon, Text = Text };

                ForceUpdate();
            }
        }

        public static void InsertMenuItem(MenuItem Item, int Index)
        {
            lock (_menuitems)
                _menuitems.Insert(Index, Item);

            if (Item.Parent == null)
                Item.OnChanged += m => ForceUpdate();

            ForceUpdate();
        }

        private static void ForceUpdate()
        {
            try
            {
                _wf.BeginInvoke(new Action(Update));
            }
            catch (InvalidOperationException) { }
        }
    }
}
