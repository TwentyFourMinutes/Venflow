using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Shared;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class EntityCacheEmitter
    {
        internal static SourceText Emit(IList<Entity> cachableEntites)
        {
            var members = new List<SyntaxNode>();
            var cachablesCache = new Dictionary<ITypeSymbol, string>(
                SymbolEqualityComparer.Default
            );

            for (var entityIndex = 0; entityIndex < cachableEntites.Count; entityIndex++)
            {
                var cachableEntity = cachableEntites[entityIndex];

                if (cachablesCache.TryGetValue(cachableEntity.Symbol, out var cacheTypeName))
                {
                    cachableEntity.CacheName = cacheTypeName;
                    continue;
                }

                cacheTypeName =
                    "__ICachable" + cachableEntity.Symbol.GetFullName().Replace('.', '_');
                var cacheInterfaceMembers = new List<MemberDeclarationSyntax>();
                var cacheMembers = new List<MemberDeclarationSyntax>();

                cachablesCache.Add(
                    cachableEntity.Symbol,
                    cachableEntity.CacheName = "Reflow.Cachables." + cacheTypeName
                );

                for (var columnIndex = 0; columnIndex < cachableEntity.Columns.Count; columnIndex++)
                {
                    var column = cachableEntity.Columns[columnIndex];

                    cacheMembers.Add(
                        Property(column.PropertyName, Type(column.Type), CSharpModifiers.Public)
                            .WithGetAccessor()
                            .WithSetAccessor()
                    );

                    cacheInterfaceMembers.Add(
                        Property(column.PropertyName, Type(column.Type)).WithGetAccessor()
                    );
                }

                cacheInterfaceMembers.Add(
                    (MemberDeclarationSyntax)Class(
                            "__Mutable",
                            CSharpModifiers.Internal | CSharpModifiers.Sealed
                        )
                        .WithBase(Type(cachableEntity.CacheName))
                        .WithMembers(cacheMembers)
                );

                members.Add(
                    Interface(cacheTypeName, CSharpModifiers.Public)
                        .WithMembers(cacheInterfaceMembers)
                );
            }

            return File("Reflow.Cachables").WithMembers(members).GetText();
        }
    }
}
