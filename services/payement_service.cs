using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using tsenacsharp.Models;

namespace tsenacsharp.Services
{
    public class PayementService : BaseModel
    {
        public static void InsererPayement(int idBox, int idTenant, double montant, int mois, int annee, DateTime datePayement, OleDbConnection conn)
        {
            var columns = new List<string> { "idBox", "idTenant", "montant", "mois", "annee", "date_payement" };
            var values = new List<object> { idBox, idTenant, montant, mois, annee, datePayement };
            BaseModel.InsertData(conn, "Payement", columns, values);
        }

        public static void ModifierPayement(int idBox, double newMontant, int mois, int annee, DateTime datePayement, OleDbConnection conn)
        {
            var setValues = new Dictionary<string, object> { { "montant", newMontant }, { "date_payement", datePayement } };
            string condition = "idBox = ? AND mois = ? AND annee = ?";
            var parameters = new List<object> { idBox, mois, annee };
            BaseModel.UpdateData(conn, "Payement", setValues, condition, parameters);
        }

        public static Dictionary<int, List<Dictionary<string, DateTime?>>> GetLocationsByTenant(OleDbConnection conn, int idTenant)
        {
            var results = Location.GetLocationByTenantId(conn, idTenant);
            var activeBoxes = new Dictionary<int, List<Dictionary<string, DateTime?>>>();

            foreach (var (idBox, dateAction, typeAction) in results) // Déstructuration du tuple
            {
                if (!activeBoxes.ContainsKey(idBox))
                {
                    activeBoxes[idBox] = new List<Dictionary<string, DateTime?>>();
                }

                if (typeAction == 0) // Type "start"
                {
                    activeBoxes[idBox].Add(new Dictionary<string, DateTime?> { { "start", dateAction }, { "end", null } });
                }
                else if (typeAction == 1) // Type "end"
                {
                    foreach (var period in activeBoxes[idBox])
                    {
                        if (!period["end"].HasValue)
                        {
                            period["end"] = dateAction;
                            break;
                        }
                    }
                }
            }

            return activeBoxes;
        }

        public static DateTime? GetStarting(OleDbConnection conn, int idTenant)
        {
            var locations = GetLocationsByTenant(conn, idTenant);

            if (locations.Count == 0)
                return null;

            var startDates = locations.Values.SelectMany(periods => periods).Select(period => period["start"]).Where(start => start.HasValue).ToList();

            if (startDates.Count == 0)
                return null;

            return startDates.Min();
        }

        public static bool IsBoxActive(List<Dictionary<string, DateTime?>> box, int mois, int annee)
        {
            var currentYearMonth = new Tuple<int, int>(annee, mois); // Ignore le jour et l'heure

            foreach (var period in box)
            {
                var start = period["start"].Value;
                var startYearMonth = new Tuple<int, int>(start.Year, start.Month);

                Tuple<int, int>? endYearMonth = null;
                if (period["end"] != null)
                {
                    var end = period["end"].Value;
                    endYearMonth = new Tuple<int, int>(end.Year, end.Month);
                }

                // Vérification de la période
                if ((startYearMonth.Item1 < currentYearMonth.Item1) ||
                    (startYearMonth.Item1 == currentYearMonth.Item1 && startYearMonth.Item2 <= currentYearMonth.Item2))
                {
                    if (endYearMonth == null ||
                        (endYearMonth.Item1 > currentYearMonth.Item1) ||
                        (endYearMonth.Item1 == currentYearMonth.Item1 && endYearMonth.Item2 >= currentYearMonth.Item2))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void PayerLocations(int idBoxCible, int idTenant, double montant, int mois, int annee, DateTime datePayement, OleDbConnection conn)
        {
            var locations = GetLocationsByTenant(conn, idTenant);
            var dateStarting = GetStarting(conn, idTenant);
            if (!dateStarting.HasValue) return;

            int moisParcours = dateStarting.Value.Month;
            int anneeParcours = dateStarting.Value.Year;


            while (montant > 0)
            {
                Console.WriteLine("DateActuelle:" + moisParcours + "/" + anneeParcours);
                var boxAPayer = new List<int>();
                foreach (var entry in locations)
                {
                    foreach (var period in entry.Value)
                    {
                        if (IsBoxActive(new List<Dictionary<string, DateTime?>> { period }, moisParcours, anneeParcours))
                        {
                            boxAPayer.Add(entry.Key);
                            break;
                        }
                    }
                }

                // Afficher boxAPayer avant le tri
                Console.WriteLine("Box à payer avant le tri :");
                foreach (var idBox in boxAPayer)
                {
                    Console.WriteLine($"ID Box: {idBox}, Référence: {Box.GetReferenceById(conn, idBox)}");
                }

                // Trier boxAPayer par référence croissante
                boxAPayer.Sort((idBox1, idBox2) =>
                {
                    int reference1 = Box.GetReferenceById(conn, idBox1);
                    int reference2 = Box.GetReferenceById(conn, idBox2);

                    // Lever une exception si une référence est négative
                    if (reference1 < 0 || reference2 < 0)
                    {
                        throw new InvalidOperationException("Une référence de box est négative.");
                    }

                    return reference1.CompareTo(reference2);
                });

                // Afficher boxAPayer après le tri
                Console.WriteLine("Box à payer après le tri :");
                foreach (var idBox in boxAPayer)
                {
                    Console.WriteLine($"ID Box: {idBox}, Référence: {Box.GetReferenceById(conn, idBox)}");
                }

                foreach (var idBox in boxAPayer)
                {
                    montant = PayerBox(conn, idBox, idTenant, montant, moisParcours, anneeParcours, datePayement);
                    if (montant <= 0)
                        return;
                }

                var nextMonth = BaseModel.NextMonth(moisParcours, anneeParcours);
                moisParcours = nextMonth.Item1;
                anneeParcours = nextMonth.Item2;
            }
        }
        public static double PayerBox(OleDbConnection conn, int idBox, int idTenant, double montant, int mois, int annee, DateTime datePayement)
        {
            var box = Box.GetBoxById(conn, idBox);
            double prix = box.GetPrix(mois, annee, conn);
            double montantPaye = box.GetValueAtDate(conn, mois, annee, datePayement);
            double resteAPayer = Math.Max(0, prix - montantPaye);

            if (resteAPayer > 0)
            {
                double paiementEffectue = Math.Min(montant, resteAPayer);
                InsererPayement(idBox, idTenant, paiementEffectue, mois, annee, datePayement, conn);
                montant -= paiementEffectue;
            }

            return montant;
        }


        public static double CalculerDette(int idTenant, int mois, int annee, OleDbConnection conn)
        {
            var locations = GetLocationsByTenant(conn, idTenant);
            var dateStart = GetStarting(conn, idTenant);
            if (!dateStart.HasValue)
                return 0;

            int moisParcours = dateStart.Value.Month;
            int anneeParcours = dateStart.Value.Year;
            double totalPaye = 0;
            double totalAPayer = 0;

            DateTime dateVerif = new DateTime(annee,mois, 28);

            while ((anneeParcours < annee) || (anneeParcours == annee && moisParcours <= mois))
            {
                foreach (var entry in locations)
                {
                    foreach (var period in entry.Value)
                    {
                        if (IsBoxActive(new List<Dictionary<string, DateTime?>> { period }, moisParcours, anneeParcours))
                        {
                            Console.WriteLine("idboxxx:" + entry.Key);
                            var box = Box.GetBoxById(conn, entry.Key);
                            totalAPayer += box.GetPrix(moisParcours, anneeParcours, conn);
                            totalPaye += box.GetValueAtDate(conn, moisParcours, anneeParcours, dateVerif);
                        }
                    }
                }

                if (moisParcours == mois && anneeParcours == annee)
                    break;

                var nextMonth = BaseModel.NextMonth(moisParcours, anneeParcours);
                moisParcours = nextMonth.Item1;
                anneeParcours = nextMonth.Item2;
            }

            return Math.Max(0, totalAPayer - totalPaye);
        }
    }
}
