using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fmslapi.WPF.Variables;
using VirtualPultValves.DataAccess;

namespace VirtualPultValves.Model
{
    public static class VarHostMy
    {
        private static VariablesHost _vh;
        public static VariablesHost GetVarHostMy
        {
            get
            {
                if (_vh == null)
                {
                    DataReader dr = new DataReader();
                    string namehost = dr.ReadVHName("pack://application:,,,/VirtualPultValves;component/Data/InitXMLFile.xml","VH");
                    _vh = new VariablesHost(namehost);
                }
                return _vh;

            }


        }
    }
}
