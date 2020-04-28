using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow
{
    public class CachedCommand<TEntity> : IAsyncDisposable where TEntity : class
    {
        public VenflowCommand<TEntity> Command { get; }

        public NpgsqlParameterCollection Parameters => Command.UnderlyingCommand.Parameters;

        private CachedCommand(VenflowCommand<TEntity> command)
        {
            Command = command;
        }

        internal async static Task<CachedCommand<TEntity>> CreateAsync(VenflowDbConnection connection, VenflowCommand<TEntity> command, CancellationToken cancellationToken)
        {
            command.UnderlyingCommand.Connection = connection.Connection;

            await command.UnderlyingCommand.PrepareAsync(cancellationToken);

            return new CachedCommand<TEntity>(command);
        }

        public void ClearParameters()
            => Command.UnderlyingCommand.Parameters.Clear();

        public ValueTask DisposeAsync()
        {
            Command.UnderlyingCommand.Unprepare();

            return Command.DisposeAsync();
        }

        public static implicit operator VenflowCommand<TEntity>(CachedCommand<TEntity> command) => command.Command;
    }
}
