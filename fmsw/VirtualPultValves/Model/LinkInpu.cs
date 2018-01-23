using fmslapi;
using fmslapi.Channel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using fmslapi.Bindings.WPF;
using ValueModel.BaseModel;
namespace VirtualPultValves.Model
{
    public class LinkInpu
    {
        const int LINVAR = 6;
        const int LOUTVAR = 7;
        private static volatile LinkInpu instance;
        //private readonly SyncChannel _rcvchannel;
        private readonly IChannel _chan;
        private readonly DispatcherTimer _dt;
        public static LinkInpu Instance
        {
            get
            {
                if (instance == null)
                    instance = new LinkInpu();

                return instance;
            }
        }



        private BitVector32[] dins = new BitVector32[LOUTVAR];
        private void sendBuf()
        {
            var bs = new UInt32[LOUTVAR];


            for (var i = 0; i < LOUTVAR; i++)
                bs[i] = (uint)dins[i].Data;
            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write((UInt32)2003); //запись клапонов
            foreach (var bb in bs)
                wr.Write(bb);

            _chan.SendMessage(ms.ToArray());

        }
        public void SetSendVar(bool vall, int poss, int valnum)
        {
            for (var i = 0; i < dins.Length; i++)
                dins[i] = new BitVector32();
            dins[valnum][1 << poss] = vall;
            sendBuf();



        }
        public void SetSendVar(int vall, int valnum)
        {
            for (var i = 0; i < dins.Length; i++)
                dins[i] = new BitVector32();
            dins[valnum][BitVector32.CreateSection(100)] = vall;

            sendBuf();

        }






        public LinkInpu()
        {

            var m = Manager.GetAPI("VirtKlapany", new Guid("{0BDF2636-CF7F-42D3-AF39-7801EDAFEFD5}"));

            _chan = m.JoinChannel("IO_KLAPANS", null);
            _chan.SyncReceive = true;
            //_rcvchannel = new SyncChannel(m.JoinChannel("MODEL_TO_KLAPANY", null));

            _dt = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Normal, OnTimer, Dispatcher.CurrentDispatcher);
            _dt.Start();

            var repos = ModelVariableRepository.Instance;
            ExpressionBinding.SetBinding(repos.BoolFMSValues[0], BoolVarFMS.VaRStateBoolPropertyFMS, "__FMS_WD_INPU1_LOADED");
            ExpressionBinding.SetBinding(repos.BoolFMSValues[1], BoolVarFMS.VaRStateBoolPropertyFMS, "__FMS_WD_INPU2_LOADED");
        }
        private void OnTimer(object sender, EventArgs e)
        {
            ISenderChannel sc;
            ReceivedMessage rm;

            while (_chan.Receive(out sc, out rm))
            {
                var b = new UInt32[LINVAR];
                var rd = new BinaryReader(new MemoryStream(rm.Data));
                if (rd.ReadUInt32() != 2002)
                    break;
                for (var i = 0; i < LINVAR; i++)
                    b[i] = rd.ReadUInt32();

                var repos = ModelVariableRepository.Instance;

                for (int i = 0; i < 3; i++)
                {
                    repos.BitValues[i].VaRStateInt = (Int32)b[i];


                }
                for (int i = 0; i < 3; i++)
                    repos.IntValues[i].VaRStateInt = (Int32)b[i + 3];


            }
        }
    }
}
