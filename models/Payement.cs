using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace tsenacsharp.Models
{
    public class Payement : BaseModel
    {
        public int Id { get; set; }
        public int IdBox { get; set; }
        public int IdTenant { get; set; }
        public double Montant { get; set; }
        public int Mois { get; set; }
        public int Annee { get; set; }
        public DateTime DatePayement { get; set; }

        public Payement(int id, int idBox, int idTenant, double montant, int mois, int annee, DateTime datePayement)
        {
            Id = id;
            IdBox = idBox;
            IdTenant = idTenant;
            Montant = montant;
            Mois = mois;
            Annee = annee;
            DatePayement = datePayement;
        }

        public static List<Payement> Filtrer(int? box, OleDbConnection conn)
        {
            string columns = "id, idBox, idTenant, montant, mois, annee, date_payement";
            string condition = box.HasValue ? "idBox = ?" : "";
            var parameters = new List<object>();

            if (box.HasValue)
            {
                parameters.Add(box.Value);
            }

            var result = BaseModel.ReadData(conn, "Payement", columns, condition, parameters);
            var payements = new List<Payement>();

            foreach (var row in result)
            {
                var payement = new Payement(
                    Convert.ToInt32(row[0]),
                    Convert.ToInt32(row[1]),
                    Convert.ToInt32(row[2]),
                    Convert.ToDouble(row[3]),
                    Convert.ToInt32(row[4]),
                    Convert.ToInt32(row[5]),
                    Convert.ToDateTime(row[6])
                );
                payements.Add(payement);
            }

            return payements;
        }
    }
}
