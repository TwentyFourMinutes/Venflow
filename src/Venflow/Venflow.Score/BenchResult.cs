using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Venflow.Score
{

    public class BenchResult
    {
        public string Method { get; set; }
        public string Job { get; set; }
        public string Mean { get; set; }
        public string Allocated { get; set; }
    }
}
