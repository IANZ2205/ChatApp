using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using ChatApp.Common;
using ChatApp.Common.Protocol;
using ChatApp.Server.Data;
using ChatApp.Server.Network;

namespace ChatApp.Server
{
    public partial class Form1 : Form
    {
        private readonly ChatServer _server;
        private readonly UserRepository _users = new UserRepository();
        private readonly List<UserInfoPayload> _onlineUsers = new();
        private readonly System.Windows.Forms.Timer _ticker = new System.Windows.Forms.Timer { Interval = 1000 };
        private int _lastUserCount = -1;

        public Form1()
        {
            InitializeComponent();

            _server = new ChatServer(OnServerLog);
            _server.StateChanged += OnServerStateChanged;

            Theme.Apply(this);
            ApplyDashboardColors();

            _ticker.Tick += Ticker_Tick;
            _ticker.Start();

            UpdateUi();
        }

        private void ApplyDashboardColors()
        {
            // Force dark colors on the tab pages (they default to light grey)
            tabDashboard.BackColor = Theme.WindowBg;
            tabUsers.BackColor = Theme.WindowBg;
            tabSettings.BackColor = Theme.WindowBg;
            tabControl.BackColor = Theme.WindowBg;
            tabControl.ForeColor = Theme.TextFg;

            // Style the grid
            dgvUsers.BackgroundColor = Theme.PanelBg;
            dgvUsers.DefaultCellStyle.BackColor = Theme.PanelBg;
            dgvUsers.DefaultCellStyle.ForeColor = Theme.TextFg;
            dgvUsers.DefaultCellStyle.SelectionBackColor = Theme.Accent;
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Theme.WindowBg;
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextFg;
            dgvUsers.GridColor = Color.FromArgb(70, 70, 70);
            dgvUsers.EnableHeadersVisualStyles = false;

            // Stop button red-ish
            btnStop.BackColor = Color.FromArgb(180, 60, 60);
            btnKick.BackColor = Color.FromArgb(180, 60, 60);
        }

        // ---------- Server event handlers ----------
        private void OnServerLog(string message)
        {
            if (InvokeRequired) { BeginInvoke(new Action<string>(OnServerLog), message); return; }
            lstLog.Items.Add($"{DateTime.Now:HH:mm:ss}  {message}");
            if (lstLog.Items.Count > 2000) lstLog.Items.RemoveAt(0);
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private void OnServerStateChanged()
        {
            if (InvokeRequired) { BeginInvoke(new Action(OnServerStateChanged)); return; }
            UpdateUi();
        }

        private void UpdateUi()
        {
            bool running = _server.IsRunning;
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            lblStatus.Text = running ? "● Status: Running" : "● Status: Stopped";
            lblStatus.ForeColor = running ? Theme.Success : Theme.Danger;
        }

        // ---------- Ticker (every second) ----------
        private void Ticker_Tick(object? sender, EventArgs e)
        {
            Stats.Sample();

            lblUptime.Text = $"Uptime: {Stats.UptimeText}";

            // Refresh stats text
            int online = _server.IsRunning ? _server.GetOnlineUsers().Count : 0;
            int registered = _users.GetUserCount();
            int messages = _users.GetMessageCount();
            int files = _users.GetFileCount();

            lblStats.Text =
                $"Online: {online}     Registered: {registered}     " +
                $"Messages: {messages}     Files: {files}\r\n" +
                $"CPU: {Stats.CpuPercent:F1}%     Memory: {Stats.MemoryMB:F1} MB     " +
                $"Sent: {FormatBytes(Stats.BytesSent)}     Received: {FormatBytes(Stats.BytesReceived)}";

            // Refresh online list
            RefreshOnlineUsers();

            pnlGraph.Invalidate();
        }

        private static string FormatBytes(long n)
        {
            if (n < 1024) return $"{n} B";
            if (n < 1024 * 1024) return $"{n / 1024.0:F1} KB";
            if (n < 1024L * 1024 * 1024) return $"{n / (1024.0 * 1024):F1} MB";
            return $"{n / (1024.0 * 1024 * 1024):F2} GB";
        }

        private void RefreshOnlineUsers()
        {
            var current = _server.IsRunning ? _server.GetOnlineUsers() : new List<UserInfoPayload>();

            if (current.Count != _lastUserCount ||
                !current.All(u => _onlineUsers.Any(o => o.UserId == u.UserId)))
            {
                _onlineUsers.Clear();
                _onlineUsers.AddRange(current);
                _lastUserCount = current.Count;

                lstUsers.Items.Clear();
                foreach (var u in _onlineUsers.OrderBy(u => u.Username))
                    lstUsers.Items.Add(new OnlineUserItem(u.UserId, u.Username, u.Address ?? "—"));
                lblOnlineTitle.Text = $"ONLINE USERS ({_onlineUsers.Count})";
            }
        }

        // ---------- Log colouring ----------
        private void lstLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();

            string line = lstLog.Items[e.Index].ToString() ?? "";
            Color color = Color.FromArgb(230, 230, 230);

            if (line.Contains("ERROR") || line.Contains("Exception") || line.Contains("error"))
                color = Color.FromArgb(240, 100, 100);
            else if (line.Contains("too long") || line.Contains("failed") || line.Contains("FAIL"))
                color = Color.FromArgb(240, 200, 80);
            else if (line.Contains("ONLINE") || line.Contains("LOGIN") || line.Contains("OK") ||
                     line.Contains("joined") || line.Contains("Server started"))
                color = Color.FromArgb(120, 220, 150);
            else if (line.Contains("MESSAGE") || line.Contains("FILE") || line.Contains("GROUP") ||
                     line.Contains("PRIVATE") || line.Contains("PROFILE"))
                color = Color.FromArgb(120, 180, 240);
            else if (line.Contains("Disconnected") || line.Contains("stopped"))
                color = Color.FromArgb(200, 160, 160);

            using var brush = new SolidBrush(color);
            e.Graphics.DrawString(line, e.Font ?? this.Font, brush, e.Bounds);
        }

        // ---------- Graph panel ----------
        private void pnlGraph_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(30, 30, 32));

            int w = pnlGraph.Width;
            int h = pnlGraph.Height;
            if (w < 10 || h < 10) return;

            // Horizontal grid lines
            using (var grid = new Pen(Color.FromArgb(55, 55, 60)))
            {
                for (int i = 1; i < 4; i++)
                {
                    int y = h * i / 4;
                    g.DrawLine(grid, 0, y, w, y);
                }
            }

            DrawLine(g, Stats.CpuHistory, w, h, Color.FromArgb(80, 220, 120), 100.0);
            DrawLine(g, Stats.MemHistory, w, h, Color.FromArgb(80, 160, 240), 256.0);

            // Legend
            using var legendFont = new Font("Segoe UI", 8F);
            using (var cpuBrush = new SolidBrush(Color.FromArgb(80, 220, 120)))
            using (var memBrush = new SolidBrush(Color.FromArgb(80, 160, 240)))
            {
                g.DrawString("● CPU %", legendFont, cpuBrush, 6, 4);
                g.DrawString("● Memory MB (max 256)", legendFont, memBrush, 70, 4);
            }
        }

        private static void DrawLine(Graphics g, double[] data, int w, int h, Color color, double max)
        {
            if (data.Length < 2) return;
            using var pen = new Pen(color, 2);
            float stepX = (float)w / (data.Length - 1);
            for (int i = 0; i < data.Length - 1; i++)
            {
                float x1 = i * stepX;
                float y1 = h - (float)(Math.Min(data[i], max) / max * h);
                float x2 = (i + 1) * stepX;
                float y2 = h - (float)(Math.Min(data[i + 1], max) / max * h);
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        // ---------- Button handlers ----------
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPort.Text.Trim(), out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Invalid port. Choose 1–65535.");
                return;
            }
            Database.Initialize();
            Stats.Reset();
            _server.Start(port);
        }

        private void btnStop_Click(object sender, EventArgs e) => _server.Stop();

        private void btnKick_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem == null) { MessageBox.Show("Select a user first."); return; }
            var item = lstUsers.SelectedItem as OnlineUserItem;
            if (item == null) { MessageBox.Show("Select a user first."); return; }
            string username = item.Username;
            if (MessageBox.Show($"Kick {username}?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            if (!_server.KickUser(username, "You were kicked by the administrator."))
                MessageBox.Show("User is no longer online.");
        }

        private void btnBroadcast_Click(object sender, EventArgs e)
        {
            string text = txtAnnounce.Text.Trim();
            if (text.Length == 0) return;
            if (!_server.IsRunning) { MessageBox.Show("Start the server first."); return; }

            _server.Broadcast(Envelope.Create(MessageTypes.SystemNotice, new SystemNotice { Text = text }));
            OnServerLog($"BROADCAST: {text}");
            txtAnnounce.Clear();
        }

        private void btnRefreshUsers_Click(object sender, EventArgs e)
        {
            dgvUsers.Rows.Clear();
            var all = _users.GetAllUsers();
            var onlineIds = _server.GetOnlineUsers().Select(u => u.UserId).ToHashSet();

            foreach (var u in all)
            {
                string status = onlineIds.Contains(u.UserId) ? "🟢 Online" : "⚪ Offline";
                dgvUsers.Rows.Add(
                    u.UserId,
                    u.Username,
                    u.FullName,
                    FormatDate(u.CreatedAt),
                    u.LastSeen == null ? "—" : FormatDate(u.LastSeen),
                    status);
            }
        }

        private static string FormatDate(string iso)
        {
            if (DateTime.TryParse(iso, out var dt))
                return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            return iso;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _ticker.Stop();
            _server.Stop();
            base.OnFormClosing(e);
        }
        private class OnlineUserItem
        {
            public int UserId { get; }
            public string Username { get; }
            public string Address { get; }
            public OnlineUserItem(int id, string username, string address)
            {
                UserId = id; Username = username; Address = address;
            }
            public override string ToString() => $"{Username}  [{Address}]";
        }
    }
}