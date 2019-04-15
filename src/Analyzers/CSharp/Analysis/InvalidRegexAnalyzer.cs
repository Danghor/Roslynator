// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InvalidRegexAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.ValidateArgumentsCorrectly); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var memberAccessExpr =invocationExpr.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpr?.Name.ToString() != nameof(Regex.Match)) return;
            var memberSymbol = context.SemanticModel.
                GetSymbolInfo(memberAccessExpr).Symbol as IMethodSymbol;
            if (!memberSymbol?.ToString().StartsWith("System.Text.RegularExpressions.Regex.Match") ?? true) return;
            ArgumentListSyntax argumentList = invocationExpr.ArgumentList;
            if ((argumentList?.Arguments.Count ?? 0) < 2) return;
            if (!(argumentList.Arguments[1].Expression is LiteralExpressionSyntax regexLiteral)) return;
            var regexOpt = context.SemanticModel.GetConstantValue(regexLiteral);
            if (!regexOpt.HasValue) return;
            if (!(regexOpt.Value is string regex)) return;

            try
            {
                Regex.Match("", regex);
            }
            catch (ArgumentException e)
            {
                var diagnostic =Diagnostic.Create(Rule, regexLiteral.GetLocation(), e.Message);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}