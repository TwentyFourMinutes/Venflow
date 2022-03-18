using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Shared;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public static class CSharpCodeGenerator
    {
        public static CompilationUnitSyntax Compilation(
            IEnumerable<MemberDeclarationSyntax> namespaces
        )
        {
            return CompilationUnit().WithMembers(List(namespaces));
        }

        public static CSharpFileSyntax File(string namespaceName)
        {
            return new CSharpFileSyntax(namespaceName);
        }

        public static CSharpNamespaceSyntax Namespace(string name)
        {
            return new CSharpNamespaceSyntax(name);
        }

        public static CSharpInterfaceSyntax Interface(
            string name,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpInterfaceSyntax(name, modifiers);
        }

        public static CSharpClassSyntax Class(
            string name,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpClassSyntax(name, modifiers);
        }

        public static CSharpStructSyntax Struct(
            string name,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpStructSyntax(name, modifiers);
        }

        public static CSharpConversionOperatorSyntax ImplicitOperator(
            TypeSyntax toType,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpConversionOperatorSyntax(
                SyntaxKind.ImplicitKeyword,
                toType,
                modifiers
            );
        }

        public static CSharpConstructorSyntax Constructor(
            string name,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpConstructorSyntax(name, modifiers);
        }

        public static CSharpParameterSyntax Parameter(
            string name,
            TypeSyntax type,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpParameterSyntax(name, type, modifiers);
        }

        public static CSharpTypeParameterSyntax TypeParameter(string name)
        {
            return new CSharpTypeParameterSyntax(name);
        }

        public static CSharpAttributeSyntax Attribute(TypeSyntax type)
        {
            return new CSharpAttributeSyntax(type);
        }

        public static CSharpPropertySyntax Property(
            string name,
            TypeSyntax type,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpPropertySyntax(name, type, modifiers);
        }

        public static CSharpFieldSyntax Field(
            string name,
            TypeSyntax type,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpFieldSyntax(name, type, modifiers);
        }

        public static CSharpMethodSyntax Method(
            string name,
            TypeSyntax returnType,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpMethodSyntax(name, returnType, modifiers);
        }

        public static CSharpLocalSyntax Local(
            string name,
            TypeSyntax type,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpLocalSyntax(name, type, modifiers);
        }

        public static CSharpInstanceSyntax Instance(TypeSyntax type)
        {
            return new CSharpInstanceSyntax(type);
        }

        public static ArrayCreationExpressionSyntax ArrayInitializer(
            ArrayTypeSyntax type,
            params ExpressionSyntax[] expressions
        )
        {
            return ArrayInitializer(type, (IEnumerable<ExpressionSyntax>)expressions);
        }

        public static ArrayCreationExpressionSyntax ArrayInitializer(
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

        public static InitializerExpressionSyntax DictionaryInitializer(
            IEnumerable<InitializerExpressionSyntax> expressions
        )
        {
            return InitializerExpression(
                SyntaxKind.CollectionInitializerExpression,
                SeparatedList<ExpressionSyntax>(expressions)
            );
        }

        public static InitializerExpressionSyntax DictionaryEntry(
            ExpressionSyntax key,
            ExpressionSyntax value
        )
        {
            return InitializerExpression(
                SyntaxKind.ComplexElementInitializerExpression,
                SeparatedList(new[] { key, value })
            );
        }

        public static SwitchStatementSyntax Switch(
            ExpressionSyntax switchOn,
            params SwitchSectionSyntax[] switchSections
        )
        {
            return SwitchStatement(switchOn, List(switchSections));
        }

        public static SwitchStatementSyntax Switch(
            ExpressionSyntax switchOn,
            IEnumerable<SwitchSectionSyntax> switchSections
        )
        {
            return SwitchStatement(switchOn, List(switchSections));
        }

        public static SwitchSectionSyntax Case(
            ExpressionSyntax caseOn,
            params StatementSyntax[] statements
        )
        {
            return SwitchSection(
                SingletonList<SwitchLabelSyntax>(CaseSwitchLabel(caseOn)),
                List(statements)
            );
        }

        public static SwitchSectionSyntax Case(
            ExpressionSyntax caseOn,
            IEnumerable<StatementSyntax> statements
        )
        {
            return SwitchSection(
                SingletonList<SwitchLabelSyntax>(CaseSwitchLabel(caseOn)),
                List(statements)
            );
        }

        public static SwitchSectionSyntax DefaultCase(params StatementSyntax[] statements)
        {
            return SwitchSection(
                SingletonList<SwitchLabelSyntax>(DefaultSwitchLabel()),
                List(statements)
            );
        }

        public static NameSyntax Var()
        {
            return IdentifierName(
                Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())
            );
        }

        public static LiteralExpressionSyntax Default()
        {
            return LiteralExpression(
                SyntaxKind.DefaultLiteralExpression,
                Token(SyntaxKind.DefaultKeyword)
            );
        }

        public static TypeSyntax Void()
        {
            return PredefinedType(Token(SyntaxKind.VoidKeyword));
        }

        public static TypeSyntax NullableType(ISymbol type)
        {
            return SyntaxFactory.NullableType(Type(type.GetFullName()));
        }

        public static TypeSyntax NullableType(IPropertySymbol type)
        {
            return SyntaxFactory.NullableType(Type(type.Type.GetFullName()));
        }

        public static TypeSyntax NullableType(Type type)
        {
            return SyntaxFactory.NullableType(Type(type.FullName));
        }

        public static TypeSyntax NullableType<TType>()
        {
            return SyntaxFactory.NullableType(Type(typeof(TType).FullName));
        }

        public static TypeSyntax NullableType(string type)
        {
            return SyntaxFactory.NullableType(Type(type));
        }

        public static TypeSyntax Type(ISymbol type)
        {
            return Type(type.GetFullName());
        }

        public static TypeSyntax Type(IPropertySymbol type)
        {
            return Type(type.Type.GetFullName());
        }

        public static TypeSyntax Type(Type type)
        {
            return Type(type.FullName);
        }

        public static TypeSyntax Type<TType>()
        {
            return Type(typeof(TType).FullName);
        }

        public static TypeSyntax Type(Type type, string memberName)
        {
            return Type(type.FullName + "." + memberName);
        }

        public static TypeSyntax Type(TypeCode type)
        {
            return Type(
                "System."
                    + type switch
                    {
                        TypeCode.Empty => throw new ArgumentException(),
                        _ => type.ToString()
                    }
            );
        }

        public static TypeSyntax Type(string type)
        {
            var sections = type.Split('.');

            NameSyntax qualifiedName = AliasQualifiedName(
                SyntaxFactory.IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                IdentifierName(sections[0])
            );

            if (sections.Length == 1)
                return qualifiedName;

            for (var sectionIndex = 1; sectionIndex < sections.Length; sectionIndex++)
            {
                qualifiedName = QualifiedName(
                    qualifiedName,
                    IdentifierName(sections[sectionIndex])
                );
            }

            return qualifiedName;
        }

        public static ArrayTypeSyntax Array(TypeSyntax type, int length = -1)
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

        public static TypeSyntax Generic(string name)
        {
            return IdentifierName(name);
        }

        public static TypeSyntax GenericType(Type type, params TypeSyntax[] types)
        {
            if (!type.IsGenericType)
                throw new ArgumentException();

            var index = type.FullName.IndexOf('`');

            return GenericType(type.FullName.Substring(0, index), types);
        }

        public static TypeSyntax GenericType(string name, params TypeSyntax[] types)
        {
            var sections = name.Split('.');

            NameSyntax qualifiedName = AliasQualifiedName(
                SyntaxFactory.IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                IdentifierName(sections[0])
            );

            var genericName = SyntaxFactory
                .GenericName(Identifier(sections[sections.Length - 1]))
                .WithTypeArgumentList(TypeArgumentList(SeparatedList(types)));

            if (sections.Length == 1)
                return QualifiedName(qualifiedName, genericName);

            for (var sectionIndex = 1; sectionIndex < sections.Length - 1; sectionIndex++)
            {
                qualifiedName = QualifiedName(
                    qualifiedName,
                    IdentifierName(sections[sectionIndex])
                );
            }

            return QualifiedName(qualifiedName, genericName);
        }

        public static GenericNameSyntax GenericName(string name, params TypeSyntax[] typeArguments)
        {
            return SyntaxFactory
                .GenericName(Identifier(name))
                .WithTypeArgumentList(TypeArgumentList(SeparatedList(typeArguments)));
        }

        public static SimpleNameSyntax Variable(string name)
        {
            return IdentifierName(name);
        }

        public static LiteralExpressionSyntax Constant(char value)
        {
            return LiteralExpression(SyntaxKind.CharacterLiteralExpression)
                .WithToken(Literal(value));
        }

        public static LiteralExpressionSyntax Constant(short value)
        {
            return LiteralExpression(SyntaxKind.NumericLiteralExpression).WithToken(Literal(value));
        }

        public static LiteralExpressionSyntax Constant(ushort value)
        {
            return LiteralExpression(SyntaxKind.NumericLiteralExpression).WithToken(Literal(value));
        }

        public static LiteralExpressionSyntax Constant(uint value)
        {
            return LiteralExpression(SyntaxKind.NumericLiteralExpression).WithToken(Literal(value));
        }

        public static LiteralExpressionSyntax Constant(int value)
        {
            return LiteralExpression(SyntaxKind.NumericLiteralExpression).WithToken(Literal(value));
        }

        public static LiteralExpressionSyntax Constant(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression).WithToken(Literal(value));
        }

        public static LiteralExpressionSyntax Constant(bool value)
        {
            return LiteralExpression(
                value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression
            );
        }

        public static InvocationExpressionSyntax FastCast(
            TypeSyntax toType,
            ExpressionSyntax expression
        )
        {
            return Invoke(Type(typeof(Unsafe)), GenericName("As", toType), expression);
        }

        public static CastExpressionSyntax Cast(TypeSyntax toType, ExpressionSyntax expression)
        {
            return CastExpression(toType, expression);
        }

        public static TypeOfExpressionSyntax TypeOf(TypeSyntax type)
        {
            return TypeOfExpression(type);
        }

        public static CSharpLambdaSyntax Lambda(params string[] parameters)
        {
            return new CSharpLambdaSyntax(parameters);
        }

        public static ConditionalExpressionSyntax Conditional(
            ExpressionSyntax condition,
            ExpressionSyntax then,
            ExpressionSyntax @else
        )
        {
            return ConditionalExpression(condition, then, @else);
        }

        public static CSharpIfSyntax If(
            ExpressionSyntax condition,
            params StatementSyntax[] statements
        )
        {
            return new CSharpIfSyntax(condition, statements);
        }

        public static CSharpIfSyntax If(
            ExpressionSyntax condition,
            IEnumerable<StatementSyntax> statements
        )
        {
            return new CSharpIfSyntax(condition, statements);
        }

        public static CSharpIfSyntax If(IfStatementSyntax ifStatement)
        {
            return new CSharpIfSyntax(ifStatement);
        }

        public static IsPatternExpressionSyntax Is(
            ExpressionSyntax expression,
            CSharpLocalSyntax local
        )
        {
            var localSyntax = (LocalDeclarationStatementSyntax)local;

            return IsPatternExpression(
                expression,
                DeclarationPattern(
                    localSyntax.Declaration.Type,
                    SingleVariableDesignation(localSyntax.Declaration.Variables.First().Identifier)
                )
            );
        }

        public static IsPatternExpressionSyntax IsNot(
            ExpressionSyntax expression,
            CSharpLocalSyntax local
        )
        {
            var localSyntax = (LocalDeclarationStatementSyntax)local;

            return IsPatternExpression(
                expression,
                UnaryPattern(
                    DeclarationPattern(
                        localSyntax.Declaration.Type,
                        SingleVariableDesignation(
                            localSyntax.Declaration.Variables.First().Identifier
                        )
                    )
                )
            );
        }

        public static ExpressionSyntax Equal(
            ExpressionSyntax expression,
            LiteralExpressionSyntax to
        )
        {
            if (to.Kind() == SyntaxKind.DefaultLiteralExpression)
                return Equal(expression, (ExpressionSyntax)to);
            else
                return IsPatternExpression(expression, ConstantPattern(to));
        }

        public static BinaryExpressionSyntax Equal(ExpressionSyntax expression, ExpressionSyntax to)
        {
            return BinaryExpression(SyntaxKind.EqualsExpression, expression, to);
        }

        public static BinaryExpressionSyntax NotEqual(
            ExpressionSyntax expression,
            ExpressionSyntax to
        )
        {
            return BinaryExpression(SyntaxKind.NotEqualsExpression, expression, to);
        }

        public static PrefixUnaryExpressionSyntax Not(ExpressionSyntax expression)
        {
            return PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expression);
        }

        public static BinaryExpressionSyntax And(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LogicalAndExpression, left, right);
        }

        public static BinaryExpressionSyntax Or(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LogicalOrExpression, left, right);
        }

        public static ArgumentSyntax Ref(NameSyntax variable)
        {
            return Argument(variable).WithRefKindKeyword(Token(SyntaxKind.RefKeyword));
        }

        public static ArgumentSyntax Out(NameSyntax variable)
        {
            return Argument(variable).WithRefKindKeyword(Token(SyntaxKind.OutKeyword));
        }

        public static InvocationExpressionSyntax Invoke(
            NameSyntax member,
            string memberName,
            params SyntaxNode[] parameters
        )
        {
            return Invoke(member, IdentifierName(memberName), parameters);
        }

        public static InvocationExpressionSyntax Invoke(
            ExpressionSyntax member,
            string memberName,
            params SyntaxNode[] parameters
        )
        {
            return Invoke(member, IdentifierName(memberName), parameters);
        }

        public static InvocationExpressionSyntax Invoke(
            ExpressionSyntax expression,
            SimpleNameSyntax memberName,
            params SyntaxNode[] parameters
        )
        {
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression,
                        memberName
                    )
                )
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList(
                            parameters.Select(
                                x =>
                                    x.IsKind(SyntaxKind.Argument)
                                      ? (ArgumentSyntax)x
                                      : Argument((ExpressionSyntax)x)
                            )
                        )
                    )
                );
        }

        public static AssignmentExpressionSyntax AssignLocal(
            ExpressionSyntax local,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return AssignmentExpression(operation, local, right);
        }

        public static ExpressionStatementSyntax AssignMember(
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

        public static ExpressionStatementSyntax AssignMember(
            string member,
            IPropertySymbol memberProperty,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return AssignMember(
                IdentifierName(member),
                IdentifierName(memberProperty.Name),
                right,
                operation
            );
        }

        public static ExpressionStatementSyntax AssignMember(
            NameSyntax member,
            IPropertySymbol memberProperty,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return AssignMember(member, IdentifierName(memberProperty.Name), right, operation);
        }

        public static ExpressionStatementSyntax AssignMember(
            ExpressionSyntax member,
            string memberName,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return AssignMember(member, IdentifierName(memberName), right, operation);
        }

        public static ExpressionStatementSyntax AssignMember(
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

        public static MemberAccessExpressionSyntax AccessMember(
            ExpressionSyntax member,
            IPropertySymbol property
        )
        {
            return AccessMember(member, property.Name);
        }

        public static MemberAccessExpressionSyntax AccessMember(
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

        public static MemberAccessExpressionSyntax EnumMember<T>(T member) where T : struct, Enum
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                Type<T>(),
                IdentifierName(member.ToString())
            );
        }

        public static BaseExpressionSyntax Base()
        {
            return BaseExpression();
        }

        public static ThisExpressionSyntax This()
        {
            return ThisExpression();
        }

        public static ExpressionSyntax Value()
        {
            return IdentifierName("value");
        }

        public static ReturnStatementSyntax Return(ExpressionSyntax? syntax = null)
        {
            return ReturnStatement(syntax);
        }

        public static BreakStatementSyntax Break()
        {
            return BreakStatement();
        }

        public static ContinueStatementSyntax Continue()
        {
            return ContinueStatement();
        }

        public static AssignmentExpressionSyntax SetBit(NameSyntax local, int index)
        {
            return AssignLocal(local, Constant(1 << index), SyntaxKind.OrAssignmentExpression);
        }

        public static ExpressionStatementSyntax SetBit(
            ExpressionSyntax name,
            SimpleNameSyntax memberName,
            int index
        )
        {
            return AssignMember(
                name,
                memberName,
                Constant(1 << index),
                SyntaxKind.OrAssignmentExpression
            );
        }

        public static BinaryExpressionSyntax IsBitSet(
            ExpressionSyntax expression,
            TypeSyntax type,
            int index
        )
        {
            return BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                Cast(
                    type,
                    ParenthesizedExpression(
                        BinaryExpression(
                            SyntaxKind.BitwiseAndExpression,
                            expression,
                            Constant(1 << index)
                        )
                    )
                ),
                Constant(0)
            );
        }

        public static BinaryExpressionSyntax ShiftLeft(
            ExpressionSyntax expression,
            ExpressionSyntax count
        )
        {
            return BinaryExpression(SyntaxKind.LeftShiftExpression, expression, count);
        }

        public static BinaryExpressionSyntax ShiftRight(
            ExpressionSyntax expression,
            ExpressionSyntax count
        )
        {
            return BinaryExpression(SyntaxKind.RightShiftExpression, expression, count);
        }

        public static BinaryExpressionSyntax BitwiseOr(
            ExpressionSyntax left,
            ExpressionSyntax right
        )
        {
            return BinaryExpression(SyntaxKind.BitwiseOrExpression, left, right);
        }

        public static BinaryExpressionSyntax BitwiseAnd(
            ExpressionSyntax left,
            ExpressionSyntax right
        )
        {
            return BinaryExpression(SyntaxKind.BitwiseAndExpression, left, right);
        }

        public static PrefixUnaryExpressionSyntax BitwiseNot(ExpressionSyntax expression)
        {
            return PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, expression);
        }

        public static BinaryExpressionSyntax LessThen(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LessThanExpression, left, right);
        }

        public static BinaryExpressionSyntax LessOrEqualThen(
            ExpressionSyntax left,
            ExpressionSyntax right
        )
        {
            return BinaryExpression(SyntaxKind.LessThanOrEqualExpression, left, right);
        }

        public static PostfixUnaryExpressionSyntax Increment(ExpressionSyntax variable)
        {
            return PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, variable);
        }

        public static PostfixUnaryExpressionSyntax Decrement(ExpressionSyntax variable)
        {
            return PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, variable);
        }

        public static ExpressionSyntax Add(ExpressionSyntax a, ExpressionSyntax b)
        {
            return BinaryExpression(SyntaxKind.AddExpression, a, b);
        }

        public static ExpressionSyntax Substract(ExpressionSyntax a, ExpressionSyntax b)
        {
            return BinaryExpression(SyntaxKind.SubtractExpression, a, b);
        }

        public static ExpressionSyntax Multiply(ExpressionSyntax a, ExpressionSyntax b)
        {
            return BinaryExpression(SyntaxKind.MultiplyExpression, a, b);
        }

        public static ExpressionSyntax Divide(ExpressionSyntax a, ExpressionSyntax b)
        {
            return BinaryExpression(SyntaxKind.DivideExpression, a, b);
        }

        public static ExpressionSyntax Modulo(ExpressionSyntax a, ExpressionSyntax b)
        {
            return BinaryExpression(SyntaxKind.ModuloExpression, a, b);
        }

        public static ElementAccessExpressionSyntax AccessElement(
            ExpressionSyntax collection,
            ExpressionSyntax index
        )
        {
            return ElementAccessExpression(collection)
                .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(index))));
        }

        public static ParenthesizedExpressionSyntax Parenthesis(ExpressionSyntax expression)
        {
            return ParenthesizedExpression(expression);
        }

        public static ThrowStatementSyntax Throw(ExpressionSyntax? exception = null)
        {
            return ThrowStatement(exception);
        }

        public static LiteralExpressionSyntax Null()
        {
            return LiteralExpression(SyntaxKind.NullLiteralExpression);
        }

        public static ForStatementSyntax For(
            ExpressionSyntax condition,
            ExpressionSyntax increment,
            params StatementSyntax[] statements
        )
        {
            return For(condition, increment, (IEnumerable<StatementSyntax>)statements);
        }

        public static ForStatementSyntax For(
            ExpressionSyntax condition,
            ExpressionSyntax increment,
            IEnumerable<StatementSyntax> statements
        )
        {
            return ForStatement(Block(statements))
                .WithCondition(condition)
                .WithIncrementors(SingletonSeparatedList(increment));
        }

        public static ForStatementSyntax For(
            CSharpLocalSyntax local,
            ExpressionSyntax condition,
            ExpressionSyntax increment,
            params StatementSyntax[] statements
        )
        {
            return For(local, condition, increment, (IEnumerable<StatementSyntax>)statements);
        }

        public static ForStatementSyntax For(
            CSharpLocalSyntax local,
            ExpressionSyntax condition,
            ExpressionSyntax increment,
            IEnumerable<StatementSyntax> statements
        )
        {
            var localSyntax = (LocalDeclarationStatementSyntax)(StatementSyntax)local;

            return ForStatement(Block(statements))
                .WithDeclaration(localSyntax.Declaration)
                .WithCondition(condition)
                .WithIncrementors(SingletonSeparatedList(increment));
        }

        public static WhileStatementSyntax While(
            ExpressionSyntax condition,
            params StatementSyntax[] statements
        )
        {
            return While(condition, (IEnumerable<StatementSyntax>)statements);
        }

        public static WhileStatementSyntax While(
            ExpressionSyntax condition,
            IEnumerable<StatementSyntax> statements
        )
        {
            return WhileStatement(condition, Block(statements));
        }

        public static AwaitExpressionSyntax Await(ExpressionSyntax expression)
        {
            return AwaitExpression(expression);
        }

        public static ExpressionStatementSyntax Statement(ExpressionSyntax expression)
        {
            return ExpressionStatement(expression);
        }

        public static IEnumerable<StatementSyntax> Concat(params StatementSyntax[] otherStatements)
        {
            return otherStatements;
        }

        public static IEnumerable<StatementSyntax> Concat(
            StatementSyntax firstStatement,
            params StatementSyntax[] otherStatements
        )
        {
            return Concat(firstStatement, (IList<StatementSyntax>)otherStatements);
        }

        public static IEnumerable<StatementSyntax> Concat(
            StatementSyntax firstStatement,
            IList<StatementSyntax> otherStatements
        )
        {
            yield return firstStatement;

            for (var statementIndex = 0; statementIndex < otherStatements.Count; statementIndex++)
            {
                yield return otherStatements[statementIndex];
            }
        }

        public static IEnumerable<StatementSyntax> Concat(
            StatementSyntax firstStatement,
            IEnumerable<StatementSyntax> otherStatements
        )
        {
            yield return firstStatement;

            foreach (var statememt in otherStatements)
            {
                yield return statememt;
            }
        }

        public static IEnumerable<StatementSyntax> Concat(
            IEnumerable<StatementSyntax> firstStatements,
            params StatementSyntax[] secondStatements
        )
        {
            return firstStatements.Concat(secondStatements);
        }

        public static StatementSyntax EmitIf(bool condition, Func<StatementSyntax> statement)
        {
            return condition ? statement.Invoke() : Block();
        }

        public static class Options
        {
            public static bool EmitSkipLocalsInit { get; set; } = false;
            public static bool HideFromEditor { get; set; } = true;

            public static void Init(Compilation compilation)
            {
                if (
                    ((CSharpCompilationOptions)compilation.Options).AllowUnsafe
                    && !compilation.Assembly.Modules
                        .SelectMany(x => x.GetAttributes())
                        .Any(
                            x =>
                                x.AttributeClass!.GetFullName()
                                    is "System.Runtime.CompilerServices.SkipLocalsInitAttribute"
                        )
                )
                {
                    CSharpCodeGenerator.Options.EmitSkipLocalsInit = true;
                }
            }
        }
    }
}
