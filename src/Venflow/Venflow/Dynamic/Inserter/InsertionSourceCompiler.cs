using System.Collections.Generic;
using System.Runtime.Serialization;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionSourceCompiler
    {
        private int _visitedEntitiesCount;
        private readonly LinkedList<EntityRelationHolder> _entities;
        private readonly Dictionary<long, LinkedListNode<EntityRelationHolder>> _entityRelationLookup;
        private readonly ObjectIDGenerator _visitedEntityIds;
        private readonly ObjectIDGenerator _visitedEntityHolderIds;

        internal InsertionSourceCompiler()
        {
            _entities = new LinkedList<EntityRelationHolder>();
            _entityRelationLookup = new Dictionary<long, LinkedListNode<EntityRelationHolder>>();
            _visitedEntityIds = new ObjectIDGenerator();
            _visitedEntityHolderIds = new ObjectIDGenerator();
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

        internal void Compile(Entity entity)
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

                leftNode.Value.Relations.Add(relation);

                rightNode.Value.AssigningRelations.Add(relation.RightEntity.Relations[relation.LeftEntity.EntityName]); // TODO: Use RelationId instead, in order to prevent bugs if one entity has two relations with the same entity.

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

                Compile(relation.RightEntity);
            }

            if (_visitedEntitiesCount == 1 && _entities.Count == 0)
            {
                _entities.AddFirst(new EntityRelationHolder(entity));
            }
        }
    }
}
