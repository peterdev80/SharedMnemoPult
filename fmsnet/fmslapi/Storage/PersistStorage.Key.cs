using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace fmslapi.Storage
{
    /// <summary>
    /// Работа с постоянным хранилищем
    /// </summary>
    internal partial class PersistStorage
    {
        private class Key : IKey
        {
            private readonly byte[] _key;
            private readonly byte[] _index;
            private readonly PersistStorage _stg;

            public Key(byte[] Key, byte[] Index, PersistStorage Storage, byte[] KeyPrefix = null)
            {
                _index = Index;
                _stg = Storage;

                if (KeyPrefix != null)
                {
                    var ms = new MemoryStream();
                    ms.Write(KeyPrefix, 0, KeyPrefix.Length);
                    ms.Write(Key, 0, Key.Length);

                    _key = ms.ToArray();    
                }
                else
                    _key = Key;
            }

            public void Store<T>(T Value, bool Sync)
            {
                var size = Marshal.SizeOf(typeof(T));
                var arr = new byte[size];

                var ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(Value, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);

                Store(arr, Sync);
            }

            public void Store(string Value, bool Sync)
            {
                var a = Encoding.UTF8.GetBytes(Value);

                Store(a, Sync);
            }

            public virtual void Store(byte[] Data)
            {
                Store(Data, true);
            }

            public virtual void Store(byte[] Data, bool Sync)
            {
                if (Data.Length == 0)
                {
                    Remove();
                    return;
                }

                var ms = new MemoryStream();
                var wr = new BinaryWriter(ms);

                var tk = Sync ? Guid.NewGuid().ToByteArray() : Guid.Empty.ToByteArray();          // Ключ транзакции

                wr.Write('I');
                wr.Write(tk);
                wr.Write((UInt16)_key.Length);
                wr.Write(_key);
                if (_index != null)
                {
                    wr.Write((UInt16)_index.Length);
                    wr.Write(_index);
                }
                else
                    wr.Write((UInt16)0);
                wr.Write((UInt32)Data.Length);
                wr.Write(Data);

                _stg._chan.SendMessage(ms.ToArray());

                // Ожидание подтверждения
                if (Sync)
                    _stg.WaitKey(tk);
            }

            public void Remove()
            {
                var ms = new MemoryStream();
                var wr = new BinaryWriter(ms);

                var tk = Guid.NewGuid().ToByteArray();          // Ключ транзакции

                wr.Write('J');
                wr.Write(tk);
                wr.Write((UInt16)_key.Length);
                wr.Write(_key);
                wr.Write((UInt16)_index.Length);
                wr.Write(_index);

                _stg._chan.SendMessage(ms.ToArray());

                // Ожидание подтверждения
                _stg.WaitKey(tk);
            }

            public virtual byte[] Get()
            {
                var ms = new MemoryStream();
                var wr = new BinaryWriter(ms);

                wr.Write('K');
                wr.Write((UInt16)_key.Length);
                wr.Write(_key); 

                _stg._chan.SendMessage(ms.ToArray());

                while (true)
                {
                    _stg._evt.WaitOne(20);

                    byte[] r;

                    lock (_stg._dic)
                    {
                        if (!_stg._dic.TryGetValue(new kw(_key), out r))
                            continue;

                        _stg._dic.Remove(new kw(_key));
                    }

                    return r;
                }
            }

            public T Get<T>() where T : struct
            {
                var b = Get();

                T rv = default(T);

                var size = Marshal.SizeOf(typeof(T));

                if (b.Length < size)
                    return rv;
                
                var ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(b, 0, ptr, size);

                rv = (T)Marshal.PtrToStructure(ptr, typeof(T));
                Marshal.FreeHGlobal(ptr);

                return rv;
            }

            public void Get(Action<IKey, byte[]> Callback)
            {
                if (Callback == null)
                    return;

                ThreadPool.QueueUserWorkItem(x => Callback(this, Get()));
            }

            byte[] IKey.Key => _key;
        }

        private class CachedKey : Key
        {
            private byte[] _value;

            public CachedKey(byte[] Key, byte[] Index, PersistStorage Storage, byte[] Value)
                : base(Key, Index, Storage)
            {
                _value = Value;
            }

            // ReSharper disable once UnusedMember.Local
            public CachedKey(byte[] Key, byte[] Index, PersistStorage Storage)
                : base(Key, Index, Storage)
            {
            }

            public override byte[] Get()
            {
                return _value ?? (_value = base.Get());
            }

            public override void Store(byte[] Data)
            {
                _value = Data;
                base.Store(Data);
            }

            public override void Store(byte[] Data, bool Sync)
            {
                _value = Data;
                base.Store(Data, Sync);
            }
        }
    }
}
