using System;
using System.Data.OleDb;

namespace tsenacsharp.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public int MoisDebut { get; set; }
        public int MoisFin { get; set; }
        public double Valeur { get; set; }

        public Discount(int id, int moisDebut, int moisFin, double valeur)
        {
            Id = id;
            MoisDebut = moisDebut;
            MoisFin = moisFin;
            Valeur = valeur;
        }

        public static double GetDiscountValue(int mois, OleDbConnection conn)
        {
            string query = @"SELECT valeur FROM Discount 
                             WHERE ? BETWEEN mois_debut AND mois_fin";

            OleDbCommand cmd = new OleDbCommand(query, conn);
            cmd.Parameters.AddWithValue("?", mois);

            try
            {
                var result = cmd.ExecuteScalar();

                return result != null ? Convert.ToDouble(result) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'exécution de la requête : " + ex.Message);
                return 0;
            }
        }
    }
}
