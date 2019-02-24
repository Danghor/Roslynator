﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal abstract class SymbolFilterRule
    {
        public abstract SymbolFilterResult Result { get; }

        public abstract bool IsSuccess(ISymbol symbol);
    }
}
