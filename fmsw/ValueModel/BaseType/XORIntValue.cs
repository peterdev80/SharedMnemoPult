using System;
using System.Collections.Generic;
using System.Text;

namespace ValueModel.BaseType
{
    public class XORIntValue : IntValue
    {
        private IntValue fVal1l;
        private IntValue fVal2;
        private List<IntValue> ListValue = new List<IntValue>();
        public XORIntValue()
        {
        }
        public void AddVal(IntValue val)
        {
            val.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(val_PropertyChanged);
            ListValue.Add(val);
        }

        void val_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.ValueState = 0;
            foreach (IntValue iv in ListValue)
                if (iv.ValueState > 0) this.ValueState = 1;
        }





        public IntValue Val1
        {
            get { return fVal1l; } 
           set 
           {
               fVal1l = value;
               if (Val1.ValueState > 0 ) this.ValueState = 1;
              

               fVal1l.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Val1_PropertyChanged);
           }
       }

      public  IntValue Val2 { get {return fVal2;}
          set
          {
              fVal2 = value;
              if (Val2.ValueState > 0) this.ValueState = 1;
              
              fVal2.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Val1_PropertyChanged);
          }}
        
        public XORIntValue(IntValue val1, IntValue val2)

        {
            Val1 = val1;
            Val2 = val2;
        }

        void Val1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Val1.ValueState > 0 || Val2.ValueState > 0) this.ValueState = 1;
            else
                this.ValueState = 0;
        }
        private void SetLoad()
        {



           
        }
    }
}
