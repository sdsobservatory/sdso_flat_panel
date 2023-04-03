using ASCOM.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASCOM.SDSO
{
    [ComVisible(false)]
    public partial class SetupDialogForm : Form
    {
        private readonly TraceLogger tl;

        public SetupDialogForm(TraceLogger tlDriver)
        {
            InitializeComponent();
            tl = tlDriver;
            InitUI();
        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            try
            {
                CoverCalibrator.comPort = (string)comboBoxComPort.SelectedItem;
            }
            catch { }
            tl.Enabled = chkTrace.Checked;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void InitUI()
        {
            chkTrace.Checked = tl.Enabled;
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            if (comboBoxComPort.Items.Contains(CoverCalibrator.comPort))
            {
                comboBoxComPort.SelectedItem = CoverCalibrator.comPort;
            }
        }
    }
}