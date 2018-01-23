using System.Windows.Forms;

namespace fmsldr
{
    public partial class FWaiting : Form
    {
        // ReSharper disable once NotAccessedField.Local
        private static FWaiting _instance;

        public FWaiting()
        {
            _instance = this;

            InitializeComponent();

#if EmbedLdr
            tray.Visible = false;
#else
            tray.Visible = true;
#endif
        }

        public new void CreateHandle()
        {
            if (IsHandleCreated)
                return;

            base.CreateHandle();
        }
    }
}
