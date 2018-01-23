using fmslapi.Bindings.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ValueModel.BaseModel
{
    public class VHBinding
    {
        #region Static Binding
        public static void CicloIntBinding(VarClass bnd, string varObmen, string varID)
        {

//#if DEBUG
//            try
//            {
//                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
//                    return;
//            }
//            catch (ApplicationException) { }
//#endif
            ExpressionBinding.SetBinding(bnd, CicloIntVar.VarCicloIntProperty, varObmen);


        }
        public static void BitIntBinding(VarClass bnd, string varObmen, string varID)
        {
//#if DEBUG
//            try
//            {
//                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
//                    return;
//            }
//            catch (ApplicationException) { }
//#endif

            ExpressionBinding.SetBinding(bnd, BitIntVar.VarBitIntProperty, varObmen);




        }
        public static void BitBinding(VarClass bnd, string varObmen, string varID)
        {
//#if DEBUG
//            try
//            {
//                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
//                    return;
//            }
//            catch (ApplicationException) { }
//#endif
            ExpressionBinding.SetBinding(bnd, BitVar.VaRStateBitProperty, varObmen);



        }
        public static void BoolBinding(VarClass bnd, string varObmen, string varID)
        {
//#if DEBUG
//            try
//            {
//                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
//                    return;
//            }
//            catch (ApplicationException) { }
//#endif

            ExpressionBinding.SetBinding(bnd, BoolVar.VaRStateBoolProperty, varObmen);
        }
        public static void DoubleBinding(VarClass bnd, string varObmen, string varID)
        {

//#if DEBUG
//            try
//            {
//                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
//                    return;
//            }
//            catch (ApplicationException) { }
//#endif

            ExpressionBinding.SetBinding(bnd, DoubleVar.VaRStateDoubleProperty, varObmen);
        }
        public static void IntBinding(VarClass bnd, string varObmen, string varID)
        {
//#if DEBUG
//            try
//            {
//                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
//                    return;
//            }
//            catch (ApplicationException) { }
//#endif


            ExpressionBinding.SetBinding(bnd, IntVar.VaRStateIntProperty, varObmen);
        }
        public static void FloatBinding(VarClass bnd, DependencyProperty dp, string varObmen, string varID)
        {

//#if DEBUG
//            try
//            {
//                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
//                    return;
//            }
//            catch (ApplicationException) { }
//#endif

            ExpressionBinding.SetBinding(bnd, dp, varObmen);

        }
        public static void StateBinding(VarClass bnd, DependencyProperty dp, string varObmen, string varID)
        {


//#if DEBUG
//            try
//            {
//                if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
//                    return;
//            }
//            catch (ApplicationException) { }
//#endif


            ExpressionBinding.SetBinding(bnd, dp, varObmen);
        }

      


        #endregion
        /*  #region Static Command
      public static void ComBoolK(string varID, string VarName, object PARAM)
        {
            int i = 0;
            bool val = false;
            if (!int.TryParse(PARAM.ToString(), out i)) throw new Exception("В качестве парамет параметра Комманды должен быть инт");
            if (i != 0) val = true;
            var VaRSend = VHClass.Instance.GetVHPC(varID).VHCurr.GetBoolVariable(VarName);
            VaRSend.AutoSend = true;
            VaRSend.Value = val;
        }

        public static void ComIntK(string varID, string VarName, object PARAM)
        {
            int i = 0;
            if (!int.TryParse(PARAM.ToString(), out i)) throw new Exception("В качестве парамет параметра Комманды должен быть инт");
            var VaRSend = VHClass.Instance.GetVHPC(varID).VHCurr.GetIntVariable(VarName);
            VaRSend.AutoSend = true;
            VaRSend.Value = i;
        }
        public static void ComComK(string varID, string VarName, object PARAM)
        {
            int i = 0;
            if (!int.TryParse(PARAM.ToString(), out i)) throw new Exception("В качестве парамет параметра Комманды должен быть инт");
            var VaRSend = VHClass.Instance.GetVHPC(varID).VHCurr.GetKVariable(VarName);
            VaRSend.AutoSend = true;
            VaRSend.Set();
        }
        public static void ComBitK(string varID, string VarName, object PARAM, List<BitPosValue> KeyBit)
        {
            int i = 0;
            if (!int.TryParse(PARAM.ToString(), out i)) throw new Exception("В качестве парамет параметра Комманды должен быть инт");
            BitPosValue _val = KeyBit[i];
            var VaRSend = VHClass.Instance.GetVHPC(varID).VHCurr.GetIntVariable(VarName);
            VaRSend.AutoSend = true;
            VaRSend.Value = (int)_val;
        }
        #endregion*/
    }
}
