using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer.Sections
{
    internal class LambdaCollectionSection
        : GeneratorSection<
              EntityConfigurationSection,
              LambdaCollectionSection.SyntaxReceiver,
              Dictionary<ITypeSymbol, List<FluentCallDefinition>>
          >
    {
        protected override Dictionary<ITypeSymbol, List<FluentCallDefinition>> Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            EntityConfigurationSection previous
        )
        {
            var instanceMembers = new List<MemberDeclarationSyntax>();
            var staticMembers = new List<MemberDeclarationSyntax>();

            var classSyntaxWalker = new ClassSyntaxWalker(
                context.Compilation,
                instanceMembers,
                staticMembers
            );

            foreach (var classSyntax in syntaxReceiver.Candidates)
            {
                foreach (var childNode in classSyntax.ChildNodes())
                {
                    switch (childNode)
                    {
                        case FieldDeclarationSyntax:
                        case PropertyDeclarationSyntax { Initializer: not null }:
                            break;
                        default:
                            continue;
                    }

                    var member = (MemberDeclarationSyntax)childNode;

                    if (IsStaticMember(member))
                    {
                        staticMembers.Add(member);
                    }
                    else
                    {
                        instanceMembers.Add(member);
                    }
                }

                classSyntaxWalker.CollectLambdas(classSyntax);

                if (!classSyntaxWalker.HasInstanceConstructor && instanceMembers.Count > 0)
                {
                    for (
                        var variableMemberIndex = 0;
                        variableMemberIndex < instanceMembers.Count;
                        variableMemberIndex++
                    )
                    {
                        classSyntaxWalker.VisitWithName(
                            instanceMembers[variableMemberIndex],
                            ".cctor"
                        );
                    }

                    instanceMembers.Clear();
                }

                if (!classSyntaxWalker.HasStaticConstructor && staticMembers.Count > 0)
                {
                    for (
                        var variableMemberIndex = 0;
                        variableMemberIndex < staticMembers.Count;
                        variableMemberIndex++
                    )
                    {
                        classSyntaxWalker.VisitWithName(
                            staticMembers[variableMemberIndex],
                            ".cctor"
                        );
                    }

                    staticMembers.Clear();
                }
            }

            return classSyntaxWalker.DatabaseFluentCalls;
        }

        private static bool IsStaticMember(MemberDeclarationSyntax member)
        {
            for (var modifierIndex = 0; modifierIndex < member.Modifiers.Count; modifierIndex++)
            {
                if (member.Modifiers[modifierIndex].IsKind(SyntaxKind.StaticKeyword))
                    return true;
            }

            return false;
        }

        private class ClassSyntaxWalker : SyntaxWalker
        {
            internal Dictionary<
                ITypeSymbol,
                List<FluentCallDefinition>
            > DatabaseFluentCalls { get; }

            internal bool HasInstanceConstructor { get; private set; }
            internal bool HasStaticConstructor { get; private set; }

            private uint _lambdaIndex;
            private string _currentMemberName = null!;
            private ClassDeclarationSyntax _classSyntax = null!;
            private string _className = null!;
            private SemanticModel? _semanticModel;

            private readonly Compilation _compilation;
            private readonly List<MemberDeclarationSyntax> _instanceMembers;
            private readonly List<MemberDeclarationSyntax> _staticMembers;
            private readonly ScopeCollection _closureScopes;

            internal ClassSyntaxWalker(
                Compilation compilation,
                List<MemberDeclarationSyntax> instanceMembers,
                List<MemberDeclarationSyntax> staticMembers
            )
            {
                _compilation = compilation;
                _instanceMembers = instanceMembers;
                _staticMembers = staticMembers;

                DatabaseFluentCalls = new(SymbolEqualityComparer.Default);
                _closureScopes = new();
            }

            public void VisitWithName(SyntaxNode node, string memberName)
            {
                _currentMemberName = memberName;
                Visit(node);
            }

            public override void Visit(SyntaxNode node)
            {
                if (node is MemberDeclarationSyntax)
                {
                    if (node is BaseTypeDeclarationSyntax)
                    {
                        return;
                    }

                    if (node is MethodDeclarationSyntax methodSyntax)
                    {
                        _currentMemberName = methodSyntax.Identifier.Text;
                    }
                    else if (node is PropertyDeclarationSyntax propertySyntax)
                    {
                        if (propertySyntax.ExpressionBody is not null)
                        {
                            _currentMemberName = "get_" + propertySyntax.Identifier.Text;
                        }
                        else if (propertySyntax.AccessorList is not null)
                        {
                            for (
                                var accessorIndex = 0;
                                accessorIndex < propertySyntax.AccessorList.Accessors.Count;
                                accessorIndex++
                            )
                            {
                                var accessor = propertySyntax.AccessorList.Accessors[accessorIndex];

                                if (
                                    accessor.IsKind(SyntaxKind.GetAccessorDeclaration)
                                    && propertySyntax.Initializer is null
                                )
                                {
                                    _currentMemberName = "get_" + propertySyntax.Identifier.Text;
                                }
                                else if (
                                    accessor.IsKind(SyntaxKind.SetAccessorDeclaration)
                                    && propertySyntax.Initializer is null
                                    && accessor.Body is not null
                                )
                                {
                                    _currentMemberName = "set_" + propertySyntax.Identifier.Text;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (node is FieldDeclarationSyntax)
                    {
                        return;
                    }
                    else if (node is ConstructorDeclarationSyntax constructorSyntax)
                    {
                        var isStaticConstructor = IsStaticMember(constructorSyntax);
                        _currentMemberName = isStaticConstructor ? ".cctor" : ".ctor";

                        List<MemberDeclarationSyntax> memebrs;

                        // TODO: How does this behave with different ctors?
                        if (isStaticConstructor)
                        {
                            if (HasStaticConstructor)
                                return;

                            HasStaticConstructor = true;
                            memebrs = _staticMembers;
                        }
                        else
                        {
                            if (HasInstanceConstructor)
                                return;

                            HasInstanceConstructor = true;
                            memebrs = _instanceMembers;
                        }

                        for (var memberIndex = 0; memberIndex < memebrs.Count; memberIndex++)
                        {
                            base.Visit(memebrs[memberIndex]);
                        }

                        memebrs.Clear();
                    }
                    else if (node is EventDeclarationSyntax eventSyntax)
                    {
                        for (
                            var accessorIndex = 0;
                            accessorIndex < eventSyntax.AccessorList!.Accessors.Count;
                            accessorIndex++
                        )
                        {
                            var accessor = eventSyntax.AccessorList.Accessors[accessorIndex];

                            if (
                                accessor.IsKind(SyntaxKind.AddAccessorDeclaration)
                                && accessor.Body is not null
                            )
                            {
                                _currentMemberName = "add_" + eventSyntax.Identifier.Text;
                            }
                            else if (
                                accessor.IsKind(SyntaxKind.RemoveAccessorDeclaration)
                                && accessor.Body is not null
                            )
                            {
                                _currentMemberName = "remove_" + eventSyntax.Identifier.Text;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else if (node is EventFieldDeclarationSyntax)
                    {
                        return;
                    }
                    else if (node is ConversionOperatorDeclarationSyntax conversionOperatorSyntax)
                    {
                        _currentMemberName =
                            conversionOperatorSyntax.ImplicitOrExplicitKeyword.Text switch
                            {
                                "implicit" => "op_Implicit",
                                "explicit" => "op_Explicit",
                                _
                                  => throw new InvalidOperationException(
                                      $"The conversion token '{conversionOperatorSyntax.ImplicitOrExplicitKeyword}' is not known, please report this on GitHub."
                                  )
                            };
                    }
                    else if (node is IndexerDeclarationSyntax indexSyntax)
                    {
                        if (indexSyntax.ExpressionBody is not null)
                        {
                            _currentMemberName = "get_Item";
                        }
                        else
                        {
                            for (
                                var accessorIndex = 0;
                                accessorIndex < indexSyntax.AccessorList!.Accessors.Count;
                                accessorIndex++
                            )
                            {
                                var accessor = indexSyntax.AccessorList!.Accessors[accessorIndex];

                                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                                {
                                    ;
                                    _currentMemberName = "get_Item";
                                }
                                else if (
                                    accessor.IsKind(SyntaxKind.SetAccessorDeclaration)
                                    && accessor.Body is not null
                                )
                                    _currentMemberName = "set_Item";
                            }
                        }
                    }
                    else if (node is OperatorDeclarationSyntax operatorSyntax)
                    {
                        _currentMemberName = operatorSyntax.Kind() switch
                        {
                            SyntaxKind.PlusToken => "op_Addition",
                            SyntaxKind.MinusToken => "op_Subtraction",
                            SyntaxKind.AsteriskToken => "op_Multiply",
                            SyntaxKind.SlashToken => "op_Division",
                            SyntaxKind.PercentToken => "op_Modulus",
                            SyntaxKind.GreaterThanToken => "op_GreaterThan",
                            SyntaxKind.LessThanToken => "op_LessThan",
                            SyntaxKind.LessThanLessThanToken => "op_LeftShift",
                            SyntaxKind.GreaterThanGreaterThanToken => "op_RightShift",
                            SyntaxKind.AmpersandToken => "op_BitwiseAnd",
                            SyntaxKind.BarToken => "op_BitwiseOr",
                            SyntaxKind.CaretToken => "op_ExclusiveOr",
                            _
                              => throw new InvalidOperationException(
                                  $"The operator token '{operatorSyntax.Kind()}' is not known, please report this on GitHub."
                              )
                        };
                    }
                    else if (node is DestructorDeclarationSyntax)
                    {
                        _currentMemberName = "Finalize";
                    }
                }
                else if (node is BlockSyntax blockSyntax)
                {
                    _closureScopes.EnterScope(blockSyntax.Span);

                    base.Visit(blockSyntax);

                    _closureScopes.LeaveScope();

                    return;
                }
                else if (node is LambdaExpressionSyntax lambdaSyntax)
                {
                    _semanticModel ??= _compilation.GetSemanticModel(
                        _classSyntax!.SyntaxTree,
                        true
                    );

                    var lambdaSymbol = _semanticModel.GetTypeInfo(lambdaSyntax).ConvertedType;

                    if (
                        lambdaSymbol is not null
                        && lambdaSymbol.GetFullName() == "System.Linq.Expressions.Expression"
                    )
                    {
                        return;
                    }

                    var dataFlow = _semanticModel.AnalyzeDataFlow(lambdaSyntax);

                    if (dataFlow is null || !dataFlow.Succeeded)
                    {
                        throw new InvalidOperationException();
                    }

                    var hasClosure = dataFlow.CapturedInside.Length > 0;

                    if (TryGetFluentCallBaseData(lambdaSyntax, out var data))
                    {
                        if (
                            !DatabaseFluentCalls.TryGetValue(
                                data.DatabaseSymbol,
                                out var fluentCalls
                            )
                        )
                        {
                            fluentCalls = new List<FluentCallDefinition>();

                            DatabaseFluentCalls.Add(data.DatabaseSymbol, fluentCalls);
                        }

                        var lambdaLink = new LambdaLinkDefinition(
                            _className,
                            _currentMemberName,
                            hasClosure ? uint.MaxValue : _lambdaIndex,
                            dataFlow.CapturedInside.Length > 0
                        );

                        if (hasClosure)
                        {
                            _closureScopes.AddFluentCallToScope(
                                dataFlow.CapturedInside,
                                lambdaLink
                            );
                        }

                        fluentCalls.Add(
                            new FluentCallDefinition(
                                _semanticModel,
                                lambdaSyntax,
                                data.Invocations,
                                lambdaLink
                            )
                        );
                    }

                    if (hasClosure)
                    {
                        _closureScopes.AddFluentCallToScope(dataFlow.CapturedInside);
                    }
                    else
                    {
                        _lambdaIndex++;
                    }
                }

                base.Visit(node);
            }

            private bool TryGetFluentCallBaseData(
                LambdaExpressionSyntax lambdaSyntax,
                out (ITypeSymbol DatabaseSymbol, List<InvocationExpressionSyntax> Invocations) data
            )
            {
                if (
                    lambdaSyntax.ExpressionBody
                        is not InterpolatedStringExpressionSyntax interpolatedStringSyntax
                    && (
                        lambdaSyntax.ExpressionBody is not LiteralExpressionSyntax expressionSyntax
                        || expressionSyntax.Kind() != SyntaxKind.StringLiteralExpression
                    )
                )
                {
                    data = default;

                    return false;
                }

                var invocations = new List<InvocationExpressionSyntax>();

                var lastInvocationSyntax =
                    lambdaSyntax.FirstAncestorOrSelf<InvocationExpressionSyntax>();

                while (lastInvocationSyntax is not null)
                {
                    invocations.Add(lastInvocationSyntax);

                    lastInvocationSyntax =
                        lastInvocationSyntax.Parent!.FirstAncestorOrSelf<InvocationExpressionSyntax>();
                }

                var memberAccessSyntax = (MemberAccessExpressionSyntax)invocations[0].Expression;

                ITypeSymbol databaseSymbol;

                if (
                    memberAccessSyntax.Expression
                    is MemberAccessExpressionSyntax tableMemberAccessSyntax
                )
                {
                    databaseSymbol = _semanticModel.GetTypeInfo(
                        tableMemberAccessSyntax.Expression
                    ).Type!;
                }
                else
                {
                    databaseSymbol = _semanticModel.GetTypeInfo(
                        memberAccessSyntax.Expression
                    ).Type!;
                }

                data = (databaseSymbol, invocations);

                return true;
            }

            internal void CollectLambdas(ClassDeclarationSyntax classSyntax)
            {
                _classSyntax = classSyntax;

                SyntaxNode? node = classSyntax;

                while (node is not BaseNamespaceDeclarationSyntax and not null)
                {
                    node = node.Parent;
                }

                if (node is null)
                {
                    throw new InvalidOperationException();
                }

                _className =
                    ((BaseNamespaceDeclarationSyntax)node).Name.ToString()
                    + "."
                    + classSyntax.Identifier.Text;

                base.Visit(classSyntax);

                _lambdaIndex = 0;
                _semanticModel = null;
                HasStaticConstructor = false;
                HasStaticConstructor = false;
            }
        }

        private class ScopeCollection
        {
            private readonly IndexedStack<Scope> _scopeStack;
            private readonly List<Scope> _scopes;

            internal ScopeCollection()
            {
                _scopeStack = new();
                _scopes = new();
            }

            internal void EnterScope(TextSpan span)
            {
                var scope = new Scope(span, _scopeStack.Count);
                _scopes.Add(scope);
                _scopeStack.Push(scope);
            }

            internal void AddFluentCallToScope(
                ImmutableArray<ISymbol> capturedVariables,
                LambdaLinkDefinition? lamdaLink = null
            )
            {
                if (_scopeStack.Count > 1)
                {
                    for (
                        var capturedSymbolIndex = 0;
                        capturedSymbolIndex < capturedVariables.Length;
                        capturedSymbolIndex++
                    )
                    {
                        var capturedSymbol = capturedVariables[capturedSymbolIndex];

                        var location = capturedSymbol.Locations[0].SourceSpan;

                        var scopeIndex = 0;

                        for (; scopeIndex < _scopeStack.Count; scopeIndex++)
                        {
                            var scope = _scopeStack[scopeIndex];

                            if (!scope.Span.OverlapsWith(location))
                            {
                                _scopeStack[scopeIndex - 1].LambdaLinks.Add(lamdaLink);

                                return;
                            }
                        }

                        _scopeStack[scopeIndex].LambdaLinks.Add(lamdaLink);
                    }
                }
                else
                {
                    _scopes[0].LambdaLinks.Add(lamdaLink);
                }
            }

            internal void LeaveScope()
            {
                _scopeStack.Pop();

                if (_scopeStack.Count == 0)
                    Build();
            }

            internal void Build()
            {
                _scopes.Sort((x, y) => x.Deepness.CompareTo(y.Deepness));

                uint consumedScopeIndex = 0;

                for (var scopeIndex = 0; scopeIndex < _scopes.Count; scopeIndex++)
                {
                    var scope = _scopes[scopeIndex];

                    for (var lambdaIndex = 0; lambdaIndex < scope.LambdaLinks.Count; lambdaIndex++)
                    {
                        var lambdaLink = scope.LambdaLinks[lambdaIndex];

                        if (lambdaLink is not null)
                        {
                            lambdaLink.LambdaIndex = consumedScopeIndex << 16 | ((uint)lambdaIndex);
                        }
                    }

                    if (scope.LambdaLinks.Count > 0)
                    {
                        consumedScopeIndex++;
                    }
                }
            }
        }

        private class Scope
        {
            internal TextSpan Span { get; }
            internal List<LambdaLinkDefinition?> LambdaLinks { get; }
            internal int Deepness { get; }

            internal Scope(TextSpan span, int deepness)
            {
                Span = span;
                Deepness = deepness;
                LambdaLinks = new();
            }
        }

        internal class SyntaxReceiver : ISyntaxContextReceiver
        {
            private static readonly HashSet<string> _validInvocationNames =
                new() { "Query", "QueryRaw", "Insert", "Update", "Delete" };

            internal HashSet<ClassDeclarationSyntax> Candidates { get; }

            internal SyntaxReceiver()
            {
                Candidates = new();
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is LambdaExpressionSyntax lambda)
                {
                    var classDeclaration = lambda.FirstAncestorOrSelf<ClassDeclarationSyntax>();

                    if (classDeclaration is null || Candidates.Contains(classDeclaration))
                    {
                        return;
                    }

                    var invocationSyntax = lambda.FirstAncestorOrSelf<InvocationExpressionSyntax>();

                    if (invocationSyntax is null)
                    {
                        return;
                    }

                    var memberAccessSyntax = invocationSyntax
                        .ChildNodes()
                        .OfType<MemberAccessExpressionSyntax>()
                        .FirstOrDefault();

                    if (
                        memberAccessSyntax is null
                        || !_validInvocationNames.Contains(memberAccessSyntax.Name.Identifier.Text)
                    )
                    {
                        return;
                    }

                    Candidates.Add(classDeclaration);
                }
            }
        }
    }
}
