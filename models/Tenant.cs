using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace tsenacsharp.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Nom { get; set; }

        public Tenant(int id, string nom)
        {
            Id = id;
            Nom = nom;
        }

        public static List<Tenant> GetTenants(OleDbConnection conn)
        {
            List<object[]> tenantsData = BaseModel.ReadData(conn, "Tenant", "*", null, new List<object>());
            List<Tenant> result = new List<Tenant>();

            foreach (var tenant in tenantsData)
            {
                result.Add(new Tenant(
                    Convert.ToInt32(tenant[0]),   // id
                    tenant[1].ToString()          // nom
                ));
            }

            return result;
        }

        public static Tenant GetTenantById(OleDbConnection conn, int id)
        {
            string condition = "id = ?";
            List<object> parameters = new List<object> { id };
            List<object[]> tenantData = BaseModel.ReadData(conn, "Tenant", "*", condition, parameters);

            if (tenantData.Count > 0)
            {
                return new Tenant(
                    Convert.ToInt32(tenantData[0][0]),  // id
                    tenantData[0][1].ToString()         // nom
                );
            }

            return null;
        }

        public List<Box> GetBoxes(OleDbConnection conn)
        {
            List<Box> boxes = new List<Box>();
            string condition = "idTenant = ?";
            List<object> parameters = new List<object> { this.Id };

            List<object[]> idBoxesData = BaseModel.ReadData(conn, "Location", "idBox", condition, parameters);
            HashSet<int> uniqueBoxIds = new HashSet<int>();

            foreach (var idBox in idBoxesData)
            {
                uniqueBoxIds.Add(Convert.ToInt32(idBox[0]));
            }

            foreach (int boxId in uniqueBoxIds)
            {
                Box box = Box.GetBoxById(conn, boxId);
                if (box != null)
                {
                    boxes.Add(box);
                }
            }

            return boxes;
        }
    }
}
