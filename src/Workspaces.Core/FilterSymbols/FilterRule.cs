// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal static class FilterRule
    {
        public static SymbolFilterResult GetResult(ISymbol symbol, ImmutableArray<SymbolFilterRule> rules)
        {
            foreach (SymbolFilterRule rule in rules)
            {
                if (!rule.IsSuccess(symbol))
                    return rule.Result;
            }

            return SymbolFilterResult.Success;
        }

        public static SymbolFilterResult GetResult(AttributeData attribute, ImmutableArray<AttributeFilterRule> rules)
        {
            foreach (AttributeFilterRule rule in rules)
            {
                if (!rule.IsSuccess(attribute))
                    return rule.Result;
            }

            return SymbolFilterResult.Success;
        }
    }
}
