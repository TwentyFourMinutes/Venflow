using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionSourceCompiler
    {
        private int _visitedEntitiesCount;
        private readonly HashSet<long> _visitedEntities;
        private readonly HashSet<uint> _visitedRelations;
        private readonly LinkedList<EntityRelationHolder> _entities;
        private readonly Dictionary<long, LinkedListNode<EntityRelationHolder>> _entityRelationLookup;
        private readonly ObjectIDGenerator _visitedEntityIds;
        private readonly ObjectIDGenerator _visitedEntityHolderIds;

        internal InsertionSourceCompiler()
        {
            _visitedEntities = new HashSet<long>();
            _visitedRelations = new HashSet<uint>();
            _entities = new LinkedList<EntityRelationHolder>();
            _entityRelationLookup = new Dictionary<long, LinkedListNode<EntityRelationHolder>>();
            _visitedEntityIds = new ObjectIDGenerator();
            _visitedEntityHolderIds = new ObjectIDGenerator();
        }

        internal void Compile(Entity entity)
        {
            var entityId = RuntimeHelpers.GetHashCode(entity);

            _visitedEntities.Add(entityId);

            if (entity.Relations is null)
            {
                if (_visitedEntities.Count == 1)
                {
                    _entities.AddFirst(new EntityRelationHolder(entity));
                }

                return;
            }

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                var leftHashCode = RuntimeHelpers.GetHashCode(relation.LeftEntity);
                var rightHashCode = RuntimeHelpers.GetHashCode(relation.RightEntity);

                if (relation.ForeignKeyLocation == ForeignKeyLocation.Left && relation.LeftNavigationProperty is { } && !_visitedRelations.Contains(relation.RelationId))
                {
                    var hasLeftEntityHolder = _entityRelationLookup.TryGetValue(leftHashCode, out var leftEntityHolder);

                    if (!_entityRelationLookup.TryGetValue(rightHashCode, out var rightEntityHolder))
                    {
                        rightEntityHolder = new LinkedListNode<EntityRelationHolder>(new EntityRelationHolder(relation.RightEntity));

                        _entityRelationLookup.Add(rightHashCode, rightEntityHolder);

                        if (hasLeftEntityHolder)
                        {
                            _entities.AddBefore(leftEntityHolder, rightEntityHolder);

                        }
                        else
                        {
                            _entities.AddLast(rightEntityHolder);
                        }
                    }

                    if (!hasLeftEntityHolder)
                    {
                        leftEntityHolder = new LinkedListNode<EntityRelationHolder>(new EntityRelationHolder(relation.LeftEntity));

                        _entityRelationLookup.Add(leftHashCode, leftEntityHolder);

                        _entities.AddAfter(rightEntityHolder, leftEntityHolder);
                    }

                    _visitedRelations.Add(relation.RelationId);

                    leftEntityHolder.Value.Relations.Add(relation);

                    rightEntityHolder.Value.AssigningRelations.Add(relation.RightEntity.Relations[relation.LeftEntity.EntityName]);
                }
            }

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                if (relation.LeftNavigationProperty is null || _visitedEntities.Contains(RuntimeHelpers.GetHashCode(relation.RightEntity)))
                    continue;

                Compile(relation.RightEntity);
            }

            if (_visitedEntities.Count == 1)
            {
                _entities.AddFirst(new EntityRelationHolder(entity));
            }
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

        internal void Compilev2(Entity entity)
        {
            _visitedEntitiesCount++;

            if (entity.Relations is null)
            {
                if (_visitedEntitiesCount == 1)
                {
                    _entities.AddFirst(new EntityRelationHolder(entity));
                }

                return;
            }

            if (_visitedEntitiesCount == 1)
            {
                _ = _visitedEntityIds.GetId(entity, out _);
            }

            var leftId = _visitedEntityHolderIds.GetId(entity, out _);

            if (!_entityRelationLookup.TryGetValue(leftId, out var leftNode))
            {
                leftNode = new LinkedListNode<EntityRelationHolder>(new EntityRelationHolder(entity));

                _entityRelationLookup.Add(leftId, leftNode);

                _entities.AddLast(leftNode);
            }

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                if (relation.LeftNavigationProperty is null ||
                    relation.ForeignKeyLocation != ForeignKeyLocation.Left)
                {
                    continue;
                }

                var rightId = _visitedEntityHolderIds.GetId(relation.RightEntity, out _);

                if (_entityRelationLookup.TryGetValue(rightId, out var rightNode))
                {
                    _entities.Remove(rightNode);
                }
                else
                {
                    rightNode = new LinkedListNode<EntityRelationHolder>(new EntityRelationHolder(relation.RightEntity));

                    _entityRelationLookup.Add(rightId, rightNode);
                }

                _entities.AddBefore(leftNode, rightNode);
            }

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                if (relation.LeftNavigationProperty is null)
                {
                    continue;
                }

                _ = _visitedEntityIds.GetId(relation.RightEntity, out var isNew);

                if (!isNew)
                {
                    continue;
                }

                Compilev2(relation.RightEntity);
            }

            if (_visitedEntitiesCount == 1 && _entities.Count == 0)
            {
                _entities.AddFirst(new EntityRelationHolder(entity));
            }
        }
    }
}
