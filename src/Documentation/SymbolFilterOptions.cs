// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator.Documentation
{
    internal class SymbolFilterOptions
    {
        private readonly MetadataNameSet _ignoredNames;
        private readonly MetadataNameSet _ignoredAttributeNames;

        public static SymbolFilterOptions Default { get; } = new SymbolFilterOptions();

        public static SymbolFilterOptions Documentation { get; } = new SymbolFilterOptions(
            visibilityFilter: VisibilityFilter.Public,
            symbolGroupFilter: SymbolGroupFilter.NamespaceOrTypeOrMember,
            ignoredNames: null,
            ignoredAttributeNames: GetDocumentationIgnoredAttributeNames().Select(f => MetadataName.Parse(f)));

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
            VisibilityFilter visibilityFilter = VisibilityFilter.All,
            SymbolGroupFilter symbolGroupFilter = SymbolGroupFilter.NamespaceOrTypeOrMember,
            IEnumerable<MetadataName> ignoredNames = null,
            IEnumerable<MetadataName> ignoredAttributeNames = null)
        {
            VisibilityFilter = visibilityFilter;
            SymbolGroupFilter = symbolGroupFilter;
            _ignoredNames = (ignoredNames != null) ? new MetadataNameSet(ignoredNames) : null;
            _ignoredAttributeNames = (ignoredAttributeNames != null) ? new MetadataNameSet(ignoredAttributeNames) : null;
        }

        public VisibilityFilter VisibilityFilter { get; }

        public SymbolGroupFilter SymbolGroupFilter { get; }

        public ImmutableArray<MetadataName> IgnoredNames
        {
            get { return _ignoredNames?.Values ?? ImmutableArray<MetadataName>.Empty; }
        }

        public ImmutableArray<MetadataName> IgnoredAttributeNames
        {
            get { return _ignoredAttributeNames?.Values ?? ImmutableArray<MetadataName>.Empty; }
        }

        public bool IncludesSymbolGroup(SymbolGroupFilter symbolGroupFilter)
        {
            return (SymbolGroupFilter & symbolGroupFilter) == symbolGroupFilter;
        }

        public bool IsVisible(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Namespace:
                    return IsVisibleNamespace((INamespaceSymbol)symbol);
                case SymbolKind.NamedType:
                    return IsVisibleType((INamedTypeSymbol)symbol);
                case SymbolKind.Event:
                case SymbolKind.Field:
                case SymbolKind.Method:
                case SymbolKind.Property:
                    return IsVisibleMember(symbol);
            }

            Debug.Fail(symbol.Kind.ToString());
            return true;
        }

        public virtual bool IsVisibleNamespace(INamespaceSymbol namespaceSymbol)
        {
            return IncludesSymbolGroup(SymbolGroupFilter.Namespace)
                && _ignoredNames?.Contains(namespaceSymbol) != true;
        }

        public virtual bool IsVisibleType(INamedTypeSymbol typeSymbol)
        {
            return !typeSymbol.IsImplicitlyDeclared
                && IncludesSymbolGroup(typeSymbol.TypeKind.ToSymbolGroupFilter())
                && typeSymbol.IsVisible(VisibilityFilter)
                && _ignoredNames?.Contains(typeSymbol) != true;
        }

        public virtual bool IsVisibleMember(ISymbol symbol)
        {
            bool canBeImplicitlyDeclared = false;

            switch (symbol.Kind)
            {
                case SymbolKind.Event:
                    {
                        if (!IncludesSymbolGroup(SymbolGroupFilter.Event))
                            return false;

                        break;
                    }
                case SymbolKind.Field:
                    {
                        var fieldSymbol = (IFieldSymbol)symbol;

                        if (fieldSymbol.IsConst)
                        {
                            if (!IncludesSymbolGroup(SymbolGroupFilter.Const))
                            {
                                return false;
                            }
                            else if (!IncludesSymbolGroup(SymbolGroupFilter.Field))
                            {
                                return false;
                            }
                        }

                        break;
                    }
                case SymbolKind.Property:
                    {
                        var propertySymbol = (IPropertySymbol)symbol;

                        if (propertySymbol.IsIndexer)
                        {
                            if (!IncludesSymbolGroup(SymbolGroupFilter.Indexer))
                            {
                                return false;
                            }
                            else if (!IncludesSymbolGroup(SymbolGroupFilter.Property))
                            {
                                return false;
                            }
                        }

                        break;
                    }
                case SymbolKind.Method:
                    {
                        if (!IncludesSymbolGroup(SymbolGroupFilter.Method))
                            return false;

                        var methodSymbol = (IMethodSymbol)symbol;

                        switch (methodSymbol.MethodKind)
                        {
                            case MethodKind.Constructor:
                                {
                                    TypeKind typeKind = methodSymbol.ContainingType.TypeKind;

                                    Debug.Assert(typeKind.Is(TypeKind.Class, TypeKind.Struct, TypeKind.Enum), methodSymbol.ToDisplayString(Roslynator.SymbolDisplayFormats.Test));

                                    if (typeKind == TypeKind.Class)
                                    {
                                        if (!methodSymbol.Parameters.Any())
                                            canBeImplicitlyDeclared = true;
                                    }
                                    else if (typeKind == TypeKind.Struct)
                                    {
                                        if (!methodSymbol.Parameters.Any())
                                            return false;
                                    }
                                    else if (typeKind == TypeKind.Enum)
                                    {
                                        return false;
                                    }

                                    break;
                                }
                            case MethodKind.Conversion:
                            case MethodKind.UserDefinedOperator:
                            case MethodKind.Ordinary:
                                break;
                            case MethodKind.ExplicitInterfaceImplementation:
                            case MethodKind.StaticConstructor:
                            case MethodKind.Destructor:
                            case MethodKind.EventAdd:
                            case MethodKind.EventRaise:
                            case MethodKind.EventRemove:
                            case MethodKind.PropertyGet:
                            case MethodKind.PropertySet:
                                return false;
                            default:
                                {
                                    Debug.Fail(methodSymbol.MethodKind.ToString());
                                    return false;
                                }
                        }

                        break;
                    }
                default:
                    {
                        Debug.Assert(symbol.Kind == SymbolKind.NamedType, symbol.Kind.ToString());
                        return false;
                    }
            }

            return (canBeImplicitlyDeclared || !symbol.IsImplicitlyDeclared)
                && symbol.IsVisible(VisibilityFilter);
        }

        public virtual bool IsVisibleAttribute(INamedTypeSymbol attributeType)
        {
            if (_ignoredAttributeNames?.Contains(attributeType) == true)
                return false;
#if DEBUG
            switch (attributeType.MetadataName)
            {
                case "FooAttribute":
                case "BarAttribute":
                    return true;
            }

            if (!object.ReferenceEquals(this, Documentation)
                && !Documentation.IsVisible(attributeType))
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
