﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SharpUnion.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ChildWithGeneric : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        "DU1",
        "Child with Generic",
        "A DU member contains a generic. All generics should be defined on the parent type.",
        "SharpUnion",
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterSymbolStartAction(context =>
        {
            if (context.Symbol is INamedTypeSymbol symbol)
            {
                foreach (var attribute in symbol.GetAttributes())
                {
                    if (attribute.AttributeClass?.IsSharpUnionAttribute() == true)
                    {
                        context.RegisterSymbolEndAction(context => Analyze(context, symbol));
                        return;
                    }
                }
            }
        }, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol symbol)
    {
        var recordMembers = symbol.GetTypeMembers();
        foreach (var member in recordMembers)
        {
            var ignoreMember = false;
            foreach (var attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass?.IsSharpUnionIgnoreAttribute() == true)
                {
                    ignoreMember = true;
                }
            }

            if (!ignoreMember && member.TypeArguments.Length > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations.FirstOrDefault()));
            }
        }
    }
}