// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator.Documentation
{
    internal sealed class SymbolHierarchy
    {
        private SymbolHierarchy(SymbolHierarchyItem root)
        {
            Root = root;
        }

        public SymbolHierarchyItem Root { get; }

        public static SymbolHierarchy Create(
            IEnumerable<IAssemblySymbol> assemblies,
            SymbolFilterOptions filter = null,
            IComparer<INamedTypeSymbol> comparer = null)
        {
            Func<INamedTypeSymbol, bool> predicate = null;

            if (filter != null)
            {
                predicate = t => filter.IsVisibleType(t)
                    && filter.IsVisibleNamespace(t.ContainingNamespace);
            }

            IEnumerable<INamedTypeSymbol> types = assemblies.SelectMany(a => a.GetTypes(predicate));

            return Create(types, comparer);
        }

        public static SymbolHierarchy Create(IEnumerable<INamedTypeSymbol> types, IComparer<INamedTypeSymbol> comparer = null)
        {
            if (comparer == null)
                comparer = SymbolDefinitionComparer.SystemNamespaceFirstInstance;

            INamedTypeSymbol objectType = FindObjectType();

            if (objectType == null)
                throw new InvalidOperationException("Object type not found.");

            Dictionary<INamedTypeSymbol, SymbolHierarchyItem> allItems = types
                .ToDictionary(f => f, f => new SymbolHierarchyItem(f));

            allItems[objectType] = new SymbolHierarchyItem(objectType, isExternal: true);

            foreach (INamedTypeSymbol type in types)
            {
                INamedTypeSymbol t = type.BaseType;

                while (t != null)
                {
                    if (!allItems.ContainsKey(t.OriginalDefinition))
                        allItems[t.OriginalDefinition] = new SymbolHierarchyItem(t.OriginalDefinition, isExternal: true);

                    t = t.BaseType;
                }
            }

            SymbolHierarchyItem root = FillHierarchyItem(allItems[objectType], null);

            return new SymbolHierarchy(root);

            SymbolHierarchyItem FillHierarchyItem(SymbolHierarchyItem item, SymbolHierarchyItem parent)
            {
                item.Parent = parent;

                allItems.Remove(item.Symbol);

                SymbolHierarchyItem[] derivedTypes = allItems
                    .Select(f => f.Value)
                    .Where(f => f.Symbol.BaseType?.OriginalDefinition == item.Symbol.OriginalDefinition
                        || f.Symbol.Interfaces.Any(i => i.OriginalDefinition == item.Symbol.OriginalDefinition))
                    .ToArray();

                if (derivedTypes.Length > 0)
                {
                    if (item.Symbol.SpecialType == SpecialType.System_Object)
                    {
                        Array.Sort(derivedTypes, (x, y) =>
                        {
                            if (x.Symbol.IsStatic)
                            {
                                if (!y.Symbol.IsStatic)
                                {
                                    return -1;
                                }
                            }
                            else if (y.Symbol.IsStatic)
                            {
                                return 1;
                            }

                            return Compare(x, y);
                        });
                    }
                    else
                    {
                        Array.Sort(derivedTypes, Compare);
                    }

                    SymbolHierarchyItem last = FillHierarchyItem(derivedTypes[0], item);

                    SymbolHierarchyItem next = last;

                    SymbolHierarchyItem child = null;

                    for (int i = 1; i < derivedTypes.Length; i++)
                    {
                        child = FillHierarchyItem(derivedTypes[i], item);

                        child.next = next;

                        next = child;
                    }

                    last.next = child ?? last;

                    item.lastChild = last;
                }

                return item;
            }

            INamedTypeSymbol FindObjectType()
            {
                foreach (INamedTypeSymbol type in types)
                {
                    INamedTypeSymbol t = type;

                    do
                    {
                        if (t.SpecialType == SpecialType.System_Object)
                            return t;

                        t = t.BaseType;
                    }
                    while (t != null);
                }

                return null;
            }

            int Compare(SymbolHierarchyItem x, SymbolHierarchyItem y)
            {
                return -comparer.Compare(x.Symbol, y.Symbol);
            }
        }
    }
}
