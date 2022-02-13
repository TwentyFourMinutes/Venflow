using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Models;

namespace Reflow.Analyzer.Operations
{
    internal class Insert
    {
        internal Command Command { get; }
        internal VirtualEntity[] VirtualEntities { get; }

        private Insert(Command command, VirtualEntity[] virtualEntities)
        {
            Command = command;
            VirtualEntities = virtualEntities;
        }

        internal static Insert Construct(Database database, Command command)
        {
            var compiler = new RelationCompiler(database);

            compiler.CompileFromRoot(command.Entity);

            return new Insert(command, compiler.GetEntities());
        }

        internal class VirtualEntity
        {
            internal Entity Entity { get; }
            internal List<EntityRelation> SelfAssignedRelations { get; }
            internal List<EntityRelation> ForeignAssignedRelations { get; }
            internal EntityRelation? DirectAssignedRelation { get; set; }

            internal VirtualEntity(Entity entity)
            {
                Entity = entity;
                SelfAssignedRelations = new List<EntityRelation>();
                ForeignAssignedRelations = new List<EntityRelation>();
            }
        }

        private class RelationCompiler
        {
            internal readonly ObjectIDGenerator VisitedEntityIds;
            internal readonly HashSet<uint> ReachableRelations;

            private readonly Database _database;
            private readonly LinkedList<VirtualEntity> _entities;
            private readonly ObjectIDGenerator _processedEntities;
            private readonly List<VirtualEntity> _reachableEntities;

            internal RelationCompiler(Database database)
            {
                VisitedEntityIds = new ObjectIDGenerator();
                ReachableRelations = new HashSet<uint>();

                _entities = new LinkedList<VirtualEntity>();
                _processedEntities = new ObjectIDGenerator();
                _reachableEntities = new List<VirtualEntity>();

                _database = database;
            }

            internal VirtualEntity[] GetEntities()
            {
                var entities = new VirtualEntity[_entities.Count];

                var index = 0;

                for (var entry = _entities.First; entry is not null; entry = entry.Next)
                {
                    entities[index++] = entry.Value;
                }

                return entities;
            }

            internal void CompileFromRoot(ITypeSymbol entitySymbol)
            {
                var entity = _database.Entities[entitySymbol];

                if (entity.Relations is null)
                {
                    _entities.AddFirst(new VirtualEntity(entity));

                    return;
                }

                VisitedEntityIds.GetId(entity, out _);
                CollectAllReachableEntities(entity, null);

                BaseCompile();
            }

            private void CollectAllReachableEntities(Entity entity, EntityRelation? toAssign)
            {
                var entityHolder = new VirtualEntity(entity);

                if (toAssign is not null)
                {
                    entityHolder.DirectAssignedRelation = toAssign;
                }

                _reachableEntities.Add(entityHolder);

                for (
                    var relationIndex = entity.Relations!.Count - 1;
                    relationIndex >= 0;
                    relationIndex--
                )
                {
                    var relation = entity.Relations[relationIndex];

                    //var rightEntity = _database.Entities[relation.RightEntitySymbol];

                    //if (!rightEntity.HasDbGeneratedPrimaryKey)
                    //{
                    //    ReachableRelations.Add(relation.Id);

                    //    continue;
                    //}

                    if (
                        relation.LeftNavigationProperty is null
                        || !ReachableRelations.Add(relation.Id)
                    )
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

                for (
                    var relationIndex = entity.Relations.Count - 1;
                    relationIndex >= 0;
                    relationIndex--
                )
                {
                    var relation = entity.Relations[relationIndex];

                    if (relation.LeftNavigationProperty is null)
                        continue;

                    var rightEntity = _database.Entities[relation.RightEntitySymbol];

                    VisitedEntityIds.GetId(rightEntity, out var newEntity);

                    if (newEntity)
                        CollectAllReachableEntities(rightEntity, relation.Mirror);
                }
            }

            private void BaseCompile()
            {
                while (_reachableEntities.Count > 0)
                {
                    var startReachableCount = _reachableEntities.Count;

                    for (var entityIndex = 0; entityIndex < _reachableEntities.Count; entityIndex++)
                    {
                        var entityHolder = _reachableEntities[entityIndex];
                        var entity = entityHolder.Entity;

                        if (entity.Relations is not null)
                        {
                            var noDirectDependencies = true;

                            for (
                                var relationIndex = entity.Relations.Count - 1;
                                relationIndex >= 0;
                                relationIndex--
                            )
                            {
                                var relation = entity.Relations[relationIndex];

                                if (!ReachableRelations.Contains(relation.Id))
                                    continue;

                                var rightEntity = _database.Entities[relation.RightEntitySymbol];

                                _processedEntities.HasId(rightEntity, out var newEntity);
                                VisitedEntityIds.HasId(rightEntity, out var notReachable);

                                if (
                                    newEntity
                                    && !notReachable
                                    && relation.ForeignKeyLocation == ForeignKeyLocation.Left
                                )
                                {
                                    noDirectDependencies = false;
                                    break;
                                }
                            }

                            if (!noDirectDependencies)
                                continue;

                            for (
                                var relationIndex = entity.Relations.Count - 1;
                                relationIndex >= 0;
                                relationIndex--
                            )
                            {
                                var relation = entity.Relations[relationIndex];

                                if (!ReachableRelations.Contains(relation.Id))
                                    continue;

                                var rightEntity = _database.Entities[relation.RightEntitySymbol];

                                VisitedEntityIds.HasId(rightEntity, out var notReachable);

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
                        throw new InvalidOperationException(
                            $"The entities {string.Join(", ", _reachableEntities.Select(x => "'" + x.Entity.Symbol.Name + "'"))} create a relation loop which can't be resolved. You can fix this by splitting up your insert into multiple ones. However if you do get this error, please create an issue on GitHub with a reproduceable example."
                        );
                    }
                }
            }
        }
    }
}
