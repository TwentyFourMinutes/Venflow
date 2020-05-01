using Npgsql;
using System;
using Venflow.Modeling;

namespace Venflow.Commands
{
    public class QueryCommand<TEntity> : VenflowCommand<TEntity> where TEntity : class
    {
        public bool OrderPreservedColumns { get; set; }

        public Func<NpgsqlDataReader, TEntity>? EntityFactory { get; set; }

        internal QueryCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity) : base(underlyingCommand, entity)
        {

        }
    }
}
