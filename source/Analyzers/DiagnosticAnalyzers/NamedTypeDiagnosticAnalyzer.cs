﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NamedTypeDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.RemovePartialModifierFromTypeWithSinglePart,
                    DiagnosticDescriptors.MarkClassAsStatic);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.RegisterSymbolAction(f => AnalyzeNamedType(f), SymbolKind.NamedType);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            if (GeneratedCodeAnalyzer?.IsGeneratedCode(context) == true)
                return;

            var symbol = (INamedTypeSymbol)context.Symbol;

            TypeKind kind = symbol.TypeKind;

            if (kind == TypeKind.Class
                || kind == TypeKind.Struct
                || kind == TypeKind.Interface)
            {
                ImmutableArray<SyntaxReference> syntaxReferences = symbol.DeclaringSyntaxReferences;

                if (syntaxReferences.Length == 1)
                {
                    var declaration = syntaxReferences[0].GetSyntax(context.CancellationToken) as MemberDeclarationSyntax;

                    if (declaration != null)
                    {
                        SyntaxToken partialToken = declaration.GetModifiers()
                            .FirstOrDefault(f => f.IsKind(SyntaxKind.PartialKeyword));

                        if (partialToken.IsKind(SyntaxKind.PartialKeyword))
                        {
                            context.ReportDiagnostic(
                                DiagnosticDescriptors.RemovePartialModifierFromTypeWithSinglePart,
                                partialToken.GetLocation());
                        }
                    }
                }
            }

            if (MarkClassAsStaticRefactoring.CanRefactor(symbol))
            {
                foreach (SyntaxReference syntaxReference in symbol.DeclaringSyntaxReferences)
                {
                    var classDeclaration = syntaxReference.GetSyntax(context.CancellationToken) as ClassDeclarationSyntax;

                    if (classDeclaration?.Modifiers.Contains(SyntaxKind.StaticKeyword) == false)
                    {
                        context.ReportDiagnostic(
                            DiagnosticDescriptors.MarkClassAsStatic,
                            classDeclaration.Identifier.GetLocation());

                        break;
                    }
                }
            }
        }
    }
}
