﻿using System.Collections.Generic;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class RelationPath
    {
        internal EntityRelation CurrentRelation { get; }
        internal List<RelationPath> TrailingPath { get; }

        internal RelationPath(EntityRelation currentRelation)
        {
            CurrentRelation = currentRelation;

            TrailingPath = new();
        }

        internal RelationPath AddToPath(EntityRelation relation, out bool isNew)
        {
            for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
            {
                var trailingPath = TrailingPath[pathIndex];

                if (trailingPath.CurrentRelation == relation)
                {
                    isNew = false;

                    return trailingPath;
                }
            }

            var path = new RelationPath(relation);

            TrailingPath.Add(path);

            isNew = true;

            return path;
        }

        internal RelationPath AddToPath<T>(EntityRelation relation, T value, out bool isNew)
        {
            for (int pathIndex = TrailingPath.Count - 1; pathIndex >= 0; pathIndex--)
            {
                var trailingPath = TrailingPath[pathIndex];

                if (trailingPath.CurrentRelation == relation)
                {
                    isNew = false;

                    return trailingPath;
                }
            }

            var path = new RelationPath<T>(relation, value);

            TrailingPath.Add(path);

            isNew = true;

            return path;
        }
    }

    internal class RelationPath<T> : RelationPath
    {
        internal T Value { get; }

        internal RelationPath(EntityRelation currentRelation, T value) : base(currentRelation)
        {
            Value = value;
        }
    }
}
