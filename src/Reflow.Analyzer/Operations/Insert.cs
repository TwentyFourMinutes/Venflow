using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer.Operations
{
    //internal class Insert
    //{
    //    internal InsertType Type { get; private set; }
    //    internal ITypeSymbol Entity { get; private set; }

    //    private class FluentReader : FluentSyntaxReader<Query>
    //    {
    //        private ITypeSymbol? _previousJoinSymbol;
    //        private readonly Database _database;

    //        internal FluentReader(Database database, Query query, FluentCallDefinition fluentCall)
    //            : base(query, fluentCall)
    //        {
    //            _database = database;
    //        }

    //        protected override bool ValidateHead(
    //            LambdaExpressionSyntax lambdaSyntax,
    //            IMethodSymbol methodSymbol,
    //            SeparatedSyntaxList<ArgumentSyntax> arguments
    //        )
    //        {
    //            if (methodSymbol.Name is not "InsertAsync")
    //            {
    //                return false;
    //            }

    //            WithLinkData(
    //                new QueryLinkData(
    //                    Value.Entity,
    //                    contentLength,
    //                    parameterIndecies.Count == 0 ? null : parameterIndecies,
    //                    queryHelperStrings.Count == 0 ? null : queryHelperStrings
    //                )
    //            );

    //            return true;
    //        }

    //        protected override void ReadTail(
    //            IMethodSymbol methodSymbol,
    //            SeparatedSyntaxList<ArgumentSyntax> arguments
    //        )
    //        {

    //        }

    //        protected override bool ValidateTail()
    //        {
    //            if (!Value.Type.HasFlag(QueryType.Single) && !Value.Type.HasFlag(QueryType.Many))
    //            {
    //                return false;
    //            }

    //            return true;
    //        }
    //    }
    //}

    [Flags]
    internal enum InsertType : byte
    {
        None = 0,
        Single = 1 << 0,
        Many = 1 << 1,
    }

    internal class InsertLinkData : ILambdaLinkData
    {
        internal MethodLocation? Location { get; set; }

        internal ITypeSymbol Entity { get; }

        internal InsertLinkData(ITypeSymbol entity)
        {
            Entity = entity;
        }
    }
}
