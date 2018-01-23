using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace fmslapi.Storage
{
    /// <summary>
    /// Работа с постоянным хранилищем
    /// </summary>
    internal partial class PersistStorage
    {
        private class Index : IIndex
        {
            private readonly byte[] _index;
            private readonly PersistStorage _stg;
            private readonly bool _unique;

            public Index(byte[] Index, PersistStorage Storage, bool UniqueContent = false)
            {
                _index = Index;
                _stg = Storage;
                _unique = UniqueContent;
            }

            public IKey GetKey(byte[] Key)
            {
                return new Key(Key, _index, _stg, _unique ? _index : null);
            }

            public IKey GetKey(string Key)
            {
                return new Key(Encoding.UTF8.GetBytes(Key), _index, _stg, _unique ? _index : null);
            }

            public byte[] GetContent()
            {
                var ms = new MemoryStream();
                var wr = new BinaryWriter(ms);

                //var tk = Guid.NewGuid().ToByteArray();          // Ключ транзакции

                wr.Write('N');
                //wr.Write(tk);
                wr.Write((UInt16)_index.Length);
                wr.Write(_index);

                _stg.ClearKey(_index);

                _stg._chan.SendMessage(ms.ToArray());

                return _stg.WaitKey(_index);
            }

            public void GetContent(Action<IIndex, byte[]> Callback)
            {
                if (Callback == null)
                    return;

                ThreadPool.QueueUserWorkItem(x => Callback(this, GetContent()));
            }

            public void Remove(IKey Key)
            {
                Key?.Remove();
            }

            /// <summary>
            /// Возвращает список всех ключей индекса
            /// </summary>
            public IList<IKey> GetKeys()
            {
                var ms = new MemoryStream(GetContent());
                var rd = new BinaryReader(ms);
                var cnt = rd.ReadUInt32();

                var lst = new List<IKey>();

                for (var i = 0; i < cnt; i++)
                {
                    var l = rd.ReadUInt16();
                    var k = rd.ReadBytes(l);

                    var v = rd.ReadBytes(rd.ReadInt32());

                    var ky = new CachedKey(k, _index, _stg, v);

                    lst.Add(ky);
                }

                return lst;
            }
        }
    }
}
