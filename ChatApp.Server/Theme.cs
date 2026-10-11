using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Server
{
    public static class Theme
    {
        public static readonly Color WindowBg = Color.FromArgb(30, 30, 30);
        public static readonly Color PanelBg = Color.FromArgb(45, 45, 48);
        public static readonly Color InputBg = Color.FromArgb(55, 55, 58);
        public static readonly Color TextFg = Color.White;
        public static readonly Color Accent = Color.FromArgb(0, 122, 204);
        public static readonly Color Danger = Color.FromArgb(220, 60, 60);
        public static readonly Color Success = Color.FromArgb(60, 180, 90);
        public static readonly Color Warning = Color.FromArgb(230, 180, 40);

        public static readonly Font DefaultFont = new Font("Segoe UI", 9.5f);

        public static void Apply(Form form)
        {
            form.BackColor = WindowBg;
            form.ForeColor = TextFg;
            form.Font = DefaultFont;
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                switch (c)
                {
                    case TextBox tb:
                        tb.BackColor = InputBg;
                        tb.ForeColor = TextFg;
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        break;

                    case ListBox lb:
                        lb.BackColor = PanelBg;
                        lb.ForeColor = TextFg;
                        lb.BorderStyle = BorderStyle.FixedSingle;
                        break;

                    case Button btn:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.BackColor = Accent;
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 150, 230);
                        break;

                    case Label lbl:
                        lbl.ForeColor = TextFg;
                        lbl.BackColor = Color.Transparent;
                        break;
                }

                if (c.HasChildren) ApplyToControls(c.Controls);
            }
        }
    }
}