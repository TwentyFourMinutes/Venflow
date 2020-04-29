using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow
{
    public class PreparedCommand<TEntity> : IDisposable where TEntity : class
    {
        public VenflowCommand<TEntity> Command { get; }

        public NpgsqlParameterCollection Parameters => Command.UnderlyingCommand.Parameters;

        private PreparedCommand(VenflowCommand<TEntity> command)
        {
            Command = command;
        }

        internal async static Task<PreparedCommand<TEntity>> CreateAsync(NpgsqlConnection connection, VenflowCommand<TEntity> command, CancellationToken cancellationToken)
        {
            command.UnderlyingCommand.Connection = connection;

            await command.UnderlyingCommand.PrepareAsync(cancellationToken);

            return new PreparedCommand<TEntity>(command);
        }

        public void ClearParameters()
            => Command.UnderlyingCommand.Parameters.Clear();

        public void Dispose()
        {
            Command.UnderlyingCommand.Unprepare();

            Command.Dispose();
        }

        public static implicit operator VenflowCommand<TEntity>(PreparedCommand<TEntity> command) => command.Command;
    }
}
