using System;
using System.Windows.Forms;
using PHARMA.Services;

namespace PHARMA.UI.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _auth = new AuthService();

        public LoginForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            DoLogin();
        }

        private void DoLogin()
        {
            var user = txtUser.Text.Trim();
            var pass = txtPass.Text;

            if (string.IsNullOrEmpty(user))
            {
                MessageBox.Show("Enter username.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUser.Focus();
                return;
            }

            try
            {
                if (_auth.Login(user, pass))
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPass.SelectAll();
                    txtPass.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error:\n" + ex.Message + "\n\nCheck ODBC connection string in App.config.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtUser.Focused) { txtPass.Focus(); e.Handled = true; }
                else if (txtPass.Focused) { DoLogin(); e.Handled = true; }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnCancel.PerformClick();
            }
        }
    }
}