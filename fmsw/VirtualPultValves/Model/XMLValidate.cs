using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualPultValves.Model
{
    struct XMLValidate
    {
        public string State;
        public string Target;
        public int type;
        public XMLValidate(string state, string target, int vtype)
        {
            State = state;
            Target = target;
            type = vtype;

        }
    }
    class XMLValidates
    {
        public List<XMLValidate> XMLTegs;
        public XMLValidates()
        {
            XMLTegs = new List<XMLValidate>() { 
                new XMLValidate("State","Bit",0),
               new XMLValidate("State","Bool",1),
                new XMLValidate("State","Int",2) ,
             new XMLValidate("State","Double",3),
             new XMLValidate("State","BoolFMS",4)
             /*,
               new XMLValidate("Send","Bit",4),
              new XMLValidate("Send","Int",5),
                 new XMLValidate("Send","KBool",6),
                 new XMLValidate("Send","KVar",7) */
            
            
            };

        }
    }
}
