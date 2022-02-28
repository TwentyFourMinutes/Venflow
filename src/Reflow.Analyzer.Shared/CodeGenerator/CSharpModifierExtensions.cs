using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Reflow.Analyzer.CodeGenerator
{
    public static class CSharpModifierExtensions
    {
        internal static SyntaxTokenList GetSyntaxTokens(this CSharpModifiers modifiers)
        {
            var syntaxTokens = new List<SyntaxToken>();

            while (modifiers != 0)
            {
                syntaxTokens.Add(
                    Token(
                        (modifiers & ~modifiers + 1) switch
                        {
                            CSharpModifiers.Private => SyntaxKind.PrivateKeyword,
                            CSharpModifiers.Protected => SyntaxKind.ProtectedKeyword,
                            CSharpModifiers.Internal => SyntaxKind.InternalKeyword,
                            CSharpModifiers.Public => SyntaxKind.PublicKeyword,
                            CSharpModifiers.Partial => SyntaxKind.PartialKeyword,
                            CSharpModifiers.Sealed => SyntaxKind.SealedKeyword,
                            CSharpModifiers.Static => SyntaxKind.StaticKeyword,
                            CSharpModifiers.Abstract => SyntaxKind.AbstractKeyword,
                            CSharpModifiers.Override => SyntaxKind.OverrideKeyword,
                            CSharpModifiers.Virtual => SyntaxKind.VirtualKeyword,
                            CSharpModifiers.Async => SyntaxKind.AsyncKeyword,
                            CSharpModifiers.ReadOnly => SyntaxKind.ReadOnlyKeyword,
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
