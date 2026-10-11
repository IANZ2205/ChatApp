using ChatApp.Common.Protocol;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ChatApp.Client
{
    public partial class NewGroupForm : Form
    {
        public string GroupName => txtName.Text.Trim();
        public List<int> SelectedUserIds { get; } = new();

        public NewGroupForm(List<FriendInfo> friends)
        {
            InitializeComponent();
            Theme.Apply(this);
            foreach (var f in friends)
                clbFriends.Items.Add(new FriendEntry(f.UserId, f.Username), false);
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a group name.");
                return;
            }
            SelectedUserIds.Clear();
            foreach (var item in clbFriends.CheckedItems)
                if (item is FriendEntry fe) SelectedUserIds.Add(fe.UserId);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private class FriendEntry
        {
            public int UserId { get; }
            public string Username { get; }
            public FriendEntry(int id, string name) { UserId = id; Username = name; }
            public override string ToString() => Username;
        }
    }
}