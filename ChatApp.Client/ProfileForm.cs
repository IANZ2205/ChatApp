using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ChatApp.Client.Network;
using ChatApp.Common.Protocol;

namespace ChatApp.Client
{
    public partial class ProfileForm : Form
    {
        private readonly ServerConnection _connection;
        private readonly int _targetUserId;
        private readonly bool _editable;
        private string? _newPhotoBase64;

        public ProfileForm(ServerConnection connection, int targetUserId, bool editable)
        {
            InitializeComponent();
            _connection = connection;
            _targetUserId = targetUserId;
            _editable = editable;

            btnSave.Visible = editable;
            btnPickPhoto.Visible = editable;

            _connection.MessageReceived += OnConnectionMessage;
            Text = $"Profile (id={targetUserId})";

            _connection.Send(Envelope.Create(MessageTypes.GetProfile,
                new GetProfileRequest { UserId = targetUserId }));
        }

        private void OnConnectionMessage(Envelope envelope)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action<Envelope>(HandleEnvelope), envelope);
            }
            catch { }
        }

        private void HandleEnvelope(Envelope envelope)
        {
            if (envelope.Type != MessageTypes.ProfileData) return;
            var p = envelope.ReadData<ProfileData>();
            if (p == null) return;
            if (_targetUserId > 0 && p.UserId > 0 && p.UserId != _targetUserId) return;

            if (!p.Success)
            {
                MessageBox.Show(p.FullName, "Profile");
                ReEnableSave();
                return;
            }

            txtUsername.Text = p.Username;
            txtFullName.Text = p.FullName;
            txtBio.Text = p.Bio;
            Text = $"Profile - {p.Username}";

            if (!string.IsNullOrEmpty(p.PhotoBase64))
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(p.PhotoBase64);
                    using var ms = new MemoryStream(bytes);
                    using var temp = Image.FromStream(ms);
                    picPhoto.Image = new Bitmap(temp);
                }
                catch { picPhoto.Image = null; }
            }

            ReEnableSave();
        }

        private void ReEnableSave()
        {
            if (!_editable) return;
            btnSave.Text = "Save";
            btnSave.Enabled = true;
        }

        private void btnPickPhoto_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                byte[] bytes = File.ReadAllBytes(dlg.FileName);
                if (bytes.Length > 2 * 1024 * 1024) { MessageBox.Show("Photo must be under 2 MB."); return; }
                _newPhotoBase64 = Convert.ToBase64String(bytes);
                using var ms = new MemoryStream(bytes);
                using var temp = Image.FromStream(ms);
                picPhoto.Image = new Bitmap(temp);
            }
            catch (Exception ex) { MessageBox.Show($"Could not load photo: {ex.Message}"); }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!_connection.IsConnected) { MessageBox.Show("Not connected."); return; }
            btnSave.Text = "Saving...";
            btnSave.Enabled = false;
            _connection.Send(Envelope.Create(MessageTypes.UpdateProfile, new UpdateProfileRequest
            {
                FullName = txtFullName.Text.Trim(),
                Bio = txtBio.Text.Trim(),
                PhotoBase64 = _newPhotoBase64
            }));
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _connection.MessageReceived -= OnConnectionMessage;
            base.OnFormClosing(e);
        }
    }
}