// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class WithoutAttributeFilterRule : WithAttributeFilterRule
    {
        public WithoutAttributeFilterRule(IEnumerable<MetadataName> attributeNames) : base(attributeNames)
        {
        }

        public override SymbolFilterResult Result => SymbolFilterResult.WithoutAttibute;

        public override bool IsSuccess(ISymbol symbol)
        {
            return !base.IsSuccess(symbol);
        }
    }
}
