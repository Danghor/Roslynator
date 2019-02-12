﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.FindSymbols;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class FindSymbolsCommand : MSBuildWorkspaceCommand
    {
        private static readonly SymbolDisplayFormat _nameAndContainingTypesSymbolDisplayFormat = SymbolDisplayFormat.CSharpErrorMessageFormat.Update(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
                | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName,
            parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut
                | SymbolDisplayParameterOptions.IncludeType
                | SymbolDisplayParameterOptions.IncludeName
                | SymbolDisplayParameterOptions.IncludeDefaultValue);

        public FindSymbolsCommand(
            FindSymbolsCommandLineOptions options,
            ImmutableArray<Visibility> visibilities,
            SymbolSpecialKinds symbolKinds,
            ImmutableArray<MetadataName> ignoredAttributes,
            in ProjectFilter projectFilter) : base(projectFilter)
        {
            Options = options;
            Visibilities = visibilities;
            SymbolKinds = symbolKinds;
            IgnoredAttributes = ignoredAttributes;
        }

        public FindSymbolsCommandLineOptions Options { get; }

        public ImmutableArray<Visibility> Visibilities { get; }

        public SymbolSpecialKinds SymbolKinds { get; }

        public ImmutableArray<MetadataName> IgnoredAttributes { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            HashSet<string> ignoredSymbols = (Options.IgnoredSymbols.Any())
                ? new HashSet<string>(Options.IgnoredSymbols)
                : null;

            var options = new SymbolFinderOptions(
                symbolKinds: SymbolKinds,
                visibilities: Visibilities,
                ignoredAttributes: IgnoredAttributes,
                ignoreObsolete: Options.IgnoreObsolete,
                ignoreGeneratedCode: Options.IgnoreGeneratedCode,
                unusedOnly: Options.UnusedOnly);

            var progress = new FindSymbolsProgress();

            ImmutableArray<ISymbol> allSymbols;

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                WriteLine($"Analyze '{project.Name}'", Verbosity.Minimal);

                allSymbols = await AnalyzeProject(project, options, progress, cancellationToken);
            }
            else
            {
                Solution solution = projectOrSolution.AsSolution();

                WriteLine($"Analyze solution '{solution.FilePath}'", Verbosity.Minimal);

                ImmutableArray<ISymbol>.Builder symbols = null;

                Stopwatch stopwatch = Stopwatch.StartNew();

                foreach (Project project in FilterProjects(solution, s => s
                    .GetProjectDependencyGraph()
                    .GetTopologicallySortedProjects(cancellationToken)
                    .ToImmutableArray()))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    WriteLine($"  Analyze '{project.Name}'", Verbosity.Minimal);

                    ImmutableArray<ISymbol> projectSymbols = await AnalyzeProject(project, options, progress, cancellationToken);

                    if (!projectSymbols.Any())
                        continue;

                    if (ignoredSymbols?.Count > 0)
                    {
                        Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                        ImmutableDictionary<string, ISymbol> symbolsById = ignoredSymbols
                            .Select(f => (id: f, symbol: DocumentationCommentId.GetFirstSymbolForDeclarationId(f, compilation)))
                            .Where(f => f.id != null)
                            .ToImmutableDictionary(f => f.id, f => f.symbol);

                        ignoredSymbols.ExceptWith(symbolsById.Select(f => f.Key));

                        projectSymbols = projectSymbols.Except(symbolsById.Select(f => f.Value)).ToImmutableArray();

                        if (!projectSymbols.Any())
                            continue;
                    }

                    int maxKindLength = projectSymbols
                        .Select(f => f.GetSpecialKind())
                        .Distinct()
                        .Max(f => f.ToString().Length);

                    foreach (ISymbol symbol in projectSymbols.OrderBy(f => f, SymbolDefinitionComparer.Instance))
                    {
                        WriteSymbol(symbol, Verbosity.Normal, indentation: "    ", addCommentId: true, padding: maxKindLength);
                    }

                        (symbols ?? (symbols = ImmutableArray.CreateBuilder<ISymbol>())).AddRange(projectSymbols);
                }

                stopwatch.Stop();

                allSymbols = symbols?.ToImmutableArray() ?? ImmutableArray<ISymbol>.Empty;

                WriteLine($"Done analyzing solution '{solution.FilePath}' in {stopwatch.Elapsed:mm\\:ss\\.ff}", Verbosity.Minimal);
            }

            //TODO: Summary?
            if (allSymbols.Any())
            {
                Dictionary<SymbolSpecialKind, int> countByKind = allSymbols
                    .GroupBy(f => f.GetSpecialKind())
                    .OrderByDescending(f => f.Count())
                    .ThenBy(f => f.Key)
                    .ToDictionary(f => f.Key, f => f.Count());

                int maxKindLength = countByKind.Max(f => f.Key.ToString().Length);

                int maxCountLength = countByKind.Max(f => f.Value.ToString().Length);

                WriteLine(Verbosity.Normal);

                foreach (ISymbol symbol in allSymbols.OrderBy(f => f, SymbolDefinitionComparer.Instance))
                {
                    WriteSymbol(symbol, Verbosity.Normal, colorNamespace: true, padding: maxKindLength);
                }

                WriteLine(Verbosity.Normal);

                foreach (KeyValuePair<SymbolSpecialKind, int> kvp in countByKind)
                {
                    WriteLine($"{kvp.Value.ToString().PadLeft(maxCountLength)} {kvp.Key.ToString().ToLowerInvariant()} symbols", Verbosity.Normal);
                }
            }

            WriteLine(Verbosity.Minimal);
            WriteLine($"{allSymbols.Length} {((allSymbols.Length == 1) ? "symbol" : "symbols")} found", ConsoleColor.Green, Verbosity.Minimal);
            WriteLine(Verbosity.Minimal);

            return CommandResult.Success;
        }

        private static Task<ImmutableArray<ISymbol>> AnalyzeProject(
            Project project,
            SymbolFinderOptions options,
            IFindSymbolsProgress progress,
            CancellationToken cancellationToken)
        {
            return SymbolFinder.FindSymbolsAsync(project, options, progress, cancellationToken);
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Analysis was canceled.", Verbosity.Quiet);
        }

        private static void WriteSymbol(
            ISymbol symbol,
            Verbosity verbosity,
            string indentation = "",
            bool addCommentId = false,
            bool colorNamespace = false,
            int padding = 0)
        {
            if (!ShouldWrite(verbosity))
                return;

            bool isObsolete = symbol.HasAttribute(MetadataNames.System_ObsoleteAttribute);

            Write(indentation, verbosity);

            string kindText = symbol.GetSpecialKind().ToString().ToLowerInvariant();

            if (isObsolete)
            {
                Write(kindText, ConsoleColor.DarkGray, verbosity);
            }
            else
            {
                Write(kindText, verbosity);
            }

            Write(' ', padding - kindText.Length + 1, verbosity);

            string namespaceText = symbol.ContainingNamespace.ToDisplayString();

            if (namespaceText.Length > 0)
            {
                if (colorNamespace || isObsolete)
                {
                    Write(namespaceText, ConsoleColor.DarkGray, verbosity);
                    Write(".", ConsoleColor.DarkGray, verbosity);
                }
                else
                {
                    Write(namespaceText, verbosity);
                    Write(".", verbosity);
                }
            }

            string nameText = symbol.ToDisplayString(_nameAndContainingTypesSymbolDisplayFormat);

            if (isObsolete)
            {
                Write(nameText, ConsoleColor.DarkGray, verbosity);
            }
            else
            {
                Write(nameText, verbosity);
            }

            if (addCommentId
                && ShouldWrite(Verbosity.Diagnostic))
            {
                WriteLine(verbosity);
                Write(indentation);
                Write("ID:", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                Write(' ', padding - 2, Verbosity.Diagnostic);
                WriteLine(symbol.GetDocumentationCommentId(), ConsoleColor.DarkGray, Verbosity.Diagnostic);
            }
            else
            {
                WriteLine(verbosity);
            }
        }

        private class FindSymbolsProgress : IFindSymbolsProgress
        {
            public void OnSymbolFound(ISymbol symbol)
            {
            }
        }
    }
}
