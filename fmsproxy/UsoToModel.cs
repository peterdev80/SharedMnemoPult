using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fmslapi;
using System.IO;

namespace fmsproxy
{
    /// <summary>
    /// Соединение потока данных от УСО пульта к модели
    /// </summary>
    public class UsoToModel : ModelVarProxy
    {
        public static event Action SoundOff;

        private int __PUSK_FROM_USO_index;
        private int __STOP_FROM_USO_index;
        private int __soundoff_index;
        private IKVariable __soundoff;

        private IIntVariable __u_ruo_kurs;
        private IIntVariable __u_ruo_tangaj;
        private IIntVariable __u_ruo_kren;

        static UsoToModel()
        {
            UsoToInpu.SoundOff += () => { if (SoundOff != null) SoundOff(); };
        }

        protected override void CustomPostRegistration(IEnumerable<IVariable> Variables)
        {
            __PUSK_FROM_USO_index = _pvars[Variables.FirstOrDefault(v => v.VariableName == "__PUSK_FROM_USO")];
            __STOP_FROM_USO_index = _pvars[Variables.FirstOrDefault(v => v.VariableName == "__STOP_FROM_USO")];
            __soundoff = Variables.Where(v => v.VariableName == "__OTBOI_ZVUKA").FirstOrDefault() as IKVariable;

            __u_ruo_kurs = Variables.FirstOrDefault(v => v.VariableName == "__U_RUO_KURS") as IIntVariable;
            __u_ruo_tangaj = Variables.FirstOrDefault(v => v.VariableName == "__U_RUO_TANGAJ") as IIntVariable;
            __u_ruo_kren = Variables.FirstOrDefault(v => v.VariableName == "__U_RUO_KREN") as IIntVariable;

            __u_ruo_kren.CheckDups = true;
            __u_ruo_kurs.CheckDups = true;
            __u_ruo_tangaj.CheckDups = true;

            __soundoff_index = _pvars[__soundoff];
            __soundoff.VariableChanged += __soundoff_VariableChanged;

            Bivni.OnRUO += Bivni_OnRUO;
        }

        void Bivni_OnRUO(int X, int R, int Y)
        {
            __u_ruo_kurs.Value = X;
            __u_ruo_kren.Value = R;
            __u_ruo_tangaj.Value = Y;

            _varchan.SendChanges();
        }

        void __soundoff_VariableChanged(IVariable Sender, bool IsInit)
        {
            if (SoundOff != null)
                SoundOff();
        }

        protected override void IndividualUdpSendProcessPre(IVariable Variable, BinaryWriter Writer)
        {
            if (Variable.VariableName == "__PUSK_FROM_USO")
            {
                Writer.Write((Int16)__STOP_FROM_USO_index);
                Writer.Write((byte)0);
            }

            if (Variable.VariableName == "__STOP_FROM_USO")
            {
                Writer.Write((Int16)__PUSK_FROM_USO_index);
                Writer.Write((byte)0);
            }
        }

        protected override void IndividualVarProcess(ushort Index, IVariable Variable, object NewValue, ref bool UndoVar)
        {
            if (Index == __soundoff_index && SoundOff != null)
                SoundOff();
        }

        public override void CloseProxy()
        {
            Bivni.OnRUO -= Bivni_OnRUO;
            __soundoff.VariableChanged -= __soundoff_VariableChanged;
            base.CloseProxy();
        }
    }
}
