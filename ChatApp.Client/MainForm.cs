using ChatApp.Client.Network;
using ChatApp.Common;
using ChatApp.Common.Models;
using ChatApp.Common.Protocol;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Client
{
    public partial class MainForm : Form
    {
        public bool LoggedOut { get; private set; }
        private readonly List<PendingRequestInfo> _pendingRequests = new();
        private readonly ServerConnection _connection;
        private readonly int _userId;
        private readonly string _username;

        private readonly Dictionary<int, ConversationInfo> _conversations = new();
        private readonly Dictionary<int, string> _online = new();
        private readonly Dictionary<int, List<ChatMessage>> _messageCache = new();
        private readonly List<FriendInfo> _friends = new();

        private int _currentConversationId = 0;
        private bool _suppressConversationEvent = false;
        private string? _pendingCreateGroupRequestId;

        private NotifyIcon? _tray;
        private bool _soundsEnabled = true;
        private DateTime _lastSound = DateTime.MinValue;

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }
        private const uint FLASHW_ALL = 3;
        private const uint FLASHW_TIMERNOFG = 12;

        private class DownloadState
        {
            public MemoryStream Buffer = new();
            public string FileName = "";
            public int ExpectedChunks;
            public int ReceivedChunks;
        }
        private readonly Dictionary<int, DownloadState> _downloads = new();

        public MainForm(ServerConnection connection, int userId, string username)
        {
            InitializeComponent();
            Theme.Apply(this);
            _connection = connection;
            _userId = userId;
            _username = username;

            lblWho.Text = $"Logged in as: {username} (#{userId})";
            _connection.MessageReceived += OnMessageReceived;

            SetupTray();

            _connection.Send(Envelope.Create(MessageTypes.GetOnlineList, new { }));
            _connection.Send(Envelope.Create(MessageTypes.GetConversations, new { }));
            _connection.Send(Envelope.Create(MessageTypes.FriendGetList, new { }));
            _connection.Send(Envelope.Create(MessageTypes.GetPendingRequests, new { }));
        }

        // ---------- Tray ----------
        private void SetupTray()
        {
            _tray = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "DevChat"
            };
            _tray.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = FormWindowState.Normal;
                Activate();
            };
        }

        private void NotifyNewMessage(ChatMessage msg)
        {
            if (msg.SenderId == _userId) return;
            bool focused = (WindowState != FormWindowState.Minimized) && ContainsFocus;
            if (focused && msg.ConversationId == _currentConversationId) return;

            string body = msg.Kind == "File" ? $"📄 {msg.FileName}" : msg.Content;
            if (body.Length > 100) body = body.Substring(0, 100) + "…";
            _tray?.ShowBalloonTip(3000, msg.SenderName, body, ToolTipIcon.Info);

            if (_soundsEnabled && (DateTime.UtcNow - _lastSound).TotalSeconds > 1)
            {
                _lastSound = DateTime.UtcNow;
                try { SystemSounds.Asterisk.Play(); } catch { }
            }
            if (!focused) FlashTaskbar();
        }

        private void FlashTaskbar()
        {
            try
            {
                var info = new FLASHWINFO
                {
                    cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
                    hwnd = this.Handle,
                    dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                    uCount = 5,
                    dwTimeout = 0
                };
                FlashWindowEx(ref info);
            }
            catch { }
        }

        // ---------- Incoming ----------
        private void OnMessageReceived(Envelope envelope)
        {
            if (InvokeRequired) { BeginInvoke(new Action<Envelope>(OnMessageReceived), envelope); return; }

            switch (envelope.Type)
            {
                case MessageTypes.MessageNew:
                    var msg = envelope.ReadData<ChatMessage>();
                    if (msg != null) OnIncomingMessage(msg);
                    break;

                case MessageTypes.HistoryResult:
                    var hist = envelope.ReadData<HistoryResult>();
                    if (hist != null)
                    {
                        _messageCache[hist.ConversationId] = new List<ChatMessage>(hist.Messages);
                        if (hist.ConversationId == _currentConversationId) RenderCurrentConversation();
                    }
                    break;

                case MessageTypes.OnlineList:
                    var online = envelope.ReadData<OnlineListResult>();
                    if (online != null)
                    {
                        _online.Clear();
                        foreach (var u in online.Users) _online[u.UserId] = u.Username;
                        RefreshOnlineList();
                    }
                    break;

                case MessageTypes.PresenceUpdate:
                    var p = envelope.ReadData<PresenceUpdate>();
                    if (p != null)
                    {
                        if (p.IsOnline) _online[p.UserId] = p.Username; else _online.Remove(p.UserId);
                        RefreshOnlineList();
                        var fr = _friends.FirstOrDefault(f => f.UserId == p.UserId);
                        if (fr != null) { fr.IsOnline = p.IsOnline; RefreshFriendsList(); }
                    }
                    break;

                case MessageTypes.ConversationList:
                    var cl = envelope.ReadData<ConversationListResult>();
                    if (cl != null)
                    {
                        _conversations.Clear();
                        foreach (var c in cl.Conversations) _conversations[c.ConversationId] = c;
                        RefreshConversationList();
                        if (_currentConversationId == 0 && _conversations.Count > 0)
                        {
                            var first = _conversations.Values.OrderBy(c => c.Type == "Group" ? 0 : 1).First();
                            SelectConversation(first.ConversationId);
                        }
                    }
                    break;

                case MessageTypes.OpenPrivateChatResult:
                    var created = envelope.ReadData<ConversationInfo>();
                    if (created != null) AddOrUpdateConversation(created, true);
                    break;

                case MessageTypes.ConversationAdded:
                    var added = envelope.ReadData<ConversationInfo>();
                    if (added != null)
                    {
                        bool isMine = !string.IsNullOrEmpty(_pendingCreateGroupRequestId)
                                      && envelope.RequestId == _pendingCreateGroupRequestId;
                        if (isMine) _pendingCreateGroupRequestId = null;
                        AddOrUpdateConversation(added, selectIt: isMine);
                    }
                    break;

                case MessageTypes.SystemNotice:
                    var notice = envelope.ReadData<SystemNotice>();
                    if (notice != null)
                    {
                        AppendSystem($"📢 ANNOUNCEMENT: {notice.Text}");
                        _tray?.ShowBalloonTip(4000, "Announcement", notice.Text, ToolTipIcon.Warning);
                    }
                    break;

                case MessageTypes.FileUploadResult:
                    var fu = envelope.ReadData<FileUploadStartResult>();
                    if (fu != null && !fu.Success) MessageBox.Show($"Upload failed: {fu.Message}");
                    break;

                case MessageTypes.FileDownloadChunk:
                    var chunk = envelope.ReadData<FileDownloadChunk>();
                    if (chunk != null) HandleDownloadChunk(chunk);
                    break;

                case MessageTypes.FileDownloadEnd:
                    var end = envelope.ReadData<FileDownloadEnd>();
                    if (end != null) HandleDownloadEnd(end.FileId);
                    break;

                case MessageTypes.FriendListResult:
                    var fl = envelope.ReadData<FriendListResult>();
                    if (fl != null)
                    {
                        _friends.Clear();
                        _friends.AddRange(fl.Friends);
                        RefreshFriendsList();
                    }
                    break;

                case MessageTypes.FriendRequestIncoming:
                    var inc = envelope.ReadData<FriendRequestIncoming>();
                    if (inc != null) HandleIncomingFriendRequest(inc);
                    break;
                case MessageTypes.PendingRequestsResult:
                    var pr = envelope.ReadData<PendingRequestsResult>();
                    if (pr != null)
                    {
                        _pendingRequests.Clear();
                        _pendingRequests.AddRange(pr.Requests);
                        RefreshRequestsList();
                    }
                    break;

                case MessageTypes.FriendRequestResult:
                    var frr = envelope.ReadData<FriendRequestResult>();
                    if (frr != null)
                        MessageBox.Show(frr.Success
                            ? $"Friend request sent to {frr.PeerUsername}."
                            : $"Could not send request: {frr.Message}");
                    break;

                case MessageTypes.Kicked:
                    var k = envelope.ReadData<KickNotice>();
                    MessageBox.Show(k?.Reason ?? "You were removed.",
                        "Kicked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    break;
            }
        }

        // ---------- Friends ----------
        private void RefreshFriendsList()
        {
            lstFriends.Items.Clear();
            foreach (var f in _friends.OrderBy(f => f.Username))
            {
                string dot = f.IsOnline ? "🟢" : "⚪";
                lstFriends.Items.Add(new FriendItem(f.UserId, $"{dot} {f.Username}"));
            }
            lblFriends.Text = $"Friends ({_friends.Count}):";
        }
        private void RefreshRequestsList()
        {
            lstRequests.Items.Clear();
            foreach (var r in _pendingRequests)
                lstRequests.Items.Add(new RequestItem(r.FriendshipId, r.RequesterUsername));
            lblRequests.Text = $"Friend Requests ({_pendingRequests.Count}):";
        }

        private void btnAcceptRequest_Click(object sender, EventArgs e) => RespondToSelected(true);
        private void btnDeclineRequest_Click(object sender, EventArgs e) => RespondToSelected(false);

        private void RespondToSelected(bool accept)
        {
            if (lstRequests.SelectedItem is not RequestItem r)
            {
                MessageBox.Show("Select a request first.");
                return;
            }
            _connection.Send(Envelope.Create(MessageTypes.FriendResponse,
                new FriendResponseReq { FriendshipId = r.FriendshipId, Accept = accept }));
        }

        private void HandleIncomingFriendRequest(FriendRequestIncoming inc)
        {
            // The server also pushes a refreshed PendingRequestsResult, so the list will update automatically.
            _tray?.ShowBalloonTip(3000, "Friend request",
                $"{inc.RequesterUsername} wants to be your friend.", ToolTipIcon.Info);
        }

        private void btnAddFriend_Click(object sender, EventArgs e)
        {
            string username = ShowInputDialog("Add Friend", "Enter the username of the person you want to add:");
            if (string.IsNullOrWhiteSpace(username)) return;
            _connection.Send(Envelope.Create(MessageTypes.FriendSendRequest,
                new FriendSendRequestReq { TargetUsername = username.Trim() }));
        }

        private string ShowInputDialog(string title, string prompt)
        {
            using var form = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new System.Drawing.Size(360, 120),
                MinimizeBox = false,
                MaximizeBox = false
            };
            var lbl = new Label { Text = prompt, Left = 12, Top = 15, Width = 336 };
            var txt = new TextBox { Left = 12, Top = 40, Width = 336 };
            var ok = new Button { Text = "OK", Left = 200, Top = 75, Width = 70, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Left = 278, Top = 75, Width = 70, DialogResult = DialogResult.Cancel };

            form.Controls.Add(lbl);
            form.Controls.Add(txt);
            form.Controls.Add(ok);
            form.Controls.Add(cancel);
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            return form.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : "";
        }

        private void lstFriends_DoubleClick(object sender, EventArgs e)
        {
            if (lstFriends.SelectedItem is not FriendItem f) return;
            _connection.Send(Envelope.Create(MessageTypes.OpenPrivateChat,
                new OpenPrivateChatRequest { TargetUserId = f.Id }));
        }
        private void miViewProfile_Click(object sender, EventArgs e)
        {
            if (lstFriends.SelectedItem is not FriendItem f)
            {
                MessageBox.Show("Select a friend first.");
                return;
            }
            using var dlg = new ProfileForm(_connection, f.Id, editable: false);
            dlg.ShowDialog(this);
        }
        private void btnViewFriendProfile_Click(object sender, EventArgs e)
        {
            if (lstFriends.SelectedItem is not FriendItem f)
            {
                MessageBox.Show("Select a friend in the Friends list first.");
                return;
            }
            using var dlg = new ProfileForm(_connection, f.Id, editable: false);
            dlg.ShowDialog(this);
        }

        // ---------- New Group ----------
        private void btnNewGroup_Click(object sender, EventArgs e)
        {
            if (_friends.Count == 0)
            {
                MessageBox.Show("You need at least one friend to create a group. Add friends first.");
                return;
            }

            using var dlg = new NewGroupForm(_friends);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            string requestId = Guid.NewGuid().ToString("N");
            _pendingCreateGroupRequestId = requestId;

            _connection.Send(Envelope.Create(
                MessageTypes.CreateGroup,
                new CreateGroupRequest
                {
                    Name = dlg.GroupName,
                    MemberUserIds = dlg.SelectedUserIds
                },
                requestId));
        }
        private void btnAddToGroup_Click(object sender, EventArgs e)
        {
            if (_currentConversationId == 0)
            {
                MessageBox.Show("Select a group in the Conversations list first.");
                return;
            }
            if (!_conversations.TryGetValue(_currentConversationId, out var conv))
            {
                MessageBox.Show("Conversation not found.");
                return;
            }
            if (conv.Type != "Group")
            {
                MessageBox.Show("You can only add members to a group, not a private chat.");
                return;
            }
            if (conv.Name == "General")
            {
                MessageBox.Show("You cannot add members to the General room.");
                return;
            }
            if (_friends.Count == 0)
            {
                MessageBox.Show("You have no friends to add yet.");
                return;
            }

            using var picker = new Form
            {
                Text = $"Add member to '{conv.Name}'",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new System.Drawing.Size(320, 260),
                MinimizeBox = false,
                MaximizeBox = false
            };
            var lbl = new Label { Text = "Select a friend:", Left = 12, Top = 15, Width = 296 };
            var lst = new ListBox { Left = 12, Top = 40, Width = 296, Height = 160 };
            foreach (var f in _friends) lst.Items.Add(new FriendItem(f.UserId, f.Username));
            var ok = new Button { Text = "Add", Left = 152, Top = 215, Width = 75, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Left = 233, Top = 215, Width = 75, DialogResult = DialogResult.Cancel };

            picker.Controls.Add(lbl);
            picker.Controls.Add(lst);
            picker.Controls.Add(ok);
            picker.Controls.Add(cancel);
            picker.AcceptButton = ok;
            picker.CancelButton = cancel;

            if (picker.ShowDialog(this) != DialogResult.OK) return;
            if (lst.SelectedItem is not FriendItem chosen) return;

            _connection.Send(Envelope.Create(MessageTypes.AddMemberToGroup,
                new AddMemberToGroupRequest
                {
                    ConversationId = _currentConversationId,
                    TargetUserId = chosen.Id
                }));
        }
        private void btnMyProfile_Click(object sender, EventArgs e)
        {
            using var dlg = new ProfileForm(_connection, _userId, editable: true);
            dlg.ShowDialog(this);
        }
        
        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Log out now?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            LoggedOut = true;
            _connection.Send(Envelope.Create(MessageTypes.Logout, new { }));
            // Give the socket a moment to flush
            System.Threading.Thread.Sleep(150);
            Close();
        }

        // ---------- Incoming message ----------
        private void OnIncomingMessage(ChatMessage msg)
        {
            if (!_messageCache.TryGetValue(msg.ConversationId, out var list))
                _messageCache[msg.ConversationId] = list = new List<ChatMessage>();
            list.Add(msg);

            if (msg.ConversationId == _currentConversationId)
            {
                AppendMessageToView(msg);

                // We're reading it now → tell the server
                _connection.Send(Envelope.Create(MessageTypes.MarkRead,
                    new MarkReadRequest { ConversationId = msg.ConversationId, LastMessageId = msg.MessageId }));
            }
            else if (_conversations.TryGetValue(msg.ConversationId, out var conv))
            {
                conv.UnreadCount++;
                conv.LastMessage = msg.Kind == "File" ? $"📄 {msg.FileName}" : msg.Content;
                RefreshConversationList();
            }
            NotifyNewMessage(msg);
        }

        // ---------- File download ----------
        private void HandleDownloadChunk(FileDownloadChunk chunk)
        {
            if (!_downloads.TryGetValue(chunk.FileId, out var state))
            {
                state = new DownloadState { FileName = chunk.FileName, ExpectedChunks = chunk.Total };
                _downloads[chunk.FileId] = state;
            }
            try
            {
                byte[] data = Convert.FromBase64String(chunk.DataB64);
                state.Buffer.Write(data, 0, data.Length);
                state.ReceivedChunks++;
                btnDownload.Text = $"⬇ {state.ReceivedChunks}/{state.ExpectedChunks}";
            }
            catch { }
        }

        private void HandleDownloadEnd(int fileId)
        {
            if (!_downloads.TryGetValue(fileId, out var state)) return;
            _downloads.Remove(fileId);
            btnDownload.Text = "⬇ Download";

            using var dlg = new SaveFileDialog { FileName = state.FileName };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(dlg.FileName, state.Buffer.ToArray());
                    MessageBox.Show($"Saved to: {dlg.FileName}");
                }
                catch (Exception ex) { MessageBox.Show($"Save failed: {ex.Message}"); }
            }
            state.Buffer.Dispose();
        }

        // ---------- Conversations ----------
        private void RefreshConversationList()
        {
            _suppressConversationEvent = true;
            try
            {
                int prev = _currentConversationId;
                lstConversations.Items.Clear();
                foreach (var c in _conversations.Values.OrderBy(c => c.Type == "Group" ? 0 : 1))
                {
                    string display = c.Type == "Group" ? $"{c.Name} ({c.MemberCount})" : $"👤 {c.Name}";
                    if (c.UnreadCount > 0) display += $"  ({c.UnreadCount})";
                    lstConversations.Items.Add(new ConvItem(c.ConversationId, display));
                }
                for (int i = 0; i < lstConversations.Items.Count; i++)
                    if (lstConversations.Items[i] is ConvItem ci && ci.Id == prev)
                    { lstConversations.SelectedIndex = i; break; }
            }
            finally { _suppressConversationEvent = false; }
        }

        private void AddOrUpdateConversation(ConversationInfo info, bool selectIt)
        {
            _conversations[info.ConversationId] = info;
            RefreshConversationList();
            if (selectIt) SelectConversation(info.ConversationId);
        }

        private void SelectConversation(int convId)
        {
            _currentConversationId = convId;
            // Tell the server we read this conversation
            var list = _messageCache.TryGetValue(convId, out var msgs) ? msgs : null;
            int lastId = (list != null && list.Count > 0) ? list[^1].MessageId : 0;
            _connection.Send(Envelope.Create(MessageTypes.MarkRead,
                new MarkReadRequest { ConversationId = convId, LastMessageId = lastId }));
            if (_conversations.TryGetValue(convId, out var conv)) conv.UnreadCount = 0;
            RefreshConversationList();
            if (!_conversations.TryGetValue(convId, out conv)) return;
            lblTitle.Text = $"Messages - {conv.Name}";

            if (_messageCache.ContainsKey(convId)) RenderCurrentConversation();
            else
            {
                lstMessages.Items.Clear();
                _connection.Send(Envelope.Create(MessageTypes.GetHistory,
                    new GetHistoryRequest { ConversationId = convId, BeforeMessageId = 0, Limit = 50 }));
            }
        }

        private void lstConversations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressConversationEvent) return;
            if (lstConversations.SelectedItem is not ConvItem item) return;
            SelectConversation(item.Id);
        }

        private void RenderCurrentConversation()
        {
            lstMessages.Items.Clear();
            if (_messageCache.TryGetValue(_currentConversationId, out var list))
                foreach (var m in list) AppendMessageToView(m);
        }

        // ---------- Online list ----------
        private void RefreshOnlineList()
        {
            lstOnline.Items.Clear();
            foreach (var kv in _online.OrderBy(k => k.Value))
            {
                string marker = kv.Key == _userId ? " (you)" : "";
                lstOnline.Items.Add(new OnlineItem(kv.Key, $"{kv.Value}{marker}"));
            }
            lblOnline.Text = $"Online ({_online.Count}):";
        }

        private void lstOnline_DoubleClick(object sender, EventArgs e)
        {
            if (lstOnline.SelectedItem is not OnlineItem user) return;
            if (user.Id == _userId) return;
            _connection.Send(Envelope.Create(MessageTypes.OpenPrivateChat,
                new OpenPrivateChatRequest { TargetUserId = user.Id }));
        }

        // ---------- Chat ----------
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (_currentConversationId == 0) return;
            string text = txtInput.Text.Trim();
            if (text.Length == 0) return;
            _connection.Send(Envelope.Create(MessageTypes.SendMessage,
                new SendMessageRequest { ConversationId = _currentConversationId, Text = text }));
            txtInput.Clear();
            txtInput.Focus();
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnSend_Click(sender, EventArgs.Empty);
            }
        }

        // ---------- File upload ----------
        private void btnAttach_Click(object sender, EventArgs e)
        {
            if (_currentConversationId == 0) { MessageBox.Show("Select a conversation first."); return; }
            using var dlg = new OpenFileDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var fi = new FileInfo(dlg.FileName);
            if (fi.Length > Constants.MaxFileSize)
            { MessageBox.Show($"File too large. Max {Constants.MaxFileSize / (1024 * 1024)} MB."); return; }

            int convId = _currentConversationId;
            string fileName = fi.Name;

            btnAttach.Enabled = false;
            btnAttach.Text = "Uploading...";

            Task.Run(async () =>
            {
                try
                {
                    _connection.Send(Envelope.Create(MessageTypes.FileUploadStart,
                        new FileUploadStartRequest { ConversationId = convId, FileName = fileName, FileSize = fi.Length }));
                    string? uploadId = WaitForUploadId(3000);
                    if (string.IsNullOrEmpty(uploadId))
                    {
                        Invoke(new Action(() => MessageBox.Show("Server did not accept the upload.")));
                        return;
                    }

                    byte[] all = File.ReadAllBytes(dlg.FileName);
                    int total = (int)Math.Ceiling((double)all.Length / Constants.FileChunkSize);
                    if (total == 0) total = 1;
                    for (int i = 0; i < total; i++)
                    {
                        int offset = i * Constants.FileChunkSize;
                        int len = Math.Min(Constants.FileChunkSize, all.Length - offset);
                        string b64 = Convert.ToBase64String(all, offset, len);
                        _connection.Send(Envelope.Create(MessageTypes.FileUploadChunk,
                            new FileUploadChunkRequest { UploadId = uploadId, Seq = i, DataB64 = b64 }));
                        int progress = (i + 1) * 100 / total;
                        Invoke(new Action(() => btnAttach.Text = $"⬆ {progress}%"));
                        await Task.Delay(1);
                    }
                    _connection.Send(Envelope.Create(MessageTypes.FileUploadEnd,
                        new FileUploadEndRequest { UploadId = uploadId }));
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() => MessageBox.Show($"Upload error: {ex.Message}")));
                }
                finally
                {
                    Invoke(new Action(() =>
                    {
                        btnAttach.Enabled = true;
                        btnAttach.Text = "📎 Attach";
                    }));
                }
            });
        }

        private string? _lastUploadId;
        private string? WaitForUploadId(int timeoutMs)
        {
            _lastUploadId = null;
            Action<Envelope> capture = null!;
            capture = (env) =>
            {
                if (env.Type == MessageTypes.FileUploadResult)
                {
                    var r = env.ReadData<FileUploadStartResult>();
                    if (r != null && r.Success) _lastUploadId = r.UploadId;
                    _connection.MessageReceived -= capture;
                }
            };
            _connection.MessageReceived += capture;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (_lastUploadId == null && sw.ElapsedMilliseconds < timeoutMs)
                System.Threading.Thread.Sleep(20);
            return _lastUploadId;
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (!_messageCache.TryGetValue(_currentConversationId, out var list)) return;
            var fileMsg = list.LastOrDefault(m => m.Kind == "File" && m.FileId.HasValue);
            if (fileMsg == null) { MessageBox.Show("No file message in this conversation yet."); return; }
            _connection.Send(Envelope.Create(MessageTypes.FileDownloadRequest,
                new FileDownloadRequest { FileId = fileMsg.FileId!.Value }));
        }

        private void AppendMessageToView(ChatMessage msg)
        {
            var view = new MessageView
            {
                MessageId = msg.MessageId,
                ConversationId = msg.ConversationId,
                SenderId = msg.SenderId,
                SenderName = msg.SenderName,
                Content = msg.Content,
                Kind = msg.Kind,
                FileName = msg.FileName,
                FileSize = msg.FileSize,
                SentAt = msg.SentAt,
                IsOwn = msg.SenderId == _userId,
                IsSystem = msg.Kind == "System",
                ShowSenderName = msg.SenderId != _userId
                                 && msg.Kind != "System"
                                 && (_conversations.TryGetValue(msg.ConversationId, out var c) && c.Type == "Group")
            };

            lstMessages.Items.Add(view);
            lstMessages.TopIndex = lstMessages.Items.Count - 1;
        }

        private void AppendSystem(string text)
        {
            var view = new MessageView
            {
                Kind = "System",
                Content = text,
                IsSystem = true,
                SentAt = DateTime.UtcNow
            };
            lstMessages.Items.Add(view);
            lstMessages.TopIndex = lstMessages.Items.Count - 1;
        }
        // ---------- Chat bubble rendering ----------
        private const int BubbleMaxWidth = 400;
        private const int BubblePadding = 10;
        private const int BubbleRadius = 10;

        private void lstMessages_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstMessages.Items.Count)
            { e.ItemHeight = 20; return; }

            if (lstMessages.Items[e.Index] is not MessageView m)
            { e.ItemHeight = 20; return; }

            if (m.IsSystem)
            { e.ItemHeight = 26; return; }

            string text = m.Kind == "File"
                ? $"📄 {m.FileName} ({FormatFileSize(m.FileSize ?? 0)})"
                : m.Content;

            Size textSize = TextRenderer.MeasureText(
                text, lstMessages.Font,
                new Size(BubbleMaxWidth - 2 * BubblePadding, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            int h = textSize.Height + 2 * BubblePadding + 16; // +16 for time row
            if (m.ShowSenderName) h += 16;
            e.ItemHeight = h + 8;
        }

        private void lstMessages_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var bg = new SolidBrush(lstMessages.BackColor))
                g.FillRectangle(bg, e.Bounds);

            if (lstMessages.Items[e.Index] is not MessageView m) return;

            if (m.IsSystem)
            {
                using var sysFont = new Font(lstMessages.Font.FontFamily, 8f, FontStyle.Italic);
                var size = TextRenderer.MeasureText(m.Content, sysFont);
                int cx = (lstMessages.ClientSize.Width - size.Width) / 2;
                TextRenderer.DrawText(g, m.Content, sysFont,
                    new Point(cx, e.Bounds.Top + 6),
                    Color.FromArgb(150, 150, 150));
                return;
            }

            string text = m.Kind == "File"
                ? $"📄 {m.FileName}\r\n({FormatFileSize(m.FileSize ?? 0)})"
                : m.Content;

            Size textSize = TextRenderer.MeasureText(
                text, lstMessages.Font,
                new Size(BubbleMaxWidth - 2 * BubblePadding, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            int bubbleW = Math.Min(BubbleMaxWidth, textSize.Width + 2 * BubblePadding);
            int bubbleH = textSize.Height + 2 * BubblePadding + 14;
            if (m.ShowSenderName) bubbleH += 16;

            int listW = lstMessages.ClientSize.Width;
            int x = m.IsOwn ? (listW - bubbleW - 12) : 12;
            int y = e.Bounds.Top + 4;

            var rect = new Rectangle(x, y, bubbleW, bubbleH);

            using (var path = RoundedRect(rect, BubbleRadius))
            using (var fill = new SolidBrush(m.IsOwn ? Theme.Accent : Color.FromArgb(62, 62, 66)))
            {
                g.FillPath(fill, path);
            }

            int textY = y + BubblePadding - 2;

            if (m.ShowSenderName)
            {
                using var nameFont = new Font(lstMessages.Font.FontFamily, 8f, FontStyle.Bold);
                TextRenderer.DrawText(g, m.SenderName, nameFont,
                    new Point(x + BubblePadding, textY),
                    Color.FromArgb(180, 220, 255));
                textY += 16;
            }

            TextRenderer.DrawText(g, text, lstMessages.Font,
                new Rectangle(x + BubblePadding, textY,
                              bubbleW - 2 * BubblePadding, textSize.Height),
                Color.White,
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            using var timeFont = new Font(lstMessages.Font.FontFamily, 7f);
            string timeStr = m.SentAt.ToLocalTime().ToString("HH:mm");
            Size timeSize = TextRenderer.MeasureText(timeStr, timeFont);
            TextRenderer.DrawText(g, timeStr, timeFont,
                new Point(x + bubbleW - timeSize.Width - 8, y + bubbleH - 14),
                Color.FromArgb(210, 210, 210));
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (d <= 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024):F1} MB";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try { if (_tray != null) { _tray.Visible = false; _tray.Dispose(); } } catch { }
            _connection.MessageReceived -= OnMessageReceived;
            base.OnFormClosing(e);
        }

        private class ConvItem
        {
            public int Id { get; }
            public string Text { get; }
            public ConvItem(int id, string text) { Id = id; Text = text; }
            public override string ToString() => Text;
        }

        private class OnlineItem
        {
            public int Id { get; }
            public string Text { get; }
            public OnlineItem(int id, string text) { Id = id; Text = text; }
            public override string ToString() => Text;
        }

        private class FriendItem
        {
            public int Id { get; }
            public string Text { get; }
            public FriendItem(int id, string text) { Id = id; Text = text; }
            public override string ToString() => Text;
        }
        private class RequestItem
        {
            public int FriendshipId { get; }
            public string Text { get; }
            public RequestItem(int id, string text) { FriendshipId = id; Text = text; }
            public override string ToString() => Text;
            public bool LoggedOut { get; private set; }
        }
        private class MessageView
        {
            public int MessageId { get; set; }
            public int ConversationId { get; set; }
            public int SenderId { get; set; }
            public string SenderName { get; set; } = "";
            public string Content { get; set; } = "";
            public string Kind { get; set; } = "Text";
            public string? FileName { get; set; }
            public long? FileSize { get; set; }
            public DateTime SentAt { get; set; }
            public bool IsOwn { get; set; }
            public bool IsSystem { get; set; }
            public bool ShowSenderName { get; set; }
        }
    }
}