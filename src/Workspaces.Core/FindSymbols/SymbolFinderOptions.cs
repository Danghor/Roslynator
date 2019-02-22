// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Roslynator.FindSymbols
{
    internal class SymbolFinderOptions : SymbolFilterOptions
    {
        internal SymbolFinderOptions(
            VisibilityFilter visibility = VisibilityFilter.All,
            SymbolGroupFilter symbolGroups = SymbolGroupFilter.TypeOrMember,
            IEnumerable<MetadataName> ignoredSymbols = null,
            IEnumerable<MetadataName> ignoredAttributes = null,
            IEnumerable<MetadataName> withAttributes = null,
            IEnumerable<MetadataName> withoutAttributes = null,
            bool ignoreGeneratedCode = false,
            bool unusedOnly = false) : base(visibility, symbolGroups, ignoredSymbols, ignoredAttributes)
        {
            WithAttributes = (withAttributes != null) ? new MetadataNameSet(withAttributes) : MetadataNameSet.Empty;
            WithoutAttributes = (withoutAttributes != null) ? new MetadataNameSet(withoutAttributes) : MetadataNameSet.Empty;

            IgnoreGeneratedCode = ignoreGeneratedCode;
            UnusedOnly = unusedOnly;
        }

        new public static SymbolFinderOptions Default { get; } = new SymbolFinderOptions();

        public bool IgnoreGeneratedCode { get; }

        public bool UnusedOnly { get; }

        public MetadataNameSet WithAttributes { get; }

        public MetadataNameSet WithoutAttributes { get; }

        public override SymbolFilterResult GetResult(INamespaceSymbol namespaceSymbol)
        {
            SymbolFilterResult result = base.GetResult(namespaceSymbol);

            if (result != SymbolFilterResult.Success)
                return result;

            return VerifyAttributes(namespaceSymbol);
        }

        public override SymbolFilterResult GetResult(INamedTypeSymbol typeSymbol)
        {
            SymbolFilterResult result = base.GetResult(typeSymbol);

            if (result != SymbolFilterResult.Success)
                return result;

            return VerifyAttributes(typeSymbol);
        }

        public override SymbolFilterResult GetResult(IEventSymbol symbol)
        {
            SymbolFilterResult result = base.GetResult(symbol);

            if (result != SymbolFilterResult.Success)
                return result;

            return VerifyAttributes(symbol);
        }

        public override SymbolFilterResult GetResult(IFieldSymbol symbol)
        {
            SymbolFilterResult result = base.GetResult(symbol);

            if (result != SymbolFilterResult.Success)
                return result;

            return VerifyAttributes(symbol);
        }

        public override SymbolFilterResult GetResult(IMethodSymbol symbol)
        {
            SymbolFilterResult result = base.GetResult(symbol);

            if (result != SymbolFilterResult.Success)
                return result;

            return VerifyAttributes(symbol);
        }

        public override SymbolFilterResult GetResult(IPropertySymbol symbol)
        {
            SymbolFilterResult result = base.GetResult(symbol);

            if (result != SymbolFilterResult.Success)
                return result;

            return VerifyAttributes(symbol);
        }

        private SymbolFilterResult VerifyAttributes(ISymbol symbol)
        {
            if (WithAttributes.IsEmpty
                && WithoutAttributes.IsEmpty)
            {
                return SymbolFilterResult.Success;
            }

            ImmutableArray<AttributeData> attributes = symbol.GetAttributes();

            if (!WithAttributes.IsEmpty)
            {
                bool hasAttribute = false;

                foreach (AttributeData attribute in attributes)
                {
                    if (WithAttributes.Contains(attribute.AttributeClass))
                    {
                        hasAttribute = true;
                        break;
                    }
                }

                if (!hasAttribute)
                    return SymbolFilterResult.HasNotAttribute;
            }

            if (!WithoutAttributes.IsEmpty)
            {
                foreach (AttributeData attribute in attributes)
                {
                    if (WithoutAttributes.Contains(attribute.AttributeClass))
                        return SymbolFilterResult.HasAttribute;
                }
            }

            return  SymbolFilterResult.Success;
        }
    }
}
