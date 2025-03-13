using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace tsenacsharp.Models
{
    internal class Location
    {
        public int Id { get; set; }
        public int IdBox { get; set; }
        public int IdTenant { get; set; }
        public DateTime DateAction { get; set; }
        public int TypeAction { get; set; }

        public Location(int id, int idBox, int idTenant, DateTime dateAction, int typeAction)
        {
            Id = id;
            IdBox = idBox;
            IdTenant = idTenant;
            DateAction = dateAction;
            TypeAction = typeAction;
        }

        public static List<(int idBox, DateTime dateAction, int typeAction)> GetLocationByTenantId(OleDbConnection conn, int tenantId)
        {
            string condition = "idTenant = ?";
            List<object> parameters = new List<object> { tenantId };
            List<object[]> results = BaseModel.ReadData(conn, "Location", "idBox, date_action, type_action", condition, parameters);

            List<(int, DateTime, int)> locations = new List<(int, DateTime, int)>();
            foreach (var row in results)
            {
                locations.Add((Convert.ToInt32(row[0]), Convert.ToDateTime(row[1]), Convert.ToInt32(row[2])));
            }
            return locations;
        }

        public static Location GetLocationByIdBoxIdTenant(OleDbConnection conn, int idBox, int idTenant)
        {
            string condition = "idBox = ? AND idTenant = ? AND type_action = ?";
            List<object> parameters = new List<object> { idBox, idTenant, 0 };
            List<object[]> results = BaseModel.ReadData(conn, "Location", "id, date_action, type_action", condition, parameters);

            if (results.Count > 0)
            {
                return new Location(
                    Convert.ToInt32(results[0][0]),
                    idBox,
                    idTenant,
                    Convert.ToDateTime(results[0][1]),
                    Convert.ToInt32(results[0][2])
                );
            }
            return null;
        }
    }
}
