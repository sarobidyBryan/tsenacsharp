using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace tsenacsharp.Models
{
    public class Market : BaseModel
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public double Longueur { get; set; }
        public double Largeur { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Market(int id, string nom, double longueur, double largeur, double x, double y)
        {
            Id = id;
            Nom = nom;
            Longueur = longueur;
            Largeur = largeur;
            X = x;
            Y = y;
        }

        public static List<Market> GetAllMarket(OleDbConnection conn)
        {
            List<object[]> marketsData = BaseModel.ReadData(conn, "Market", "*", null, new List<object>());
            List<Market> result = new List<Market>();

            foreach (var market in marketsData)
            {
                result.Add(new Market(
                    Convert.ToInt32(market[0]),   // id
                    market[1].ToString(),         // nom
                    Convert.ToDouble(market[2]),  // longueur
                    Convert.ToDouble(market[3]),  // largeur
                    Convert.ToDouble(market[4]),  // x
                    Convert.ToDouble(market[5])   // y
                ));
            }

            return result;
        }

        public static Market GetMarketById(OleDbConnection conn, int id)
        {
            string query = "SELECT * FROM Market WHERE id = ?";
            using (OleDbCommand command = new OleDbCommand(query, conn))
            {
                command.Parameters.AddWithValue("?", id);
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Market(
                            reader.GetInt32(0),   // id
                            reader.GetString(1),  // nom
                            reader.GetInt32(2),  // longueur
                            reader.GetInt32(3),  // largeur
                            reader.GetInt32(4),  // x
                            reader.GetInt32(5)   // y
                        );
                    }
                }
            }
            return null;
        }

        public double GetPrixMarket(OleDbConnection conn)
        {
            string condition = "idMarket = ?";
            List<object> parameters = new List<object> { Id };
            List<object[]> result = BaseModel.ReadData(conn, "Market_Box", "prixBase", condition, parameters);

            return result.Count > 0 ? Convert.ToDouble(result[0][0]) : 0.0;
        }
    }
}
