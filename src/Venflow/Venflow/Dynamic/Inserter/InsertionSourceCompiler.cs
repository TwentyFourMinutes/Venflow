using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Venflow.Commands;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionSourceCompiler
    {
        internal readonly ObjectIDGenerator VisitedEntityIds;
        internal readonly HashSet<uint> ReachableRelations;

        private readonly LinkedList<EntityRelationHolder> _entities;
        private readonly ObjectIDGenerator _processedEntities;
        private readonly List<EntityRelationHolder> _reachableEntities;

        internal InsertionSourceCompiler()
        {
            VisitedEntityIds = new ObjectIDGenerator();
            ReachableRelations = new HashSet<uint>();

            _entities = new LinkedList<EntityRelationHolder>();
            _processedEntities = new ObjectIDGenerator();
            _reachableEntities = new List<EntityRelationHolder>();
        }

        internal EntityRelationHolder[] GetEntities()
        {
            var entities = new EntityRelationHolder[_entities.Count];

            var index = 0;

            for (var entry = _entities.First; entry is { }; entry = entry.Next)
            {
                entities[index++] = entry.Value;
            }

            return entities;
        }

        internal void CompileFromRoot(Entity entity)
        {
            if (entity.Relations is null)
            {
                _entities.AddFirst(new EntityRelationHolder(entity));

                return;
            }

            VisitedEntityIds.GetId(entity, out _);
            CollectAllReachableEntities(entity, null);

            BaseCompile();
        }

        internal void CompileFromRelations(Entity entity, IRelationPath rootPath)
        {
            VisitedEntityIds.GetId(entity, out _);

            var entityHolder = new EntityRelationHolder(entity);

            _reachableEntities.Add(entityHolder);


            if (rootPath is { })
            {
                for (int i = rootPath.TrailingPath.Count - 1; i >= 0; i--)
                {
                    var path = rootPath.TrailingPath[i];

                    var relation = path.CurrentRelation;

                    ReachableRelations.Add(relation.RelationId);

                    if (!relation.RightEntity.HasDbGeneratedPrimaryKey)
                    {
                        BaseCompileFromRelations(path, relation.Sibiling);

                        continue;
                    }

                    if (relation.ForeignKeyLocation == ForeignKeyLocation.Left)
                    {
                        entityHolder.SelfAssignedRelations.Add(relation);
                    }
                    else if (relation.ForeignKeyLocation == ForeignKeyLocation.Right)
                    {
                        entityHolder.ForeignAssignedRelations.Add(relation);
                    }

                    BaseCompileFromRelations(path, null);
                }
            }

            BaseCompile();
        }

        private void BaseCompileFromRelations(IRelationPath relationPath, EntityRelation? toAssign)
        {
            var entityHolder = new EntityRelationHolder(relationPath.Entity);

            if (toAssign is { })
            {
                entityHolder.DirectAssignedRelation = toAssign;
            }

            VisitedEntityIds.GetId(relationPath.Entity, out _);
            _reachableEntities.Add(entityHolder);

            for (int pathIndex = relationPath.TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
            {
                var path = relationPath.TrailingPath[pathIndex];

                var relation = path.CurrentRelation;

                ReachableRelations.Add(relation.RelationId);

                if (!relation.RightEntity.HasDbGeneratedPrimaryKey)
                {
                    BaseCompileFromRelations(path, relation.Sibiling);

                    continue;
                }

                if (relation.ForeignKeyLocation == ForeignKeyLocation.Left)
                {
                    entityHolder.SelfAssignedRelations.Add(relation);
                }
                else if (relation.ForeignKeyLocation == ForeignKeyLocation.Right)
                {
                    entityHolder.ForeignAssignedRelations.Add(relation);
                }

                BaseCompileFromRelations(path, null);
            }
        }

        private void CollectAllReachableEntities(Entity entity, EntityRelation? toAssign)
        {
            var entityHolder = new EntityRelationHolder(entity);

            if (toAssign is { })
            {
                entityHolder.DirectAssignedRelation = toAssign;
            }

            _reachableEntities.Add(entityHolder);

            for (int relationIndex = entity.Relations.Count - 1; relationIndex >= 0; relationIndex--)
            {
                var relation = entity.Relations[relationIndex];

                if (!relation.RightEntity.HasDbGeneratedPrimaryKey)
                {
                    ReachableRelations.Add(relation.RelationId);

                    continue;
                }

                if (relation.LeftNavigationProperty is null ||
                    !ReachableRelations.Add(relation.RelationId))
                    continue;

                if (relation.ForeignKeyLocation == ForeignKeyLocation.Left)
                {
                    entityHolder.SelfAssignedRelations.Add(relation);
                }
                else if (relation.ForeignKeyLocation == ForeignKeyLocation.Right)
                {
                    entityHolder.ForeignAssignedRelations.Add(relation);
                }
            }

            for (int relationIndex = entity.Relations.Count - 1; relationIndex >= 0; relationIndex--)
            {
                var relation = entity.Relations[relationIndex];

                if (relation.LeftNavigationProperty is null)
                    continue;

                VisitedEntityIds.GetId(relation.RightEntity, out var newEntity);

                if (newEntity)
                    CollectAllReachableEntities(relation.RightEntity, relation.Sibiling);
            }
        }

        private void BaseCompile()
        {
            while (_reachableEntities.Count > 0)
            {
                var startReachableCount = _reachableEntities.Count;

                for (int entityIndex = 0; entityIndex < _reachableEntities.Count; entityIndex++)
                {
                    var entityHolder = _reachableEntities[entityIndex];
                    var entity = entityHolder.Entity;

                    if (entity.Relations is { })
                    {
                        var noDirectDependencies = true;

                        for (int relationIndex = entity.Relations.Count - 1; relationIndex >= 0; relationIndex--)
                        {
                            var relation = entity.Relations[relationIndex];

                            if (!ReachableRelations.Contains(relation.RelationId))
                                continue;

                            _processedEntities.HasId(relation.RightEntity, out var newEntity);
                            VisitedEntityIds.HasId(relation.RightEntity, out var notReachable);

                            if (newEntity &&
                                !notReachable &&
                                relation.ForeignKeyLocation == ForeignKeyLocation.Left)
                            {
                                noDirectDependencies = false;

                                break;
                            }
                        }


                        if (!noDirectDependencies)
                            continue;

                        for (int relationIndex = entity.Relations.Count - 1; relationIndex >= 0; relationIndex--)
                        {
                            var relation = entity.Relations[relationIndex];

                            if (!ReachableRelations.Contains(relation.RelationId))
                                continue;

                            VisitedEntityIds.HasId(relation.RightEntity, out var notReachable);

                            if (notReachable)
                                continue;

                            _processedEntities.GetId(entity, out _);
                        }
                    }

                    _entities.AddLast(entityHolder);

                    _reachableEntities.RemoveAt(entityIndex);

                    entityIndex--;
                }

                if (startReachableCount == _reachableEntities.Count)
                {
                    throw new InvalidOperationException($"The entities {string.Join(", ", _reachableEntities.Select(x => "'" + x.Entity.EntityName + "'"))} create a relation loop which can't be resolved. You can fix this by splitting up your insert into multiple ones. However if you do get this error, please create an issue on GitHub with a reproduceable example.");
                }
            }
        }
    }
}
