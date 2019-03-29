﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CodeFixes;
using System.Threading.Tasks;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1043RemovePartialModifierFromTypeWithSinglePartTests : AbstractCSharpFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } =
            DiagnosticDescriptors.RemovePartialModifierFromTypeWithSinglePart;

        public override DiagnosticAnalyzer Analyzer { get; } =
            new RemovePartialModifierFromTypeWithSinglePartAnalyzer();

        public override CodeFixProvider FixProvider { get; } =
            new RemovePartialModifierFromTypeWithSinglePartCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task Test_Class()
        {
            await VerifyDiagnosticAndFixAsync(@"
public [|partial|] class Foo
{
}
", @"
public class Foo
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task Test_Interface()
        {
            await VerifyDiagnosticAndFixAsync(@"
public [|partial|] interface Foo
{
}
", @"
public interface Foo
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task Test_Struct()
        {
            await VerifyDiagnosticAndFixAsync(@"
public [|partial|] struct Foo
{
}
", @"
public struct Foo
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task Test_MultipleMethodsNestedInClass()
        {
            await VerifyDiagnosticAndFixAsync(@"
public [|partial|] class Foo
{
    partial void FooMethod();

    partial void FooMethod()
    {
    }
}
", @"
public class Foo
{
    partial void FooMethod();

    partial void FooMethod()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task Test_MultipleMethodsNestedInStruct()
        {
            await VerifyDiagnosticAndFixAsync(@"
public [|partial|] struct Foo
{
    partial void FooMethod();

    partial void FooMethod()
    {
    }
}
", @"
public struct Foo
{
    partial void FooMethod();

    partial void FooMethod()
    {
    }
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task TestNoDiagnostic_NoPartialModifier()
        {
            await VerifyNoDiagnosticAsync(@"
public class Foo
{
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task TestNoDiagnostic_MultipleClassesNestedInStaticClass()
        {
            await VerifyNoDiagnosticAsync(@"
public static class A
{
    public partial class Foo
    {
    }
    
    public partial class Foo
    {
    }  
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task TestNoDiagnostic_MultipleClassesNestedInStruct()
        {
            await VerifyNoDiagnosticAsync(@"
public struct A
{
    public partial class Foo
    {
    }
    
    public partial class Foo
    {
    }  
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task TestNoDiagnostic_MultipleInterfacesNestedInStaticClass()
        {
            await VerifyNoDiagnosticAsync(@"
public static class A
{
    public partial interface Foo
    {
    }
    
    public partial interface Foo
    {
    }  
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task TestNoDiagnostic_MultipleInterfacesNestedInStruct()
        {
            await VerifyNoDiagnosticAsync(@"
public struct A
{
    public partial interface Foo
    {
    }
    
    public partial interface Foo
    {
    }  
}
");
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart)]
        public async Task TestNoDiagnostic_MultipleNestedClassesWithSameName()
        {
            await VerifyNoDiagnosticAsync(@"
public static class Foo
{
    private partial class Foo2
    {
    }

    private partial class Foo2
    {
    }
}

public struct FooStruct
{
    private partial class Foo2
    {
    }

    private partial class Foo2
    {
    }
}
");
        }
    }
}