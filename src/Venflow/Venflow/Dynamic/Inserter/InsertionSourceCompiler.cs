using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionSourceCompiler
    {
        private readonly LinkedList<EntityRelationHolder> _entities;
        private readonly ObjectIDGenerator _visitedEntityIds;
        private readonly ObjectIDGenerator _processedEntities;
        private readonly List<Entity> _reachableEntities;

        internal InsertionSourceCompiler()
        {
            _entities = new LinkedList<EntityRelationHolder>();
            _visitedEntityIds = new ObjectIDGenerator();
            _processedEntities = new ObjectIDGenerator();
            _reachableEntities = new List<Entity>();
        }

        internal EntityRelationHolder[] GenerateSortedEntities()
        {
            var entities = new EntityRelationHolder[_entities.Count];

            var index = 0;

            for (var entry = _entities.First; entry is { }; entry = entry.Next)
            {
                entities[index++] = entry.Value;
            }

            return entities;
        }

        internal void RootCompile(Entity entity)
        {
            if (entity.Relations is null)
            {
                _entities.AddFirst(new EntityRelationHolder(entity));

                return;
            }

            _visitedEntityIds.GetId(entity, out _);
            CollectAllReachableEntities(entity);

            BaseCompile();
        }

        private void CollectAllReachableEntities(Entity entity)
        {
            _reachableEntities.Add(entity);

            for (int relationIndex = 0; relationIndex < entity.Relations.Count; relationIndex++)
            {
                var relation = entity.Relations[relationIndex];

                if (relation.LeftNavigationProperty is null)
                    continue;

                _visitedEntityIds.GetId(relation.RightEntity, out var newEntity);

                if (newEntity)
                    CollectAllReachableEntities(relation.RightEntity);
            }
        }

        private void BaseCompile()
        {
            while (_reachableEntities.Count > 0)
            {
                var startReachableCount = _reachableEntities.Count;

                for (int entityIndex = 0; entityIndex < _reachableEntities.Count; entityIndex++)
                {
                    var entity = _reachableEntities[entityIndex];

                    var noDirectDependencies = true;

                    for (int relationIndex = 0; relationIndex < entity.Relations.Count; relationIndex++)
                    {
                        var relation = entity.Relations[relationIndex];

                        _processedEntities.HasId(relation.RightEntity, out var newEntity);
                        _visitedEntityIds.HasId(relation.RightEntity, out var notReachable);

                        if (newEntity &&
                            !notReachable &&
                            relation.ForeignKeyLocation == ForeignKeyLocation.Left)
                        {
                            noDirectDependencies = false;

                            break;
                        }
                    }

                    if (noDirectDependencies)
                    {
                        var leftNode = _entities.AddLast(new EntityRelationHolder(entity));

                        for (int relationIndex = 0; relationIndex < entity.Relations.Count; relationIndex++)
                        {
                            var relation = entity.Relations[relationIndex];

                            _visitedEntityIds.HasId(relation.RightEntity, out var notReachable);

                            if (notReachable)
                                continue;

                            if (relation.ForeignKeyLocation == ForeignKeyLocation.Left &&
                                relation.LeftNavigationProperty is { })
                            {
                                leftNode.Value.Relations.Add(relation);
                            }
                            else if (relation.LeftNavigationProperty is { })
                            {
                                leftNode.Value.AssigningRelations.Add(relation);
                            }
                        }

                        _processedEntities.GetId(entity, out _);

                        _reachableEntities.RemoveAt(entityIndex);

                        entityIndex--;
                    }
                }

                if (startReachableCount == _reachableEntities.Count)
                {
                    throw new InvalidOperationException($"The entities {string.Join(", ", _reachableEntities.Select(x => "'" + x.EntityName + "'"))} create a relation loop which can't be resolved. You can fix this by splitting up your insert into multiple ones. However if you do get this error, please create an issue on GitHub with a reproduceable example.");
                }
            }
        }
    }
}
