﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator
{
    internal enum SymbolFilterResult
    {
        Success = 0,
        NotVisible = 1,
        UnsupportedSymbolGroup = 2,
        Ignored = 3,
        HasAttribute = 4,
        HasNotAttribute = 5,
        ImplicitlyDeclared = 6,
        Other = 7
    }
}
