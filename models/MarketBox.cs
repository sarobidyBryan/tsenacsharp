using System;
using System.Collections.Generic;
using System.Data.OleDb;


namespace tsenacsharp.Models
{
    public class MarketBox
    {
        public Market Market { get; set; }
        public List<Box> Boxes { get; set; }

        public MarketBox(Market market, List<Box> boxes)
        {
            Market = market;
            Boxes = boxes;
        }

        public static List<Box> GetBoxesByMarket(OleDbConnection conn, int idMarket)
        {
            string condition = "idMarket = ?";
            List<object> parameters = new List<object> { idMarket };

            var marketBoxData = BaseModel.ReadData(conn, "Market_Box", "*", condition, parameters);
            List<Box> boxObjects = new List<Box>();
            
            foreach (var mb in marketBoxData)
            {
                Box box = Box.GetBoxById(conn, Convert.ToInt32(mb[2]));
                Console.WriteLine(Convert.ToInt32(mb[2]));
                if (box != null)
                {
                    boxObjects.Add(box);
                }
            }

            return boxObjects;
        }
    }
}
