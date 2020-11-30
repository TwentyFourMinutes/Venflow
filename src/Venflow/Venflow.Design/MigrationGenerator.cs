using System;
using System.Collections.Generic;
using System.Text;
using Venflow.Design.Operations;

namespace Venflow.Design
{
    internal class MigrationGenerator
    {
        private readonly StringBuilder _sourceCode;
        private readonly string _parentNamespace;
        private readonly string _migrationName;
        private readonly string _fullMigrationName;

        internal MigrationGenerator(string parentNamespace, string migrationName, string fullMigrationName)
        {
            _sourceCode = new StringBuilder();
            _parentNamespace = parentNamespace;
            _migrationName = migrationName;
            _fullMigrationName = fullMigrationName;
        }

        internal string GenerateMigrationClass(List<IMigrationChange> migrationChanges)
        {
            Start();

            WriteMigrations(migrationChanges);

            return Finish();
        }
        private void Start()
        {
            // TODO: Add namespaces from column types on the fly
            _sourceCode.Append(
@"using System;
using Venflow.Design;
using Venflow.Design.Operations;

namespace ").Append(_parentNamespace).Append(@"
{
    internal sealed class ").Append(_migrationName).Append(@" : Migration
    {
        public override string Name => """).Append(_fullMigrationName).Append(@""";

        public override void Changes()
        {
            ");
        }

        private void WriteMigrations(List<IMigrationChange> migrationChanges)
        {
            var lastEntity = default(MigrationEntity);

            foreach (var migrationChange in migrationChanges)
            {
                if (migrationChange.MigrationEntity != lastEntity)
                {
                    if (lastEntity is not null)
                    {
                        _sourceCode.Length -= Environment.NewLine.Length * 2;

                        _sourceCode.AppendLine(@"
            });");
                        _sourceCode.AppendLine();
                        _sourceCode.Append("            ");
                    }

                    lastEntity = migrationChange.MigrationEntity;

                    _sourceCode.Append(@"Entity(""").Append(lastEntity.Name).AppendLine(@""", migration =>
            {");
                }

                _sourceCode.Append("                ");

                migrationChange.CreateMigration(_sourceCode);

                _sourceCode.AppendLine();
                _sourceCode.AppendLine();
            }

            _sourceCode.Length -= Environment.NewLine.Length;

            _sourceCode.Append("            });");
        }

        private string Finish()
        {
            _sourceCode.Append(@"
        }
    }
}");
            return _sourceCode.ToString();
        }
    }
}
