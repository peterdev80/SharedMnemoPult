using System;
using System.Windows;
using System.IO;

namespace fmsman.Formats
{
    public partial class AdmLoc
    {
        private readonly Connection _connection;

        public AdmLoc(Connection Connection)
        {
            _connection = Connection;
            InitializeComponent();
        }

        private void SendCmd(Action<BinaryWriter> Filler)
        {
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write('Z');
            wr.Write('H');
            Filler(wr);

            _connection.SendCmd(ms.ToArray());
        }

        private void Killall(object sender, RoutedEventArgs e)
        {
            SendCmd(f => f.Write('Y'));  
        }

        private void lstart(object sender, RoutedEventArgs e)
        {
            var t = tblstart.Text;

            SendCmd(f =>
                {
                    f.Write('F');
                    f.Write(t);
                });
        }

        private void rstart(object sender, RoutedEventArgs e)
        {
            var t = tbrstart.Text;

            SendCmd(f =>
            {
                f.Write('G');
                f.Write(tbrstarthost.Text);
                f.Write(t);
            });
        }

        private void start(object sender, RoutedEventArgs e)
        {
            SendCmd(f =>
            {
                f.Write('D');
                f.Write(starttb.Text);
            });
        }

        private void shutdown(object sender, RoutedEventArgs e)
        {
            SendCmd(f => f.Write('Z')); 
        }
    }
}
