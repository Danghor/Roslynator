// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    //TODO: PredicateSymbolFilterRule?
    internal class PredicateSymbolFilterRule : SymbolFilterRule
    {
        private readonly Func<ISymbol, bool> _predicate;

        public PredicateSymbolFilterRule(Func<ISymbol, bool> predicate, SymbolFilterResult result)
        {
            _predicate = predicate;
            Result = result;
        }

        public override bool IsSuccess(ISymbol symbol)
        {
            return _predicate(symbol);
        }

        public override SymbolFilterResult Result { get; }

        public PredicateSymbolFilterRule Invert()
        {
            return new PredicateSymbolFilterRule(f => !_predicate(f), Result);
        }
    }
}
