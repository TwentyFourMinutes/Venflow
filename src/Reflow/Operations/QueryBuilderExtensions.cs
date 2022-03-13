namespace Reflow
{
    public static class QueryBuilderExtensions
    {
        public static Operations.JoinQueryBuilder<T> Caching<T>(
            this Operations.QueryBuilder<T> query
        ) where T : class, new()
        {
            _ = query;
            throw new InvalidOperationException();
        }
    }
}
