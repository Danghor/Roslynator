// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal class SymbolFilterOptions
    {
        private readonly MetadataNameSet _ignoredSymbols;
        private readonly MetadataNameSet _ignoredAttributes;

        public static SymbolFilterOptions Default { get; } = new SymbolFilterOptions();

        public static SymbolFilterOptions Documentation { get; } = new SymbolFilterOptions(
            visibility: VisibilityFilter.Public,
            symbolGroups: SymbolGroupFilter.TypeOrMember,
            ignoredSymbols: null,
            ignoredAttributes: GetDocumentationIgnoredAttributeNames().Select(f => MetadataName.Parse(f)));

        internal static string[] GetDocumentationIgnoredAttributeNames()
        {
            return new string[]
            {
                "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute",
                "System.Diagnostics.ConditionalAttribute",
                "System.Diagnostics.DebuggableAttribute",
                "System.Diagnostics.DebuggerBrowsableAttribute",
                "System.Diagnostics.DebuggerDisplayAttribute",
                "System.Diagnostics.DebuggerHiddenAttribute",
                "System.Diagnostics.DebuggerNonUserCodeAttribute",
                "System.Diagnostics.DebuggerStepperBoundaryAttribute",
                "System.Diagnostics.DebuggerStepThroughAttribute",
                "System.Diagnostics.DebuggerTypeProxyAttribute",
                "System.Diagnostics.DebuggerVisualizerAttribute",
                "System.Reflection.DefaultMemberAttribute",
                "System.Reflection.AssemblyConfigurationAttribute",
                "System.Reflection.AssemblyCultureAttribute",
                "System.Reflection.AssemblyVersionAttribute",
                "System.Runtime.CompilerServices.AsyncIteratorStateMachineAttribute",
                "System.Runtime.CompilerServices.AsyncStateMachineAttribute",
                "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
                "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
                "System.Runtime.CompilerServices.IsReadOnlyAttribute",
                "System.Runtime.CompilerServices.InternalsVisibleToAttribute",
                "System.Runtime.CompilerServices.IteratorStateMachineAttribute",
                "System.Runtime.CompilerServices.MethodImplAttribute",
                "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
                "System.Runtime.CompilerServices.StateMachineAttribute",
                "System.Runtime.CompilerServices.TupleElementNamesAttribute",
                "System.Runtime.CompilerServices.TypeForwardedFromAttribute",
                "System.Runtime.CompilerServices.TypeForwardedToAttribute"
            };
        }
#if DEBUG
        private static readonly MetadataNameSet _knownVisibleAttributes = new MetadataNameSet(new string[]
        {
            "Microsoft.CodeAnalysis.CommitHashAttribute",
            "System.AttributeUsageAttribute",
            "System.CLSCompliantAttribute",
            "System.ComVisibleAttribute",
            "System.FlagsAttribute",
            "System.ObsoleteAttribute",
            "System.ComponentModel.DefaultValueAttribute",
            "System.ComponentModel.EditorBrowsableAttribute",
            "System.Composition.MetadataAttributeAttribute",
            "System.Reflection.AssemblyCompanyAttribute",
            "System.Reflection.AssemblyCopyrightAttribute",
            "System.Reflection.AssemblyDescriptionAttribute",
            "System.Reflection.AssemblyFileVersionAttribute",
            "System.Reflection.AssemblyInformationalVersionAttribute",
            "System.Reflection.AssemblyMetadataAttribute",
            "System.Reflection.AssemblyProductAttribute",
            "System.Reflection.AssemblyTitleAttribute",
            "System.Reflection.AssemblyTrademarkAttribute",
            "System.Runtime.CompilerServices.InternalImplementationOnlyAttribute",
            "System.Runtime.InteropServices.GuidAttribute",
            "System.Runtime.Versioning.TargetFrameworkAttribute",
            "System.Xml.Serialization.XmlArrayItemAttribute",
            "System.Xml.Serialization.XmlAttributeAttribute",
            "System.Xml.Serialization.XmlElementAttribute",
            "System.Xml.Serialization.XmlRootAttribute",
        });
#endif

        internal SymbolFilterOptions(
            VisibilityFilter visibility = VisibilityFilter.All,
            SymbolGroupFilter symbolGroups = SymbolGroupFilter.TypeOrMember,
            IEnumerable<MetadataName> ignoredSymbols = null,
            IEnumerable<MetadataName> ignoredAttributes = null)
        {
            Visibility = visibility;
            SymbolGroups = symbolGroups;
            _ignoredSymbols = (ignoredSymbols != null) ? new MetadataNameSet(ignoredSymbols) : null;
            _ignoredAttributes = (ignoredAttributes != null) ? new MetadataNameSet(ignoredAttributes) : null;
        }

        public VisibilityFilter Visibility { get; }

        public SymbolGroupFilter SymbolGroups { get; }

        public ImmutableArray<MetadataName> IgnoredSymbols
        {
            get { return _ignoredSymbols?.Values ?? ImmutableArray<MetadataName>.Empty; }
        }

        public ImmutableArray<MetadataName> IgnoredAttributes
        {
            get { return _ignoredAttributes?.Values ?? ImmutableArray<MetadataName>.Empty; }
        }

        public bool IncludesSymbolGroup(SymbolGroupFilter symbolGroupFilter)
        {
            return (SymbolGroups & symbolGroupFilter) == symbolGroupFilter;
        }

        public bool IsSuccess(ISymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(INamespaceSymbol namespaceSymbol)
        {
            return GetResult(namespaceSymbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(INamedTypeSymbol typeSymbol)
        {
            return GetResult(typeSymbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(IEventSymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(IFieldSymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(IPropertySymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public bool IsSuccess(IMethodSymbol symbol)
        {
            return GetResult(symbol) == SymbolFilterResult.Success;
        }

        public SymbolFilterResult GetResult(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Namespace:
                    return GetResult((INamespaceSymbol)symbol);
                case SymbolKind.NamedType:
                    return GetResult((INamedTypeSymbol)symbol);
                case SymbolKind.Event:
                    return GetResult((IEventSymbol)symbol);
                case SymbolKind.Field:
                    return GetResult((IFieldSymbol)symbol);
                case SymbolKind.Property:
                    return GetResult((IPropertySymbol)symbol);
                case SymbolKind.Method:
                    return GetResult((IMethodSymbol)symbol);
                default:
                    throw new ArgumentException("", nameof(symbol));
            }
        }

        public virtual SymbolFilterResult GetResult(INamespaceSymbol namespaceSymbol)
        {
            if (_ignoredSymbols?.Contains(namespaceSymbol) == true)
                return SymbolFilterResult.Ignored;

            return SymbolFilterResult.Success;
        }

        public virtual SymbolFilterResult GetResult(INamedTypeSymbol typeSymbol)
        {
            if (typeSymbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!IncludesSymbolGroup(typeSymbol.TypeKind.ToSymbolGroupFilter()))
                return SymbolFilterResult.UnsupportedSymbolGroup;

            if (!typeSymbol.IsVisible(Visibility))
                return SymbolFilterResult.NotVisible;

            if (_ignoredSymbols?.Contains(typeSymbol) == true)
                return SymbolFilterResult.Ignored;

            if (_ignoredSymbols?.Contains(typeSymbol.ContainingSymbol) == true)
                return SymbolFilterResult.Ignored;

            return SymbolFilterResult.Success;
        }

        public virtual SymbolFilterResult GetResult(IEventSymbol symbol)
        {
            if (!IncludesSymbolGroup(SymbolGroupFilter.Event))
                return SymbolFilterResult.UnsupportedSymbolGroup;

            if (symbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!symbol.IsVisible(Visibility))
                return SymbolFilterResult.NotVisible;

            if (_ignoredSymbols?.Contains(symbol.ContainingSymbol) == true)
                return SymbolFilterResult.Ignored;

            return SymbolFilterResult.Success;
        }

        public virtual SymbolFilterResult GetResult(IFieldSymbol symbol)
        {
            var group = SymbolGroupFilter.None;

            if (symbol.IsConst)
            {
                group = (symbol.ContainingType.TypeKind == TypeKind.Enum) ? SymbolGroupFilter.EnumField : SymbolGroupFilter.Const;
            }
            else
            {
                group = SymbolGroupFilter.Field;
            }

            if (!IncludesSymbolGroup(group))
                return SymbolFilterResult.UnsupportedSymbolGroup;

            if (symbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!symbol.IsVisible(Visibility))
                return SymbolFilterResult.NotVisible;

            if (_ignoredSymbols?.Contains(symbol.ContainingSymbol) == true)
                return SymbolFilterResult.Ignored;

            return SymbolFilterResult.Success;
        }

        public virtual SymbolFilterResult GetResult(IPropertySymbol symbol)
        {
            if (!IncludesSymbolGroup((symbol.IsIndexer) ? SymbolGroupFilter.Indexer : SymbolGroupFilter.Property))
                return SymbolFilterResult.UnsupportedSymbolGroup;

            if (symbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!symbol.IsVisible(Visibility))
                return SymbolFilterResult.NotVisible;

            if (_ignoredSymbols?.Contains(symbol.ContainingSymbol) == true)
                return SymbolFilterResult.Ignored;

            return SymbolFilterResult.Success;
        }

        public virtual SymbolFilterResult GetResult(IMethodSymbol symbol)
        {
            bool canBeImplicitlyDeclared = false;

            if (!IncludesSymbolGroup(SymbolGroupFilter.Method))
                return SymbolFilterResult.UnsupportedSymbolGroup;

            switch (symbol.MethodKind)
            {
                case MethodKind.Constructor:
                    {
                        TypeKind typeKind = symbol.ContainingType.TypeKind;

                        Debug.Assert(typeKind.Is(TypeKind.Class, TypeKind.Struct, TypeKind.Enum), symbol.ToDisplayString(Roslynator.SymbolDisplayFormats.Test));

                        if (typeKind == TypeKind.Class
                            && !symbol.Parameters.Any())
                        {
                            canBeImplicitlyDeclared = true;
                        }

                        break;
                    }
                case MethodKind.Conversion:
                case MethodKind.UserDefinedOperator:
                case MethodKind.Ordinary:
                case MethodKind.StaticConstructor:
                case MethodKind.Destructor:
                case MethodKind.ExplicitInterfaceImplementation:
                    {
                        break;
                    }
                default:
                    {
                        return SymbolFilterResult.Other;
                    }
            }

            if (!canBeImplicitlyDeclared && symbol.IsImplicitlyDeclared)
                return SymbolFilterResult.ImplicitlyDeclared;

            if (!symbol.IsVisible(Visibility))
                return SymbolFilterResult.NotVisible;

            if (_ignoredSymbols?.Contains(symbol.ContainingSymbol) == true)
                return SymbolFilterResult.Ignored;

            return SymbolFilterResult.Success;
        }

        public virtual bool IsVisibleAttribute(INamedTypeSymbol attributeType)
        {
            if (_ignoredAttributes?.Contains(attributeType) == true)
                return false;
#if DEBUG
            switch (attributeType.MetadataName)
            {
                case "FooAttribute":
                case "BarAttribute":
                    return true;
            }

            if (!object.ReferenceEquals(this, Documentation)
                && !Documentation.IsVisibleAttribute(attributeType))
            {
                return true;
            }

            if (_knownVisibleAttributes.Contains(attributeType))
                return true;

            Debug.Fail(attributeType.ToDisplayString());
#endif
            return true;
        }
    }
}
