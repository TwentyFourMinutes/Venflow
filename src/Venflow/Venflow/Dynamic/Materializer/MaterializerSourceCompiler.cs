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
            var leftQueryHolder = new QueryEntityHolder(joinPath.JoinOptions.Join.RightEntity, _queryEntityHolderIndex++);

            if (joinPath.JoinOptions.Join.RelationType == RelationType.OneToMany)
                rightQueryHolder.InitializeNavigation.Add(joinPath.JoinOptions.Join);

            _entities.AddLast(leftQueryHolder);

            leftQueryHolder.AssigningRelations.Add((joinPath.JoinOptions.Join, rightQueryHolder));

            if (joinPath.JoinOptions.Join.RightNavigationProperty is { })
            {
                leftQueryHolder.AssignedRelations.Add((joinPath.JoinOptions.Join.Sibiling, rightQueryHolder));
            }

            for (int i = 0; i < joinPath.TrailingJoinPath.Count; i++)
            {
                BaseCompile(joinPath.TrailingJoinPath[i], leftQueryHolder);
            }
        }
    }
}
