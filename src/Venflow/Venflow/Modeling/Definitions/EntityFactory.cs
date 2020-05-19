using System.Collections.Generic;
using System.Text;

namespace Venflow.Modeling.Definitions
{
    internal class EntityFactory<TEntity> : IEntityFactory where TEntity : class
    {
        public IEntity Entity { get; private set; }

        private Entity<TEntity> _entity;
        private EntityColumnCollection<TEntity> _columns;

        private readonly EntityBuilder<TEntity> _entityBuilder;

        internal EntityFactory(EntityBuilder<TEntity> entityBuilder)
        {
            _entityBuilder = entityBuilder;
        }

        void IEntityFactory.BuildEntity()
        {
            _columns = _entityBuilder.Build();

            _entity = new Entity<TEntity>(_entityBuilder.Type, _entityBuilder.ChangeTrackerFactory?.ProxyType,
                   _entityBuilder.TableName, _columns, (PrimaryEntityColumn<TEntity>)_columns[0],
                   GetColumnListString(_columns, false), GetColumnListString(_columns, true),
                   _entityBuilder.InsertWriter, _entityBuilder.ChangeTrackerFactory?.GetProxyFactory(),
                   _entityBuilder.ChangeTrackerFactory?.GetProxyApplyingFactory(_columns));

            Entity = _entity;
        }

        void IEntityFactory.ApplyRelations(Dictionary<string, IEntity> entities)
        {
            for (int i = 0; i < _entityBuilder.Relations.Count; i++)
            {
                var relation = _entityBuilder.Relations[i];

                var foreignEntity = entities[relation.RelationEntityName];
            }
        }

        private string GetColumnListString(EntityColumnCollection<TEntity> columns, bool excludePrimaryColumns)
        {
            var sb = new StringBuilder();

            var offset = excludePrimaryColumns ? columns.RegularColumnsOffset : 0;

            for (int i = offset; i < columns.Count; i++)
            {
                var column = columns[i];

                sb.Append('"');
                sb.Append(column.ColumnName);
                sb.Append("\",");
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }

    internal interface IEntityFactory
    {
        IEntity Entity { get; }

        void BuildEntity();

        void ApplyRelations(Dictionary<string,IEntity> entities);
    }
}
