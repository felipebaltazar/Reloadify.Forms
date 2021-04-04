using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Reloadify.Analyzers
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<(bool IsPartial, ClassDeclarationSyntax UserClass)> ClassToAugment { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (ClassToAugment is null)
                ClassToAugment = new List<(bool IsPartial, ClassDeclarationSyntax UserClass)>();

            if (syntaxNode is ClassDeclarationSyntax cds)
            {
                var isPartial = cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                ClassToAugment.Add((isPartial, cds));
            }
        }
    }
}
