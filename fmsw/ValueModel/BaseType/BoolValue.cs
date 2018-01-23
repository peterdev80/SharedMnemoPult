using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ValueModel.BaseType
{
  public  class BoolValue : Valuer<bool>
    {
        public BoolValue()
        {
            ValueState = false;
        }
        public BoolValue(bool var)
        {
            ValueState = var;

        }
    }
}
