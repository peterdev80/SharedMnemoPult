using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ValueModel.BaseType
{
    [Flags]
    public enum BitPosValue : uint
    {
        None = 0,
        key1 = 1,
        key2 = 2,
        key3 = 4,
        key4 = 8,
        key5 = 16,
        key6 = 32,
        key7 = 64,
        key8 = 128,
        key9 = 256,
        key10 = 512,
        key11 = 1024,
        key12 = 2048,
        key13 = 4096,
        key14 = 8192,
        key15 = 16384,
        key16 = 32768,
        key17 = 65536,
        key18 = 131072,
        key19 = 262144,
        key20 = 524288,
        key21 = 1048576,
        key22 = 2097152,
        key23 = 4194304,
        key24 = 8388608,
        key25 = 16777216,
        key26 = 33554432,
        key27 = 67108864,
        key28 = 134217728,
        key29 = 268435456,
        key30 = 536870912,
        key31 = 1073741824,
        key32 = 2147483648

    }
}
