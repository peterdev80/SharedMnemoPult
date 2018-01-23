using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using fmslapi;
using System.IO;
using fmslapi.Channel;

namespace fmsproxy
{
    public class UsoToInpu : ModelVarProxy
    {
        public static Action SoundOff;
        private int[] _soffind;
        private IVariable[] _sofv;
        private IVariable[] _allv;
        private List<IVariable> _keymap1 = new List<IVariable>();
        private List<IVariable> _keymap2 = new List<IVariable>();
        private UInt32 numpack = 0;
        private bool _emulwago;
        private IChannel _wagochan;
        private static readonly sbyte[] km1 = new sbyte[]
        { 11,   // 0
          2,    // 1
          3,    // 2
          4,    // 3
          5,    // 4
          6,    // 5
          7,    // 6
          8,    // 7
          9,    // 8
          10,   // 9
          15,   // →
          14,   // ←
          13,   // ↓
          12,   // ↑
          48,   // ИСП
          49,   // СБР
          0,    // ВКЛ
          1,    // ОТКЛ
          23,   // +1
          22,   // -1
          18,   // Тест
          19,   // УПР
          20,   // ИП 
          21,   // Ф1
          16,   // Вкл. питания
          17    // Выкл. питания  
        };

        private static readonly sbyte[] km2 = new sbyte[]
        { 35,   // 0
          26,   // 1
          27,   // 2
          28,   // 3
          29,   // 4
          30,   // 5
          31,   // 6
          32,   // 7
          33,   // 8
          34,   // 9
          39,   // →
          38,   // ←
          37,   // ↓
          36,   // ↑
          50,   // ИСП
          51,   // СБР
          24,   // ВКЛ
          25,   // ОТКЛ
          47,   // +1
          46,   // -1
          42,   // Тест
          43,   // УПР
          44,   // ИП
          45,   // Ф1
          40,   // Вкл. питания
          41    // Выкл. питания
        };

        private static void snb(ref UInt16 acc, bool val)
        {
            acc <<= 1;
            acc |= (val ? (UInt16)1 : (UInt16)0);
        }

        protected override void CustomPostRegistration(IEnumerable<IVariable> Variables)
        {
            _emulwago = _config.GetBool("emulate.wago");

            var lsoi = new List<int>();
            var lsov = new List<IVariable>();

            Action<string> addso = s =>
            {
                var so = Variables.Where(v => v.VariableName == s).FirstOrDefault() as IKVariable;
                var soi = _pvars[so];
                lsoi.Add(soi);
                lsov.Add(so);
                so.VariableChanged += __soundoff_VariableChanged;
            };

            addso("__INPU1_KOM_OTKL");
            addso("__INPU2_KOM_OTKL");

            _soffind = lsoi.ToArray();
            _sofv = lsov.ToArray();

            if (!_emulwago)
                return;

            _allv = Variables.ToArray();

            foreach (var v in _allv)
            {
                v.NeedLocalFeedback = true;
                v.VariableChanged += v_VariableChanged;
            }

            foreach (var k in km1)
                _keymap1.Add(_allv[k]);

            foreach (var k in km2)
                _keymap2.Add(_allv[k]);

            _wagochan = _manager.JoinChannel("IO_NEPTUN", null);
        }

        void v_VariableChanged(IVariable Sender, bool IsInit)
        {
            if (IsInit)
                return;

            if (Sender.VariableType == VariableType.Boolean)
                if (!((IBoolVariable)Sender).Value)
                    return;

            var ms = new MemoryStream();
            var wr = new BinaryWriter(ms);
            wr.Write((UInt32)0x71AF5A13);
            wr.Write((UInt16)7);                    // Sender - WAGO

            var indx1 = _keymap1.IndexOf(Sender) + 1;
            var indx2 = _keymap2.IndexOf(Sender) + 1;

            if (indx1 <= 0 && indx2 <= 0)
                return;

            if (indx1 > 0)
                wr.Write((UInt16)1);                // Receiver - INPU1

            if (indx2 > 0)
                wr.Write((UInt16)2);                // Receiver - INPU2

            wr.Write((UInt32)2);                    // ID - Keypress
            wr.Write((UInt32)numpack++);            // Numpack

            wr.Write((byte)4);
            wr.Write((UInt64)(indx1 > 0 ? indx1 : indx2 > 0 ? indx2 : 0));                    // Код клавиши
            wr.Write((UInt64)0);                    // Резерв

            var wp = ms.ToArray();

            _wagochan.SendMessage(wp);
        }

        void __soundoff_VariableChanged(IVariable Sender, bool IsInit)
        {
            if (SoundOff != null)
                SoundOff();
        }

        protected override void IndividualVarProcess(ushort Index, IVariable Variable, object NewValue, ref bool UndoVar)
        {
            if (_soffind.Any(vi => vi == Index && SoundOff != null))
                SoundOff();
        }

        public override void CloseProxy()
        {
            foreach (var v in _sofv)
                v.VariableChanged -= __soundoff_VariableChanged;

            if (_allv != null)
                foreach (var v in _allv)
                    v.VariableChanged -= v_VariableChanged;

            if (_wagochan != null)
                _wagochan.Leave();

            base.CloseProxy();
        }
    }
}
