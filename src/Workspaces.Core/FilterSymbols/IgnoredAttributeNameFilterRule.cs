// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class IgnoredAttributeNameFilterRule : AttributeFilterRule
    {
        public MetadataNameSet MetadataNames { get; }

        public IgnoredAttributeNameFilterRule(IEnumerable<MetadataName> values)
        {
            MetadataNames = new MetadataNameSet(values);
        }

        public override SymbolFilterResult Result => SymbolFilterResult.Ignored;

        public override bool IsSuccess(AttributeData value)
        {
            return !MetadataNames.Contains(value.AttributeClass);
        }
    }
}
