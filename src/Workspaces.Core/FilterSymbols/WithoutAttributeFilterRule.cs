// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class WithoutAttributeFilterRule : WithAttributeFilterRule
    {
        public WithoutAttributeFilterRule(IEnumerable<MetadataName> attributeNames) : base(attributeNames)
        {
        }

        public override SymbolFilterReason Reason => SymbolFilterReason.WithoutAttibute;

        public override bool IsMatch(ISymbol symbol)
        {
            return !base.IsMatch(symbol);
        }
    }
}
