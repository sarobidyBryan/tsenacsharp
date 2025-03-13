using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using tsenacsharp.models;

namespace tsenacsharp.Models
{
    internal class Box : BaseModel
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public double Longueur { get; set; }
        public double Largeur { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Box(int id, string reference, double longueur, double largeur, double x, double y)
        {
            Id = id;
            Reference = reference;
            Longueur = longueur;
            Largeur = largeur;
            X = x;
            Y = y;
        }

        public static List<Box> GetAllBoxes(OleDbConnection conn)
        {
            var result = BaseModel.ReadData(conn, "Box", "*", null, new List<object>());
            var boxes = new List<Box>();
            foreach (var row in result)
            {
                boxes.Add(new Box(
                    Convert.ToInt32(row[0]),
                    row[1].ToString(),
                    Convert.ToDouble(row[2]),
                    Convert.ToDouble(row[3]),
                    Convert.ToDouble(row[4]),
                    Convert.ToDouble(row[5])
                ));
            }
            return boxes;
        }

        public static Box GetBoxById(OleDbConnection conn, int id)
        {
            string query = "SELECT * FROM Box WHERE id = ?";
            var parameters = new List<object> { id };
            var result = BaseModel.ReadData(conn, query, "*", "id = ?", parameters);
            if (result.Count > 0)
            {
                return new Box(
                    Convert.ToInt32(result[0][0]),
                    result[0][1].ToString(),
                    Convert.ToDouble(result[0][2]),
                    Convert.ToDouble(result[0][3]),
                    Convert.ToDouble(result[0][4]),
                    Convert.ToDouble(result[0][5])
                );
            }
            return null;
        }

        public static int? GetReferenceById(OleDbConnection conn, int id)
        {
            string query = "SELECT reference FROM Box WHERE id = ?";
            var parameters = new List<object> { id };
            var result = BaseModel.ReadData(conn, query, "reference", "id = ?", parameters);
            return result.Count > 0 ? (int?)Convert.ToInt32(result[0][0]) : null;
        }

        public bool IsPayed(OleDbConnection conn, int mois, int annee)
        {
            string condition = "idBox = ? and mois = ? and annee = ?";
            var paramsList = new List<object> { this.Id, mois, annee };
            var result = BaseModel.ReadData(conn, "Payement", "montant", condition, paramsList);
            if (result.Count == 1 && Convert.ToDouble(result[0][0]) == GetPrix(mois, annee, conn))
            {
                return true;
            }
            return false;
        }

        public Market GetMarket(OleDbConnection conn)
        {
            string condition = "idBox = ?";
            var paramsList = new List<object> { this.Id };
            var marketId = BaseModel.ReadData(conn, "Market_Box", "idMarket", condition, paramsList)[0][0];
            return Market.GetMarketById(conn, Convert.ToInt32(marketId));
        }

        public double GetPrix(int mois, int annee, OleDbConnection conn)
        {
            double surface = Longueur * Largeur;
            Market market = GetMarket(conn);
            var date = new DateTime(annee, mois, 28);
            HistoPrix histo = HistoPrix.GetHistoByDate(conn, market.Id, date);
            double prixBase = (double)histo.NouveauPrix;
            double discount = Discount.GetDiscountValue(mois, conn);
            double prix = prixBase + (prixBase * discount / 100);
            return prix * surface;
        }

        public Payement GetLastInsert(OleDbConnection conn, int idTenant)
        {
            string condition = "idBox = ? ORDER BY id DESC";
            var paramsList = new List<object> { this.Id };
            var result = BaseModel.ReadData(conn, "Payement", "id, idBox, idTenant, montant, mois, annee, date_payement", condition, paramsList);

            if (result.Count >= 1)
            {
                var row = result[0];
                return new Payement(
                    Convert.ToInt32(row[0]),
                    Convert.ToInt32(row[1]),
                    Convert.ToInt32(row[2]),
                    Convert.ToDouble(row[3]),
                    Convert.ToInt32(row[4]),
                    Convert.ToInt32(row[5]),
                    Convert.ToDateTime(row[6])
                );
            }
            else
            {
                Location location = Location.GetLocationByIdBoxIdTenant(conn, this.Id, idTenant);
                DateTime date = location.DateAction;
                return new Payement(0, this.Id, idTenant, 0, date.Month, date.Year, new DateTime(date.Year, date.Month, 28));
            }
        }

        public Dictionary<string, object> GetCurrentValues(OleDbConnection conn, int idTenant)
        {
            Payement payement = GetLastInsert(conn, idTenant);
            double prix = GetPrix(payement.Mois, payement.Annee, conn);
            if (payement.Montant == prix)
            {
                var nextMonth = BaseModel.NextMonth(payement.Mois, payement.Annee);
                return new Dictionary<string, object> { { "mois", nextMonth.Item1 }, { "annee", nextMonth.Item2 }, { "montant", 0 } };
            }
            return new Dictionary<string, object> { { "mois", payement.Mois }, { "annee", payement.Annee }, { "montant", payement.Montant } };
        }

        public double GetValueAtDate(int mois, int annee, OleDbConnection conn)
        {
            string condition = "idBox = ? and mois = ? and annee = ?";
            var paramsList = new List<object> { this.Id, mois, annee };
            var result = BaseModel.ReadData(conn, "Payement", "montant", condition, paramsList);
            return result.Count == 1 ? Convert.ToDouble(result[0][0]) : 0;
        }

        public bool EstLibre(OleDbConnection conn, int mois, int annee)
        {
            DateTime dateVerif = new DateTime(annee, mois, 1);
            string query = "SELECT date_action, type_action FROM Location WHERE idBox = ? ORDER BY date_action ASC";
            var results = BaseModel.ReadData(conn, query, "date_action, type_action", "idBox = ?", new List<object> { this.Id });

            List<Tuple<DateTime, DateTime?>> contrats = new List<Tuple<DateTime, DateTime?>>();
            DateTime? debutLocation = null;

            foreach (var result in results)
            {
                DateTime dateAction = Convert.ToDateTime(result[0]);
                if (Convert.ToInt32(result[1]) == 0)
                {
                    debutLocation = dateAction;
                }
                else if (Convert.ToInt32(result[1]) == 1 && debutLocation.HasValue)
                {
                    contrats.Add(Tuple.Create(debutLocation.Value, (DateTime?)dateAction));

                    debutLocation = null;
                }
            }

            if (debutLocation.HasValue)
            {
                contrats.Add(Tuple.Create(debutLocation.Value, (DateTime?)null));
            }

            foreach (var contrat in contrats)
            {
                if (contrat.Item1 <= dateVerif && (!contrat.Item2.HasValue || dateVerif <= contrat.Item2.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public Tenant GetLocataire(OleDbConnection conn, int mois, int annee)
        {
            DateTime dateVerif = new DateTime(annee, mois, 1);
            string query = "SELECT idTenant, date_action, type_action FROM Location WHERE idBox = ? ORDER BY date_action ASC";
            var results = BaseModel.ReadData(conn, query, "idTenant, date_action, type_action", "idBox = ?", new List<object> { this.Id });

            Tenant locataireActuel = null;
            DateTime? debutLocation = null;

            foreach (var result in results)
            {
                DateTime dateAction = Convert.ToDateTime(result[1]);
                int idTenantTmp = Convert.ToInt32(result[0]);

                if (Convert.ToInt32(result[2]) == 0)
                {
                    debutLocation = dateAction;
                    locataireActuel = Tenant.GetTenantById(conn, idTenantTmp);
                }
                else if (Convert.ToInt32(result[2]) == 1 && debutLocation.HasValue)
                {
                    debutLocation = null;
                    locataireActuel = null;
                }
            }

            return locataireActuel;
        }
    }
}
