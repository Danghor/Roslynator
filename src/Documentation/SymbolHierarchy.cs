// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator.Documentation
{
    internal sealed class SymbolHierarchy
    {
        private SymbolHierarchy(SymbolHierarchyItem root, SymbolHierarchyItem interfaceRoot)
        {
            Root = root;
            InterfaceRoot = interfaceRoot;
        }

        public SymbolHierarchyItem Root { get; }

        public SymbolHierarchyItem InterfaceRoot { get; }

        public static SymbolHierarchy Create(
            IEnumerable<IAssemblySymbol> assemblies,
            SymbolFilterOptions filter = null,
            IComparer<INamedTypeSymbol> comparer = null)
        {
            Func<INamedTypeSymbol, bool> predicate = null;

            if (filter != null)
                predicate = t => filter.IsVisibleType(t);

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

            var interfaceRoot = new SymbolHierarchyItem(null);

            List<SymbolHierarchyItem> rootInterfaces = null;

            foreach (KeyValuePair<INamedTypeSymbol, SymbolHierarchyItem> kvp in allItems)
            {
                if (IsRootInterface(kvp.Key))
                    (rootInterfaces ?? (rootInterfaces = new List<SymbolHierarchyItem>())).Add(kvp.Value);
            }

            if (rootInterfaces != null)
            {
                rootInterfaces.Sort(Compare);

                FillHierarchyItems(rootInterfaces, interfaceRoot);
            }

            return new SymbolHierarchy(root, interfaceRoot);

            SymbolHierarchyItem FillHierarchyItem(SymbolHierarchyItem item, SymbolHierarchyItem parent)
            {
                INamedTypeSymbol symbol = item.Symbol;

                if (item.Parent != null)
                    item = new SymbolHierarchyItem(symbol);

                item.Parent = parent;

                if (symbol.TypeKind != TypeKind.Interface)
                    allItems.Remove(symbol);

                SymbolHierarchyItem[] derivedTypes = allItems
                    .Select(f => f.Value)
                    .Where(f =>
                    {
                        if (symbol.TypeKind == TypeKind.Interface)
                        {
                            return f.Symbol.Interfaces.Any(i => i.OriginalDefinition == symbol.OriginalDefinition);
                        }
                        else
                        {
                            return f.Symbol.BaseType?.OriginalDefinition == symbol.OriginalDefinition;
                        }
                    })
                    .ToArray();

                if (derivedTypes.Length > 0)
                {
                    if (symbol.SpecialType == SpecialType.System_Object)
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

                    FillHierarchyItems(derivedTypes, item);
                }

                return item;
            }

            void FillHierarchyItems(IList<SymbolHierarchyItem> items, SymbolHierarchyItem parent)
            {
                SymbolHierarchyItem last = FillHierarchyItem(items[0], parent);

                SymbolHierarchyItem next = last;

                SymbolHierarchyItem child = null;

                for (int i = 1; i < items.Count; i++)
                {
                    child = FillHierarchyItem(items[i], parent);

                    child.next = next;

                    next = child;
                }

                last.next = child ?? last;

                parent.lastChild = last;
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

            bool IsRootInterface(INamedTypeSymbol interfaceSymbol)
            {
                foreach (INamedTypeSymbol interfaceSymbol2 in interfaceSymbol.Interfaces)
                {
                    foreach (KeyValuePair<INamedTypeSymbol, SymbolHierarchyItem> kvp in allItems)
                    {
                        if (kvp.Key == interfaceSymbol2)
                            return false;
                    }
                }

                return true;
            }

            int Compare(SymbolHierarchyItem x, SymbolHierarchyItem y)
            {
                return -comparer.Compare(x.Symbol, y.Symbol);
            }
        }
    }
}
