using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.LambdaLinker
{
    [Generator(LanguageNames.CSharp)]
    public class LambdaLinkGenerator : ISourceGenerator
    {
        void ISourceGenerator.Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        void ISourceGenerator.Execute(GeneratorExecutionContext context)
        {
            var candidates = (context.SyntaxContextReceiver as SyntaxContextReceiver)!.Candidates;
            var lambdaCollector = new LambdaCollector(context.Compilation);

            var members = new List<SyntaxNode>();
            var instanceVariableMembers = new List<MemberDeclarationSyntax>();
            var staticVariableMembers = new List<MemberDeclarationSyntax>();

            foreach (var classSyntax in candidates)
            {
                lambdaCollector.SetClassScope(classSyntax);

                foreach (var childNode in classSyntax.ChildNodes())
                {
                    switch (childNode)
                    {
                        case BaseTypeDeclarationSyntax:
                            lambdaCollector.IncrementMemberIndex();
                            continue;
                        default:
                            break;
                    }

                    members.Add(childNode);

                    switch (childNode)
                    {
                        case FieldDeclarationSyntax:
                        case PropertyDeclarationSyntax
                        {
                            Initializer: not null
                        }:
                            break;
                        default:
                            continue;
                    }

                    var member = (MemberDeclarationSyntax)childNode;

                    if (IsStaticMember(member))
                    {
                        staticVariableMembers.Add(member);
                    }
                    else
                    {
                        instanceVariableMembers.Add(member);
                    }
                }

                var hasInstanceConstructor = false;
                var hasStaticConstructor = false;

                for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
                {
                    var member = members[memberIndex];

                    if (member is MethodDeclarationSyntax methodSyntax)
                    {
                        lambdaCollector.Collect(member, methodSyntax.Identifier.Text);
                        lambdaCollector.ResetLambdaIndex();
                    }
                    else if (member is PropertyDeclarationSyntax propertySyntax)
                    {
                        if (propertySyntax.ExpressionBody is not null)
                        {
                            lambdaCollector.IncrementMemberIndex();
                            lambdaCollector.Collect(
                                propertySyntax.ExpressionBody,
                                "get_" + propertySyntax.Identifier.Text
                            );
                        }
                        else if (propertySyntax.AccessorList is not null)
                        {
                            for (
                                var accessorIndex = 0;
                                accessorIndex < propertySyntax.AccessorList.Accessors.Count;
                                accessorIndex++
                            ) {
                                var accessor = propertySyntax.AccessorList.Accessors[accessorIndex];

                                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                                {
                                    lambdaCollector.IncrementMemberIndex();

                                    if (accessor.Body is null)
                                    {
                                        lambdaCollector.IncrementMemberIndex();
                                    }
                                    else if (propertySyntax.Initializer is null)
                                    {
                                        lambdaCollector.Collect(
                                            accessor.Body,
                                            "get_" + propertySyntax.Identifier.Text
                                        );
                                    }
                                }
                                else if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
                                {
                                    lambdaCollector.IncrementMemberIndex();

                                    if (
                                        propertySyntax.Initializer is null
                                        && accessor.Body is not null
                                    ) {
                                        lambdaCollector.Collect(
                                            accessor.Body,
                                            "set" + propertySyntax.Identifier.Text
                                        );
                                    }
                                }
                            }
                        }
                        lambdaCollector.ResetLambdaIndex();
                    }
                    else if (member is FieldDeclarationSyntax fieldSyntax)
                    {
                        lambdaCollector.IncrementMemberIndex(
                            fieldSyntax.Declaration.Variables.Count - 1
                        );
                    }
                    else if (member is ConstructorDeclarationSyntax constructorSyntax)
                    {
                        var isStaticConstructor = IsStaticMember(constructorSyntax);
                        var identifierName = isStaticConstructor ? ".cctor" : ".ctor";

                        lambdaCollector.Collect(member, identifierName);

                        if (isStaticConstructor)
                        {
                            hasStaticConstructor = true;
                        }
                        else
                        {
                            hasInstanceConstructor = true;
                        }

                        var variableMembers = isStaticConstructor
                            ? staticVariableMembers
                            : instanceVariableMembers;

                        for (
                            var variableMemberIndex = 0;
                            variableMemberIndex < variableMembers.Count;
                            variableMemberIndex++
                        ) {
                            var variableMember = variableMembers[variableMemberIndex];

                            if (variableMember is FieldDeclarationSyntax field)
                            {
                                for (
                                    var fieldVariableIndex = 0;
                                    fieldVariableIndex < field.Declaration.Variables.Count;
                                    fieldVariableIndex++
                                ) {
                                    var fieldVariable = field.Declaration.Variables[
                                        fieldVariableIndex
                                    ];

                                    lambdaCollector.Collect(fieldVariable, identifierName);
                                }
                            }
                            else if (variableMember is PropertyDeclarationSyntax property)
                            {
                                lambdaCollector.Collect(property.Initializer!, identifierName);
                            }
                        }

                        lambdaCollector.ResetLambdaIndex();
                    }
                    else if (member is EventDeclarationSyntax eventSyntax)
                    {
                        for (
                            var accessorIndex = 0;
                            accessorIndex < eventSyntax.AccessorList!.Accessors.Count;
                            accessorIndex++
                        ) {
                            var accessor = eventSyntax.AccessorList.Accessors[accessorIndex];

                            if (accessor.IsKind(SyntaxKind.AddAccessorDeclaration))
                            {
                                lambdaCollector.IncrementMemberIndex();

                                if (accessor.Body is not null)
                                {
                                    lambdaCollector.Collect(
                                        accessor.Body,
                                        "add_" + eventSyntax.Identifier.Text
                                    );
                                }
                            }
                            else if (accessor.IsKind(SyntaxKind.RemoveAccessorDeclaration))
                            {
                                lambdaCollector.IncrementMemberIndex();

                                if (accessor.Body is not null)
                                {
                                    lambdaCollector.Collect(
                                        accessor.Body,
                                        "remove_" + eventSyntax.Identifier.Text
                                    );
                                }
                            }
                        }

                        lambdaCollector.ResetLambdaIndex();
                    }
                    else if (member is EventFieldDeclarationSyntax)
                    {
                        lambdaCollector.IncrementMemberIndex(2);
                    }
                    else if (member is ConversionOperatorDeclarationSyntax conversionOperatorSyntax)
                    {
                        var identifierName =
                            conversionOperatorSyntax.ImplicitOrExplicitKeyword.Text switch
                            {
                                "implicit" => "op_Implicit",
                                "explicit" => "op_Explicit",
                                _
                                  => throw new InvalidOperationException(
                                      $"The conversion token '{conversionOperatorSyntax.ImplicitOrExplicitKeyword}' is not known, please report this on GitHub."
                                  )
                            };

                        lambdaCollector.Collect(member, identifierName);
                        lambdaCollector.ResetLambdaIndex();
                    }
                    else if (member is IndexerDeclarationSyntax indexSyntax)
                    {
                        if (indexSyntax.ExpressionBody is not null)
                        {
                            lambdaCollector.IncrementMemberIndex();
                            lambdaCollector.Collect(indexSyntax.ExpressionBody, "get_Item");
                        }
                        else
                        {
                            for (
                                var accessorIndex = 0;
                                accessorIndex < indexSyntax.AccessorList!.Accessors.Count;
                                accessorIndex++
                            ) {
                                var accessor = indexSyntax.AccessorList!.Accessors[accessorIndex];

                                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                                {
                                    lambdaCollector.IncrementMemberIndex();
                                    lambdaCollector.Collect(accessor.Body!, "get_Item");
                                }
                                else if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
                                {
                                    lambdaCollector.IncrementMemberIndex();

                                    if (accessor.Body is not null)
                                        lambdaCollector.Collect(accessor.Body, "set_Item");
                                }
                            }
                        }

                        lambdaCollector.ResetLambdaIndex();
                    }
                    else if (member is OperatorDeclarationSyntax operatorSyntax)
                    {
                        var identifierName = operatorSyntax.Kind() switch
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

                        lambdaCollector.Collect(member, identifierName);
                        lambdaCollector.ResetLambdaIndex();
                    }
                    else if (member is DestructorDeclarationSyntax)
                    {
                        lambdaCollector.Collect(member, "Finalize");
                        lambdaCollector.ResetLambdaIndex();
                    }

                    if (member is MemberDeclarationSyntax)
                    {
                        lambdaCollector.IncrementMemberIndex();
                    }
                }

                if (!hasInstanceConstructor && instanceVariableMembers.Count > 0)
                {
                    for (
                        var variableMemberIndex = 0;
                        variableMemberIndex < instanceVariableMembers.Count;
                        variableMemberIndex++
                    ) {
                        var variableMember = instanceVariableMembers[variableMemberIndex];

                        if (variableMember is FieldDeclarationSyntax fieldSyntax)
                        {
                            for (
                                var fieldVariableIndex = 0;
                                fieldVariableIndex < fieldSyntax.Declaration.Variables.Count;
                                fieldVariableIndex++
                            ) {
                                var fieldVariable = fieldSyntax.Declaration.Variables[
                                    fieldVariableIndex
                                ];

                                lambdaCollector.Collect(fieldVariable, ".ctor");
                            }
                        }
                        else if (variableMember is PropertyDeclarationSyntax propertySyntax)
                        {
                            lambdaCollector.Collect(propertySyntax.Initializer!, ".ctor");
                        }
                    }

                    lambdaCollector.ResetLambdaIndex();
                }
                if (!hasStaticConstructor && staticVariableMembers.Count > 0)
                {
                    if (!hasInstanceConstructor && instanceVariableMembers.Count > 0)
                        lambdaCollector.IncrementMemberIndex();

                    for (
                        var variableMemberIndex = 0;
                        variableMemberIndex < staticVariableMembers.Count;
                        variableMemberIndex++
                    ) {
                        var variableMember = staticVariableMembers[variableMemberIndex];

                        if (variableMember is FieldDeclarationSyntax fieldSyntax)
                        {
                            for (
                                var fieldVariableIndex = 0;
                                fieldVariableIndex < fieldSyntax.Declaration.Variables.Count;
                                fieldVariableIndex++
                            ) {
                                lambdaCollector.Collect(
                                    fieldSyntax.Declaration.Variables[fieldVariableIndex],
                                    ".cctor"
                                );
                            }
                        }
                        else if (variableMember is PropertyDeclarationSyntax propertySyntax)
                        {
                            lambdaCollector.Collect(propertySyntax.Initializer!, ".cctor");
                        }
                    }

                    lambdaCollector.ResetLambdaIndex();
                }

                lambdaCollector.ResetMemberIndex();

                members.Clear();
                instanceVariableMembers.Clear();
                staticVariableMembers.Clear();
            }

            var (links, closureLinks) = lambdaCollector.Build();

            context.AddTemplatedSource(
                "Lambdas/Resources/LambdaLinks.sbncs",
                new { links, closureLinks }
            );
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

        private class LambdaCollector
        {
            private int _memberIndex;
            private int _lambdaIndex;
            private ClassDeclarationSyntax _classSyntax = null!;
            private string _className = null!;
            private SemanticModel? _semanticModel;

            private readonly List<LambdaLink> _lambdaLinks;
            private readonly List<ClosureLambdaLink> _closureLambdaLinks;
            private readonly Queue<SyntaxNode> _nodeQueue;
            private readonly Stack<SyntaxNode> _nodeStack;
            private readonly List<SyntaxNode> _nodeBuffer;
            private readonly Compilation _compilation;

            internal LambdaCollector(Compilation compilation)
            {
                _compilation = compilation;

                _lambdaLinks = new();
                _closureLambdaLinks = new();
                _nodeQueue = new();
                _nodeStack = new();
                _nodeBuffer = new();
            }

            internal void IncrementMemberIndex(int count = 1)
            {
                _memberIndex += count;
            }

            internal void IncrementLambdaIndex(int count = 1)
            {
                _lambdaIndex += count;
            }

            internal void ResetLambdaIndex()
            {
                _lambdaIndex = 0;
            }

            internal void ResetMemberIndex()
            {
                _memberIndex = 0;
            }

            internal void SetClassScope(ClassDeclarationSyntax syntax)
            {
                _classSyntax = syntax;

                SyntaxNode? node = syntax;

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
                    + syntax.Identifier.Text;

                _semanticModel = null;
            }

            internal void Collect(SyntaxNode syntaxNode, string identifier)
            {
                _nodeStack.Push(syntaxNode);

                while (_nodeQueue.Count > 0 || _nodeStack.Count > 0)
                {
                    var node = _nodeStack.Count > 0 ? _nodeStack.Pop() : _nodeQueue.Dequeue();

                    var traverseIntoChildren = true;

                    if (node is BlockSyntax)
                    {
                        traverseIntoChildren = false;

                        IncrementLambdaIndex();
                    }
                    else if (node is LambdaExpressionSyntax lambdaSyntax)
                    {
                        _semanticModel ??= _compilation.GetSemanticModel(
                            _classSyntax!.SyntaxTree,
                            true
                        );
                        var dataFlow = _semanticModel.AnalyzeDataFlow(lambdaSyntax);

                        if (dataFlow is null || !dataFlow.Succeeded)
                        {
                            throw new InvalidOperationException();
                        }

                        var content = lambdaSyntax.ExpressionBody!.ToString().Replace("\"", "\"\"");

                        if (dataFlow.CapturedInside.Length == 0)
                        {
                            _lambdaLinks.Add(
                                new LambdaLink(
                                    _className,
                                    $"<{identifier}>b__{_memberIndex}_{_lambdaIndex}",
                                    content
                                )
                            );
                        }
                        else
                        {
                            _closureLambdaLinks.Add(
                                new ClosureLambdaLink(
                                    _className,
                                    _memberIndex,
                                    $"<{identifier}>b__{_lambdaIndex}",
                                    content
                                )
                            );
                        }

                        IncrementLambdaIndex();
                    }

                    if (traverseIntoChildren)
                    {
                        foreach (var childNode in node.ChildNodes())
                        {
                            _nodeBuffer.Add(childNode);
                        }
                        if (_nodeBuffer.Count > 0)
                        {
                            for (
                                var bufferIndex = _nodeBuffer.Count - 1;
                                bufferIndex >= 0;
                                bufferIndex--
                            ) {
                                _nodeStack.Push(_nodeBuffer[bufferIndex]);
                            }

                            _nodeBuffer.Clear();
                        }
                    }
                    else
                    {
                        foreach (var childNode in node.ChildNodes())
                        {
                            _nodeQueue.Enqueue(childNode);
                        }
                    }
                }

                _nodeQueue.Clear();
                _nodeStack.Clear();
            }

            internal (List<LambdaLink> Links, List<ClosureLambdaLink> ClosureLinks) Build() =>
                (_lambdaLinks, _closureLambdaLinks);
        }

        private class SyntaxContextReceiver : ISyntaxContextReceiver
        {
            internal HashSet<ClassDeclarationSyntax> Candidates { get; }

            internal SyntaxContextReceiver()
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

                    var memberAccessSyntax = invocationSyntax.ChildNodes()
                        .OfType<MemberAccessExpressionSyntax>()
                        .FirstOrDefault();

                    if (
                        memberAccessSyntax is null
                        || memberAccessSyntax.Name.Identifier.Text is not "Query" and not "QueryRaw"
                    ) {
                        return;
                    }

                    var symbol = context.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol;

                    if (symbol is null)
                    {
                        return;
                    }

                    var fullName = symbol.ContainingSymbol.ToDisplayString(); // TOOO: Check for the name and assembly

                    Candidates.Add(classDeclaration);
                }
            }
        }
    }
}
