using Venflow.Enums;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Modeling.Definitions
{
    internal class EntityFactory<TEntity> : EntityFactory where TEntity : class, new()
    {
        internal override EntityBuilder EntityBuilder => _entityBuilder;

        private Entity _entity = null!;

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
                _entityBuilder.IsRegularEntity ? columns[0] : default,
                _entityBuilder.IsRegularEntity ? GetColumnListString(columns, ColumnListStringOptions.IncludePrimaryColumns) : string.Empty,
                _entityBuilder.IsRegularEntity ? GetColumnListString(columns, ColumnListStringOptions.None) : string.Empty,
                _entityBuilder.IsRegularEntity ? _entityBuilder.ChangeTrackerFactory?.GetProxyFactory() : default,
                _entityBuilder.IsRegularEntity ? _entityBuilder.ChangeTrackerFactory?.GetProxyApplyingFactory() : default);

            return _entity;
        }

        internal override void ConfigureForeignRelations(Dictionary<string, EntityBuilder> entityBuilders)
        {
            if (_entityBuilder.Relations.Count == 0)
                return;

            var entityInstance = default(object?);

            for (var relationIndex = _entityBuilder.Relations.Count - 1; relationIndex >= 0; relationIndex--)
            {
                var relation = _entityBuilder.Relations[relationIndex];

                if (relation.IsProcessed)
                {
                    continue;
                }

                if (!entityBuilders.TryGetValue(relation.RightEntityName, out var foreignEntity))
                {
                    throw new InvalidOperationException($"The entity '{relation.RightEntityName}' is being used in a relation on '{relation.LeftEntityBuilder.Type.Name}', but doesn't contain a 'Table<{relation.RightEntityName}>' entry in the Database.");
                }

                if (relation.RightNavigationProperty is not null)
                {
                    foreignEntity.IgnoreProperty(relation.RightNavigationProperty.Name);
                }

                if (relation.RelationType == RelationType.OneToMany &&
                    relation.LeftNavigationProperty is not null)
                {
                    entityInstance ??= Activator.CreateInstance(relation.LeftEntityBuilder.Type);

                    var backingValue = relation.LeftNavigationProperty.GetValue(entityInstance);

                    if (backingValue is null &&
                        relation.LeftNavigationProperty.GetBackingField() is not null)
                    {
                        backingValue = relation.LeftNavigationProperty!.GetBackingField()!.GetValue(entityInstance);
                    }

                    relation.IsLeftNavigationPropertyInitialized = backingValue is not null;
                }
                else if (relation.RelationType == RelationType.ManyToOne &&
                        relation.RightNavigationProperty is not null)
                {
                    var foreignEntityInstance = Activator.CreateInstance(foreignEntity.Type);

                    var backingValue = relation.RightNavigationProperty.GetValue(foreignEntityInstance);

                    if (backingValue is null &&
                        relation.RightNavigationProperty.GetBackingField() is not null)
                    {
                        backingValue = relation.RightNavigationProperty!.GetBackingField()!.GetValue(foreignEntityInstance);
                    }

                    relation.IsRightNavigationPropertyInitialized = backingValue is not null;
                }

                foreignEntity.Relations.Add(new EntityRelationDefinition(relation.RelationId, foreignEntity, relation.RightNavigationProperty, relation.LeftEntityBuilder.Type.Name, relation.LeftNavigationProperty, relation.ForeignKeyColumnName, ReverseRelationType(relation.RelationType), ReverseKeyLocation(relation.ForeignKeyLocation))
                {
                    IsProcessed = true,
                    IsLeftNavigationPropertyInitialized = relation.IsRightNavigationPropertyInitialized,
                    IsRightNavigationPropertyInitialized = relation.IsLeftNavigationPropertyInitialized
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

            for (var i = _entityBuilder.Relations.Count - 1; i >= 0; i--)
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

                if (relation.RelationType == RelationType.OneToMany &&
                    relation.LeftNavigationProperty is not null &&
                    !typeof(IList<>).MakeGenericType(relation.LeftNavigationProperty.PropertyType.GetGenericArguments()[0]).IsAssignableFrom(relation.LeftNavigationProperty.PropertyType))
                {
                    throw new InvalidOperationException($"The entity '{relation.LeftEntityBuilder.Type.Name}' defines the navigation property '{relation.LeftNavigationProperty.Name}' of type '{relation.LeftNavigationProperty.PropertyType.Name}', however Venflow requires the assigned instance to implement IList<T>.");
                }

                var entityRelation = new EntityRelation(relation.RelationId, _entity, relation.LeftNavigationProperty, relation.IsLeftNavigationPropertyInitialized, relation.LeftNavigationProperty?.IsNullableReferenceType(_entity.IsInNullableContext, _entity.DefaultPropNullability) ?? false, relationEntity, relation.RightNavigationProperty, relation.IsRightNavigationPropertyInitialized, relation.RightNavigationProperty?.IsNullableReferenceType(_entity.IsInNullableContext, _entity.DefaultPropNullability) ?? false, keyColumn, relation.RelationType, relation.ForeignKeyLocation);

                if (entityRelation.RightEntity.Relations is not null &&
                    entityRelation.RightEntity.Relations.TryGetValue(relation.RelationId, out var sibilingRelation))
                {
                    entityRelation.Sibiling = sibilingRelation!;
                    sibilingRelation!.Sibiling = entityRelation;
                }

                foreignEntityRelations[i] = entityRelation;

                relationIdToColumn.Add(relation.RelationId, entityRelation);

                if (entityRelation.LeftNavigationProperty is not null)
                {
#if !NET48
                    if (!relationNameToColumn.TryAdd(entityRelation.LeftNavigationProperty.Name, entityRelation))
                    {
                        throw new InvalidOperationException($"The relation between '{entityRelation.LeftEntity.EntityName}' and '{entityRelation.RightEntity.EntityName}' using the foreign property '{entityRelation.LeftNavigationProperty.Name}' and the foreign key '{entityRelation.ForeignKeyColumn.PropertyInfo.Name}', was defined in both entity configurations.");
                    }
#else
                    if (relationNameToColumn.ContainsKey(entityRelation.LeftNavigationProperty.Name))
                    {
                        throw new InvalidOperationException($"The relation between '{entityRelation.LeftEntity.EntityName}' and '{entityRelation.RightEntity.EntityName}' using the foreign property '{entityRelation.LeftNavigationProperty.Name}' and the foreign key '{entityRelation.ForeignKeyColumn.PropertyInfo.Name}', was defined in both entity configurations.");

                    }

                    relationNameToColumn.Add(entityRelation.LeftNavigationProperty.Name, entityRelation);
#endif
                }
            }

            _entity.Relations = new TrioKeyCollection<uint, string, EntityRelation>(foreignEntityRelations, relationIdToColumn, relationNameToColumn);
        }

        private string GetColumnListString(EntityColumnCollection<TEntity> columns, ColumnListStringOptions options)
        {
            var sb = new StringBuilder();

            var index = (options & ColumnListStringOptions.IncludePrimaryColumns) != 0 ? 0 : columns.RegularColumnsOffset;

            for (; index < columns.Count; index++)
            {
                var column = columns[index];

                if (column.Options.HasFlag(ColumnOptions.ReadOnly) && index > 0)
                    continue;

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
