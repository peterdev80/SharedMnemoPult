using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using ValueModel.BaseType;

namespace ValueModel.BaseModelCommand
{
    public class SendKBit : VarSendClass
    {
        private List<BitPosValue> KeyBit;
        public SendKBit(string VarID)
            : base(VarID)
        {
            KeyBit = new List<BitPosValue>(){BitPosValue.key1,BitPosValue.key2,BitPosValue.key3,BitPosValue.key4,BitPosValue.key5
                ,BitPosValue.key6,BitPosValue.key7,BitPosValue.key8,BitPosValue.key9,BitPosValue.key10,BitPosValue.key11,BitPosValue.key12,
            BitPosValue.key13,BitPosValue.key14,BitPosValue.key15,BitPosValue.key16,BitPosValue.key17,BitPosValue.key18,BitPosValue.key19,
            BitPosValue.key20,BitPosValue.key21,BitPosValue.key22,BitPosValue.key23,BitPosValue.key24,BitPosValue.key25,BitPosValue.key26,
            BitPosValue.key27,BitPosValue.key28,BitPosValue.key29,BitPosValue.key30,BitPosValue.key31,BitPosValue.key32};

        }
       /* protected override void SetBinding(string varObmen, string varID)
        {
            //для комманд не устанавливаем
        }*/
        protected override void SendValue(object PARAM)
        {
            Debug.WriteLine((this.VarID));
            VHCommands.ComBitK(this.VarID, this.VarName, PARAM, KeyBit);
        }
    }
}
