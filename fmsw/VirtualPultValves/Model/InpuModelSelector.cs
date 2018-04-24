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
        /// <summary>
        /// Конструктор для внутреннего использования через Instance
        /// </summary>
        public InpuModelSelector()
        {
            LoadedInpu = String.Empty;
        }
       //Имя загруженного ИнПУ
      public  string LoadedInpu { get; set; }  
          
     /// <summary>
     /// Перегрузка ИнПу пусстым значением
     /// </summary>
      public  void ReloadSelectInpu()
      {
          LoadedInpu = String.Empty;
      }


    }
}
