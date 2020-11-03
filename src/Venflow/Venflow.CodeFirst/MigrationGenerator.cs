using System;
using System.Collections.Generic;
using System.Text;
using Venflow.CodeFirst.Operations;

namespace Venflow.CodeFirst
{
    internal class MigrationGenerator
    {
        private readonly StringBuilder _sourceCode;
        private readonly string _parentNamespace;
        private readonly string _migrationName;

        internal MigrationGenerator(string parentNamespace, string migrationName)
        {
            _sourceCode = new StringBuilder();
            _parentNamespace = parentNamespace;
            _migrationName = migrationName;
        }

        internal string GenerateMigrationClass(List<IMigrationChange> migrationChanges)
        {
            Start();

            WriteMigrations(migrationChanges);

            return Finish();
        }
        private void Start()
        {
            _sourceCode.Append(@"
namespace ").Append(_parentNamespace).Append(@"
{
    internal sealed class ").Append(_migrationName).Append(@" : Migration
    {
        public override string Name => """).Append(_migrationName).Append(@""";

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

                _sourceCode.Append(Environment.NewLine);
                _sourceCode.Append(Environment.NewLine);
            }

            _sourceCode.Length -= Environment.NewLine.Length;

            _sourceCode.AppendLine("                }");
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
