// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator
{
    internal enum MemberDeclarationKind
    {
        None = 0,
        Const = 1,
        Field = 2,
        StaticConstructor = 3,
        Constructor = 4,
        Destructor = 5,
        Event = 6,
        Property = 7,
        Indexer = 8,
        Method = 9,
        ConversionOperator = 10,
        Operator = 11
    }
}
