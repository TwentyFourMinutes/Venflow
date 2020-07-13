using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionSourceCompiler
    {
        private readonly HashSet<int> _visitedEntities;
        private readonly HashSet<uint> _visitedRelations;
        private readonly LinkedList<EntityRelationHolder> _entityRelations;
        private readonly Dictionary<int, LinkedListNode<EntityRelationHolder>> _entityRelationLookup;

        internal InsertionSourceCompiler()
        {
            _visitedEntities = new HashSet<int>();
            _visitedRelations = new HashSet<uint>();
            _entityRelations = new LinkedList<EntityRelationHolder>();
            _entityRelationLookup = new Dictionary<int, LinkedListNode<EntityRelationHolder>>();
        }

        internal void Compile(Entity entity)
        {
            var entityId = RuntimeHelpers.GetHashCode(entity);

            _visitedEntities.Add(entityId);

            if (entity.Relations is null)
            {
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
                            _entityRelations.AddBefore(leftEntityHolder, rightEntityHolder);

                        }
                        else
                        {
                            _entityRelations.AddLast(rightEntityHolder);
                        }
                    }

                    if (!hasLeftEntityHolder)
                    {
                        leftEntityHolder = new LinkedListNode<EntityRelationHolder>(new EntityRelationHolder(relation.LeftEntity));

                        _entityRelationLookup.Add(leftHashCode, leftEntityHolder);

                        _entityRelations.AddAfter(rightEntityHolder, leftEntityHolder);
                    }

                    _visitedRelations.Add(relation.RelationId);

                    leftEntityHolder.Value.Relations.Add(relation);

                    rightEntityHolder.Value.AssigningRelations.Add(relation.RightEntity.Relations[relation.LeftEntity.EntityName]);
                }
            }

            for (int i = 0; i < entity.Relations.Count; i++)
            {
                var relation = entity.Relations[i];

                if (_visitedEntities.Contains(RuntimeHelpers.GetHashCode(relation.RightEntity)))
                    continue;

                Compile(relation.RightEntity);
            }
        }

        internal EntityRelationHolder[] GenerateSortedEntities()
        {
            var entities = new EntityRelationHolder[_entityRelations.Count];

            var index = 0;

            for (var entry = _entityRelations.First; entry is { }; entry = entry.Next)
            {
                entities[index++] = entry.Value;
            }

            return entities;
        }
    }
}
