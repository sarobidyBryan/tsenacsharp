using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace tsenacsharp.Models
{
    internal class HistoPrix:BaseModel
    {
        public int Id { get; set; }
        public int IdMarket { get; set; }
        public decimal PrixActuel { get; set; }
        public decimal NouveauPrix { get; set; }
        public DateTime DateChangement { get; set; }

        public HistoPrix(int id, int idMarket, decimal prixActuel, decimal nouveauPrix, DateTime dateChangement)
        {
            Id = id;
            IdMarket = idMarket;
            PrixActuel = prixActuel;
            NouveauPrix = nouveauPrix;
            DateChangement = dateChangement;
        }

        public static List<HistoPrix> GetHistoPrix(OleDbConnection conn, int idMarket)
        {
            string condition = "idMarket = ?";
            List<object> parameters = new List<object> { idMarket };
            List<object[]> histoPrixData = BaseModel.ReadData(conn, "Histo_Prix", "*", condition, parameters);

            List<HistoPrix> result = new List<HistoPrix>();
            foreach (var h in histoPrixData)
            {
                result.Add(new HistoPrix(
                    Convert.ToInt32(h[0]),
                    Convert.ToInt32(h[1]),
                    Convert.ToDecimal(h[2]),
                    Convert.ToDecimal(h[3]),
                    Convert.ToDateTime(h[4])
                ));
            }
            return result;
        }

        public static HistoPrix GetHistoByDate(OleDbConnection conn, int idMarket, DateTime date)
        {
            string condition = "idMarket = ? AND date_changement <= ?";
            List<object> parameters = new List<object> { idMarket, date };
            List<object[]> histoPrixData = BaseModel.ReadData(conn, "Histo_Prix", "*", condition, parameters);

            if (histoPrixData.Count > 0)
            {
                var hp = histoPrixData[histoPrixData.Count - 1]; // Dernier élément de la liste
                return new HistoPrix(
                    Convert.ToInt32(hp[0]),
                    Convert.ToInt32(hp[1]),
                    Convert.ToDecimal(hp[2]),
                    Convert.ToDecimal(hp[3]),
                    Convert.ToDateTime(hp[4])
                );
            }
            return null;
        }
    }
}
