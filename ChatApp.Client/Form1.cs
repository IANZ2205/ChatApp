using System;
using System.Windows.Forms;
using ChatApp.Client.Network;
using ChatApp.Common.Protocol;

namespace ChatApp.Client
{
    public partial class Form1 : Form
    {
        private readonly ServerConnection _connection = new ServerConnection();
        private bool _handedOff = false;

        public Form1()
        {
            InitializeComponent();
            Theme.Apply(this);

            _connection.Log += OnLog;
            _connection.Disconnected += OnDisconnected;
            _connection.MessageReceived += OnMessageReceived;
        }


        private void OnLog(string message)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(OnLog), message); return; }
            lstLog.Items.Add($"{DateTime.Now:HH:mm:ss}  {message}");
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private void OnDisconnected()
        {
            if (InvokeRequired) { BeginInvoke(new Action(OnDisconnected)); return; }
            lblStatus.Text = "Status: Disconnected";
            lblStatus.ForeColor = System.Drawing.Color.Red;
            btnConnect.Enabled = true;
            btnConnect.Text = "Connect";
            btnLogin.Enabled = false;
            btnRegister.Enabled = false;
        }

        private void OnMessageReceived(Envelope envelope)
        {
            if (InvokeRequired) { BeginInvoke(new Action<Envelope>(OnMessageReceived), envelope); return; }

            if (envelope.Type == MessageTypes.RegisterResult)
            {
                var r = envelope.ReadData<AuthResult>();
                if (r == null) return;
                MessageBox.Show(r.Success
                    ? $"Account created: {r.Username}. You can log in now."
                    : $"Registration failed: {r.Message}");
            }
            else if (envelope.Type == MessageTypes.LoginResult)
            {
                if (_handedOff) return; // already opened MainForm; ignore further logins
                var r = envelope.ReadData<AuthResult>();
                if (r == null) return;
                if (r.Success)
                {
                    _handedOff = true;
                    // Do NOT unsubscribe, do NOT dispose. Just open MainForm and hide.
                    var main = new MainForm(_connection, r.UserId, r.Username);
                    main.FormClosed += (s, e) => { try { Close(); } catch { } };
                    main.Show();
                    Hide();
                }
                else
                {
                    MessageBox.Show($"Login failed: {r.Message}");
                }
            }
            else if (envelope.Type == MessageTypes.Error)
            {
                var err = envelope.ReadData<ErrorPayload>();
                if (err != null) MessageBox.Show($"Error: {err.Message}");
            }
        }
        private void HandleMainClosed(MainForm main)
        {
            if (main.LoggedOut)
            {
                // User logged out deliberately — return to login form
                _handedOff = false;
                try { _connection.Disconnect(); } catch { }

                Show();
                lblStatus.Text = "Status: Disconnected";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                btnConnect.Text = "Connect";
                btnConnect.Enabled = true;
                btnLogin.Enabled = false;
                btnRegister.Enabled = false;

                txtUsername.Clear();
                txtPassword.Clear();
                txtFullName.Clear();
            }
            else
            {
                Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_connection.IsConnected) { _connection.Disconnect(); return; }

            if (!int.TryParse(txtPort.Text.Trim(), out int port))
            { MessageBox.Show("Invalid port."); return; }

            lblStatus.Text = "Status: Connecting...";
            lblStatus.ForeColor = System.Drawing.Color.Orange;
            btnConnect.Enabled = false;

            System.Threading.Tasks.Task.Run(() =>
            {
                _connection.Connect(txtIP.Text.Trim(), port);
                Invoke(new Action(() =>
                {
                    if (_connection.IsConnected)
                    {
                        lblStatus.Text = "Status: Connected";
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                        btnConnect.Enabled = true;
                        btnConnect.Text = "Disconnect";
                        btnLogin.Enabled = true;
                        btnRegister.Enabled = true;
                    }
                    else
                    {
                        lblStatus.Text = "Status: Disconnected";
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        btnConnect.Enabled = true;
                        btnConnect.Text = "Connect";
                    }
                }));
            });
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (!_connection.IsConnected) { MessageBox.Show("Connect first."); return; }
            string u = txtUsername.Text.Trim();
            string p = txtPassword.Text;
            if (u.Length < 3) { MessageBox.Show("Username must be ≥3 characters."); return; }
            if (p.Length < 8) { MessageBox.Show("Password must be ≥8 characters."); return; }

            _connection.Send(Envelope.Create(MessageTypes.Register,
                new RegisterRequest { Username = u, Password = p, FullName = txtFullName.Text.Trim() }));
            OnLog($"Sent REGISTER for {u}");
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!_connection.IsConnected) { MessageBox.Show("Connect first."); return; }
            _connection.Send(Envelope.Create(MessageTypes.Login,
                new LoginRequest { Username = txtUsername.Text.Trim(), Password = txtPassword.Text }));
            OnLog("Sent LOGIN");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _connection.Disconnect();
            base.OnFormClosing(e);
        }
    }
}