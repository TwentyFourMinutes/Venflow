using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.SyntaxGenerator
{
    internal static class CSharpSyntaxGenerator
    {
        internal static SourceText File(
            string[] usings,
            string namespaceName,
            params MemberDeclarationSyntax[] members
        )
        {
            return CompilationUnit(
                    externs: default,
                    attributeLists: default,
                    usings: List(usings.Select(x => UsingDirective(IdentifierName(x)))),
                    members: SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(IdentifierName(namespaceName))
                            .WithMembers(List(members))
                    )
                )
                .NormalizeWhitespace()
                .GetText(Encoding.UTF8);
        }

        internal static ClassDeclarationSyntax Class(string name, params SyntaxKind[] modifiers)
        {
            return ClassDeclaration(name).WithModifiers(TokenList(modifiers.Select(x => Token(x))));
        }

        internal static ConstructorDeclarationSyntax Constructor(
            string type,
            params SyntaxKind[] modifiers
        )
        {
            return ConstructorDeclaration(type)
                .WithModifiers(TokenList(modifiers.Select(x => Token(x))));
        }

        internal static PropertyDeclarationSyntax Property(
            TypeSyntax type,
            string name,
            params SyntaxKind[] modifiers
        )
        {
            return PropertyDeclaration(type, name)
                .WithModifiers(TokenList(modifiers.Select(x => Token(x))));
        }

        internal static AccessorDeclarationSyntax GetAccessor()
        {
            return AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);
        }

        internal static AccessorDeclarationSyntax SetAccessor()
        {
            return AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);
        }

        internal static FieldDeclarationSyntax Field(
            VariableDeclarationSyntax variable,
            params SyntaxKind[] modifiers
        )
        {
            return FieldDeclaration(variable)
                .WithModifiers(TokenList(modifiers.Select(x => Token(x))));
        }

        internal static LocalDeclarationStatementSyntax Local(
            VariableDeclarationSyntax variable,
            params SyntaxKind[] modifiers
        )
        {
            return LocalDeclarationStatement(variable)
                .WithModifiers(TokenList(modifiers.Select(x => Token(x))));
        }

        internal static ExpressionStatementSyntax AssignMember(
            string variableName,
            string memberName,
            ExpressionSyntax expression
        )
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(variableName),
                        IdentifierName(memberName)
                    ),
                    expression
                )
            );
        }

        internal static IfStatementSyntax IfStatement(
            ExpressionSyntax condition,
            params ExpressionSyntax[] statements
        )
        {
            return SyntaxFactory.IfStatement(
                condition,
                Block(List(statements.Select(x => ExpressionStatement(x))))
            );
        }

        internal static VariableDeclarationSyntax Variable(
            string name,
            TypeSyntax type,
            ExpressionSyntax? expressionSyntax = null
        )
        {
            var variableDeclarator = VariableDeclarator(Identifier(name));

            if (expressionSyntax is not null)
            {
                variableDeclarator = variableDeclarator.WithInitializer(
                    EqualsValueClause(expressionSyntax)
                );
            }

            return VariableDeclaration(type)
                .WithVariables(SingletonSeparatedList(variableDeclarator));
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

        internal static GenericNameSyntax GenericType(string name, params NameSyntax[] types)
        {
            return GenericName(Identifier(name))
                .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(types)));
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

        internal static ObjectCreationExpressionSyntax Instance(TypeSyntax type)
        {
            return ObjectCreationExpression(type);
        }

        internal static InitializerExpressionSyntax DictionaryInitializer(
            params (ExpressionSyntax Key, ExpressionSyntax Value)[] keyValuePairs
        )
        {
            return InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList(
                    keyValuePairs.Select(
                        x =>
                            (ExpressionSyntax)AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                ImplicitElementAccess()
                                    .WithArgumentList(
                                        BracketedArgumentList(
                                            SingletonSeparatedList(Argument(x.Key))
                                        )
                                    ),
                                x.Value
                            )
                    )
                )
            );
        }

        internal static NameSyntax Type(string name)
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

        internal static ParenthesizedLambdaExpressionSyntax Lambda(params string[] parameters)
        {
            return ParenthesizedLambdaExpression(
                ParameterList(SeparatedList(parameters.Select(x => Parameter(Identifier(x))))),
                default,
                default
            );
        }
    }

    internal static class SyntaxFactoryExtensions
    {
        internal static ClassDeclarationSyntax WithMembers(
            this ClassDeclarationSyntax classSyntax,
            params MemberDeclarationSyntax[] members
        )
        {
            return classSyntax.WithMembers((IEnumerable<MemberDeclarationSyntax>)members);
        }

        internal static ClassDeclarationSyntax WithMembers(
            this ClassDeclarationSyntax classSyntax,
            IEnumerable<MemberDeclarationSyntax> members
        )
        {
            return classSyntax.WithMembers(List(members));
        }

        internal static ClassDeclarationSyntax WithBase(
            this ClassDeclarationSyntax classSyntax,
            TypeSyntax type
        )
        {
            return classSyntax.WithBaseList(
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(type)))
            );
        }

        internal static ObjectCreationExpressionSyntax WithArguments(
            this ObjectCreationExpressionSyntax objectCreationSyntax,
            params ExpressionSyntax[] arguments
        )
        {
            return objectCreationSyntax.AddArgumentListArguments(
                arguments.Select(x => Argument(x)).ToArray()
            );
        }

        internal static ConstructorDeclarationSyntax WithParameters(
            this ConstructorDeclarationSyntax constructorSyntax,
            params (TypeSyntax Type, string Name, SyntaxKind? Default)[] parameters
        )
        {
            return constructorSyntax.WithParameterList(
                ParameterList(
                    SeparatedList(
                        parameters.Select(
                            x =>
                                Parameter(Identifier(x.Name))
                                    .WithType(x.Type)
                                    .WithDefault(
                                        x.Default is null
                                          ? null
                                          : EqualsValueClause(LiteralExpression(x.Default.Value))
                                    )
                        )
                    )
                )
            );
        }

        internal static PropertyDeclarationSyntax WithAccessors(
            this PropertyDeclarationSyntax propertySyntax,
            params AccessorDeclarationSyntax[] accessors
        )
        {
            return propertySyntax.WithAccessorList(AccessorList(List(accessors)));
        }

        internal static ParenthesizedLambdaExpressionSyntax WithStatements(
            this ParenthesizedLambdaExpressionSyntax parenthesizedLambdaSyntax,
            params StatementSyntax[] statements
        )
        {
            return parenthesizedLambdaSyntax.WithBlock(Block(statements));
        }

        internal static ConstructorDeclarationSyntax WithStatements(
            this ConstructorDeclarationSyntax constructorSyntax,
            params StatementSyntax[] statements
        )
        {
            return constructorSyntax.WithBody(Block(statements));
        }

        internal static AccessorDeclarationSyntax WithStatements(
            this AccessorDeclarationSyntax accessorSyntax,
            params StatementSyntax[] statements
        )
        {
            return accessorSyntax.WithBody(Block(statements));
        }
    }
}
