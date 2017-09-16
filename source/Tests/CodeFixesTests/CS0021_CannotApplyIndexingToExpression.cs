﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0021_CannotApplyIndexingToExpression
    {
        private static void Foo()
        {
            int i = 0;

            string s = i.ToString["f"];
        }
    }
}
