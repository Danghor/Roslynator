﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Host.Mef;

namespace Roslynator.FindSymbols
{
    internal static class SymbolFinder
    {
        internal static async Task<ImmutableArray<ISymbol>> FindSymbolsAsync(
            Project project,
            SymbolFinderOptions options = null,
            IFindSymbolsProgress progress = null,
            CancellationToken cancellationToken = default)
        {
            Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            INamedTypeSymbol generatedCodeAttribute = compilation.GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute");

            ImmutableArray<ISymbol>.Builder symbols = null;

            var namespaceOrTypeSymbols = new Stack<INamespaceOrTypeSymbol>();

            namespaceOrTypeSymbols.Push(compilation.Assembly.GlobalNamespace);

            while (namespaceOrTypeSymbols.Count > 0)
            {
                INamespaceOrTypeSymbol namespaceOrTypeSymbol = namespaceOrTypeSymbols.Pop();

                foreach (ISymbol symbol in namespaceOrTypeSymbol.GetMembers())
                {
                    SymbolKind kind = symbol.Kind;

                    if (kind == SymbolKind.Namespace)
                    {
                        var namespaceSymbol = (INamespaceSymbol)symbol;

                        if (options.IsSuccess(namespaceSymbol))
                            namespaceOrTypeSymbols.Push(namespaceSymbol);

                        continue;
                    }

                    bool isUnused = false;

                    if (!options.UnusedOnly
                        || UnusedSymbolUtility.CanBeUnusedSymbol(symbol))
                    {
                        SymbolFilterResult result = options.GetResult(symbol);

                        switch (result)
                        {
                            case SymbolFilterResult.Success:
                                {
                                    if (options.IgnoreGeneratedCode
                                        && GeneratedCodeUtility.IsGeneratedCode(symbol, generatedCodeAttribute, MefWorkspaceServices.Default.GetService<ISyntaxFactsService>(compilation.Language).IsComment, cancellationToken))
                                    {
                                        continue;
                                    }

                                    if (options.UnusedOnly)
                                    {
                                        isUnused = await UnusedSymbolUtility.IsUnusedSymbolAsync(symbol, project.Solution, cancellationToken).ConfigureAwait(false);
                                    }

                                    if (!options.UnusedOnly
                                        || isUnused)
                                    {
                                        progress?.OnSymbolFound(symbol);

                                        (symbols ?? (symbols = ImmutableArray.CreateBuilder<ISymbol>())).Add(symbol);
                                    }

                                    break;
                                }
                            case SymbolFilterResult.NotVisible:
                            case SymbolFilterResult.HasAttribute:
                            case SymbolFilterResult.ImplicitlyDeclared:
                                {
                                    continue;
                                }
                            case SymbolFilterResult.UnsupportedSymbolGroup:
                            case SymbolFilterResult.Ignored:
                            case SymbolFilterResult.HasNotAttribute:
                            case SymbolFilterResult.Other:
                                {
                                    break;
                                }
                            default:
                                {
                                    Debug.Fail(result.ToString());
                                    break;
                                }
                        }
                    }

                    if (!isUnused
                        && kind == SymbolKind.NamedType)
                    {
                        namespaceOrTypeSymbols.Push((INamedTypeSymbol)symbol);
                    }
                }
            }

            return symbols?.ToImmutableArray() ?? ImmutableArray<ISymbol>.Empty;
        }
    }
}
