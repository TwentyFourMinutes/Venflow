namespace Venflow.Commands
{
    public interface ISpecficVenflowCommandBuilder<out TCommand> where TCommand : class
    {
        TCommand Build();
    }
}