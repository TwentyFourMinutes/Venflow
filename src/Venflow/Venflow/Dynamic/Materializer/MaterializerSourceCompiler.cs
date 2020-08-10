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

            for (int i = 0; i < _joinBuilderValues.FullPath.Count; i++)
            {
                BaseCompile(_joinBuilderValues.FullPath[i], queryEntityHolder);
            }
        }

        private void BaseCompile(JoinPath joinPath, QueryEntityHolder rightQueryHolder)
        {
            var relation = joinPath.JoinOptions.Join.Sibiling;

            var leftQueryHolder = new QueryEntityHolder(relation.LeftEntity, _queryEntityHolderIndex++);

            _entities.AddLast(leftQueryHolder);

            if (relation.RelationType == RelationType.ManyToOne)
            {
                rightQueryHolder.InitializeNavigation.Add(relation.Sibiling);
                leftQueryHolder.AssigningRelations.Add((relation, rightQueryHolder));

                rightQueryHolder.RequiresChangedLocal = true;
                leftQueryHolder.RequiresChangedLocal = true;
            }
            else
            {
                rightQueryHolder.AssignedRelations.Add((relation.Sibiling, leftQueryHolder));
                rightQueryHolder.RequiresChangedLocal = true;
            }

            if (relation.LeftNavigationProperty is { })
            {
                if (relation.RelationType == RelationType.OneToMany)
                {
                    leftQueryHolder.InitializeNavigation.Add(relation);
                    rightQueryHolder.AssigningRelations.Add((relation.Sibiling, leftQueryHolder));


                    rightQueryHolder.RequiresChangedLocal = true;
                    leftQueryHolder.RequiresChangedLocal = true;
                }
                else
                {
                    leftQueryHolder.AssignedRelations.Add((relation, rightQueryHolder));
                    leftQueryHolder.RequiresChangedLocal = true;
                }
            }

            for (int i = 0; i < joinPath.TrailingJoinPath.Count; i++)
            {
                BaseCompile(joinPath.TrailingJoinPath[i], leftQueryHolder);
            }
        }
    }
}
