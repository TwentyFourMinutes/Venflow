using System;
using System.Text;
using NpgsqlTypes;

namespace Venflow.Design.Operations
{
    internal class CreateColumnMigration : IMigrationChange
    {
        public MigrationEntity MigrationEntity { get; }

        internal string Name { get; }
        internal Type Type { get; }
        internal ColumnDetails Details { get; }

        internal CreateColumnMigration(string name, Type type, ColumnDetails details, MigrationEntity migrationEntity)
        {
            Name = name;
            Type = type;
            Details = details;
            MigrationEntity = migrationEntity;
        }

        public void ApplyChanges(MigrationContext migrationContext)
        {
            MigrationEntity.Columns.Add(Name, new MigrationColumn(Name, Type, Details));
        }

        public void ApplyMigration(StringBuilder migration)
        {
            NpgsqlDbType columnDbType;

            if (Type == typeof(ulong))
            {
                columnDbType = NpgsqlDbType.Bigint;
            }
            // TODO: Check for native postgres enums
            else
            {
                columnDbType = NpgsqlTypeMapper.GetDbType(Type);
            }

            if (columnDbType == NpgsqlDbType.Text &&
               Details.Precision.HasValue)
            {
                columnDbType = NpgsqlDbType.Varchar;
            }

            migration.Append(@"ALTER TABLE """)
                     .Append(MigrationEntity.Name)
                     .Append(@""" ADD COLUMN """)
                     .Append(Name)
                     .Append(@""" ")
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

            if (Details.IsPrimaryKey)
            {
                migration.Append(" PRIMARY KEY");
            }
            else if (!Details.IsNullable)
            {
                migration.Append(" NOT NULL");
            }

            migration.AppendLine(";");
        }

        public void CreateMigration(StringBuilder migrationClass)
        {
            migrationClass.Append("migration.AddColumn(").Append(Name).Append(", typeof(").Append(Type.FullName).Append("), new ColumnDetails { IsPrimaryKey = ").Append(Details.IsPrimaryKey.ToString().ToLower()).Append(", IsNullable = ").Append(Details.IsNullable.ToString().ToLower()).Append(", Precision = ").Append(Details?.Precision?.ToString() ?? "null").Append(", Scale = ").Append(Details?.Scale?.ToString() ?? "null").Append(" });");
        }
    }
}
