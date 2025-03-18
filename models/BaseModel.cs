using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace tsenacsharp.Models
{
    public class BaseModel
    {
        public static (int, int) NextMonth(int mois, int annee)
        {
            return mois == 12 ? (1, annee + 1) : (mois + 1, annee);
        }

        public static void InsertData(OleDbConnection conn, string table, List<string> columns, List<object> values)
        {
            string columnsStr = string.Join(", ", columns);
            string placeholders = string.Join(", ", new string[values.Count].Select(_ => "?"));

            string sql = $"INSERT INTO {table} ({columnsStr}) VALUES ({placeholders})";

            try
            {
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@p{i}", values[i]);
                    }
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Données insérées avec succès dans {table}.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erreur d'insertion : {e.Message}");
            }
        }

        public static List<object[]> ReadData(OleDbConnection conn, string table, string columns = "*", string condition = null, List<object> parameters = null)
        {
            List<object[]> results = new List<object[]>();
            string sql = $"SELECT {columns} FROM {table}";
            if (condition != null)
            {
                sql += $" WHERE {condition}";
            }
            //Console.WriteLine(sql);
            try
            {
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            cmd.Parameters.AddWithValue($"@p{i}", parameters[i]);
                        }
                    }

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object[] row = new object[reader.FieldCount];
                            reader.GetValues(row);
                            results.Add(row);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erreur de lecture : {e.Message}");
            }
            return results;
        }

        public static void UpdateData(OleDbConnection conn, string table, Dictionary<string, object> setValues, string condition, List<object> conditionParams)
        {
            string setStr = string.Join(", ", setValues.Keys.Select(k => $"{k} = ?"));
            string sql = $"UPDATE {table} SET {setStr} WHERE {condition}";

            try
            {
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    foreach (var value in setValues.Values)
                    {
                        cmd.Parameters.AddWithValue("?", value);
                    }

                    foreach (var param in conditionParams)
                    {
                        cmd.Parameters.AddWithValue("?", param);
                    }

                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Enregistrements mis à jour avec succès dans {table}.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erreur de mise à jour : {e.Message}");
            }
        }

        public static void DeleteData(OleDbConnection conn, string table, string condition)
        {
            string sql = $"DELETE FROM {table} WHERE {condition}";

            try
            {
                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Enregistrements supprimés avec succès de {table}.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erreur de suppression : {e.Message}");
            }
        }
    }
}
