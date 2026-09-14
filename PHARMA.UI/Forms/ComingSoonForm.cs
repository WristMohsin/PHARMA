using System;
using System.Drawing;
using System.Windows.Forms;

namespace PHARMA.UI.Forms
{
    /// <summary>
    /// Lightweight placeholder for modules planned for a later phase.
    /// </summary>
    public class ComingSoonForm : Form
    {
        public ComingSoonForm(string moduleKey, string moduleTitle)
        {
            Text = "Coming Soon";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 180);
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Microsoft Sans Serif", 9F);
            ShowInTaskbar = false;

            string title = string.IsNullOrEmpty(moduleTitle) ? moduleKey : moduleTitle;
            string key = string.IsNullOrEmpty(moduleKey) ? "" : moduleKey;

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(380, 24)
            };
            var lblBody = new Label
            {
                Text = "This module is planned for a later phase." +
                       (string.IsNullOrEmpty(key) ? "" : ("\n\nModule key: " + key)),
                Location = new Point(20, 55),
                Size = new Size(380, 60)
            };
            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(320, 130),
                Size = new Size(80, 28)
            };
            AcceptButton = btnOk;
            Controls.Add(lblTitle);
            Controls.Add(lblBody);
            Controls.Add(btnOk);
        }

        public static void ShowFor(IWin32Window owner, string moduleKey, string moduleTitle)
        {
            using (var f = new ComingSoonForm(moduleKey, moduleTitle))
            {
                if (owner != null)
                    f.ShowDialog(owner);
                else
                    f.ShowDialog();
            }
        }
    }
}
