namespace ChatApp.Server
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.tabUsers = new System.Windows.Forms.TabPage();
            this.tabSettings = new System.Windows.Forms.TabPage();

            this.lblStatus = new System.Windows.Forms.Label();
            this.lblIPInfo = new System.Windows.Forms.Label();
            this.lblPortLabel = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblUptime = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblOnlineTitle = new System.Windows.Forms.Label();
            this.lstUsers = new System.Windows.Forms.ListBox();
            this.btnKick = new System.Windows.Forms.Button();
            this.lblLogTitle = new System.Windows.Forms.Label();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.lblStatsTitle = new System.Windows.Forms.Label();
            this.lblStats = new System.Windows.Forms.Label();
            this.pnlGraph = new System.Windows.Forms.Panel();
            this.lblAnnounce = new System.Windows.Forms.Label();
            this.txtAnnounce = new System.Windows.Forms.TextBox();
            this.btnBroadcast = new System.Windows.Forms.Button();

            this.dgvUsers = new System.Windows.Forms.DataGridView();
            this.btnRefreshUsers = new System.Windows.Forms.Button();
            this.lblSettingsInfo = new System.Windows.Forms.Label();

            this.tabControl.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this.tabUsers.SuspendLayout();
            this.tabSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();

            // tabControl
            this.tabControl.Controls.Add(this.tabDashboard);
            this.tabControl.Controls.Add(this.tabUsers);
            this.tabControl.Controls.Add(this.tabSettings);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;

            // tabDashboard
            this.tabDashboard.Text = "Dashboard";
            this.tabDashboard.UseVisualStyleBackColor = true;
            // tabUsers
            this.tabUsers.Text = "Registered Users";
            this.tabUsers.UseVisualStyleBackColor = true;
            // tabSettings
            this.tabSettings.Text = "Settings";
            this.tabSettings.UseVisualStyleBackColor = true;

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Location = new System.Drawing.Point(12, 12);
            this.lblStatus.Text = "● Status: Stopped";
            // lblIPInfo
            this.lblIPInfo.AutoSize = true;
            this.lblIPInfo.Location = new System.Drawing.Point(260, 16);
            this.lblIPInfo.Text = "IP: 0.0.0.0";
            // lblPortLabel
            this.lblPortLabel.AutoSize = true;
            this.lblPortLabel.Location = new System.Drawing.Point(400, 16);
            this.lblPortLabel.Text = "Port:";
            // txtPort
            this.txtPort.Location = new System.Drawing.Point(440, 13);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(60, 24);
            this.txtPort.Text = "8765";
            // lblUptime
            this.lblUptime.AutoSize = true;
            this.lblUptime.Location = new System.Drawing.Point(530, 16);
            this.lblUptime.Text = "Uptime: 00:00:00";
            // btnStart
            this.btnStart.Location = new System.Drawing.Point(12, 45);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 28);
            this.btnStart.Text = "▶ Start";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // btnStop
            this.btnStop.Location = new System.Drawing.Point(120, 45);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 28);
            this.btnStop.Text = "■ Stop";
            this.btnStop.Enabled = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            // lblOnlineTitle
            this.lblOnlineTitle.AutoSize = true;
            this.lblOnlineTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblOnlineTitle.Location = new System.Drawing.Point(12, 90);
            this.lblOnlineTitle.Text = "ONLINE USERS (0)";
            // lstUsers
            this.lstUsers.FormattingEnabled = true;
            this.lstUsers.ItemHeight = 20;
            this.lstUsers.Location = new System.Drawing.Point(12, 115);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(300, 324);
            // btnKick
            this.btnKick.Location = new System.Drawing.Point(12, 445);
            this.btnKick.Name = "btnKick";
            this.btnKick.Size = new System.Drawing.Size(300, 30);
            this.btnKick.Text = "Kick Selected User";
            this.btnKick.Click += new System.EventHandler(this.btnKick_Click);

            // lblLogTitle
            this.lblLogTitle.AutoSize = true;
            this.lblLogTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLogTitle.Location = new System.Drawing.Point(324, 90);
            this.lblLogTitle.Text = "LIVE EVENT LOG";
            // lstLog
            this.lstLog.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 18;
            this.lstLog.Location = new System.Drawing.Point(324, 115);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(744, 360);
            this.lstLog.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstLog_DrawItem);

            // lblStatsTitle
            this.lblStatsTitle.AutoSize = true;
            this.lblStatsTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatsTitle.Location = new System.Drawing.Point(12, 490);
            this.lblStatsTitle.Text = "STATISTICS & DIAGNOSTICS";
            // lblStats
            this.lblStats.AutoSize = true;
            this.lblStats.Location = new System.Drawing.Point(12, 515);
            this.lblStats.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.lblStats.Text = "Online: 0    Registered: 0    Messages: 0    Files: 0";
            // pnlGraph
            this.pnlGraph.Location = new System.Drawing.Point(400, 510);
            this.pnlGraph.Name = "pnlGraph";
            this.pnlGraph.Size = new System.Drawing.Size(668, 90);
            this.pnlGraph.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlGraph_Paint);

            // lblAnnounce
            this.lblAnnounce.AutoSize = true;
            this.lblAnnounce.Location = new System.Drawing.Point(12, 630);
            this.lblAnnounce.Text = "Announcement:";
            // txtAnnounce
            this.txtAnnounce.Location = new System.Drawing.Point(110, 627);
            this.txtAnnounce.Name = "txtAnnounce";
            this.txtAnnounce.Size = new System.Drawing.Size(820, 24);
            // btnBroadcast
            this.btnBroadcast.Location = new System.Drawing.Point(940, 625);
            this.btnBroadcast.Name = "btnBroadcast";
            this.btnBroadcast.Size = new System.Drawing.Size(128, 28);
            this.btnBroadcast.Text = "📢 Broadcast";
            this.btnBroadcast.Click += new System.EventHandler(this.btnBroadcast_Click);

            this.tabDashboard.Controls.Add(this.lblStatus);
            this.tabDashboard.Controls.Add(this.lblIPInfo);
            this.tabDashboard.Controls.Add(this.lblPortLabel);
            this.tabDashboard.Controls.Add(this.txtPort);
            this.tabDashboard.Controls.Add(this.lblUptime);
            this.tabDashboard.Controls.Add(this.btnStart);
            this.tabDashboard.Controls.Add(this.btnStop);
            this.tabDashboard.Controls.Add(this.lblOnlineTitle);
            this.tabDashboard.Controls.Add(this.lstUsers);
            this.tabDashboard.Controls.Add(this.btnKick);
            this.tabDashboard.Controls.Add(this.lblLogTitle);
            this.tabDashboard.Controls.Add(this.lstLog);
            this.tabDashboard.Controls.Add(this.lblStatsTitle);
            this.tabDashboard.Controls.Add(this.lblStats);
            this.tabDashboard.Controls.Add(this.pnlGraph);
            this.tabDashboard.Controls.Add(this.lblAnnounce);
            this.tabDashboard.Controls.Add(this.txtAnnounce);
            this.tabDashboard.Controls.Add(this.btnBroadcast);

            // DataGridView
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.Location = new System.Drawing.Point(12, 60);
            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.RowHeadersVisible = false;
            this.dgvUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsers.Size = new System.Drawing.Size(1056, 620);
            this.dgvUsers.Columns.Add("UserId", "ID");
            this.dgvUsers.Columns.Add("Username", "Username");
            this.dgvUsers.Columns.Add("FullName", "Full Name");
            this.dgvUsers.Columns.Add("CreatedAt", "Created");
            this.dgvUsers.Columns.Add("LastSeen", "Last Seen");
            this.dgvUsers.Columns.Add("Status", "Status");

            // btnRefreshUsers
            this.btnRefreshUsers.Location = new System.Drawing.Point(12, 15);
            this.btnRefreshUsers.Name = "btnRefreshUsers";
            this.btnRefreshUsers.Size = new System.Drawing.Size(120, 30);
            this.btnRefreshUsers.Text = "🔄 Refresh";
            this.btnRefreshUsers.Click += new System.EventHandler(this.btnRefreshUsers_Click);

            this.tabUsers.Controls.Add(this.btnRefreshUsers);
            this.tabUsers.Controls.Add(this.dgvUsers);

            // Settings tab
            this.lblSettingsInfo.AutoSize = true;
            this.lblSettingsInfo.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblSettingsInfo.Location = new System.Drawing.Point(20, 20);
            this.lblSettingsInfo.Text =
                "Settings are configurable by editing the Constants file in ChatApp.Common.\r\n\r\n" +
                "• Default Port: 8765\r\n" +
                "• Maximum line length: 8 MB\r\n" +
                "• Maximum message length: 4000 chars\r\n" +
                "• Maximum file size: 10 MB\r\n" +
                "• File chunk size: 32 KB\r\n\r\n" +
                "Server data folder:\r\n" +
                "  " + ChatApp.Server.Data.Database.DataFolder + "\r\n\r\n" +
                "Database file:\r\n" +
                "  " + ChatApp.Server.Data.Database.DbPath;

            this.tabSettings.Controls.Add(this.lblSettingsInfo);

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1080, 720);
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.tabControl);
            this.Name = "Form1";
            this.Text = "DevChat Server Administrator Control Panel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            this.tabControl.ResumeLayout(false);
            this.tabDashboard.ResumeLayout(false);
            this.tabDashboard.PerformLayout();
            this.tabUsers.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDashboard;
        private System.Windows.Forms.TabPage tabUsers;
        private System.Windows.Forms.TabPage tabSettings;

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblIPInfo;
        private System.Windows.Forms.Label lblPortLabel;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblUptime;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblOnlineTitle;
        private System.Windows.Forms.ListBox lstUsers;
        private System.Windows.Forms.Button btnKick;
        private System.Windows.Forms.Label lblLogTitle;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.Label lblStatsTitle;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.Panel pnlGraph;
        private System.Windows.Forms.Label lblAnnounce;
        private System.Windows.Forms.TextBox txtAnnounce;
        private System.Windows.Forms.Button btnBroadcast;

        private System.Windows.Forms.DataGridView dgvUsers;
        private System.Windows.Forms.Button btnRefreshUsers;
        private System.Windows.Forms.Label lblSettingsInfo;
    }
}