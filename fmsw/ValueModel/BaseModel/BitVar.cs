using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ValueModel.BaseType;

namespace ValueModel.BaseModel
{
    public class BitVar : VarClass
    {

        private List<BitPosValue> KeyBit;

        public ObservableCollection<BoolValue> ValState { get; set; }
        public static readonly DependencyProperty VaRStateBitProperty = DependencyProperty.Register(
              "VaRStateBit", typeof(int), typeof(VarClass), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnStatVarUpd));
        public int VaRStateInt
        {
            get { return (int)GetValue(VaRStateBitProperty); }
            set { this.SetValue(VaRStateBitProperty, value); }
        }


        public BitVar(string varID)
            : base(varID)
        {
            KeyBit = new List<BitPosValue>(){BitPosValue.key1,BitPosValue.key2,BitPosValue.key3,BitPosValue.key4,BitPosValue.key5
                ,BitPosValue.key6,BitPosValue.key7,BitPosValue.key8,BitPosValue.key9,BitPosValue.key10,BitPosValue.key11,BitPosValue.key12,
            BitPosValue.key13,BitPosValue.key14,BitPosValue.key15,BitPosValue.key16,BitPosValue.key17,BitPosValue.key18,BitPosValue.key19,
            BitPosValue.key20,BitPosValue.key21,BitPosValue.key22,BitPosValue.key23,BitPosValue.key24,BitPosValue.key25,BitPosValue.key26,
            BitPosValue.key27,BitPosValue.key28,BitPosValue.key29,BitPosValue.key30,BitPosValue.key31,BitPosValue.key32};

            ValState = new ObservableCollection<BoolValue>();
            for (int i = 0; i < 32; i++)
                ValState.Add(new BoolValue());
        }
        protected override void SetBinding(string varObmen, string varID)
        {
            VHBinding.BitBinding(this, varObmen, varID);
        }


        private static void OnStatVarUpd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            BitVar con = d as BitVar;

            if (con.ValState != null)
            {
                for (int i = 0; i < con.ValState.Count; i++)
                {
                    BitPosValue val = (BitPosValue)Enum.Parse(typeof(BitPosValue), e.NewValue.ToString());

                    if (val.HasFlag(con.KeyBit[i])) con.ValState[i].ValueState = true; else con.ValState[i].ValueState = false;


                }
            }
#if DEBUG
            if (con.ValState == null) Debug.WriteLine("WARNING незаплонированное изменение переменной");
#endif

        }
    }
}
