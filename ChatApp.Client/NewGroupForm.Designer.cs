namespace ChatApp.Client
{
    partial class NewGroupForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblFriends = new System.Windows.Forms.Label();
            this.clbFriends = new System.Windows.Forms.CheckedListBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // lblName
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(15, 15);
            this.lblName.Text = "Group Name:";
            // txtName
            this.txtName.Location = new System.Drawing.Point(15, 35);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(340, 23);
            // lblFriends
            this.lblFriends.AutoSize = true;
            this.lblFriends.Location = new System.Drawing.Point(15, 70);
            this.lblFriends.Text = "Select Friends:";
            // clbFriends
            this.clbFriends.FormattingEnabled = true;
            this.clbFriends.Location = new System.Drawing.Point(15, 90);
            this.clbFriends.Name = "clbFriends";
            this.clbFriends.Size = new System.Drawing.Size(340, 202);
            this.clbFriends.CheckOnClick = true;
            // btnCreate
            this.btnCreate.Location = new System.Drawing.Point(195, 305);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 28);
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(280, 305);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 28);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // NewGroupForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 380);
            this.MinimumSize = new System.Drawing.Size(400, 380);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.clbFriends);
            this.Controls.Add(this.lblFriends);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "NewGroupForm";
            this.Text = "New Group";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblFriends;
        private System.Windows.Forms.CheckedListBox clbFriends;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
    }
}