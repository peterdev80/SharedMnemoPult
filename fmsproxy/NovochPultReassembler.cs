using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace fmsproxy
{
    public class NovochPultReassembler : PacketReassembler
    {
        private int _skipfromindex = -1;
        private bool _skipfast;
        private readonly HashSet<int> _skiplist = new HashSet<int>();

        public override void ReassemblyPacket(byte[] Data, Action<byte[]> Enqueue)
        {
            var mvp = _proxy as ModelVarProxy;
            if (mvp == null)
            {
                Enqueue(Data);
                return;
            }
            
            var rdr = new BinaryReader(new MemoryStream(Data));
            var length = rdr.ReadInt16();

            var oms = new MemoryStream();
            var wrt = new BinaryWriter(oms);
            wrt.Write((Int16)0);

            bool hasout = false;

            while (true)
            {
                var index = rdr.ReadInt16();
                if (index == 2000)
                    break;

                byte len = 0;
                mvp.VarLengths.TryGetValue(index, out len);
                if (len == 0)
                    break;

                var buf = rdr.ReadBytes(len); 
                
                if (index >= _skipfromindex && _skipfromindex > 0)
                    if (_skipfast)
                        break;
                    else
                        continue;

                if (_skiplist.Contains(index))
                    continue;

                wrt.Write((Int16)index);
                wrt.Write(buf);
                hasout = true;
            }

            if (!hasout)
                return;

            wrt.Write((Int16)2000);
            wrt.Write((long)0);
            oms.Seek(0, SeekOrigin.Begin);
            wrt.Write((Int16)oms.Length);

            Enqueue(oms.ToArray());
        }

        public override void Init()
        {
            var mvp = _proxy as ModelVarProxy;
            if (mvp == null)
                return;

            var skipfrom = _config["skip.from"];
            _skipfromindex = mvp.GetIndex200(skipfrom);
            _skipfast = _config.GetBool("skip.fast");

            var skips = _config.AsWordsArray("skip.name");

            foreach (var sv in skips)
                _skiplist.Add(mvp.GetIndex200(sv));
        }
    }
}
