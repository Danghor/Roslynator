// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class WithAttributeFilterRule : SymbolFilterRule
    {
        public WithAttributeFilterRule(IEnumerable<MetadataName> attributeNames)
        {
            Attributes = new MetadataNameSet(attributeNames);
        }

        public override SymbolFilterResult Result => SymbolFilterResult.WithAttibute;

        public MetadataNameSet Attributes { get; }

        public override bool IsSuccess(ISymbol symbol)
        {
            foreach (AttributeData attribute in symbol.GetAttributes())
            {
                if (Attributes.Contains(attribute.AttributeClass))
                    return true;
            }

            return false;
        }
    }
}
