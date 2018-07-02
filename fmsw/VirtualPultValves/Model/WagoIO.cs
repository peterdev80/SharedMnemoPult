using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using fmslapi;
using System.Windows.Threading;
using System.Collections.Specialized;
using ValueModel.BaseType;

namespace VirtualPultValves.Model
{
    public sealed class WagoIO
    {
        private static volatile WagoIO instance;
        private readonly IChannel _channel;
        private readonly DispatcherTimer _dt;
        private int _senderType = 7;
        private static UInt32 _pid;

        private int _stage;

        #region Data OUT
        private List<TCModel> ListTC;

        private BitVector32[] dins = new BitVector32[16];

        public int TypePult { get; set; }

        public void SetSendVar(bool vall, int poss, int valnum)
        {
            dins[valnum][1 << poss] = vall;
        }

        public BoolValue DejRegim1 { get; set; }
        public BoolValue DejRegim2 { get; set; }
        public BoolValue CentOgon { get; set; }
        public BoolValue clInpu_1 { get; set; }
        public BoolValue clInpu_2 { get; set; }
        public BoolValue cLapmPult { get; set;}
        public BoolValue LampSvyaz1 { get; set; }
        public BoolValue LampSvyaz2 { get; set; }
        public BoolValue LampSvyaz3 { get; set; }

        public int SenderType { get { return _senderType; }  set { _senderType = value; } }

        public void SetListTC(List<TCModel> val1, int stage)
        {
            ListTC = val1;
            _stage = stage;

        }
        #endregion

        public static WagoIO Instance
        {
            get
            {
                if (instance == null)
                    instance = new WagoIO();
                
                return instance;
            }
        }

        public WagoIO()
        {
            for (var i = 0; i < dins.Length; i++)
                dins[i] = new BitVector32();

            //по созданию таймера мы вносим 3 а затем Инпу изменяет ее
            TypePult = 3;

            DejRegim1 = new BoolValue();
            DejRegim2 = new BoolValue();
            CentOgon = new BoolValue();
            clInpu_1 = new BoolValue();
            clInpu_2 = new BoolValue();
            cLapmPult = new BoolValue();
            LampSvyaz1 = new BoolValue();
            LampSvyaz2 = new BoolValue();
            LampSvyaz3 = new BoolValue();

            _channel = new Channel("WagoPort", "ModelAddr");

            _dt = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Normal, OnTimer, Dispatcher.CurrentDispatcher);
            _dt.Start();
        }
        
        private void OnTimer(object sender, EventArgs e)
        {
            byte[] m;

            while ((m = _channel.TryGetMessage()) != null)
            {
                var rd = new BinaryReader(new MemoryStream(m));

                if (rd.ReadUInt32() != 0x71AF5A13)
                    continue;

                if (rd.ReadUInt16() != 3)
                    continue;

                if (rd.ReadUInt16() != 7)
                    continue;

                rd.ReadUInt32();
                rd.ReadUInt32();

                var b = new UInt16[32];

                for (var i = 0; i < 32; i++)
                    b[i] = rd.ReadUInt16();

                var v = (BitPosValue)b[0];
                var v1 = (BitPosValue)b[1];
                var v2 = (BitPosValue)b[2];
                var v3 = (BitPosValue)b[3];
                var v4 = (BitPosValue)b[4];
                var v6 = (BitPosValue)b[6];

                if (ListTC != null)
                {
                    if (_stage == 1)
                    {
                        ListTC[0].ValTC = v.HasFlag(BitPosValue.key1);
                        ListTC[1].ValTC = v.HasFlag(BitPosValue.key2);
                        ListTC[2].ValTC = v.HasFlag(BitPosValue.key3);
                        ListTC[3].ValTC = v.HasFlag(BitPosValue.key4);
                        ListTC[4].ValTC = v.HasFlag(BitPosValue.key5);
                        ListTC[5].ValTC = v.HasFlag(BitPosValue.key6);
                        ListTC[6].ValTC = v.HasFlag(BitPosValue.key7);
                        ListTC[7].ValTC = v.HasFlag(BitPosValue.key8);
                        ListTC[8].ValTC = v.HasFlag(BitPosValue.key9);
                        ListTC[9].ValTC = v.HasFlag(BitPosValue.key10);
                        ListTC[10].ValTC = v.HasFlag(BitPosValue.key11);
                        ListTC[11].ValTC = v.HasFlag(BitPosValue.key12);

                        ListTC[12].ValTC = v.HasFlag(BitPosValue.key13);
                        ListTC[13].ValTC = v.HasFlag(BitPosValue.key14);
                        ListTC[14].ValTC = v.HasFlag(BitPosValue.key15);
                        ListTC[15].ValTC = v.HasFlag(BitPosValue.key16);

                        ListTC[16].ValTC = v1.HasFlag(BitPosValue.key1);
                        ListTC[17].ValTC = v1.HasFlag(BitPosValue.key2);
                        ListTC[18].ValTC = v1.HasFlag(BitPosValue.key3);
                        ListTC[19].ValTC = v1.HasFlag(BitPosValue.key4);
                        ListTC[20].ValTC = v1.HasFlag(BitPosValue.key5);
                        ListTC[21].ValTC = v1.HasFlag(BitPosValue.key6);
                        ListTC[22].ValTC = v1.HasFlag(BitPosValue.key7);
                        ListTC[23].ValTC = v1.HasFlag(BitPosValue.key8);
                    }

                    if (_stage == 2)
                    {
                        ListTC[0].ValTC = v1.HasFlag(BitPosValue.key10);
                        ListTC[1].ValTC = v1.HasFlag(BitPosValue.key11);
                        ListTC[2].ValTC = v1.HasFlag(BitPosValue.key12);
                        ListTC[3].ValTC = v1.HasFlag(BitPosValue.key13);
                        ListTC[4].ValTC = v1.HasFlag(BitPosValue.key14);
                        ListTC[5].ValTC = v1.HasFlag(BitPosValue.key15);
                        ListTC[6].ValTC = v1.HasFlag(BitPosValue.key16);
                        ListTC[7].ValTC = v2.HasFlag(BitPosValue.key1);
                        ListTC[8].ValTC = v2.HasFlag(BitPosValue.key2);
                        ListTC[9].ValTC = v2.HasFlag(BitPosValue.key3);
                        ListTC[10].ValTC = v2.HasFlag(BitPosValue.key4);
                        ListTC[11].ValTC = v2.HasFlag(BitPosValue.key5);

                        ListTC[12].ValTC = v2.HasFlag(BitPosValue.key6);
                        ListTC[13].ValTC = v2.HasFlag(BitPosValue.key7);
                        ListTC[14].ValTC = v2.HasFlag(BitPosValue.key8);
                        ListTC[15].ValTC = v2.HasFlag(BitPosValue.key9);

                        ListTC[16].ValTC = v2.HasFlag(BitPosValue.key10);
                        ListTC[17].ValTC = v2.HasFlag(BitPosValue.key11);
                        ListTC[18].ValTC = v2.HasFlag(BitPosValue.key12);
                        ListTC[19].ValTC = v2.HasFlag(BitPosValue.key13);
                        ListTC[20].ValTC = v2.HasFlag(BitPosValue.key14);
                        ListTC[21].ValTC = v2.HasFlag(BitPosValue.key15);
                        ListTC[22].ValTC = v2.HasFlag(BitPosValue.key16);
                        ListTC[23].ValTC = v3.HasFlag(BitPosValue.key1);
                    }
                }

                if (v6.HasFlag(BitPosValue.key13))
                {
                    for (var i = 0; i <= 23; i++)
                        ListTC[i].ValTC = true;

                    CentOgon.ValueState = false;
                }
                CentOgon.ValueState = v1.HasFlag(BitPosValue.key9);
                DejRegim1.ValueState = v4.HasFlag(BitPosValue.key15);
                DejRegim2.ValueState = v4.HasFlag(BitPosValue.key16);
                clInpu_1.ValueState = v3.HasFlag(BitPosValue.key5) || v3.HasFlag(BitPosValue.key6);
                clInpu_2.ValueState = v3.HasFlag(BitPosValue.key2) || v3.HasFlag(BitPosValue.key4);
                cLapmPult.ValueState = v3.HasFlag(BitPosValue.key3);


                LampSvyaz1.ValueState = v6.HasFlag(BitPosValue.key11);
                LampSvyaz2.ValueState = v6.HasFlag(BitPosValue.key13);
                LampSvyaz3.ValueState = v6.HasFlag(BitPosValue.key15);
            }

            var bs = new UInt16[34];    // Пакеты меньшего размера отбрасываются в LinkINPU700

            for (var i = 0; i < 16; i++)
                bs[i] = (ushort)dins[i].Data;

            bs[15] = (ushort)TypePult;
            bs[5] = 0x7fff;                     // Предохранители

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);

            wr.Write((UInt32)0x71AF5A13);
            wr.Write((UInt16)SenderType);            // sender 7-пульт, 8-ИнПу1, 9-ИнПу2
            wr.Write((UInt16)3);            // receiver
            wr.Write((UInt32)120);          // id
            wr.Write((UInt32)_pid++);       // num

            foreach (var bb in bs)
                wr.Write(bb);

            _channel.SendMessage(ms.ToArray());
        }
    }
}
