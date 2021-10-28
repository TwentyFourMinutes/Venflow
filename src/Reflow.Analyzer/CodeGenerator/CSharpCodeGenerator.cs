using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Internal
{
    public static class CSharpCodeGenerator
    {
        public static CSharpFileSyntax File(string namespaceName)
        {
            return new CSharpFileSyntax(namespaceName);
        }
        public static CSharpClassSyntax Class(
            string name,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpClassSyntax(name, modifiers);
        }

        public static CSharpConstructorSyntax Constructor(
            string name,
            CSharpModifiers modifiers = CSharpModifiers.None
        )
        {
            return new CSharpConstructorSyntax(name, modifiers);
        }
        public static CSharpParameterSyntax Parameter(string name, TypeSyntax type)
        {
            return new CSharpParameterSyntax(name, type, CSharpModifiers.None);
        }

        public static CSharpAttributeSyntax Attribute(NameSyntax type)
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

        public static SwitchSectionSyntax Case(
            LiteralExpressionSyntax caseOn,
            params StatementSyntax[] statements
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
            return IdentifierName("var");
        }

        public static TypeSyntax Void()
        {
            return SyntaxFactory.PredefinedType(Token(SyntaxKind.VoidKeyword));
        }

        public static TypeSyntax Type(ISymbol type)
        {
            return Type(type.GetFullName());
        }

        public static TypeSyntax Type(Type type)
        {
            return Type(type.FullName);
        }

        public static TypeSyntax Type(Type type, string memberName)
        {
            return Type(type.FullName + "." + memberName);
        }

        public static TypeSyntax Type(TypeCode type)
        {
            return Type(
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

            var genericName = GenericName(Identifier(sections[sections.Length - 1]))
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
        public static SimpleNameSyntax Variable(string name)
        {
            return IdentifierName(name);
        }

        public static SimpleNameSyntax IdentifierName(Type type)
        {
            return SyntaxFactory.IdentifierName(type.FullName);
        }

        public static SimpleNameSyntax IdentifierName(Type type, string memberName)
        {
            return SyntaxFactory.IdentifierName(type.FullName + "." + memberName);
        }

        public static SimpleNameSyntax IdentifierName(string name)
        {
            return SyntaxFactory.IdentifierName(name);
        }

        public static LiteralExpressionSyntax Constant(short value)
        {
            return Constant((int)value);
        }
        public static LiteralExpressionSyntax Constant(int value)
        {
            return LiteralExpression(SyntaxKind.NumericLiteralExpression).WithToken(Literal(value));
        }
        public static LiteralExpressionSyntax Constant(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression).WithToken(Literal(value));
        }

        public static CastExpressionSyntax Cast(TypeSyntax toType, ExpressionSyntax expression)
        {
            return CastExpression(toType, expression);
        }
        public static TypeOfExpressionSyntax TypeOf(TypeSyntax type)
        {
            return SyntaxFactory.TypeOfExpression(type);
        }
        public static CSharpLambdaSyntax Lambda(params string[] parameters)
        {
            return new CSharpLambdaSyntax(parameters);
        }

        public static CSharpIfSyntax If(
            ExpressionSyntax condition,
            params StatementSyntax[] statements
        )
        {
            return new CSharpIfSyntax(condition, statements);
        }

        public static ExpressionStatementSyntax AssignLocal(
            ExpressionSyntax local,
            ExpressionSyntax right,
            SyntaxKind operation = SyntaxKind.SimpleAssignmentExpression
        )
        {
            return ExpressionStatement(AssignmentExpression(operation, local, right));
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
            string memberName
        )
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                member,
                IdentifierName(memberName)
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

        public static BinaryExpressionSyntax IsBitSet(
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

        public static BinaryExpressionSyntax ShiftRight(
            ExpressionSyntax expression,
            ExpressionSyntax count
        )
        {
            return BinaryExpression(SyntaxKind.RightShiftExpression, expression, count);
        }

        public static ParenthesizedExpressionSyntax Parenthesis(ExpressionSyntax expression)
        {
            return ParenthesizedExpression(expression);
        }

        public static ThrowStatementSyntax Throw(ExpressionSyntax? exception = null)
        {
            return ThrowStatement(exception);
        }
    }
}
