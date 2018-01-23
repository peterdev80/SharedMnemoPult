using System.Windows.Forms;
using System.Diagnostics;

namespace fmslapi
{
    public partial class WaitForFMSLDR : Form
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public bool ManualClosed = true;

        public WaitForFMSLDR()
        {
            InitializeComponent();
        }

        private void WaitForFMSLDR_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ManualClosed)
                Process.GetCurrentProcess().Kill();
        }
    }
}
