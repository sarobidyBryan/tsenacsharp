using System;
using System.Data.OleDb;

namespace tsenacsharp.DB
{
    internal class AccessDB
    {
        private static readonly string dbPath = @"D:\ITU\prog\tsenacsharp\db\tsebaDB.accdb";
        private static readonly string connectionString =
            $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";

        public static OleDbConnection GetConnection()
        {
            try
            {
                OleDbConnection conn = new OleDbConnection(connectionString);
                conn.Open();
                Console.WriteLine("Connexion réussie à la base de données Access.");
                return conn;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erreur de connexion : {e.Message}");
                return null; // Retourne null en cas d'échec
            }
        }
    }
}
