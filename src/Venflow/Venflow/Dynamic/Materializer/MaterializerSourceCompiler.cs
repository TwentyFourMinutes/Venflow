using System;
using System.Collections.Generic;
using Venflow.Commands;
using Venflow.Enums;

namespace Venflow.Dynamic.Materializer
{
    internal class MaterializerSourceCompiler
    {
        private int _queryEntityHolderIndex;
        private readonly LinkedList<QueryEntityHolder> _entities;
        private readonly JoinBuilderValues _joinBuilderValues;

        internal MaterializerSourceCompiler(JoinBuilderValues joinBuilderValues)
        {
            _entities = new LinkedList<QueryEntityHolder>();

            _joinBuilderValues = joinBuilderValues;
        }

        internal QueryEntityHolder[] GenerateSortedEntities()
        {
            var entities = new QueryEntityHolder[_entities.Count];

            var index = 0;

            for (var entry = _entities.First; entry is { }; entry = entry.Next)
            {
                entities[index++] = entry.Value;
            }

            return entities;
        }

        internal void Compile()
        {
            var queryEntityHolder = new QueryEntityHolder(_joinBuilderValues.Root, _queryEntityHolderIndex++);

            _entities.AddFirst(queryEntityHolder);

            for (int i = _joinBuilderValues.FullPath.Count - 1; i >= 0; i--)
            {
                BaseCompile(_joinBuilderValues.FullPath[i], queryEntityHolder);
            }
        }

        private void BaseCompile(JoinPath joinPath, QueryEntityHolder rightQueryHolder)
        {
            var relation = joinPath.JoinOptions.Join.Sibiling;

            var leftQueryHolder = new QueryEntityHolder(relation.LeftEntity, _queryEntityHolderIndex++);

            _entities.AddLast(leftQueryHolder);

#if !NET48

            if (VenflowConfiguration.ShouldUseDeepValidation)
            {
                if ((joinPath.JoinOptions.JoinBehaviour == JoinBehaviour.FullJoin ||
                    joinPath.JoinOptions.JoinBehaviour == JoinBehaviour.LeftJoin) &&
                    !relation.IsRightNavigationPropertyNullable)
                {
                    throw new InvalidOperationException($"The join you configured 'Join...(x => x.{relation.RightNavigationProperty.Name})' from the entity '{relation.RightEntity.EntityName}' to the entity '{relation.LeftEntity.EntityName}' is configured as a LeftJoin, however the property '{relation.RightNavigationProperty.Name}' on the entity '{relation.RightEntity.EntityName}' isn't marked as null-able!");
                }
                else if ((joinPath.JoinOptions.JoinBehaviour == JoinBehaviour.FullJoin ||
                        joinPath.JoinOptions.JoinBehaviour == JoinBehaviour.RightJoin) &&
                        !relation.IsLeftNavigationPropertyNullable &&
                        relation.LeftNavigationProperty is { })
                {
                    throw new InvalidOperationException($"The join you configured 'Join...(x => x.{relation.RightNavigationProperty.Name})' from the entity '{relation.RightEntity.EntityName}' to the entity '{relation.LeftEntity.EntityName}' is configured as a RightJoin, however the property '{relation.LeftNavigationProperty.Name}' on the entity '{relation.LeftEntity.EntityName}' isn't marked as null-able!");
                }
            }
#endif

            if (relation.RelationType == RelationType.ManyToOne)
            {
                if (relation.RightNavigationProperty.CanWrite)
                    rightQueryHolder.InitializeNavigations.Add(relation.Sibiling);

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

            if (relation.LeftNavigationProperty is { })
            {
                if (relation.RelationType == RelationType.OneToMany)
                {
                    if (relation.LeftNavigationProperty.CanWrite)
                        leftQueryHolder.InitializeNavigations.Add(relation);

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

            for (int i = joinPath.TrailingJoinPath.Count - 1; i >= 0; i--)
            {
                BaseCompile(joinPath.TrailingJoinPath[i], leftQueryHolder);
            }
        }
    }
}
