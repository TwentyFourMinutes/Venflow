using System;

namespace Venflow.Design.Data
{
    public class MigrationDatabaseEntity
    {
        public string Name { get; set; }

        public string Checksum { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
