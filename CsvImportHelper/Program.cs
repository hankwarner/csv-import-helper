using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvImportHelper.Models;
using Dapper;

namespace CsvImportHelper
{
    class Program
    {
        public const string basePath = @"C:\Users\h.warner\Desktop\csv to upload\";
        public static List<Model> records { get; set; } = new List<Model>();
        public static string connString = "Data Source=srv-pro-sqls-02;Initial Catalog=FergusonIntegration;Integrated Security=true";


        static void Main()
        {
            // pull csv from folder
            var sourceDirectory = new DirectoryInfo(basePath);

            var allFiles = sourceDirectory.GetFiles("*.csv").Where(f => f.Name.EndsWith(".csv"));

            // map to object
            foreach (var file in allFiles)
            {
                using (var reader = new StreamReader(basePath + file.Name))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    records.AddRange(csv.GetRecords<Model>());
                }
            }

            // Return if no new file to process
            if (!records.Any())
            {
                return;
            }

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();

                foreach (var record in records)
                {
                    var mpn = record.MPN;
                    var bulkPack = record.BulkPack;
                    var bulkPackQty = record.BulkPackQuantity;

                    try
                    {
                        //var query = @"
                        //    SELECT ALT1Code 
                        //    FROM [FergusonIntegration].[ferguson].[Items]
                        //    WHERE MPN = @mpn";

                        //var res = conn.QueryFirstOrDefault<string>(query, new {  mpn });

                        //if (!string.IsNullOrEmpty(res)) continue;

                        var query = @"
                            UPDATE [FergusonIntegration].[ferguson].[Items]
                            SET [BulkPack] = @bulkPack, [BulkPackQuantity] = @bulkPackQty
                            WHERE MPN = @mpn";

                        conn.Execute(query, new { bulkPack, bulkPackQty, mpn }, commandTimeout: 6);
                        Console.WriteLine($"MPN {mpn} updated");
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                conn.Close();
            }
            
        }
    }
}
