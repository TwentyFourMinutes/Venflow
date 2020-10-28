using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.CodeFirst
{

    internal class MigrationGenerator
    {
        private readonly StringBuilder _sourceCode;
        private readonly string _migrationName;

        internal MigrationGenerator(string migrationName)
        {
            _sourceCode = new StringBuilder();
            _migrationName = migrationName;
        }

        internal void Start()
        {
            _sourceCode.Append($@"
namespace Venflow.CodeFirst
{{
    internal sealed class {_migrationName} : Migration
    {{");
        }

        internal void Finish()
        {
            _sourceCode.Append($@"
    }}
}}");
        }
    }
}
