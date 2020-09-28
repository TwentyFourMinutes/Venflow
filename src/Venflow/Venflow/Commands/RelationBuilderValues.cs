using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Venflow.Dynamic;
using Venflow.Modeling;
using Venflow.Modeling.Definitions;

namespace Venflow.Commands
{
    internal class RelationBuilderValues
    {
        internal List<RelationPath> TrailingPath { get; }
        internal List<RelationPath> FlattenedPath { get; }

        private RelationPath _currentPath;

        internal RelationBuilderValues()
        {
            _currentPath = default!;
            TrailingPath = new();
            FlattenedPath = new();
        }

        internal EntityRelation[] GetFlattenedRelations()
        {
            var flattenedPathSpan = FlattenedPath.AsSpan();
            var entityRelations = new EntityRelation[flattenedPathSpan.Length];
            var entityRelationsSpan = entityRelations.AsSpan();

            for (int i = flattenedPathSpan.Length - 1; i >= 0; i--)
            {
                entityRelationsSpan[i] = flattenedPathSpan[i].CurrentRelation;
            }

            return entityRelations;
        }

        internal Entity BaseRelationWith<TRootEntity, TTarget>(Entity parent, Expression<Func<TRootEntity, TTarget>> propertySelector)
            where TRootEntity : class, new()
            where TTarget : class
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!parent.Relations!.TryGetValue(foreignProperty.Name, out var relation))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TRootEntity).Name}' isn't in any relation with the entity '{typeof(TRootEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            AddToPath(relation, true);

            return relation.RightEntity;
        }

        internal Entity BaseRelationWith<TRootEntity, TTarget, T>(Entity parent, Expression<Func<TRootEntity, TTarget>> propertySelector, T value)
            where TRootEntity : class, new()
            where TTarget : class
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!parent.Relations!.TryGetValue(foreignProperty.Name, out var relation))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TRootEntity).Name}' isn't in any relation with the entity '{typeof(TRootEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            AddToPath(relation, value, true);

            return relation.RightEntity;
        }

        internal Entity BaseAndWith<TRelationEntity, TTarget>(Entity parent, Expression<Func<TRelationEntity, TTarget>> propertySelector)
            where TRelationEntity : class, new()
            where TTarget : class
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!parent.Relations!.TryGetValue(foreignProperty.Name, out var relation))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TRelationEntity).Name}' isn't in any relation with the entity '{typeof(TRelationEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            AddToPath(relation, false);

            return relation.RightEntity;
        }

        internal Entity BaseAndWith<TRelationEntity, TTarget, T>(Entity parent, Expression<Func<TRelationEntity, TTarget>> propertySelector, T value)
            where TRelationEntity : class, new()
            where TTarget : class
        {
            var foreignProperty = propertySelector.ValidatePropertySelector(false);

            if (!parent.Relations!.TryGetValue(foreignProperty.Name, out var relation))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TRelationEntity).Name}' isn't in any relation with the entity '{typeof(TRelationEntity).Name}' over the foreign property '{foreignProperty.Name}'. Ensure that you defined the relation in your configuration file.");
            }

            AddToPath(relation, value, false);

            return relation.RightEntity;
        }

        private void AddToPath<T>(EntityRelation relation, T value, bool newFullPath)
        {
            if (newFullPath)
            {
                for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
                {
                    var path = TrailingPath[pathIndex];

                    if (path.CurrentRelation == relation)
                    {
                        _currentPath = path;

                        return;
                    }
                }

                _currentPath = new RelationPath<T>(relation, value);

                TrailingPath.Add(_currentPath);

                FlattenedPath.Add(_currentPath);
            }
            else
            {
                _currentPath = _currentPath.AddToPath(relation, value, out var isNew);

                if (isNew)
                {
                    FlattenedPath.Add(_currentPath);
                }
            }
        }

        private void AddToPath(EntityRelation relation, bool newFullPath)
        {
            if (newFullPath)
            {
                for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
                {
                    var path = TrailingPath[pathIndex];

                    if (path.CurrentRelation == relation)
                    {
                        _currentPath = path;

                        return;
                    }
                }

                _currentPath = new RelationPath(relation);

                TrailingPath.Add(_currentPath);

                FlattenedPath.Add(_currentPath);
            }
            else
            {
                _currentPath = _currentPath.AddToPath(relation, out var isNew);

                if (isNew)
                {
                    FlattenedPath.Add(_currentPath);
                }
            }
        }
    }
}
