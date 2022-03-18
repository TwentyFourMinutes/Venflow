﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public class CSharpClassSyntax
    {
        private ClassDeclarationSyntax _classSyntax;

        public CSharpClassSyntax(string name, CSharpModifiers modifiers)
        {
            _classSyntax = ClassDeclaration(name);

            if (modifiers != CSharpModifiers.None)
            {
                _classSyntax = _classSyntax.WithModifiers(modifiers.GetSyntaxTokens());
            }

            if (CSharpCodeGenerator.Options.EmitSkipLocalsInit)
            {
                WithAttributes(
                    CSharpCodeGenerator.Attribute(
                        CSharpCodeGenerator.Type(
                            "System.Runtime.CompilerServices.SkipLocalsInitAttribute"
                        )
                    )
                );
            }
        }

        public static implicit operator SyntaxNode(CSharpClassSyntax syntax)
        {
            return syntax._classSyntax;
        }

        public CSharpClassSyntax WithMembers(params SyntaxNode[] members)
        {
            return WithMembers((IEnumerable<SyntaxNode>)members);
        }

        public CSharpClassSyntax WithMembers(IEnumerable<SyntaxNode> members)
        {
            _classSyntax = _classSyntax.WithMembers(List(members));
            return this;
        }

        public CSharpClassSyntax WithBase(TypeSyntax type)
        {
            _classSyntax = _classSyntax.WithBaseList(
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(type)))
            );

            return this;
        }

        public CSharpClassSyntax WithAttributes(params CSharpAttributeSyntax[] attributes)
        {
            _classSyntax = _classSyntax.WithAttributeLists(
                SingletonList(
                    AttributeList(SeparatedList(attributes.Select(x => (AttributeSyntax)x)))
                )
            );

            return this;
        }

        public CSharpClassSyntax WithBaseTypes(params TypeSyntax[] parameters)
        {
            _classSyntax = _classSyntax.WithBaseList(
                BaseList(SeparatedList(parameters.Select(x => (BaseTypeSyntax)SimpleBaseType(x))))
            );

            return this;
        }
    }
}
