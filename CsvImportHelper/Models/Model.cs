using System;
using System.Collections.Generic;
using System.Text;

namespace CsvImportHelper.Models
{
    public class Model
    {
        public string MPN { get; set; }

        public bool BulkPack { get; set; }

        public int BulkPackQuantity { get; set; }
    }
}
