// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class SymbolFilterOptions
    {
        public static SymbolFilterOptions Default { get; } = new SymbolFilterOptions();

        internal SymbolFilterOptions(
            VisibilityFilter visibility = VisibilityFilter.All,
            SymbolGroupFilter symbolGroups = SymbolGroupFilter.TypeOrMember,
            IEnumerable<SymbolFilterRule> rules = null,
            IEnumerable<AttributeFilterRule> attributeRules = null)
        {
            Visibility = visibility;
            SymbolGroups = symbolGroups;

            Rules = rules?.ToImmutableArray() ?? ImmutableArray<SymbolFilterRule>.Empty;
            AttributeRules = attributeRules?.ToImmutableArray() ?? ImmutableArray<AttributeFilterRule>.Empty;
        }

        public VisibilityFilter Visibility { get; }

        public SymbolGroupFilter SymbolGroups { get; }

        public ImmutableArray<SymbolFilterRule> Rules { get; }

        public ImmutableArray<AttributeFilterRule> AttributeRules { get; }

        public bool IncludesSymbolGroup(SymbolGroupFilter symbolGroupFilter)
        {
            return (SymbolGroups & symbolGroupFilter) == symbolGroupFilter;
        }

        public bool IsSuccess(ISymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(INamespaceSymbol namespaceSymbol)
        {
            return GetResult(namespaceSymbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(INamedTypeSymbol typeSymbol)
        {
            return GetResult(typeSymbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(IEventSymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(IFieldSymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(IPropertySymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(IMethodSymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public SymbolFilterResult GetResult(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Namespace:
                    return GetResult((INamespaceSymbol)symbol);
                case SymbolKind.NamedType:
                    return GetResult((INamedTypeSymbol)symbol);
                case SymbolKind.Event:
                    return GetResult((IEventSymbol)symbol);
                case SymbolKind.Field:
                    return GetResult((IFieldSymbol)symbol);
                case SymbolKind.Property:
                    return GetResult((IPropertySymbol)symbol);
                case SymbolKind.Method:
                    return GetResult((IMethodSymbol)symbol);
                default:
                    throw new ArgumentException("", nameof(symbol));
            }
        }

        public virtual SymbolFilterResult GetResult(INamespaceSymbol namespaceSymbol)
        {
            return FilterRule.GetResult(namespaceSymbol, Rules);
        }

        public virtual SymbolFilterResult GetResult(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!IncludesSymbolGroup(typeSymbol.TypeKind.ToSymbolGroupFilter()))
                return SymbolFilterResult.SymbolGroup;

            if (!typeSymbol.IsVisible(Visibility))
                return SymbolFilterResult.Visibility;

            return FilterRule.GetResult(typeSymbol, Rules);
        }

        public virtual SymbolFilterResult GetResult(IEventSymbol symbol)
        {
            if (symbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!IncludesSymbolGroup(SymbolGroupFilter.Event))
                return SymbolFilterResult.SymbolGroup;

            if (!symbol.IsVisible(Visibility))
                return SymbolFilterResult.Visibility;

            return FilterRule.GetResult(symbol, Rules);
        }

        public virtual SymbolFilterResult GetResult(IFieldSymbol symbol)
        {
            if (symbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            var group = SymbolGroupFilter.None;

            if (symbol.IsConst)
            {
                group = (symbol.ContainingType.TypeKind == TypeKind.Enum) ? SymbolGroupFilter.EnumField : SymbolGroupFilter.Const;
            }
            else
            {
                group = SymbolGroupFilter.Field;
            }

            if (!IncludesSymbolGroup(group))
                return SymbolFilterResult.SymbolGroup;

            if (!symbol.IsVisible(Visibility))
                return SymbolFilterResult.Visibility;

            return FilterRule.GetResult(symbol, Rules);
        }

        public virtual SymbolFilterResult GetResult(IPropertySymbol symbol)
        {
            if (symbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!IncludesSymbolGroup((symbol.IsIndexer) ? SymbolGroupFilter.Indexer : SymbolGroupFilter.Property))
                return SymbolFilterResult.SymbolGroup;

            if (!symbol.IsVisible(Visibility))
                return SymbolFilterResult.Visibility;

            return FilterRule.GetResult(symbol, Rules);
        }

        public virtual SymbolFilterResult GetResult(IMethodSymbol symbol)
        {
            bool canBeImplicitlyDeclared = false;

            if (!IncludesSymbolGroup(SymbolGroupFilter.Method))
                return SymbolFilterResult.SymbolGroup;

            switch (symbol.MethodKind)
            {
                case MethodKind.Constructor:
                    {
                        TypeKind typeKind = symbol.ContainingType.TypeKind;

                        Debug.Assert(typeKind.Is(TypeKind.Class, TypeKind.Struct, TypeKind.Enum), symbol.ToDisplayString(SymbolDisplayFormats.Test));

                        if (typeKind == TypeKind.Class
                            && !symbol.Parameters.Any())
                        {
                            canBeImplicitlyDeclared = true;
                        }

                        break;
                    }
                case MethodKind.Conversion:
                case MethodKind.UserDefinedOperator:
                case MethodKind.Ordinary:
                case MethodKind.StaticConstructor:
                case MethodKind.Destructor:
                case MethodKind.ExplicitInterfaceImplementation:
                    {
                        break;
                    }
                default:
                    {
                        return SymbolFilterResult.Other;
                    }
            }

            if (!canBeImplicitlyDeclared && symbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!symbol.IsVisible(Visibility))
                return SymbolFilterResult.Visibility;

            return FilterRule.GetResult(symbol, Rules);
        }

        public bool IsSuccess(AttributeData attribute)
        {
            return GetResult(attribute) == SymbolFilterResult.Success;
        }

        public virtual SymbolFilterResult GetResult(AttributeData attribute)
        {
            return FilterRule.GetResult(attribute, AttributeRules);
        }
    }
}
