using System;
using System.Collections.Generic;
using System.Data.OleDb; // ✅ Pour OleDbConnection
using tsenacsharp.Models; // ✅ Pour MarketBox (si elle est bien dans tsenacsharp.Models)

namespace tsenacsharp.Services
{
    public class SituationService
    {
        public static List<MarketBox> GetSituation(OleDbConnection conn)
        {
            var markets = Market.GetAllMarket(conn); // Récupère tous les marchés
            var result = new List<MarketBox>();
            foreach (var market in markets)
            {
                var boxes = MarketBox.GetBoxesByMarket(conn, market.Id); // Récupère les boîtes associées au marché
                var marketBox = new MarketBox(market, boxes); // Crée un objet MarketBox avec le marché et ses boîtes
                result.Add(marketBox); // Ajoute à la liste des résultats
            }

            return result; // Retourne la liste des MarketBox
        }
    }
}
