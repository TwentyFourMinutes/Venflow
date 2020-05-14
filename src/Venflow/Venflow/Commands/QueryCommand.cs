using Npgsql;
using System;
using Venflow.Modeling;

namespace Venflow.Commands
{
    public class QueryCommand<TEntity> : VenflowCommand<TEntity> where TEntity : class
    {
        internal bool OrderPreservedColumns { get; set; }

        internal bool TrackChanges { get; set; }

        internal Func<NpgsqlDataReader, TEntity>? EntityFactory { get; set; }

        internal QueryCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity) : base(underlyingCommand, entity)
        {

        }
    }
}
