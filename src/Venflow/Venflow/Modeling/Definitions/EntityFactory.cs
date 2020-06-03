using System;
using System.Collections.Generic;
using System.Text;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions
{
    internal class EntityFactory<TEntity> : EntityFactory where TEntity : class
    {
        internal override EntityBuilder EntityBuilder => _entityBuilder;

        private Entity _entity;

        private readonly EntityBuilder<TEntity> _entityBuilder;

        internal EntityFactory(EntityBuilder<TEntity> entityBuilder)
        {
            _entityBuilder = entityBuilder;
        }

        internal override Entity BuildEntity()
        {
            var columns = _entityBuilder.Build();

            _entity = new Entity<TEntity>(_entityBuilder.Type, _entityBuilder.ChangeTrackerFactory?.ProxyType,
                                          _entityBuilder.TableName, columns, (PrimaryEntityColumn<TEntity>)columns[0],
                                          GetColumnListString(columns, false, false), GetColumnListString(columns, false, true), GetColumnListString(columns, true, false),
                                          _entityBuilder.InsertWriter, _entityBuilder.ChangeTrackerFactory?.GetProxyFactory(),
                                          _entityBuilder.ChangeTrackerFactory?.GetProxyApplyingFactory(columns));

            return _entity;
        }

        internal override void ConfigureForeignRelations(Dictionary<string, EntityBuilder> entityBuilders)
        {
            if (_entityBuilder.Relations.Count == 0)
                return;

            for (int i = 0; i < _entityBuilder.Relations.Count; i++)
            {
                var relation = _entityBuilder.Relations[i];

                if (!relation.IsProcessed)
                {
                    var foreignEntity = entityBuilders[relation.RelationEntityName];

                    if (relation.IsKeyInRelation)
                    {
                        foreignEntity.IgnoreProperty(relation.ForeignKeyColumnName);
                    }

                    foreignEntity.Relations.Add(new EntityRelationDefinition(relation.ForeignProperty, !relation.IsKeyInRelation, relation.ForeignKeyProperty, _entityBuilder.Type.Name, GetReverseRelationType(relation.RelationType))
                    {
                        IsProcessed = true
                    });

                    relation.IsProcessed = true;
                }
            }
        }

        internal override void ApplyForeignRelations(Dictionary<string, Entity> entities)
        {
            if (_entityBuilder.Relations.Count == 0)
            {
                return;
            }

            var foreignEntities = new ForeignEntity[_entityBuilder.Relations.Count];
            var nameToEntity = new Dictionary<string, ForeignEntity>();

            for (int i = 0; i < _entityBuilder.Relations.Count; i++)
            {
                var relation = _entityBuilder.Relations[i];

                var relationEntity = entities[relation.RelationEntityName];

                EntityColumn keyColumn;

                if (relation.IsKeyInRelation)
                {
                    keyColumn = relationEntity.GetColumn(relation.ForeignKeyColumnName);
                }
                else
                {
                    keyColumn = _entity.GetColumn(relation.ForeignKeyColumnName);
                }

                var foreignEntity = new ForeignEntity(relationEntity, keyColumn, relation.ForeignProperty);

                if (nameToEntity.TryGetValue(relation.RelationEntityName, out var existingRelation) &&
                    existingRelation.ForeignEntityColumn == foreignEntity.ForeignEntityColumn)
                {
                    throw new InvalidEntityRelationException($"The relation between '{_entity.EntityName}' and '{relation.RelationEntityName}' is configured twice. Only configure the relation in one of the EntityConfigurations.");
                }

                foreignEntities[i] = foreignEntity;

                nameToEntity.Add(relation.RelationEntityName, foreignEntity);
            }

            _entity.Relations = new DualKeyCollection<string, ForeignEntity>(foreignEntities, nameToEntity);
        }

        private RelationType GetReverseRelationType(RelationType relationType)
        {
            switch (relationType)
            {
                case RelationType.OneToOne:
                    return RelationType.OneToOne;
                case RelationType.OneToMany:
                    return RelationType.ManyToOne;
                case RelationType.ManyToOne:
                    return RelationType.OneToMany;
                default:
                    throw new NotSupportedException();
            }
        }

        private string GetColumnListString(EntityColumnCollection<TEntity> columns, bool excludePrimaryColumns, bool explictNames)
        {
            var sb = new StringBuilder();

            var offset = excludePrimaryColumns ? columns.RegularColumnsOffset : 0;

            for (int i = offset; i < columns.Count; i++)
            {
                var column = columns[i];

                if (explictNames)
                {
                    sb.Append(_entityBuilder.TableName);
                    sb.Append('.');
                }

                sb.Append('"');

                sb.Append(column.ColumnName);
                sb.Append("\", ");
            }

            sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }
    }

    internal abstract class EntityFactory
    {
        internal abstract EntityBuilder EntityBuilder { get; }

        protected EntityFactory()
        {
        }

        internal abstract Entity BuildEntity();

        internal abstract void ConfigureForeignRelations(Dictionary<string, EntityBuilder> entityBuilders);

        internal abstract void ApplyForeignRelations(Dictionary<string, Entity> entities);
    }
}
