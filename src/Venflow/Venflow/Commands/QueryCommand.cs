using Npgsql;
using System;
using System.Text;
using Venflow.Modeling;

namespace Venflow.Commands
{
    public class QueryCommand<TEntity> : VenflowCommand<TEntity> where TEntity : class
    {
        internal bool TrackChanges { get; set; }

        internal Func<NpgsqlDataReader, TEntity>? EntityFactory { get; set; }

        internal QueryCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity) : base(underlyingCommand, entity)
        {

        }

        internal static string CompileDefaultStatement(Entity<TEntity> entityConfiguration)
        {
            return CompileDefaultStatement(entityConfiguration, -1);
        }

        internal static string CompileDefaultStatement(Entity<TEntity> entityConfiguration, int count)
        {
            var builder = new StringBuilder();

            builder.Append("SELECT ");

            builder.Append(entityConfiguration.ColumnListString);

            builder.Append(" FROM ");
            builder.Append(entityConfiguration.TableName);

            if (count > 0)
            {
                builder.Append(" LIMIT ");
                builder.Append(count);
            }

            return builder.ToString();
        }
    }
}
