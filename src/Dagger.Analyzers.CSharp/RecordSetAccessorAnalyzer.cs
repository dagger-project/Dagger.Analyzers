using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dagger.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RecordSetAccessorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DAG0001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Record members must not have a set accessor",
        messageFormat: "Record '{0}' must not declare a set accessor on '{1}'. Use 'init' instead.",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A record stays immutable. Its properties and indexers use 'init' or a getter, not 'set'.",
        helpLinkUri: "https://github.com/dagger-project/Dagger.Analyzers#records");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeIndexer, SyntaxKind.IndexerDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;
        ReportSetAccessors(context, property.AccessorList, property.Identifier.ValueText);
    }

    private static void AnalyzeIndexer(SyntaxNodeAnalysisContext context)
    {
        var indexer = (IndexerDeclarationSyntax)context.Node;
        ReportSetAccessors(context, indexer.AccessorList, "this");
    }

    private static void ReportSetAccessors(SyntaxNodeAnalysisContext context, AccessorListSyntax? accessors, string memberName)
    {
        if (accessors is null || !TryGetRecord(accessors, out var record))
        {
            return;
        }

        foreach (var accessor in accessors.Accessors)
        {
            if (!accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, accessor.Keyword.GetLocation(), record.Identifier.ValueText, memberName));
        }
    }

    private static bool TryGetRecord(SyntaxNode node, out RecordDeclarationSyntax record)
    {
        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (current is BaseTypeDeclarationSyntax type)
            {
                if (type is RecordDeclarationSyntax found)
                {
                    record = found;
                    return true;
                }

                break;
            }
        }

        record = null!;
        return false;
    }
}
