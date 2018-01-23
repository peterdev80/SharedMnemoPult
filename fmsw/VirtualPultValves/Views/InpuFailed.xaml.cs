using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using VirtualPultValves.Annotations;

namespace VirtualPultValves.Views
{
    /// <summary>
    /// Исключение в ИнПУ
    /// </summary>
    [DesignTimeVisible(false)]
    public partial class InpuFailed
    {
        public InpuFailed()
        {
            InitializeComponent();
        }

        [UsedImplicitly]
        public void AssignException(Exception Ex)
        {
            t1.Text = Ex.Message;
            t2.Text = Ex.StackTrace;

            var seh = Ex as ExternalException;
            if (seh != null)
                errorcode.Text = string.Format(" 0x{0:X4}", seh.ErrorCode);
        }
    }
}
