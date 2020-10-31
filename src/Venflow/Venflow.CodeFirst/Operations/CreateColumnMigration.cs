using System;
using System.Text;
using NpgsqlTypes;

namespace Venflow.CodeFirst.Operations
{
    internal class CreateColumnMigration : IMigrationChange
    {
        internal string Name { get; }
        internal Type Type { get; }
        internal ColumnDetails Details { get; }

        internal CreateColumnMigration(string name, Type type, ColumnDetails details)
        {
            Name = name;
            Type = type;
            Details = details;
        }

        public void ApplyChanges(MigrationContext migrationContext, MigrationEntity? migrationEntity)
        {
            migrationEntity.Columns.Add(Name, new MigrationColumn(Name, Type, Details));
        }

        public void ApplyMigration(StringBuilder migration, MigrationEntity? migrationEntity)
        {
            NpgsqlDbType columnDbType;

            if (Type == typeof(ulong))
            {
                columnDbType = NpgsqlDbType.Bigint;
            }
            else
            {
                columnDbType = NpgsqlTypeMapper.GetDbType(Type);
            }

            migration.Append(@"ALTER TABLE """)
                     .Append(migrationEntity.Name)
                     .Append(@""" ADD COLUMN ")
                     .Append(Name)
                     .Append(' ')
                     .Append(columnDbType);

            if (Details.Precision.HasValue)
            {
                migration.Append('(')
                         .Append(Details.Precision.Value);

                if (Details.Scale.HasValue)
                {
                    migration.Append(',')
                             .Append(Details.Scale)
                             .Append(')');
                }
                else
                {
                    migration.Append(')');
                }
            }

            migration.Append(' ');

            if (Details.IsPrimaryKey)
            {
                migration.Append("PRIMARY KEY");
            }
            else if (!Details.IsNullable)
            {
                migration.Append("NOT NULL");
            }

            migration.AppendLine(";");
        }
    }
}
