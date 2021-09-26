using System;
using System.Collections.Generic;
using Venflow.Commands;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Materializer
{
    internal class MaterializerSourceCompiler
    {
        private int _queryEntityHolderIndex;
        private readonly LinkedList<QueryEntityHolder> _entities;
        private readonly IRelationPath? _rootRelationPath;

        internal MaterializerSourceCompiler(RelationBuilderValues? relationBuilderValues)
        {
            _entities = new LinkedList<QueryEntityHolder>();

            _rootRelationPath = relationBuilderValues;
        }

        internal QueryEntityHolder[] GenerateSortedEntities()
        {
            var entities = new QueryEntityHolder[_entities.Count];

            var index = 0;

            for (var entry = _entities.First; entry is not null; entry = entry.Next)
            {
                entities[index++] = entry.Value;
            }

            return entities;
        }

        internal void Compile(Entity rootEntity)
        {
            var queryEntityHolder = new QueryEntityHolder(rootEntity, _queryEntityHolderIndex++);

            _entities.AddFirst(queryEntityHolder);

            if (_rootRelationPath is null)
                return;

            for (int i = 0; i < _rootRelationPath.TrailingPath.Count; i++)
            {
                BaseCompile((RelationPath<JoinBehaviour>)_rootRelationPath.TrailingPath[i], queryEntityHolder);
            }
        }

        private void BaseCompile(RelationPath<JoinBehaviour> relationPath, QueryEntityHolder rightQueryHolder)
        {
            var relation = relationPath.CurrentRelation.Sibiling;

            var leftQueryHolder = new QueryEntityHolder(relation.LeftEntity, _queryEntityHolderIndex++);

            _entities.AddLast(leftQueryHolder);

#if !NET48

            if (VenflowConfiguration.ShouldUseDeepValidation)
            {
                if ((relationPath.Value == JoinBehaviour.FullJoin ||
                    relationPath.Value == JoinBehaviour.LeftJoin) &&
                    !relation.IsRightNavigationPropertyNullable)
                {
                    throw new InvalidOperationException($"The join you configured 'Join...(x => x.{relation.RightNavigationProperty.Name})' from the entity '{relation.RightEntity.EntityName}' to the entity '{relation.LeftEntity.EntityName}' is configured as a LeftJoin, however the property '{relation.RightNavigationProperty.Name}' on the entity '{relation.RightEntity.EntityName}' isn't marked as null-able!");
                }
                else if ((relationPath.Value == JoinBehaviour.FullJoin ||
                        relationPath.Value == JoinBehaviour.RightJoin) &&
                        !relation.IsLeftNavigationPropertyNullable &&
                        relation.LeftNavigationProperty is not null)
                {
                    throw new InvalidOperationException($"The join you configured 'Join...(x => x.{relation.RightNavigationProperty.Name})' from the entity '{relation.RightEntity.EntityName}' to the entity '{relation.LeftEntity.EntityName}' is configured as a RightJoin, however the property '{relation.LeftNavigationProperty.Name}' on the entity '{relation.LeftEntity.EntityName}' isn't marked as null-able!");
                }
            }
#endif

            if (relation.RelationType == RelationType.ManyToOne)
            {
                if (!relation.IsRightNavigationPropertyInitialized &&
                    (relation.RightNavigationProperty.CanWrite ||
                    relation.RightNavigationProperty.GetBackingField() is not null))
                {
                    rightQueryHolder.InitializeNavigations.Add(relation.Sibiling);
                }

                leftQueryHolder.ForeignAssignedRelations.Add((relation, rightQueryHolder));

                rightQueryHolder.RequiresChangedLocal = true;
                leftQueryHolder.RequiresChangedLocal = true;

                leftQueryHolder.RequiresDBNullCheck = true;
            }
            else
            {
                rightQueryHolder.SelfAssignedRelations.Add((relation.Sibiling, leftQueryHolder));
                rightQueryHolder.RequiresChangedLocal = true;

                if (relation.RelationType == RelationType.OneToOne &&
                        relation.IsRightNavigationPropertyNullable)
                {
                    leftQueryHolder.RequiresDBNullCheck = true;
                }
                else if (relation.RelationType == RelationType.OneToMany)
                {
                    leftQueryHolder.RequiresDBNullCheck = true;
                }
            }

            if (relation.LeftNavigationProperty is not null)
            {
                if (relation.RelationType == RelationType.OneToMany)
                {
                    if (!relation.IsLeftNavigationPropertyInitialized &&
                        (relation.LeftNavigationProperty.CanWrite ||
                        relation.LeftNavigationProperty.GetBackingField() is not null))
                    {
                        leftQueryHolder.InitializeNavigations.Add(relation);
                    }

                    rightQueryHolder.ForeignAssignedRelations.Add((relation.Sibiling, leftQueryHolder));

                    rightQueryHolder.RequiresChangedLocal = true;
                    leftQueryHolder.RequiresChangedLocal = true;

                    rightQueryHolder.RequiresDBNullCheck = true;
                }
                else
                {
                    leftQueryHolder.SelfAssignedRelations.Add((relation, rightQueryHolder));
                    leftQueryHolder.RequiresChangedLocal = true;

                    if (relation.RelationType == RelationType.OneToOne &&
                        relation.IsLeftNavigationPropertyNullable)
                    {
                        rightQueryHolder.RequiresDBNullCheck = true;
                    }
                    else if (relation.RelationType == RelationType.OneToMany)
                    {
                        rightQueryHolder.RequiresDBNullCheck = true;
                    }
                }
            }

            for (int i = 0; i < relationPath.TrailingPath.Count; i++)
            {
                BaseCompile((RelationPath<JoinBehaviour>)relationPath.TrailingPath[i], leftQueryHolder);
            }
        }
    }
}
