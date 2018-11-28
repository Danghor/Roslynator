﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.Tests.CSharp
{
    public abstract class CSharpCodeRefactoringVerifier : RefactoringVerifier
    {
        public override CodeVerificationOptions Options => CSharpCodeVerificationOptions.Default;

        protected override WorkspaceFactory WorkspaceFactory => CSharpWorkspaceFactory.Instance;
    }
}
