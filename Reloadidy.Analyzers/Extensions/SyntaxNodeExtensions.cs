using Microsoft.CodeAnalysis;

namespace Reloadify.Analyzers.Extensions
{
    internal static class SyntaxNodeExtensions
    {
        public static bool TryGetParentSyntax<T>(this SyntaxNode syntaxNode, out T result)
                where T : SyntaxNode
        {
            result = null;

            if (syntaxNode is null)
                return false;

            try
            {
                syntaxNode = syntaxNode.Parent;

                if (syntaxNode == null)
                {
                    return false;
                }

                if (syntaxNode.GetType() == typeof(T))
                {
                    result = syntaxNode as T;
                    return true;
                }

                return TryGetParentSyntax<T>(syntaxNode, out result);
            }
            catch
            {
                return false;
            }
        }
    }
}
