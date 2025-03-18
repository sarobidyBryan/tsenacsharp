using System;
using System.Data.OleDb;
using System.Windows.Forms;
using tsenacsharp.DB;

namespace tsenacsharp.Views
{
    public partial class ListePayements : UserControl
    {
        private OleDbConnection _dbConnection;

        public ListePayements(OleDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            //InitializeComponent();
        }

        private void ListePayements_Load(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT * FROM Payments";
                OleDbCommand command = new OleDbCommand(query, _dbConnection);
                OleDbDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Traiter les données récupérées ici et remplir la table
                    Console.WriteLine(reader["Amount"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération des paiements : {ex.Message}");
            }
        }
    }
}
