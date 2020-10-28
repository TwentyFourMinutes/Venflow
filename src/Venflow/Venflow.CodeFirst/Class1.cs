using System;
using NpgsqlTypes;

namespace Venflow.CodeFirst
{

    public class Migration1 : Migration
    {
        public override void Changes()
        {
            Entity("Migrations", migration =>
            {
                migration.Create();

                migration.AddColumn("Name", typeof(string), false);
                migration.AddColumn("Checksum", typeof(string), false);
                migration.AddColumn("Timestamp", typeof(DateTime), false);
            });
        }
    }
}
