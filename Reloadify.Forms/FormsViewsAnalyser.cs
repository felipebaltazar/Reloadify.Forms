using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reloadify.Forms
{
    [Generator]
    public class FormsViewsAnalyser : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                try
                {
                    Debugger.Launch();
                }
                catch (NotImplementedException) { }
            }
#endif 
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            if (!SyntaxNodeHelper.TryGetParentSyntax<NamespaceDeclarationSyntax>(receiver.ClassToAugment, out var namespaceDeclarationSyntax))
            {
                return; // or whatever you want to do in this scenario
            }
            var className = receiver.ClassToAugment.Identifier;
            var userNamespace = namespaceDeclarationSyntax.Name.ToString();

                var classSource = $@"
namespace {userNamespace}
{{
    public partial class {className}
    {{
        private readonly Reloadify.Forms.InstanceWatcher _watcher = new Reloadify.Forms.InstanceWatcher(this);
        public {className}(bool fromHotreload)
        {{
            _watcher = new Reloadify.Forms.InstanceWatcher(this);
        }}
    }}
}}
";
            context.AddSource($"{className}_instanceWatcher.cs", SourceText.From(classSource, Encoding.UTF8));
        }

        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public ClassDeclarationSyntax ClassToAugment { get; private set; }

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                Debug.WriteLine($"Visiting {syntaxNode}");
                if (syntaxNode is ClassDeclarationSyntax cds &&
                    cds.BaseList.Types.Any(t => t.Type.ToString() == "VisualElement"))
                {
                    if(!cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        var partialKeyword = SyntaxFactory.Token(SyntaxKind.PartialKeyword);
                        cds.AddModifiers(partialKeyword);
                    }

                    ClassToAugment = cds;
                }
            }

            public void OnVisitSyntaxNode(GeneratorSyntaxContext syntaxNode)
            {
                Debug.WriteLine($"Visiting {syntaxNode.Node}");
                if (syntaxNode.Node is ClassDeclarationSyntax cds)// &&
                                                             //cds.BaseList.Types.Any(t => t.Type.ToString() == "ContentPage"))
                {
                    if (!cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        var partialKeyword = SyntaxFactory.Token(SyntaxKind.PartialKeyword);
                        cds.AddModifiers(partialKeyword);
                    }

                    ClassToAugment = cds;
                }
            }
        }

        static class SyntaxNodeHelper
        {
            public static bool TryGetParentSyntax<T>(SyntaxNode syntaxNode, out T result)
                where T : SyntaxNode
            {
                // set defaults
                result = null;

                if (syntaxNode == null)
                {
                    return false;
                }

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
}
