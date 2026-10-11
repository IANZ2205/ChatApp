namespace ChatApp.Client
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblWho = new System.Windows.Forms.Label();
            this.lstConversations = new System.Windows.Forms.ListBox();
            this.lblConversations = new System.Windows.Forms.Label();
            this.btnNewGroup = new System.Windows.Forms.Button();
            this.btnAddToGroup = new System.Windows.Forms.Button();
            this.btnMyProfile = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lstMessages = new System.Windows.Forms.ListBox();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnAddFriend = new System.Windows.Forms.Button();
            this.lblFriends = new System.Windows.Forms.Label();
            this.lstFriends = new System.Windows.Forms.ListBox();
            this.cmsFriends = new System.Windows.Forms.ContextMenuStrip();
            this.miViewProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.btnViewFriendProfile = new System.Windows.Forms.Button();
            this.lblOnline = new System.Windows.Forms.Label();
            this.lstOnline = new System.Windows.Forms.ListBox();
            this.lblRequests = new System.Windows.Forms.Label();
            this.lstRequests = new System.Windows.Forms.ListBox();
            this.btnAcceptRequest = new System.Windows.Forms.Button();
            this.btnDeclineRequest = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 640);
            this.MinimumSize = new System.Drawing.Size(940, 600);
            this.Name = "MainForm";
            this.Text = "DevChat Room";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // ---- Left column (Conversations) ----
            this.lblWho.AutoSize = true;
            this.lblWho.Location = new System.Drawing.Point(12, 9);
            this.lblWho.Text = "Logged in as:";
            this.lblWho.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;

            this.lblConversations.AutoSize = true;
            this.lblConversations.Location = new System.Drawing.Point(12, 40);
            this.lblConversations.Text = "Conversations:";
            this.lblConversations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;

            this.lstConversations.FormattingEnabled = true;
            this.lstConversations.ItemHeight = 15;
            this.lstConversations.Location = new System.Drawing.Point(12, 60);
            this.lstConversations.Name = "lstConversations";
            this.lstConversations.Size = new System.Drawing.Size(210, 424);
            this.lstConversations.TabIndex = 1;
            this.lstConversations.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.lstConversations.SelectedIndexChanged += new System.EventHandler(this.lstConversations_SelectedIndexChanged);

            this.btnNewGroup.Location = new System.Drawing.Point(12, 490);
            this.btnNewGroup.Name = "btnNewGroup";
            this.btnNewGroup.Size = new System.Drawing.Size(210, 28);
            this.btnNewGroup.Text = "➕ New Group";
            this.btnNewGroup.UseVisualStyleBackColor = true;
            this.btnNewGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnNewGroup.Click += new System.EventHandler(this.btnNewGroup_Click);

            this.btnAddToGroup.Location = new System.Drawing.Point(12, 524);
            this.btnAddToGroup.Name = "btnAddToGroup";
            this.btnAddToGroup.Size = new System.Drawing.Size(210, 28);
            this.btnAddToGroup.Text = "➕ Add Member to Group";
            this.btnAddToGroup.UseVisualStyleBackColor = true;
            this.btnAddToGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnAddToGroup.Click += new System.EventHandler(this.btnAddToGroup_Click);

            this.btnMyProfile.Location = new System.Drawing.Point(12, 558);
            this.btnMyProfile.Name = "btnMyProfile";
            this.btnMyProfile.Size = new System.Drawing.Size(100, 28);
            this.btnMyProfile.Text = "👤 My Profile";
            this.btnMyProfile.UseVisualStyleBackColor = true;
            this.btnMyProfile.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnMyProfile.Click += new System.EventHandler(this.btnMyProfile_Click);

            this.btnLogout.Location = new System.Drawing.Point(118, 558);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(104, 28);
            this.btnLogout.Text = "🚪 Logout";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // ---- Middle column (Messages) ----
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(240, 40);
            this.lblTitle.Text = "Messages:";
            this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;

            this.lstMessages.FormattingEnabled = true;
            this.lstMessages.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstMessages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstMessages.IntegralHeight = false;
            this.lstMessages.Location = new System.Drawing.Point(240, 60);
            this.lstMessages.Name = "lstMessages";
            this.lstMessages.Size = new System.Drawing.Size(500, 424);
            this.lstMessages.TabIndex = 3;
            this.lstMessages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lstMessages.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstMessages_DrawItem);
            this.lstMessages.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lstMessages_MeasureItem);

            this.txtInput.Location = new System.Drawing.Point(240, 522);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(380, 23);
            this.txtInput.TabIndex = 4;
            this.txtInput.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);

            this.btnSend.Location = new System.Drawing.Point(626, 520);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(114, 26);
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);

            this.btnAttach.Location = new System.Drawing.Point(240, 490);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(100, 26);
            this.btnAttach.Text = "📎 Attach";
            this.btnAttach.UseVisualStyleBackColor = true;
            this.btnAttach.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnAttach.Click += new System.EventHandler(this.btnAttach_Click);

            this.btnDownload.Location = new System.Drawing.Point(346, 490);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(110, 26);
            this.btnDownload.Text = "⬇ Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);

            this.btnAddFriend.Location = new System.Drawing.Point(462, 490);
            this.btnAddFriend.Name = "btnAddFriend";
            this.btnAddFriend.Size = new System.Drawing.Size(120, 26);
            this.btnAddFriend.Text = "➕ Add Friend";
            this.btnAddFriend.UseVisualStyleBackColor = true;
            this.btnAddFriend.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnAddFriend.Click += new System.EventHandler(this.btnAddFriend_Click);

            // ---- Right column (Requests, Friends, Online) ----
            this.lblRequests.AutoSize = true;
            this.lblRequests.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRequests.ForeColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.lblRequests.Location = new System.Drawing.Point(756, 40);
            this.lblRequests.Text = "Friend Requests (0):";
            this.lblRequests.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            this.lstRequests.FormattingEnabled = true;
            this.lstRequests.ItemHeight = 15;
            this.lstRequests.Location = new System.Drawing.Point(756, 60);
            this.lstRequests.Name = "lstRequests";
            this.lstRequests.Size = new System.Drawing.Size(232, 124);
            this.lstRequests.TabIndex = 10;
            this.lstRequests.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            this.btnAcceptRequest.Location = new System.Drawing.Point(756, 190);
            this.btnAcceptRequest.Name = "btnAcceptRequest";
            this.btnAcceptRequest.Size = new System.Drawing.Size(112, 28);
            this.btnAcceptRequest.Text = "✓ Accept";
            this.btnAcceptRequest.UseVisualStyleBackColor = true;
            this.btnAcceptRequest.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnAcceptRequest.Click += new System.EventHandler(this.btnAcceptRequest_Click);

            this.btnDeclineRequest.Location = new System.Drawing.Point(876, 190);
            this.btnDeclineRequest.Name = "btnDeclineRequest";
            this.btnDeclineRequest.Size = new System.Drawing.Size(112, 28);
            this.btnDeclineRequest.Text = "✗ Decline";
            this.btnDeclineRequest.UseVisualStyleBackColor = true;
            this.btnDeclineRequest.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnDeclineRequest.Click += new System.EventHandler(this.btnDeclineRequest_Click);

            this.lblFriends.AutoSize = true;
            this.lblFriends.Location = new System.Drawing.Point(756, 230);
            this.lblFriends.Text = "Friends:";
            this.lblFriends.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            this.cmsFriends.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.miViewProfile });
            this.cmsFriends.Name = "cmsFriends";
            this.cmsFriends.Size = new System.Drawing.Size(160, 26);

            this.miViewProfile.Text = "👤 View Profile";
            this.miViewProfile.Click += new System.EventHandler(this.miViewProfile_Click);

            this.lstFriends.ContextMenuStrip = this.cmsFriends;
            this.lstFriends.FormattingEnabled = true;
            this.lstFriends.ItemHeight = 15;
            this.lstFriends.Location = new System.Drawing.Point(756, 250);
            this.lstFriends.Name = "lstFriends";
            this.lstFriends.Size = new System.Drawing.Size(232, 124);
            this.lstFriends.TabIndex = 12;
            this.lstFriends.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lstFriends.DoubleClick += new System.EventHandler(this.lstFriends_DoubleClick);

            this.btnViewFriendProfile.Location = new System.Drawing.Point(756, 380);
            this.btnViewFriendProfile.Name = "btnViewFriendProfile";
            this.btnViewFriendProfile.Size = new System.Drawing.Size(232, 28);
            this.btnViewFriendProfile.Text = "👤 View Friend Profile";
            this.btnViewFriendProfile.UseVisualStyleBackColor = true;
            this.btnViewFriendProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnViewFriendProfile.Click += new System.EventHandler(this.btnViewFriendProfile_Click);

            this.lblOnline.AutoSize = true;
            this.lblOnline.Location = new System.Drawing.Point(756, 418);
            this.lblOnline.Text = "Online (0):";
            this.lblOnline.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            this.lstOnline.FormattingEnabled = true;
            this.lstOnline.ItemHeight = 15;
            this.lstOnline.Location = new System.Drawing.Point(756, 438);
            this.lstOnline.Name = "lstOnline";
            this.lstOnline.Size = new System.Drawing.Size(232, 154);
            this.lstOnline.TabIndex = 14;
            this.lstOnline.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.lstOnline.DoubleClick += new System.EventHandler(this.lstOnline_DoubleClick);

            // Add all controls
            this.Controls.Add(this.lblWho);
            this.Controls.Add(this.lblConversations);
            this.Controls.Add(this.lstConversations);
            this.Controls.Add(this.btnNewGroup);
            this.Controls.Add(this.btnAddToGroup);
            this.Controls.Add(this.btnMyProfile);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lstMessages);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnAttach);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.btnAddFriend);
            this.Controls.Add(this.lblRequests);
            this.Controls.Add(this.lstRequests);
            this.Controls.Add(this.btnAcceptRequest);
            this.Controls.Add(this.btnDeclineRequest);
            this.Controls.Add(this.lblFriends);
            this.Controls.Add(this.lstFriends);
            this.Controls.Add(this.btnViewFriendProfile);
            this.Controls.Add(this.lblOnline);
            this.Controls.Add(this.lstOnline);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblWho;
        private System.Windows.Forms.Label lblConversations;
        private System.Windows.Forms.ListBox lstConversations;
        private System.Windows.Forms.Button btnNewGroup;
        private System.Windows.Forms.Button btnAddToGroup;
        private System.Windows.Forms.Button btnMyProfile;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ListBox lstMessages;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnAddFriend;
        private System.Windows.Forms.Label lblFriends;
        private System.Windows.Forms.ListBox lstFriends;
        private System.Windows.Forms.ContextMenuStrip cmsFriends;
        private System.Windows.Forms.ToolStripMenuItem miViewProfile;
        private System.Windows.Forms.Button btnViewFriendProfile;
        private System.Windows.Forms.Label lblOnline;
        private System.Windows.Forms.ListBox lstOnline;
        private System.Windows.Forms.Label lblRequests;
        private System.Windows.Forms.ListBox lstRequests;
        private System.Windows.Forms.Button btnAcceptRequest;
        private System.Windows.Forms.Button btnDeclineRequest;
    }
}