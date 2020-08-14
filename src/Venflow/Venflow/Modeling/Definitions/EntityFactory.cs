using System;
using System.Collections.Generic;
using System.Text;
using Venflow.Enums;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Modeling.Definitions
{
    internal class EntityFactory<TEntity> : EntityFactory where TEntity : class, new()
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

            _entity = new Entity<TEntity>(_entityBuilder.Type, _entityBuilder.ChangeTrackerFactory?.ProxyType, _entityBuilder.TableName,
                _entityBuilder.EntityInNullableContext, _entityBuilder.DefaultPropNullability, columns,
                _entityBuilder.IsCustomEntity ? (PrimaryEntityColumn<TEntity>)columns[0] : default,
                _entityBuilder.IsCustomEntity ? GetColumnListString(columns, ColumnListStringOptions.IncludePrimaryColumns) : string.Empty,
                _entityBuilder.IsCustomEntity ? GetColumnListString(columns, ColumnListStringOptions.IncludePrimaryColumns | ColumnListStringOptions.ExplicitNames) : string.Empty,
                _entityBuilder.IsCustomEntity ? GetColumnListString(columns, ColumnListStringOptions.None) : string.Empty,
                _entityBuilder.IsCustomEntity ? _entityBuilder.ChangeTrackerFactory?.GetProxyFactory() : default,
                _entityBuilder.IsCustomEntity ? _entityBuilder.ChangeTrackerFactory?.GetProxyApplyingFactory(columns) : default);

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

                if (!entityBuilders.TryGetValue(relation.RightEntityName, out var foreignEntity))
                {
                    throw new InvalidOperationException($"The entity '{relation.RightEntityName}' is being used in a relation on '{relation.LeftEntityBuilder.Type.Name}', but doesn't contain a 'Table<{relation.RightEntityName}>' entry in the Database.");
                }

                if (relation.RightNavigationProperty is { })
                {
                    foreignEntity.IgnoreProperty(relation.RightNavigationProperty.Name);
                }

                foreignEntity.Relations.Add(new EntityRelationDefinition(relation.RelationId, foreignEntity, relation.RightNavigationProperty, relation.LeftEntity.Type.Name, relation.LeftNavigationProperty, relation.ForeignKeyColumnName, ReverseRelationType(relation.RelationType), ReverseKeyLocation(relation.ForeignKeyLocation))
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

            var foreignEntityRelations = new EntityRelation[_entityBuilder.Relations.Count];
            var relationIdToColumn = new Dictionary<uint, EntityRelation>();
            var relationNameToColumn = new Dictionary<string, EntityRelation>();

            object? entityInstance = default;

            for (int i = 0; i < _entityBuilder.Relations.Count; i++)
            {
                var relation = _entityBuilder.Relations[i];

                var relationEntity = entities[relation.RightEntityName];

                EntityColumn keyColumn;

                if (relation.ForeignKeyLocation == ForeignKeyLocation.Left)
                {
                    keyColumn = _entity.GetColumn(relation.ForeignKeyColumnName);
                }
                else
                {
                    keyColumn = relationEntity.GetColumn(relation.ForeignKeyColumnName);
                }

                var entityRelation = new EntityRelation(relation.RelationId, _entity, relation.LeftNavigationProperty, relation.LeftNavigationProperty?.IsNullableReferenceType(_entity.IsInNullableContext, _entity.DefaultPropNullability) ?? false, relationEntity, relation.RightNavigationProperty, relation.RightNavigationProperty?.IsNullableReferenceType(_entity.IsInNullableContext, _entity.DefaultPropNullability) ?? false, keyColumn, relation.RelationType, relation.ForeignKeyLocation);

                if (VenflowConfiguration.ShouldUseDeepValidation &&
                    relation.RelationType == RelationType.OneToMany && 
                    relation.LeftNavigationProperty is { } &&
                    !relation.LeftNavigationProperty.CanWrite)
                {
                    entityInstance ??= Activator.CreateInstance(relation.LeftEntity.Type);

                    if (relation.LeftNavigationProperty.GetBackingField().GetValue(entityInstance) == null)
                    {
                        throw new InvalidOperationException($"The entity '{relation.LeftEntity}' defines the navigation property '{relation.LeftNavigationProperty}' which doesn't have a public setter and its value isn't assigned in the constructor. Either assign it in the constructor or add a public setter.");
                    }
                }

                if (entityRelation.RightEntity.Relations is { } &&
                    entityRelation.RightEntity.Relations.TryGetValue(relation.RelationId, out var sibilingRelation))
                {
                    entityRelation.Sibiling = sibilingRelation;
                    sibilingRelation.Sibiling = entityRelation;
                }

                foreignEntityRelations[i] = entityRelation;

                relationIdToColumn.Add(relation.RelationId, entityRelation);

                if (entityRelation.LeftNavigationProperty is { })
                    relationNameToColumn.Add(entityRelation.LeftNavigationProperty.Name, entityRelation);
            }

            _entity.Relations = new TrioKeyCollection<uint, string, EntityRelation>(foreignEntityRelations, relationIdToColumn, relationNameToColumn);
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
                    sb.Append('"')
                      .Append(_entityBuilder.TableName)
                      .Append('"')
                      .Append('.');
                }

                sb.Append('"')
                  .Append(column.ColumnName)
                  .Append("\", ");
            }

            sb.Length -= 2;

            return sb.ToString();
        }

        private ForeignKeyLocation ReverseKeyLocation(ForeignKeyLocation foreignKeyLocation) =>
            foreignKeyLocation switch
            {
                ForeignKeyLocation.Left => ForeignKeyLocation.Right,
                ForeignKeyLocation.Right => ForeignKeyLocation.Left,
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