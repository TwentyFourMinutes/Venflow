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

            _entity = new Entity<TEntity>(_entityBuilder.Type, _entityBuilder.ChangeTrackerFactory?.ProxyType, _entityBuilder.TableName, columns,
                (PrimaryEntityColumn<TEntity>)columns[0], GetColumnListString(columns, ColumnListStringOptions.IncludePrimaryColumns),
                GetColumnListString(columns, ColumnListStringOptions.IncludePrimaryColumns | ColumnListStringOptions.ExplicitNames),
                GetColumnListString(columns, ColumnListStringOptions.None), GetColumnListString(columns, ColumnListStringOptions.IncludePrimaryColumns | ColumnListStringOptions.ExplicitNames | ColumnListStringOptions.PrefixedPrimaryKeys),
                _entityBuilder.InsertWriter, _entityBuilder.ChangeTrackerFactory?.GetProxyFactory(),
                _entityBuilder.ChangeTrackerFactory?.GetProxyApplyingFactory(columns));

            return _entity;
        }

        internal override void ConfigureForeignRelations(Dictionary<string, EntityBuilder> entityBuilders)
        {
            if (_entityBuilder.Relations.Count == 0)
                return;

            var relationCount = _entityBuilder.Relations.Count;

            for (int i = 0; i < relationCount; i++)
            {
                var relation = _entityBuilder.Relations[i];

                if (relation.IsProcessed)
                {
                    continue;
                }
                var foreignEntity = entityBuilders[relation.RightEntityName];

                if (relation.RightNavigationProperty is { })
                {
                    foreignEntity.IgnoreProperty(relation.RightNavigationProperty.Name);
                }

                foreignEntity.Relations.Add(new EntityRelationDefinition(relation.RelationId, foreignEntity, relation.RightNavigationProperty, relation.LeftEntity.Type.Name, relation.LeftNavigationProperty, relation.ForeignKeyColumnName, ReverseRelationType(relation.RelationType), ReverseKeyLocation(relation.ForeignKeyLoaction))
                {
                    IsProcessed = true
                });

                relation.IsProcessed = true;
            }
        }

        internal override void ApplyForeignRelations(Dictionary<string, Entity> entities)
        {
            if (_entityBuilder.Relations.Count == 0)
            {
                return;
            }

            var foreignEntities = new EntityRelation[_entityBuilder.Relations.Count];
            var nameToEntity = new Dictionary<string, EntityRelation>();

            for (int i = 0; i < _entityBuilder.Relations.Count; i++)
            {
                var relation = _entityBuilder.Relations[i];

                var relationEntity = entities[relation.RightEntityName];

                EntityColumn keyColumn;

                if (relation.ForeignKeyLoaction == ForeignKeyLoaction.Left)
                {
                    keyColumn = _entity.GetColumn(relation.ForeignKeyColumnName);
                }
                else
                {
                    keyColumn = relationEntity.GetColumn(relation.ForeignKeyColumnName);
                }

                var entityRelation = new EntityRelation(relation.RelationId, _entity, relation.LeftNavigationProperty, relationEntity, relation.RightNavigationProperty, keyColumn, relation.RelationType, relation.ForeignKeyLoaction);

                foreignEntities[i] = entityRelation;

                nameToEntity.Add(relation.RightEntityName, entityRelation);
            }

            _entity.Relations = new DualKeyCollection<string, EntityRelation>(foreignEntities, nameToEntity);
        }

        private string GetColumnListString(EntityColumnCollection<TEntity> columns, ColumnListStringOptions options)
        {
            var sb = new StringBuilder();

            var explictNames = (options & ColumnListStringOptions.ExplicitNames) != 0;

            var index = (options & ColumnListStringOptions.IncludePrimaryColumns) != 0 ? 0 : columns.RegularColumnsOffset;

            for (; index < columns.Count; index++)
            {
                var column = columns[index];


                if (explictNames)
                {
                    sb.Append(_entityBuilder.TableName);
                    sb.Append('.');
                }

                sb.Append('"');

                sb.Append(column.ColumnName);

                if (index == 0 &&
                    (options & ColumnListStringOptions.PrefixedPrimaryKeys) != 0)
                {
                    sb.Append("\" AS \"$");

                    sb.Append(_entityBuilder.TableName);
                    sb.Append("$.");
                    sb.Append(column.ColumnName);
                }

                sb.Append("\", ");
            }

            sb.Length -= 2;

            return sb.ToString();
        }

        private ForeignKeyLoaction ReverseKeyLocation(ForeignKeyLoaction foreignKeyLocation) =>
            foreignKeyLocation switch
            {
                ForeignKeyLoaction.Left => ForeignKeyLoaction.Right,
                ForeignKeyLoaction.Right => ForeignKeyLoaction.Left,
                _ => throw new NotImplementedException()
            };

        private RelationType ReverseRelationType(RelationType relationType) =>
            relationType switch
            {
                RelationType.OneToMany => RelationType.ManyToOne,
                RelationType.ManyToOne => RelationType.OneToMany,
                RelationType.OneToOne => RelationType.OneToOne,
                _ => throw new NotImplementedException()
            };
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