// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class IgnoredNameSymbolFilterRule : SymbolFilterRule
    {
        public override SymbolFilterResult Result => SymbolFilterResult.Ignored;

        public MetadataNameSet MetadataNames { get; }

        public IgnoredNameSymbolFilterRule(IEnumerable<MetadataName> values)
        {
            MetadataNames = new MetadataNameSet(values);
        }

        public override bool IsSuccess(ISymbol symbol)
        {
            if (MetadataNames.Contains(symbol.ContainingNamespace))
                return false;

            switch (symbol.Kind)
            {
                case SymbolKind.Namespace:
                case SymbolKind.NamedType:
                    {
                        if (MetadataNames.Contains(symbol))
                            return false;

                        break;
                    }
                case SymbolKind.Event:
                case SymbolKind.Field:
                case SymbolKind.Method:
                case SymbolKind.Property:
                    {
                        break;
                    }
            }

            return true;
        }
    }
}
