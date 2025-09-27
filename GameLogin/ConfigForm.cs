using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace GameLogin
{
    public partial class ConfigForm : Form
    {
        private Point offset;

        #region IP/Port Properties
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AccountServerAddressIP { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort AccountServerAddressPort { get; set; }
        #endregion

        public ConfigForm()
        {
            InitializeComponent();
            Load += ConfigForm_Load;
            FormClosing += ConfigForm_FormClosing;
            ServerPortBox.KeyPress += ServerPortBox_KeyPress;
        }

        #region Form Load
        private void ConfigForm_Load(object sender, EventArgs e)
        {
            AppConfig.Load();

            AccountServerAddressIP = AppConfig.Current.AccountServerAddressIP;
            AccountServerAddressPort = AppConfig.Current.AccountServerAddressPort;

            if (ServerIPBox != null)
                ServerIPBox.Text = AccountServerAddressIP;

            if (ServerPortBox != null)
                ServerPortBox.Text = AccountServerAddressPort.ToString();

            if (PatchURLBox != null)
                PatchURLBox.Text = AppConfig.Current.PatchUrl ?? string.Empty;

            if (EnableAutoPatcherCheckBox != null)
                EnableAutoPatcherCheckBox.Checked = AppConfig.Current.Enabled;

            if (KeepAutoPatcherBackupsCheckbox != null)
                KeepAutoPatcherBackupsCheckbox.Checked = AppConfig.Current.KeepBackups;
        }
        #endregion

        #region Load Settings
        private void LoadSettings()
        {
            AppConfig.Load();

            AccountServerAddressIP = AppConfig.Current.AccountServerAddressIP;
            AccountServerAddressPort = AppConfig.Current.AccountServerAddressPort;

            if (PatchURLBox != null)
                PatchURLBox.Text = AppConfig.Current.PatchUrl ?? string.Empty;

            if (EnableAutoPatcherCheckBox != null)
                EnableAutoPatcherCheckBox.Checked = AppConfig.Current.Enabled;

            if (KeepAutoPatcherBackupsCheckbox != null)
                KeepAutoPatcherBackupsCheckbox.Checked = AppConfig.Current.KeepBackups;
        }
        #endregion

        #region Save Settings
        private void SaveSettings()
        {
            if (ServerIPBox != null)
                AccountServerAddressIP = ServerIPBox.Text.Trim();

            // Validate IP; if invalid, fall back to last good
            if (!IPAddress.TryParse(AccountServerAddressIP, out _))
                AccountServerAddressIP = AppConfig.Current.AccountServerAddressIP;

            // Parse and clamp port from TextBox
            ushort parsedPort = AppConfig.Current.AccountServerAddressPort;
            if (int.TryParse(ServerPortBox?.Text?.Trim(), out var temp))
            {
                if (temp < 1) temp = 1;
                if (temp > 65535) temp = 65535;
                parsedPort = (ushort)temp;
            }
            AccountServerAddressPort = parsedPort;

            AppConfig.Current.AccountServerAddressIP = AccountServerAddressIP;
            AppConfig.Current.AccountServerAddressPort = AccountServerAddressPort;

            string patchUrl = PatchURLBox?.Text?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(patchUrl))
            {
                // If the user typed a folder, ensure trailing slash so Uri joins are correct.
                // (If they typed a direct .json URL, leave as-is.)
                if (!patchUrl.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                    !patchUrl.EndsWith("/", StringComparison.Ordinal))
                {
                    patchUrl += "/";
                }

                if (Uri.TryCreate(patchUrl, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    AppConfig.Current.PatchUrl = patchUrl;
                }
                else
                {
                    // keep previous value if invalid
                    // (no UI error here; launcher label logger will surface issues on run)
                }
            }
            else
            {
                AppConfig.Current.PatchUrl = string.Empty;
            }

            // Enable / backups (simple boolean from CheckBox.Checked)
            if (EnableAutoPatcherCheckBox != null)
                AppConfig.Current.Enabled = EnableAutoPatcherCheckBox.Checked;

            if (KeepAutoPatcherBackupsCheckbox != null)
                AppConfig.Current.KeepBackups = KeepAutoPatcherBackupsCheckbox.Checked;

            // Persist
            AppConfig.Save();
        }
        #endregion

        #region Form Closing
        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e) => SaveSettings();
        #endregion

        #region Save Button
        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }
        #endregion

        #region Return to login button
        private void Return_To_Login_Button_Click(object sender, EventArgs e) => Close();
        #endregion

        #region Server Port (Make sure it's a number)
        private void ServerPortBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }
        #endregion

        #region Dragable Form
        private void Tab_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Capture the offset between the mouse cursor and the form's location
                offset = new Point(e.X, e.Y);
            }
        }

        private void Tab_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the new location of the form based on the offset
                Point newLocation = this.PointToScreen(new Point(e.X, e.Y));
                newLocation.Offset(-offset.X, -offset.Y);

                // Set the new location of the form
                this.Location = newLocation;
            }
        }

        private void Tab_MouseUp(object sender, MouseEventArgs e)
        {
            // Reset the offset when the mouse button is released
            offset = Point.Empty;
        }
        #endregion
    }
}