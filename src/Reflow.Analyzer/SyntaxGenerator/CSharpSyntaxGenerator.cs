using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Reflow.Analyzer.SyntaxGenerator.CSharpSyntaxGenerator;

namespace Reflow.Analyzer.SyntaxGenerator
{
    internal static class CSharpSyntaxGenerator
    {
        internal static FileSyntax File(string namespaceName)
        {
            return new FileSyntax(namespaceName);
        }
        internal static ClassSyntax Class(string name, Modifiers modifiers = Modifiers.None)
        {
            return new ClassSyntax(name, modifiers);
        }

        internal static ConstructorSyntax Constructor(
            string name,
            Modifiers modifiers = Modifiers.None
        )
        {
            return new ConstructorSyntax(name, modifiers);
        }
        internal static CSharpParameterSyntax Parameter(string name, TypeSyntax type)
        {
            return new CSharpParameterSyntax(name, type, Modifiers.None);
        }
        internal static PropertySyntax Property(
            string name,
            TypeSyntax type,
            Modifiers modifiers = Modifiers.None
        )
        {
            return new PropertySyntax(name, type, modifiers);
        }
        internal static FieldSyntax Field(
            string name,
            TypeSyntax type,
            Modifiers modifiers = Modifiers.None
        )
        {
            return new FieldSyntax(name, type, modifiers);
        }

        internal static LocalSyntax Local(
            string name,
            TypeSyntax type,
            Modifiers modifiers = Modifiers.None
        )
        {
            return new LocalSyntax(name, type, modifiers);
        }

        internal static InstanceSyntax Instance(TypeSyntax type)
        {
            return new InstanceSyntax(type);
        }

        internal static ArrayCreationExpressionSyntax ArrayInitializer(
            ArrayTypeSyntax type,
            params ExpressionSyntax[] expressions
        )
        {
            return ArrayInitializer(type, (IEnumerable<ExpressionSyntax>)expressions);
        }
        internal static ArrayCreationExpressionSyntax ArrayInitializer(
            ArrayTypeSyntax type,
            IEnumerable<ExpressionSyntax> expressions
        )
        {
            return ArrayCreationExpression(
                type,
                InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SeparatedList(expressions)
                )
            );
        }

        internal static InitializerExpressionSyntax DictionaryInitializer(
            IEnumerable<InitializerExpressionSyntax> expressions
        )
        {
            return InitializerExpression(
                SyntaxKind.CollectionInitializerExpression,
                SeparatedList<ExpressionSyntax>(expressions)
            );
        }

        internal static InitializerExpressionSyntax DictionaryEntry(
            ExpressionSyntax key,
            ExpressionSyntax value
        )
        {
            return InitializerExpression(
                SyntaxKind.ComplexElementInitializerExpression,
                SeparatedList<ExpressionSyntax>(new[] { key, value })
            );
        }

        internal static TypeSyntax Type(string name)
        {
            return Name(name);
        }
        internal static TypeSyntax Type(ITypeSymbol symbol)
        {
            return Name(symbol.GetFullName());
        }
        internal static TypeSyntax Type(TypeCode type)
        {
            return Name(
                "System."
                    + (
                        type switch
                        {
                            TypeCode.Empty => throw new ArgumentException(),
                            _ => type.ToString()
                        }
                    )
            );
        }
        internal static ArrayTypeSyntax Array(TypeSyntax type, int length = -1)
        {
            var arrayType = ArrayType(type);

            if (length < 0)
            {
                arrayType = arrayType.AddRankSpecifiers(
                    ArrayRankSpecifier(
                        SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())
                    )
                );
            }

            return arrayType;
        }
        internal static TypeSyntax GenericType(string name, params TypeSyntax[] types)
        {
            var sections = name.Split('.');

            var genericName = GenericName(Identifier(sections[sections.Length - 1]))
                .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(types)));

            if (sections.Length == 1)
                return genericName;

            if (sections.Length == 2)
            {
                return QualifiedName(IdentifierName(sections[0]), genericName);
            }

            var qualifiedName = QualifiedName(
                IdentifierName(sections[0]),
                IdentifierName(sections[1])
            );

            for (var sectionIndex = 2; sectionIndex < sections.Length - 1; sectionIndex++)
            {
                qualifiedName = QualifiedName(
                    qualifiedName,
                    IdentifierName(sections[sectionIndex])
                );
            }

            return QualifiedName(qualifiedName, genericName);
        }
        internal static NameSyntax Name(string name)
        {
            var sections = name.Split('.');

            if (sections.Length == 1)
                return IdentifierName(name);

            var qualifiedName = QualifiedName(
                IdentifierName(sections[0]),
                IdentifierName(sections[1])
            );

            for (var sectionIndex = 2; sectionIndex < sections.Length; sectionIndex++)
            {
                qualifiedName = QualifiedName(
                    qualifiedName,
                    IdentifierName(sections[sectionIndex])
                );
            }

            return qualifiedName;
        }
        internal static SimpleNameSyntax IdentifierName(string name)
        {
            return SyntaxFactory.IdentifierName(name);
        }

        internal static LiteralExpressionSyntax Constant(short value)
        {
            return Constant((int)value);
        }
        internal static LiteralExpressionSyntax Constant(int value)
        {
            return LiteralExpression(SyntaxKind.NumericLiteralExpression).WithToken(Literal(value));
        }
        internal static LiteralExpressionSyntax Constant(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression).WithToken(Literal(value));
        }

        internal static CastExpressionSyntax Cast(TypeSyntax toType, ExpressionSyntax expression)
        {
            return CastExpression(toType, expression);
        }
        internal static TypeOfExpressionSyntax TypeOf(TypeSyntax type)
        {
            return SyntaxFactory.TypeOfExpression(type);
        }
        internal static LambdaSyntax Lambda(params string[] parameters)
        {
            return new LambdaSyntax(parameters);
        }

        internal static IfSyntax If(ExpressionSyntax condition, params StatementSyntax[] statements)
        {
            return new IfSyntax(condition, statements);
        }

        internal static ExpressionStatementSyntax AssignLocal(
            ExpressionSyntax local,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return ExpressionStatement(AssignmentExpression(operation, local, right));
        }

        internal static ExpressionStatementSyntax AssignMember(
            string member,
            string memberName,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return AssignMember(
                IdentifierName(member),
                IdentifierName(memberName),
                right,
                operation
            );
        }

        internal static ExpressionStatementSyntax AssignMember(
            ExpressionSyntax member,
            string memberName,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return AssignMember(member, IdentifierName(memberName), right, operation);
        }

        internal static ExpressionStatementSyntax AssignMember(
            ExpressionSyntax member,
            SimpleNameSyntax memberName,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return ExpressionStatement(
                AssignmentExpression(
                    operation,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        member,
                        memberName
                    ),
                    right
                )
            );
        }

        internal static MemberAccessExpressionSyntax AccessMember(
            ExpressionSyntax member,
            string memberName
        )
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                member,
                IdentifierName(memberName)
            );
        }

        internal static BaseExpressionSyntax Base()
        {
            return BaseExpression();
        }

        internal static ThisExpressionSyntax This()
        {
            return ThisExpression();
        }

        internal static ExpressionSyntax Value()
        {
            return IdentifierName("value");
        }

        internal static ReturnStatementSyntax Return(ExpressionSyntax? syntax = null)
        {
            return ReturnStatement(syntax);
        }

        internal static BinaryExpressionSyntax IsBitSet(
            ExpressionSyntax expression,
            TypeSyntax type,
            LiteralExpressionSyntax index
        )
        {
            return BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                Cast(
                    type,
                    ParenthesizedExpression(
                        BinaryExpression(SyntaxKind.BitwiseAndExpression, expression, index)
                    )
                ),
                Constant(0)
            );
        }
    }

    internal class FileSyntax
    {
        private NamespaceDeclarationSyntax _namespaceSyntax;

        public FileSyntax(string namespaceName)
        {
            _namespaceSyntax = NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceName));
        }

        public FileSyntax WithMembers(params SyntaxNode[] members)
        {
            return WithMembers((IEnumerable<SyntaxNode>)members);
        }

        public FileSyntax WithMembers(IEnumerable<SyntaxNode> members)
        {
            _namespaceSyntax = _namespaceSyntax.WithMembers(List(members));

            return this;
        }

        public FileSyntax WithUsings(params string[] members)
        {
            return WithUsings((IEnumerable<string>)members);
        }

        public FileSyntax WithUsings(IEnumerable<string> members)
        {
            _namespaceSyntax = _namespaceSyntax.WithUsings(
                List(members.Select(x => UsingDirective(Name(x))))
            );

            return this;
        }

        public SourceText GetText()
        {
            return CompilationUnit()
                .WithMembers(SingletonList<MemberDeclarationSyntax>(_namespaceSyntax))
                .NormalizeWhitespace()
                .GetText(Encoding.UTF8);
        }
    }

    internal class ClassSyntax
    {
        private ClassDeclarationSyntax _classSyntax;

        public ClassSyntax(string name, Modifiers modifiers)
        {
            _classSyntax = ClassDeclaration(name);

            if (modifiers != Modifiers.None)
            {
                _classSyntax = _classSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator SyntaxNode(ClassSyntax syntax)
        {
            return syntax._classSyntax;
        }

        public ClassSyntax WithMembers(params SyntaxNode[] members)
        {
            return WithMembers((IEnumerable<SyntaxNode>)members);
        }

        public ClassSyntax WithMembers(IEnumerable<SyntaxNode> members)
        {
            _classSyntax = _classSyntax.WithMembers(List(members));
            return this;
        }

        public ClassSyntax WithBase(TypeSyntax type)
        {
            _classSyntax = _classSyntax.WithBaseList(
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(type)))
            );

            return this;
        }
    }

    internal class ConstructorSyntax
    {
        private ConstructorDeclarationSyntax _constructorSyntax;

        public ConstructorSyntax(string name, Modifiers modifiers)
        {
            _constructorSyntax = ConstructorDeclaration(name);

            if (modifiers != Modifiers.None)
            {
                _constructorSyntax = _constructorSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator ConstructorDeclarationSyntax(ConstructorSyntax syntax)
        {
            return syntax._constructorSyntax;
        }

        public ConstructorSyntax WithParameters(params CSharpParameterSyntax[] parameters)
        {
            _constructorSyntax = _constructorSyntax.WithParameterList(
                ParameterList(SeparatedList(parameters.Select(x => (ParameterSyntax)x)))
            );

            return this;
        }

        public ConstructorSyntax WithStatements(params StatementSyntax[] statements)
        {
            return WithStatements((IEnumerable<StatementSyntax>)statements);
        }

        public ConstructorSyntax WithStatements(IEnumerable<StatementSyntax> statements)
        {
            _constructorSyntax = _constructorSyntax.WithBody(Block(statements));

            return this;
        }
    }

    internal class CSharpParameterSyntax
    {
        private ParameterSyntax _parameterSyntax;

        public CSharpParameterSyntax(string name, TypeSyntax type, Modifiers modifiers)
        {
            _parameterSyntax = Parameter(Identifier(name)).WithType(type);

            if (modifiers != Modifiers.None)
            {
                _parameterSyntax = _parameterSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator ParameterSyntax(CSharpParameterSyntax syntax)
        {
            return syntax._parameterSyntax;
        }

        public CSharpParameterSyntax WithDefault(SyntaxKind syntax)
        {
            _parameterSyntax = _parameterSyntax.WithDefault(
                EqualsValueClause(LiteralExpression(syntax))
            );

            return this;
        }
    }

    internal class PropertySyntax
    {
        private PropertyDeclarationSyntax _propertySyntax;

        public PropertySyntax(string name, TypeSyntax type, Modifiers modifiers)
        {
            _propertySyntax = PropertyDeclaration(type, name);

            if (modifiers != Modifiers.None)
            {
                _propertySyntax = _propertySyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator MemberDeclarationSyntax(PropertySyntax syntax)
        {
            return syntax._propertySyntax;
        }

        public PropertySyntax WithInitializer(ExpressionSyntax expression)
        {
            _propertySyntax = _propertySyntax.WithInitializer(EqualsValueClause(expression));

            return this;
        }

        public PropertySyntax WithGetAccessor(params StatementSyntax[] statements)
        {
            _propertySyntax = _propertySyntax.AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(Block(List(statements)))
            );

            return this;
        }

        public PropertySyntax WithSetAccessor(params StatementSyntax[] statements)
        {
            _propertySyntax = _propertySyntax.AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(Block(List(statements)))
            );

            return this;
        }
    }

    internal class FieldSyntax
    {
        private FieldDeclarationSyntax _fieldSyntax;

        public FieldSyntax(string name, TypeSyntax type, Modifiers modifiers)
        {
            _fieldSyntax = FieldDeclaration(
                VariableDeclaration(type, SingletonSeparatedList(VariableDeclarator(name)))
            );

            if (modifiers != Modifiers.None)
            {
                _fieldSyntax = _fieldSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator MemberDeclarationSyntax(FieldSyntax syntax)
        {
            return syntax._fieldSyntax;
        }

        public FieldSyntax WithInitializer(ExpressionSyntax expression)
        {
            _fieldSyntax = _fieldSyntax.WithDeclaration(
                _fieldSyntax.Declaration.WithVariables(
                    SingletonSeparatedList(
                        _fieldSyntax.Declaration.Variables[0].WithInitializer(
                            EqualsValueClause(expression)
                        )
                    )
                )
            );

            return this;
        }
    }

    internal class LocalSyntax
    {
        private LocalDeclarationStatementSyntax _localSyntax;

        public LocalSyntax(string name, TypeSyntax type, Modifiers modifiers)
        {
            _localSyntax = LocalDeclarationStatement(
                VariableDeclaration(type, SingletonSeparatedList(VariableDeclarator(name)))
            );

            if (modifiers != Modifiers.None)
            {
                _localSyntax = _localSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }
        }

        public static implicit operator StatementSyntax(LocalSyntax syntax)
        {
            return syntax._localSyntax;
        }

        public LocalSyntax WithInitializer(ExpressionSyntax expression)
        {
            _localSyntax = _localSyntax.WithDeclaration(
                _localSyntax.Declaration.WithVariables(
                    SingletonSeparatedList(
                        _localSyntax.Declaration.Variables[0].WithInitializer(
                            EqualsValueClause(expression)
                        )
                    )
                )
            );

            return this;
        }
    }

    internal class InstanceSyntax
    {
        private ObjectCreationExpressionSyntax _objectCreationSyntax;

        public InstanceSyntax(TypeSyntax type)
        {
            _objectCreationSyntax = ObjectCreationExpression(type);
        }

        public static implicit operator ExpressionSyntax(InstanceSyntax syntax)
        {
            return syntax._objectCreationSyntax;
        }

        public InstanceSyntax WithInitializer(InitializerExpressionSyntax expression)
        {
            _objectCreationSyntax = _objectCreationSyntax.WithInitializer(expression);

            return this;
        }
        public InstanceSyntax WithArguments(params ExpressionSyntax[] members)
        {
            return WithArguments((IEnumerable<ExpressionSyntax>)members);
        }
        public InstanceSyntax WithArguments(IEnumerable<ExpressionSyntax> members)
        {
            _objectCreationSyntax = _objectCreationSyntax.WithArgumentList(
                ArgumentList(SeparatedList(members.Select(x => Argument(x))))
            );

            return this;
        }
    }

    internal class LambdaSyntax
    {
        private ParenthesizedLambdaExpressionSyntax _lambdaSyntax;

        public LambdaSyntax(string[] parameters)
        {
            _lambdaSyntax = ParenthesizedLambdaExpression()
                .WithParameterList(
                    ParameterList(SeparatedList(parameters.Select(x => Parameter(Identifier(x)))))
                );
        }

        public static implicit operator ExpressionSyntax(LambdaSyntax syntax)
        {
            return syntax._lambdaSyntax;
        }

        public LambdaSyntax WithStatements(IEnumerable<StatementSyntax> statements)
        {
            _lambdaSyntax = _lambdaSyntax.WithBody(Block(statements));

            return this;
        }
    }

    internal class IfSyntax
    {
        private IfStatementSyntax _ifSyntax;

        public IfSyntax(ExpressionSyntax condition, StatementSyntax[] then)
        {
            _ifSyntax = IfStatement(condition, Block(then));
        }

        public static implicit operator IfStatementSyntax(IfSyntax syntax)
        {
            return syntax._ifSyntax;
        }
    }

    [Flags]
    internal enum Modifiers : ushort
    {
        None = 0,
        Private = 1 << 0,
        Protected = 1 << 1,
        Internal = 1 << 2,
        Public = 1 << 3,
        Sealed = 1 << 4,
        Static = 1 << 5,
        Abstract = 1 << 6,
        Override = 1 << 7,
        Virtual = 1 << 8,
        Ref = 1 << 9
    }

    internal static class ModifierExtensions
    {
        internal static SyntaxTokenList GetSyntaxTokens(this Modifiers modifiers)
        {
            var syntaxTokens = new List<SyntaxToken>();

            while (modifiers != 0)
            {
                syntaxTokens.Add(
                    Token(
                        (modifiers & ~modifiers + 1) switch
                        {
                            Modifiers.Private => SyntaxKind.PrivateKeyword,
                            Modifiers.Protected => SyntaxKind.ProtectedKeyword,
                            Modifiers.Internal => SyntaxKind.InternalKeyword,
                            Modifiers.Public => SyntaxKind.PublicKeyword,
                            Modifiers.Sealed => SyntaxKind.SealedKeyword,
                            Modifiers.Static => SyntaxKind.StaticKeyword,
                            Modifiers.Abstract => SyntaxKind.AbstractKeyword,
                            Modifiers.Override => SyntaxKind.OverrideKeyword,
                            Modifiers.Virtual => SyntaxKind.VirtualKeyword,
                            _ => throw new InvalidOperationException()
                        }
                    )
                );

                modifiers &= modifiers - 1;
            }

            return TokenList(syntaxTokens);
        }
    }
}
