using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ValueModel.BaseType
{
   public class FloatValue : Valuer<Double>
    {
        public FloatValue()
        {
            Round = false;
        }       

        public bool Round { get; set; }

        protected override double ras()
        {

            if (Round) return (float)Math.Round(_ValueState);
            return base.ras();
        }
    }
}
