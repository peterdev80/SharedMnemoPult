using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ValueModel.BaseModel;
using ValueModel.BaseModelCommand;

namespace VirtualPultValves.Model
{
    public sealed class ModelVariableRepository
    {
        #region Sigleton
        private static volatile ModelVariableRepository instance;
        private static object syncRoot = new Object();


        public static ModelVariableRepository Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ModelVariableRepository();
                    }
                }

                return instance;
            }
        }
        #endregion
      
        //все принимаеммые переменные
        public ObservableCollection<BitVar> BitValues { get; private set; }
        public ObservableCollection<BoolVar> BoolValues { get; private set; }
        public ObservableCollection<IntVar> IntValues { get; private set; }
        public ObservableCollection<DoubleVar> DoubleValues { get; private set; }
        //ФМС переменные для определения ИНПУ
        public ObservableCollection<BoolVarFMS> BoolFMSValues { get; private set; }

        //все передоваемые переменые
        public ObservableCollection<VarSendClass> KomValues { get; set; }
        string varlocate = "pack://application:,,,/VirtualPultValves;component/Data/VariableList.xml";
     

        private ModelVariableRepository()
        {
            //значение по умолчанию
         //   VarLocate = "pack://application:,,,/VirtualPultValves;component/Data/VariableList.xml";
            DataAccess.DataReader dreader = new DataAccess.DataReader();
            List<string> ls = new List<string>();
            BitValues = new ObservableCollection<BitVar>();
            BoolValues = new ObservableCollection<BoolVar>();
            IntValues = new ObservableCollection<IntVar>();
            DoubleValues = new ObservableCollection<DoubleVar>();
            BoolFMSValues = new ObservableCollection<BoolVarFMS>();
            KomValues = new ObservableCollection<VarSendClass>();
            XMLValidates xmlts = new XMLValidates();
            foreach (XMLValidate xmlt in xmlts.XMLTegs)
                CreateCollectionVariable(dreader.GetVariableName(varlocate, xmlt.Target, xmlt.State), xmlt.type);

            CteateCollectionCommand(dreader.GetCommandName(varlocate));
           
        }
        private void CreateCollectionVariable(List<string> lst, int type)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (type == 0)
                {
                    BitVar bv = new BitVar("Klapany");
                    bv.VarName = lst[i];
                    BitValues.Add(bv);
                }
                if (type == 1)
                {
                    BoolVar bov = new BoolVar("Klapany");
                    bov.VarName = lst[i];
                    BoolValues.Add(bov);
                }
                if (type == 2)
                {
                    IntVar iv = new IntVar("Klapany");
                    iv.VarName = lst[i];
                    IntValues.Add(iv);
                }
                if (type == 3)
                {
                    DoubleVar dv = new DoubleVar("Klapany");
                    dv.VarName = lst[i];
                    DoubleValues.Add(dv);

                }
                if (type == 4)
                {
                    BoolVarFMS bvfms = new BoolVarFMS("Klapany");
                    bvfms.VarName = lst[i];
                    BoolFMSValues.Add(bvfms);
                }

            

        }
        }
        private void CteateCollectionCommand(Dictionary<string,string> lst)
        {
           List<string> l=lst.Keys.ToList();
            for (int i = 0; i < l.Count; i++)
            {
                
                string typ=lst[l[i]];



                if (typ == "Bit")
                {
                    SendKBit skb = new SendKBit("Klapany");
                    skb.VarName = l[i];
                    KomValues.Add(skb);

                }
                if (typ == "Int")
                {
                    SendKInt ski = new SendKInt("Klapany");
                    ski.VarName = l[i];
                    KomValues.Add(ski);

                }
                if (typ == "KBool")
                {
                    SendKBool skbool = new SendKBool("Klapany");
                    skbool.VarName = l[i];
                    KomValues.Add(skbool);
                }
                if (typ == "KVar")
                {
                    SendKVar skv = new SendKVar("Klapany");
                    skv.VarName = l[i];
                    KomValues.Add(skv);
                }

            }
        }
    }
}
