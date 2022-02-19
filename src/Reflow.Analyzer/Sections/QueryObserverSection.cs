using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.Models.Definitions;
using Reflow.Analyzer.Shared;

namespace Reflow.Analyzer.Sections
{
    internal sealed class SourceTextStream : Stream
    {
        private readonly SourceText _source;
        private readonly Encoding _encoding;
        private readonly Encoder _encoder;

        private readonly int _minimumTargetBufferCount;
        private int _position;
        private int _sourceOffset;
        private readonly char[] _charBuffer;
        private int _bufferOffset;
        private int _bufferUnreadChars;
        private bool _preambleWritten;

        private static readonly Encoding s_utf8EncodingWithNoBOM = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: false
        );

        public SourceTextStream(SourceText source, int bufferSize = 2048)
        {
            _source = source;
            _encoding = source.Encoding ?? s_utf8EncodingWithNoBOM;
            _encoder = _encoding.GetEncoder();
            _minimumTargetBufferCount = _encoding.GetMaxByteCount(charCount: 1);
            _sourceOffset = 0;
            _position = 0;
            _charBuffer = new char[Math.Min(bufferSize, _source.Length)];
            _bufferOffset = 0;
            _bufferUnreadChars = 0;
            _preambleWritten = false;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override void Flush() => throw new NotSupportedException();

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get { return _position; }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count < _minimumTargetBufferCount)
            {
                // The buffer must be able to hold at least one character from the
                // SourceText stream.  Returning 0 for that case isn't correct because
                // that indicates end of stream vs. insufficient buffer.
                throw new ArgumentException(
                    $"{nameof(count)} must be greater than or equal to {_minimumTargetBufferCount}",
                    nameof(count)
                );
            }

            var originalCount = count;

            if (!_preambleWritten)
            {
                var bytesWritten = WritePreamble(buffer, offset, count);
                offset += bytesWritten;
                count -= bytesWritten;
            }

            while (count >= _minimumTargetBufferCount && _position < _source.Length)
            {
                if (_bufferUnreadChars == 0)
                {
                    FillBuffer();
                }

                _encoder.Convert(
                    _charBuffer,
                    _bufferOffset,
                    _bufferUnreadChars,
                    buffer,
                    offset,
                    count,
                    flush: false,
                    charsUsed: out var charsUsed,
                    bytesUsed: out var bytesUsed,
                    completed: out _
                );
                _position += charsUsed;
                _bufferOffset += charsUsed;
                _bufferUnreadChars -= charsUsed;
                offset += bytesUsed;
                count -= bytesUsed;
            }

            // Return value is the number of bytes read
            return originalCount - count;
        }

        private int WritePreamble(byte[] buffer, int offset, int count)
        {
            _preambleWritten = true;
            var preambleBytes = _encoding.GetPreamble();
            if (preambleBytes == null)
            {
                return 0;
            }

            var length = Math.Min(count, preambleBytes.Length);
            Array.Copy(preambleBytes, 0, buffer, offset, length);
            return length;
        }

        private void FillBuffer()
        {
            var charsToRead = Math.Min(_charBuffer.Length, _source.Length - _sourceOffset);
            _source.CopyTo(_sourceOffset, _charBuffer, 0, charsToRead);
            _sourceOffset += charsToRead;
            _bufferOffset = 0;
            _bufferUnreadChars = charsToRead;
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }

    internal class QueryObserverSection
        : GeneratorSection<
              CommandObserverSection,
              QueryObserverSection.SyntaxReceiver,
              Dictionary<ITypeSymbol, List<FluentCallDefinition>>
          >
    {
        protected override Dictionary<ITypeSymbol, List<FluentCallDefinition>> Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            CommandObserverSection previous
        )
        {
            syntaxReceiver.SW.Stop();

            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "NONINCREMNTAL",
                        "Old Time",
                        "{0} ms",
                        "main",
                        DiagnosticSeverity.Warning,
                        true
                    ),
                    null,
                    syntaxReceiver.SW.ElapsedMilliseconds
                )
            );

            var lambdaCache =
                Cache.GetData<Dictionary<string, LambdaLinkDefinition[]>>("lambdas.json") ?? new();

            var refreshCache = false;

            var instanceMembers = new List<MemberDeclarationSyntax>();
            var staticMembers = new List<MemberDeclarationSyntax>();

            var classSyntaxWalker = new ClassSyntaxWalker(
                context.Compilation,
                instanceMembers,
                staticMembers
            );

            var fastClassSyntaxWalker = new FastClassSyntaxWalker(
                context.Compilation,
                classSyntaxWalker.DatabaseFluentCalls
            );

            using var hash = SHA256.Create();

            foreach (var classSyntax in syntaxReceiver.Candidates)
            {
                using Stream stream = new SourceTextStream(
                    classSyntax.GetText(Encoding.UTF8, SourceHashAlgorithm.Sha256)
                );

                var classHash = Convert.ToBase64String(hash.ComputeHash(stream));

                if (!lambdaCache.TryGetValue(classHash, out var lambdas))
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

                    lambdaCache.Add(
                        classHash,
                        classSyntaxWalker.DatabaseFluentCalls.Values
                            .SelectMany(x => x)
                            .Select(x => x.LambdaLink)
                            .ToArray()
                    );

                    refreshCache = true;
                }
                else
                {
                    fastClassSyntaxWalker.CollectLambdas(classSyntax, lambdas);
                }
            }

            if (refreshCache)
                Cache.SetData("lambdas.json", lambdaCache);

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

        private class FastClassSyntaxWalker : SyntaxWalker
        {
            private ClassDeclarationSyntax _classSyntax = null!;
            private LambdaLinkDefinition[] _cachedLambdaLinks = null!;
            private int _cachedLambdaLinkIndex;
            private SemanticModel? _semanticModel;

            private readonly Compilation _compilation;
            private readonly Dictionary<
                ITypeSymbol,
                List<FluentCallDefinition>
            > _databaseFluentCalls;

            internal FastClassSyntaxWalker(
                Compilation compilation,
                Dictionary<ITypeSymbol, List<FluentCallDefinition>> databaseFluentCalls
            )
            {
                _compilation = compilation;
                _databaseFluentCalls = databaseFluentCalls;
                _cachedLambdaLinkIndex = 0;
            }

            public override void Visit(SyntaxNode node)
            {
                if (node is LambdaExpressionSyntax lambdaSyntax)
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

                    if (TryGetFluentCallBaseData(lambdaSyntax, out var data))
                    {
                        if (
                            !_databaseFluentCalls.TryGetValue(
                                data.DatabaseSymbol,
                                out var fluentCalls
                            )
                        )
                        {
                            fluentCalls = new List<FluentCallDefinition>();

                            _databaseFluentCalls.Add(data.DatabaseSymbol, fluentCalls);
                        }

                        fluentCalls.Add(
                            new FluentCallDefinition(
                                _semanticModel,
                                lambdaSyntax,
                                data.Invocations,
                                _cachedLambdaLinks[_cachedLambdaLinkIndex++]
                            )
                        );
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

            internal void CollectLambdas(
                ClassDeclarationSyntax classSyntax,
                LambdaLinkDefinition[] cachedLambdaLinks
            )
            {
                _classSyntax = classSyntax;
                _cachedLambdaLinks = cachedLambdaLinks;

                base.Visit(classSyntax);

                _semanticModel = null;
                _cachedLambdaLinkIndex = 0;
            }
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
                    else if (hasClosure)
                    {
                        _closureScopes.AddFluentCallToScope(dataFlow.CapturedInside);
                    }

                    if (!hasClosure)
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
            public Stopwatch SW = new();

            private static readonly HashSet<string> _validInvocationNames =
                new() { "Query", "QueryRaw" };

            internal HashSet<ClassDeclarationSyntax> Candidates { get; }

            internal SyntaxReceiver()
            {
                Candidates = new();
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (!SW.IsRunning)
                    SW.Restart();

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
