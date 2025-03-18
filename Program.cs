using System;
using System.Data.OleDb;
using System.Windows.Forms;
using tsenacsharp.DB;

namespace tsenacsharp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            OleDbConnection dbConnection = AccessDB.GetConnection();

            if (dbConnection != null)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(dbConnection));
            }
            else
            {
                MessageBox.Show("Impossible de se connecter à la base de données.");
            }
        }
    }
}
