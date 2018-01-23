using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace VirtualPultValves.Model
{
    public sealed class InpuModelSelector 
    {
        #region Sigleton
        private static volatile InpuModelSelector instance;
        private static object syncRoot = new Object();


        public static InpuModelSelector Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new InpuModelSelector();
                    }
                }

                return instance;
            }
        }
        #endregion

        public InpuModelSelector()
        {
            LoadedInpu = String.Empty;
        }

      public  string LoadedInpu { get; set; }  
          
     
      public  void ReloadSelectInpu()
      {
          LoadedInpu = String.Empty;
      }


    }
}
