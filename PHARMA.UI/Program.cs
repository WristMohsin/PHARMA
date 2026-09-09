using System;
using System.Windows.Forms;
using PHARMA.UI.Forms;

namespace PHARMA.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainMdiForm());
                }
            }
        }
    }
}