using GamePackets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StormLibSharp;
using Mir3DCrypto;

namespace GameLogin
{

    #region Patcher Stuff
    internal sealed class PListFileEntry { public string path { get; set; } = ""; public long size { get; set; } }
    internal sealed class PListPakEntry
    {
        public string pak { get; set; } = "";
        public string pakPath { get; set; } = "";
        public long pakSize { get; set; }
        public int fileCount { get; set; }
        public long totalSize { get; set; }
        public List<PListFileEntry> files { get; set; } = new();
    }
    internal sealed class PListRoot
    {
        public string root { get; set; } = "MMOGame";
        public int pakCount { get; set; }
        public List<PListPakEntry> paks { get; set; } = new();
    }

    internal static class PatcherConfig
    {
        public static string RemotePatchBaseUrl = "https://example.com/Patch"; // TODO set real URL
        public static string Root => AppDomain.CurrentDomain.BaseDirectory;
        public static string LocalPlistPath => Path.Combine(Root, "MMOGame", "Patch", "PList.json");
        public static string TempDir => Path.Combine(Root, "MMOGame", "Patch", "temp_downloads");
    }

    internal static class PathUtil
    {
        public static string N(string p) => (p ?? "").Replace('\\', '/').Trim();
        public static string Disk(string p) => (p ?? "").Replace('/', '\\');
        public static string UrlEscape(string p)
        {
            if (string.IsNullOrEmpty(p)) return string.Empty;
            var segments = p.Replace('\\', '/').Split('/');
            for (int i = 0; i < segments.Length; i++)
                segments[i] = Uri.EscapeDataString(segments[i]);
            return string.Join("/", segments);
        }
    }

    internal static class HttpX
    {
        public static readonly HttpClient Client = new HttpClient();
        public static async Task<string> GetStringAsync(string url)
        {
            using var r = await Client.GetAsync(url);
            r.EnsureSuccessStatusCode();
            return await r.Content.ReadAsStringAsync();
        }
        public static async Task DownloadToFileAsync(string url, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using var r = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            r.EnsureSuccessStatusCode();
            await using var s = await r.Content.ReadAsStreamAsync();
            await using var f = File.Create(filePath);
            await s.CopyToAsync(f);
        }
    }

    internal static class PakEditor
    {
        public static bool ReplaceFileInPak(string pakDiskPath, string internalPath, string sourceFileOnDisk)
        {
            string mpqPath = PathUtil.Disk(internalPath).TrimStart('\\');
            try
            {
                byte[] data = File.ReadAllBytes(sourceFileOnDisk);
                data = Crypto.Encrypt(data); // encrypt before write (matches your editor)

                using var archive = new MpqArchive(pakDiskPath, FileAccess.ReadWrite);

                uint flags = 0;
                try { using var existing = archive.OpenFile(mpqPath); flags = existing.GetFlags(); } catch { /* not present */ }

                archive.FileCreateFile(mpqPath, flags, data);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReplaceFileInPak error: {ex.Message}");
                return false;
            }
        }
    }
    #endregion
    public partial class MainForm : Form
    {
        private Point offset; // Used to store the offset between the mouse cursor and the form's location

        public sealed class GameServerInfo
        {
            public string ServerName { get; set; }
            public IPEndPoint PublicAddress { get; set; }
        }

        public static string LoginAccount, RegisterAccount, ModifyAccount;
        public static string LoginPassword, RegisterPassword, ModifyPassword;
        public static bool LoggedIn;
        public static Process GameProcess;
        public static MainForm Instance;
        public static Dictionary<string, GameServerInfo> ServerTable = new Dictionary<string, GameServerInfo>();

        public static string ClientPath32, ClientPath64;

        public bool Is64Bit => uiCheckBox2.Checked;
        public bool Is32Bit => uiCheckBox1.Checked;

        // Cached account-server endpoint from Mir3dConfig.json
        private IPEndPoint _accountEndpoint;

        public MainForm()
        {
            InitializeComponent();

            Instance = this;

            ClientPath32 = "Binaries\\Win32\\MMOGame-Win32-Shipping.exe";
            ClientPath64 = "Binaries\\Win64\\MMOGame-Win64-Shipping.exe";

            PreLaunchChecks();

            // Load JSON into memory before touching UI/network
            AppConfig.Load();

            // Cache endpoint from Mir3dConfig (if valid)
            if (IPAddress.TryParse(AppConfig.Current.AccountServerAddressIP, out var ip))
                _accountEndpoint = new IPEndPoint(ip, AppConfig.Current.AccountServerAddressPort);

            // OS bitness -> checkbox state
            if (Environment.Is64BitOperatingSystem)
            {
                uiCheckBox1.Enabled = true; uiCheckBox1.Checked = false;
                uiCheckBox2.Enabled = true; uiCheckBox2.Checked = true;
            }
            else
            {
                uiCheckBox2.Enabled = false; uiCheckBox2.Checked = false;
                uiCheckBox1.Enabled = false; uiCheckBox1.Checked = true;
            }

            // Populate UI from config and (re)connect if needed
            ReloadFromConfig();
        }

        /// <summary>
        /// Pulls fresh values from config.json, updates UI, and (re)connects if endpoint changed.
        /// </summary>
        private void ReloadFromConfig()
        {
            AppConfig.Load();

            // Reflect config into UI
            AccountTextBox.Text = AppConfig.Current.AccountName ?? string.Empty;
            start_selected_zone.Text = AppConfig.Current.ServerName ?? string.Empty;

            ConnectionStatusLabel.Text = "Attempting to connect to the server.";

            // Build endpoint and decide whether to reconnect
            if (IPAddress.TryParse(AppConfig.Current.AccountServerAddressIP, out var ip))
            {
                var newEp = new IPEndPoint(ip, AppConfig.Current.AccountServerAddressPort);
                bool changed = _accountEndpoint == null || !_accountEndpoint.Equals(newEp);
                _accountEndpoint = newEp;

                // If your Network allows explicit endpoint, prefer that:
                // Network.Instance.Connect(AppConfig.Current.AccountServerAddressIP, AppConfig.Current.AccountServerAddressPort);

                if (changed)
                {
                    Network.Instance.Connect();
                }
                else if (!Network.Instance.Connected)
                {
                    Network.Instance.Connect();
                }
            }
            else
            {
                ConnectionStatusLabel.Text = "Invalid AccountServerAddressIP in config.json";
            }
        }

        private void PreLaunchChecks()
        {
            var clientFound32Bit = File.Exists(ClientPath32);
            var clientFound64Bit = File.Exists(ClientPath64);

            if (!clientFound32Bit && !clientFound64Bit)
            {
                MessageBox.Show("Client Cannot Be Found!\r\nPlease Read The README.txt");
                Environment.Exit(0);
            }
        }

        public void UILock()
        {
            MainTab.Enabled = false;
            LoginErrorLabel.Visible = false;
            RegistrationErrorLabel.Visible = false;
            Modify_ErrorLabel.Visible = false;
        }

        public void PacketProcess(object sender, EventArgs e)
        {
            Network.Instance.Process();

            if (Network.Instance.Connected)
                ConnectionStatusLabel.Text = "Connected";
            else
                ConnectionStatusLabel.Text = $"Attempting to connect to the server. Attempt: {Network.Instance.ConnectAttempt}";
        }

        public void AccountRegisterSuccessUpdate()
        {
            BeginInvoke(() =>
            {
                AccountTextBox.Text = RegisterAccount;
                AccountPasswordTextBox.Text = RegisterPassword;
                UIUnlock(null, null);
                MainTab.SelectedIndex = 0;
                MessageBox.Show("Account Created Successfully");
            });
        }

        public void AccountRegisterFailUpdate(int errorCode)
        {
            RegisterAccount = string.Empty;
            RegisterPassword = string.Empty;

            BeginInvoke(() =>
            {
                UIUnlock(null, null);

                var message = string.Empty;
                switch (errorCode)
                {
                    case 0: message = "The username length is incorrect"; break;
                    case 1: message = "The password length is incorrect"; break;
                    case 2: message = "The security question length is incorrect"; break;
                    case 3: message = "The secret answer length is incorrect"; break;
                    case 4: message = "If the promotion code is wrong, you can register directly without filling it in"; break;
                    case 5: message = "The username is formatted incorrectly"; break;
                    case 6: message = "The username is formatted incorrectly"; break;
                    case 7: message = "The username already exists"; break;
                }

                RegistrationErrorLabel.Text = message;
                RegistrationErrorLabel.Visible = true;
                RegistrationErrorLabel.ForeColor = Color.Red;
            });
        }

        public void AccountChangePasswordSuccessUpdate()
        {
            BeginInvoke(() =>
            {
                AccountTextBox.Text = ModifyAccount;
                AccountPasswordTextBox.Text = ModifyPassword;
                UIUnlock(null, null);
                MainTab.SelectedIndex = 0;
                MessageBox.Show("Password Change Completed");
            });
        }

        public void AccountChangePasswordFailUpdate(int errorCode)
        {
            ModifyAccount = string.Empty;
            ModifyPassword = string.Empty;

            BeginInvoke(() =>
            {
                UIUnlock(null, null);

                var message = string.Empty;
                switch (errorCode)
                {
                    case 0: message = "The password length is incorrect"; break;
                    case 1: message = "The account does not exist"; break;
                    case 2: message = "Password issue is incorrect"; break;
                    case 3: message = "The secret answer is incorrect"; break;
                }

                Modify_ErrorLabel.Text = message;
                Modify_ErrorLabel.Visible = true;
            });
        }

        public void AccountLogInSuccessUpdate(string data)
        {
            LoggedIn = true;

            BeginInvoke(() =>
            {
                UIUnlock(null, null);

                ServerTable.Clear();
                GameServerList.Items.Clear();

                var lines = data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var arr = line.Split(new char[2] { ':', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length != 3)
                    {
                        MessageBox.Show("Server Data Parsing Failed");
                        Environment.Exit(0);
                    }

                    var ipStr = arr[0];
                    var name = arr[2];

                    if (!int.TryParse(arr[1], out var port) || string.IsNullOrEmpty(ipStr) || string.IsNullOrEmpty(name))
                    {
                        MessageBox.Show("Server configuration error, parsing failed. Row: " + line);
                        Environment.Exit(0);
                    }

                    ServerTable.Add(name, new GameServerInfo
                    {
                        PublicAddress = new IPEndPoint(IPAddress.Parse(ipStr), port),
                        ServerName = name
                    });

                    GameServerList.Items.Add(name);
                }

                MainTab.SelectedIndex = 3;

                // Persist the login account name
                AppConfig.Current.AccountName = LoginAccount;
                AppConfig.Save();
            });
        }

        public void AccountLogInFailUpdate(int errorCode)
        {
            LoggedIn = false;
            LoginAccount = string.Empty;
            LoginPassword = string.Empty;

            BeginInvoke(() =>
            {
                UIUnlock(null, null);

                var message = string.Empty;
                switch (errorCode)
                {
                    case 0: message = "Wrong username or password"; break;
                }

                LoginErrorLabel.Text = message;
                LoginErrorLabel.ForeColor = Color.Red;
                LoginErrorLabel.Visible = true;
            });
        }

        public void AccountLogOutSuccessUpdate()
        {
            LoggedIn = false;
            LoginAccount = string.Empty;
            LoginPassword = string.Empty;

            BeginInvoke(() =>
            {
                UIUnlock(null, null);
                LoginErrorLabel.Text = "Successfully logged out";
                LoginErrorLabel.ForeColor = Color.Red;
                LoginErrorLabel.Visible = true;

                MainTab.SelectedIndex = 0;
            });
        }

        public void AccountStartGameSuccessUpdate(string ticket)
        {
            BeginInvoke(() =>
            {
                if (ServerTable.TryGetValue(start_selected_zone.Text, out var value))
                {
                    string arguments =
                        "-wegame=" + $"1,1,{value.PublicAddress.Address},{value.PublicAddress.Port}," +
                        $"1,1,{value.PublicAddress.Address},{value.PublicAddress.Port}," + start_selected_zone.Text + "  " +
                        $"/ip:1,1,{value.PublicAddress.Address} " +
                        $"/port:{value.PublicAddress.Port} " +
                        "/ticket:" + ticket +
                        " /AreaName:" + start_selected_zone.Text;

                    AppConfig.Current.ServerName = start_selected_zone.Text;
                    AppConfig.Save();

                    var psi = new ProcessStartInfo
                    {
                        Arguments = arguments,
                        UseShellExecute = true,
                        Verb = "runas"
                    };

                    if (Is32Bit && Is64Bit || !Is32Bit && !Is64Bit)
                    {
                        MessageBox.Show("Error Getting OS Version");
                        Environment.Exit(0);
                    }
                    else if (Is32Bit)
                        psi.FileName = ClientPath32;
                    else if (Is64Bit)
                        psi.FileName = ClientPath64;

                    GameProcess?.Dispose();
                    GameProcess = Process.Start(psi);

                    if (GameProcess != null)
                    {
                        GameProcessTimer.Enabled = true;
                        MainForm_FormClosing(null, null);
                        UILock();
                        InterfaceUpdateTimer.Enabled = false;
                        minimizeToTray.ShowBalloonTip(1000, "", "Game Loading Please Wait. . .", ToolTipIcon.Info);
                    }
                }
            });
        }

        public void AccountStartGameFailUpdate(int errorCode)
        {
            BeginInvoke(() =>
            {
                UIUnlock(null, null);

                var message = string.Empty;
                switch (errorCode)
                {
                    case 0: message = "No server found"; break;
                    case 1: message = "Not logged in yet!"; break;
                }

                MessageBox.Show("Failed To Start The Game\r\n" + message);
            });
        }

        public void UIUnlock(object sender, EventArgs e)
        {
            BeginInvoke(() =>
            {
                MainTab.Enabled = true;
                InterfaceUpdateTimer.Enabled = false;
            });
        }

        public void GameProgressCheck(object sender, EventArgs e)
        {
            if (GameProcess == null || !MainForm.GameProcess.HasExited)
                return;

            if (LoggedIn)
            {
                LoggedIn = false;
                Network.Instance.SendPacket(new AccountLogOutPacket { });
            }

            UIUnlock(null, null);
            TrayRestoreFromTaskBar(null, null);
            GameProcessTimer.Enabled = false;
        }

        private void LoginAccountLabel_Click(object sender, EventArgs e)
        {
            if (!Network.Instance.Connected)
            {
                LoginErrorLabel.Text = "Not connected to server..";
                LoginErrorLabel.Visible = true;
                return;
            }

            if (AccountTextBox.Text.Length <= 0)
            {
                LoginErrorLabel.Text = "Account Name Cannot Be Empty";
                LoginErrorLabel.Visible = true;
            }
            else if (AccountTextBox.Text.IndexOf(' ') >= 0)
            {
                LoginErrorLabel.Text = "Account Name Cannot Contain Spaces";
                LoginErrorLabel.Visible = true;
            }
            else if (AccountPasswordTextBox.Text.Length <= 0)
            {
                LoginErrorLabel.Text = "Password Cannot Be Blank";
                LoginErrorLabel.Visible = true;
            }
            else if (AccountTextBox.Text.IndexOf(' ') >= 0)
            {
                LoginErrorLabel.Text = "Password Cannot Contain Spaces";
                LoginErrorLabel.Visible = true;
            }
            else
            {
                LoginAccount = AccountTextBox.Text;
                LoginPassword = AccountPasswordTextBox.Text;

                Network.Instance.SendPacket(new AccountLogInPacket
                {
                    LoginAccount = LoginAccount,
                    LoginPassword = LoginPassword,
                });
                UILock();

                AccountPasswordTextBox.Text = "";
                InterfaceUpdateTimer.Enabled = true;
            }
        }

        private void Login_ForgotPassword_Click(object sender, EventArgs e) => MainTab.SelectedIndex = 2;
        private void Login_Registertab_Click(object sender, EventArgs e) => MainTab.SelectedIndex = 1;

        #region MainForm Close
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            minimizeToTray.Visible = true;
            Hide();
            if (e == null)
                return;
            e.Cancel = true;
        }
        #endregion

        #region Minimize to Tray
        private void TrayRestoreFromTaskBar(object sender, MouseEventArgs e)
        {
            if (e != null && e.Button != MouseButtons.Left)
                return;
            Visible = true;
            minimizeToTray.Visible = false;
        }

        private void Tray_Restore(object sender, EventArgs e)
        {
            Visible = true;
            minimizeToTray.Visible = false;
        }

        private void TrayCloseLauncher(object sender, EventArgs e)
        {
            minimizeToTray.Visible = false;

            if (LoggedIn)
            {
                LoggedIn = false;
                Network.Instance.SendPacket(new AccountLogOutPacket { });
            }

            Environment.Exit(Environment.ExitCode);
        }
        #endregion

        private void RegisterAccount_Click(object sender, EventArgs e)
        {
            if (!Network.Instance.Connected)
            {
                RegistrationErrorLabel.Text = "Not connected to server..";
                RegistrationErrorLabel.Visible = true;
                return;
            }

            if (Register_AccountNameTextBox.Text.Length <= 0)
            {
                RegistrationErrorLabel.Text = "Account name cannot be empty";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_AccountNameTextBox.Text.IndexOf(' ') >= 0)
            {
                RegistrationErrorLabel.Text = "Account name cannot contain spaces";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_AccountNameTextBox.Text.Length <= 5 || Register_AccountNameTextBox.Text.Length > 12)
            {
                RegistrationErrorLabel.Text = "Account name must be 6 to 12 characters long";
                RegistrationErrorLabel.Visible = true;
            }
            else if (!Regex.IsMatch(Register_AccountNameTextBox.Text, "^[a-zA-Z]+.*$"))
            {
                RegistrationErrorLabel.Text = "Account name must start with a letter";
                RegistrationErrorLabel.Visible = true;
            }
            else if (!Regex.IsMatch(Register_AccountNameTextBox.Text, "^[a-zA-Z_][A-Za-z0-9_]*$"))
            {
                RegistrationErrorLabel.Text = "Account name can only contain alphanumeric and underscores";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_PasswordTextBox.Text.Length <= 0)
            {
                RegistrationErrorLabel.Text = "Password cannot be empty";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_PasswordTextBox.Text.IndexOf(' ') >= 0)
            {
                RegistrationErrorLabel.Text = "Password cannot contain spaces";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_PasswordTextBox.Text.Length <= 5 || Register_PasswordTextBox.Text.Length > 18)
            {
                RegistrationErrorLabel.Text = "Password must be 6 to 18 characters long";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_QuestionTextBox.Text.Length <= 0)
            {
                RegistrationErrorLabel.Text = "Security question cannot be empty";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_QuestionTextBox.Text.IndexOf(' ') >= 0)
            {
                RegistrationErrorLabel.Text = "Security question cannot contain spaces";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_QuestionTextBox.Text.Length <= 1 || Register_QuestionTextBox.Text.Length > 18)
            {
                RegistrationErrorLabel.Text = "Security question must me 2 to 18 characters long";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_SecretAnswerTextBox.Text.Length <= 0)
            {
                RegistrationErrorLabel.Text = "Security answer cannot be empty";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_SecretAnswerTextBox.Text.IndexOf(' ') >= 0)
            {
                RegistrationErrorLabel.Text = "Security answer cannot contain spaces";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_SecretAnswerTextBox.Text.Length <= 1 || Register_SecretAnswerTextBox.Text.Length > 18)
            {
                RegistrationErrorLabel.Text = "Security answer must be 2 to 18 characters long";
                RegistrationErrorLabel.Visible = true;
            }
            else if (Register_ReferralCodeTextBox.Text.Length > 0 && Register_ReferralCodeTextBox.Text.Length != 4)
            {
                RegistrationErrorLabel.Text = "Referral code must me 4 characters long";
                RegistrationErrorLabel.Visible = true;
            }
            else
            {
                RegisterAccount = Register_AccountNameTextBox.Text;
                RegisterPassword = Register_PasswordTextBox.Text;

                Network.Instance.SendPacket(new AccountRegisterPacket
                {
                    RegisterAccountName = Register_AccountNameTextBox.Text,
                    RegisterPassword = Register_PasswordTextBox.Text,
                    RegisterQuestion = Register_QuestionTextBox.Text,
                    RegisterSecretAnswer = Register_SecretAnswerTextBox.Text,
                    RegisterReferralCode = Register_ReferralCodeTextBox.Text
                });

                UILock();
                Register_AccountNameTextBox.Text = string.Empty;
                Register_PasswordTextBox.Text = string.Empty;
                Register_QuestionTextBox.Text = string.Empty;
                Register_SecretAnswerTextBox.Text = string.Empty;
                Register_ReferralCodeTextBox.Text = string.Empty;
                InterfaceUpdateTimer.Enabled = true;
            }
        }

        private void RegisterBackToLogin_Click(object sender, EventArgs e) => MainTab.SelectedIndex = 0;

        private void Modify_ChangePassword_Click(object sender, EventArgs e)
        {
            if (!Network.Instance.Connected)
            {
                Modify_ErrorLabel.Text = "Not connected to server..";
                Modify_ErrorLabel.Visible = true;
                return;
            }

            if (Modify_AccountNameTextBox.Text.Length <= 0)
            {
                Modify_ErrorLabel.Text = "Account Name Cannot Be Empty";
                Modify_ErrorLabel.Visible = true;
            }
            else if (Modify_AccountNameTextBox.Text.IndexOf(' ') >= 0)
            {
                Modify_ErrorLabel.Text = "Account Name Cannot Contain Spaces";
                Modify_ErrorLabel.Visible = true;
            }
            else if (Modify_PasswordTextBox.Text.Length <= 0)
            {
                Modify_ErrorLabel.Text = "Password Cannot Be Empty";
                Modify_ErrorLabel.Visible = true;
            }
            else if (Modify_PasswordTextBox.Text.IndexOf(' ') >= 0)
            {
                Modify_ErrorLabel.Text = "Password Cannot Contain Spaces";
                Modify_ErrorLabel.Visible = true;
            }
            else if (Modify_PasswordTextBox.Text.Length <= 5 || Modify_PasswordTextBox.Text.Length > 18)
            {
                Modify_ErrorLabel.Text = "Password Must Be 6 to 18 Characters Long";
                Modify_ErrorLabel.Visible = true;
            }
            else if (Modify_QuestionTextBox.Text.Length <= 0)
            {
                Modify_ErrorLabel.Text = "Security Question Cannot Be Empty";
                Modify_ErrorLabel.Visible = true;
            }
            else if (Modify_QuestionTextBox.Text.IndexOf(' ') >= 0)
            {
                Modify_ErrorLabel.Text = "Security Question Cannot Contain Spaces";
                Modify_ErrorLabel.Visible = true;
            }
            else if (Modify_AnswerTextBox.Text.Length <= 0)
            {
                Modify_ErrorLabel.Text = "Security Answer Cannot Be Empty";
                Modify_ErrorLabel.Visible = true;
            }
            else if (Modify_AnswerTextBox.Text.IndexOf(' ') >= 0)
            {
                Modify_ErrorLabel.Text = "Security Answer Cannot Contain Spaces";
                Modify_ErrorLabel.Visible = true;
            }
            else
            {
                ModifyAccount = Modify_AccountNameTextBox.Text;
                ModifyPassword = Modify_PasswordTextBox.Text;

                Network.Instance.SendPacket(new AccountChangePasswordPacket
                {
                    AccountName = Modify_AccountNameTextBox.Text,
                    AccountPassword = Modify_PasswordTextBox.Text,
                    AccountQuestion = Modify_QuestionTextBox.Text,
                    AccountSecretAnswer = Modify_AnswerTextBox.Text
                });
                UILock();
                Modify_PasswordTextBox.Text = Modify_AnswerTextBox.Text = "";
                InterfaceUpdateTimer.Enabled = true;
            }
        }

        private void Modify_BackToLogin_Click(object sender, EventArgs e) => MainTab.SelectedIndex = 0;

        private void Launch_EnterGame_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(LoginAccount) || !LoggedIn)
                MainTab.SelectedIndex = 0;
            else if (string.IsNullOrEmpty(start_selected_zone.Text))
            {
                MessageBox.Show("You must select a server");
            }
            else if (!ServerTable.ContainsKey(start_selected_zone.Text))
            {
                MessageBox.Show("Server selection error");
            }
            else
            {
                Network.Instance.SendPacket(new AccountStartGamePacket
                {
                    LoginAccount = LoginAccount,
                    ServerName = start_selected_zone.Text
                });
                UILock();
                InterfaceUpdateTimer.Enabled = true;
            }
        }

        private void LogoutTab_Click(object sender, EventArgs e)
        {
            LoginAccount = null;
            LoginPassword = null;
            MainTab.SelectedIndex = 0;
        }

        private void StartupChoosegameServer_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1) return;

            e.DrawBackground();
            e.DrawFocusRectangle();
            StringFormat format = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString(this.GameServerList.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), (RectangleF)e.Bounds, format);
        }

        private void StartupChooseGS_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GameServerList.SelectedIndex < 0)
                start_selected_zone.Text = "";
            else
                start_selected_zone.Text = GameServerList.Items[GameServerList.SelectedIndex].ToString();
        }

        private void Login_PasswordKeyPress(object sender, KeyPressEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AccountPasswordTextBox.Text)) return;
            if (e.KeyChar != (char)13) return;
            LoginAccountLabel_Click(sender, null);
        }

        #region Close Button
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion

        private void pictureBox3_MouseClick(object sender, MouseEventArgs e)
        {
            Environment.Exit(0);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void uiCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            uiCheckBox2.Checked = !uiCheckBox1.Checked;
        }

        private void uiCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            uiCheckBox1.Checked = !uiCheckBox2.Checked;
        }

        #region Config Button
        private void ConfigButton_Click(object sender, EventArgs e)
        {
            using (var configForm = new ConfigForm())
            {
                configForm.ShowDialog(this); // Config form saves on close
            }
            ReloadFromConfig(); // Pull fresh JSON -> update UI -> reconnect if endpoint changed
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

        #region ToolTip for Config Button
        private readonly ToolTip tip = new ToolTip
        {
        };

        private void ConfigButton_MouseHover(object sender, EventArgs e)
        {
            var p = ConfigButton.PointToClient(Cursor.Position);
            tip.Show("Open Config", ConfigButton, p.X + 12, p.Y + 12, 2500);
        }
        #endregion

        private async void PatchClientButton_Click(object sender, EventArgs e)
        {
            static bool TryGetPatchBaseUri(out Uri baseUri)
            {
                var raw = AppConfig.Current?.PatchUrl?.Trim();
                if (!string.IsNullOrEmpty(raw) &&
                    Uri.TryCreate(raw, UriKind.Absolute, out baseUri) &&
                    (baseUri.Scheme == Uri.UriSchemeHttp || baseUri.Scheme == Uri.UriSchemeHttps))
                    return true;

                baseUri = null!;
                return false;
            }

            static string Norm(string p) => (p ?? "").Replace('\\', '/').Trim();
            static string Disk(string p) => (p ?? "").Replace('/', '\\');

            try
            {
                if (!TryGetPatchBaseUri(out var baseUri))
                {
                    MessageBox.Show(
                        "Patch URL is not set or invalid in Mir3dConfig.json.\r\n" +
                        "Set \"PatchUrl\": \"https://host/folder\" and try again.",
                        "Patch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string patchDir = Path.Combine(baseDir, "MMOGame", "Patch");
                string tempDir = Path.Combine(patchDir, "temp_downloads");
                string localPlistPath = Path.Combine(patchDir, "PList.json");

                // --- Clean temp_downloads at start ---
                try
                {
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
                }
                catch { /* ignore */ }
                Directory.CreateDirectory(tempDir);

                // --- Load local PList ---
                if (!File.Exists(localPlistPath))
                {
                    MessageBox.Show($"Local PList.json not found:\r\n{localPlistPath}", "Patch",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var localJson = await File.ReadAllTextAsync(localPlistPath);
                var local = System.Text.Json.JsonSerializer.Deserialize<PListRoot>(localJson) ?? new PListRoot();

                // --- Fetch remote PList ---
                using var http = new System.Net.Http.HttpClient();
                var plistUri = new Uri(baseUri, "PList.json");
                using var plistResp = await http.GetAsync(plistUri, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                plistResp.EnsureSuccessStatusCode();
                var remoteJson = await plistResp.Content.ReadAsStringAsync();
                var remote = System.Text.Json.JsonSerializer.Deserialize<PListRoot>(remoteJson) ?? new PListRoot();

                // --- Build diff (path + size) ---
                var localPaks = local.paks.ToDictionary(p => Norm(p.pakPath), StringComparer.OrdinalIgnoreCase);
                var toDownload = new List<(PListPakEntry pak, PListFileEntry file)>();

                foreach (var rpak in remote.paks)
                {
                    var rKey = Norm(rpak.pakPath);
                    if (!localPaks.TryGetValue(rKey, out var lpak))
                    {
                        foreach (var f in rpak.files) toDownload.Add((rpak, f));
                        continue;
                    }

                    var lFiles = lpak.files.ToDictionary(f => Norm(f.path), StringComparer.OrdinalIgnoreCase);
                    foreach (var rf in rpak.files)
                    {
                        var key = Norm(rf.path);
                        if (!lFiles.TryGetValue(key, out var lf) || lf.size != rf.size)
                            toDownload.Add((rpak, rf));
                    }
                }

                if (toDownload.Count == 0)
                {
                    // Still make sure we refresh the local PList to the remote one for future runs.
                    try { await File.WriteAllTextAsync(localPlistPath, remoteJson); } catch { /* ignore */ }
                    MessageBox.Show("Client is already up to date.", "Patch",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show(
                        $"Found {toDownload.Count} updated files.\r\nDownload and patch now?",
                        "Patch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                int ok = 0, fail = 0;
                var touchedPaks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var (pak, file) in toDownload)
                {
                    // Build remote file URL: PatchUrl/<pakPath>/<internal-path>
                    // Use Uri to combine safely.
                    var pakFolderUri = new Uri(baseUri, Norm(pak.pakPath) + "/");
                    var fileUri = new Uri(pakFolderUri, Norm(file.path));

                    string tmp = Path.Combine(tempDir, $"{Guid.NewGuid():N}_{Path.GetFileName(file.path)}");

                    try
                    {
                        using (var r = await http.GetAsync(fileUri, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                        {
                            r.EnsureSuccessStatusCode();
                            await using var inS = await r.Content.ReadAsStreamAsync();
                            await using var outF = File.Create(tmp);
                            await inS.CopyToAsync(outF);
                        }

                        string pakOnDisk = Path.Combine(baseDir, Disk(pak.pakPath));
                        if (!File.Exists(pakOnDisk))
                        {
                            fail++;
                            continue;
                        }

                        // Backup each pak once
                        if (!touchedPaks.Contains(pakOnDisk))
                        {
                            touchedPaks.Add(pakOnDisk);
                            var bak = pakOnDisk + ".bak";
                            try { if (!File.Exists(bak)) File.Copy(pakOnDisk, bak, overwrite: false); } catch { }
                        }

                        // Replace inside MPQ (encrypt-on-write; preserve flags)
                        try
                        {
                            byte[] data = await File.ReadAllBytesAsync(tmp);
                            data = Mir3DCrypto.Crypto.Encrypt(data);

                            uint flags = 0;
                            using (var archive = new StormLibSharp.MpqArchive(pakOnDisk, FileAccess.ReadWrite))
                            {
                                try { using var existing = archive.OpenFile(Disk(file.path)); flags = existing.GetFlags(); }
                                catch { /* not present */ }

                                archive.FileCreateFile(Disk(file.path), flags, data);
                            }
                            ok++;
                        }
                        catch { fail++; }
                    }
                    catch { fail++; }
                    finally
                    {
                        try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
                    }
                }

                // --- Overwrite local PList.json with the remote one so future diffs match ---
                try { await File.WriteAllTextAsync(localPlistPath, remoteJson); } catch { /* ignore */ }

                // --- Clean temp_downloads at the end as well ---
                try
                {
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
                }
                catch { /* ignore */ }

                MessageBox.Show($"Patch done.\r\nSuccess: {ok}\r\nFailed: {fail}", "Patch",
                    MessageBoxButtons.OK, fail == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Patch failed:\r\n{ex.Message}", "Patch", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
