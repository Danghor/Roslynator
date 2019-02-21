// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.Documentation
{
    internal abstract class SymbolDefinitionWriter
    {
        private SymbolDisplayFormat _namespaceFormat;
        private SymbolDisplayFormat _typeFormat;
        private SymbolDisplayFormat _memberFormat;
        private SymbolDisplayFormat _enumMemberFormat;

        protected SymbolDefinitionWriter(
            SymbolFilterOptions filter = null,
            DefinitionListFormat format = null,
            SymbolDocumentationProvider documentationProvider = null,
            IComparer<ISymbol> comparer = null)
        {
            Filter = filter ?? SymbolFilterOptions.Default;
            Format = format ?? DefinitionListFormat.Default;
            DocumentationProvider = documentationProvider;
            Comparer = comparer ?? SymbolDefinitionComparer.SystemNamespaceFirstInstance;
        }

        public SymbolFilterOptions Filter { get; }

        public DefinitionListFormat Format { get; }

        public SymbolDocumentationProvider DocumentationProvider { get; }

        public IComparer<ISymbol> Comparer { get; }

        public virtual bool SupportsMultilineDefinitions => Layout != SymbolDefinitionListLayout.TypeHierarchy;

        protected int Depth { get; private set; }

        internal SymbolDefinitionListLayout Layout => Format.Layout;

        public SymbolDisplayFormat NamespaceFormat
        {
            get
            {
                if (_namespaceFormat == null)
                {
                    SymbolDisplayFormat format = SymbolDefinitionDisplayFormats.FullDefinition_NameAndContainingTypesAndNamespaces;

                    format = Format.Update(format);

                    Interlocked.CompareExchange(ref _namespaceFormat, CreateNamespaceFormat(format), null);
                }

                return _namespaceFormat;
            }
        }

        public SymbolDisplayFormat TypeFormat
        {
            get
            {
                if (_typeFormat == null)
                {
                    SymbolDisplayFormat format = (Layout == SymbolDefinitionListLayout.TypeHierarchy)
                        ? SymbolDefinitionDisplayFormats.HierarchyType
                        : SymbolDefinitionDisplayFormats.FullDefinition_NameOnly;

                    format = Format.Update(format);

                    Interlocked.CompareExchange(ref _typeFormat, CreateTypeFormat(format), null);
                }

                return _typeFormat;
            }
        }

        public SymbolDisplayFormat MemberFormat
        {
            get
            {
                if (_memberFormat == null)
                {
                    SymbolDisplayFormat format = (!Format.Includes(SymbolDefinitionPartFilter.ContainingNamespace))
                        ? SymbolDefinitionDisplayFormats.FullDefinition_NameAndContainingTypes
                        : SymbolDefinitionDisplayFormats.FullDefinition_NameAndContainingTypesAndNamespaces;

                    format = Format.Update(format);

                    Interlocked.CompareExchange(ref _memberFormat, CreateMemberFormat(format), null);
                }

                return _memberFormat;
            }
        }

        public SymbolDisplayFormat EnumMemberFormat
        {
            get
            {
                if (_enumMemberFormat == null)
                {
                    SymbolDisplayFormat format = SymbolDefinitionDisplayFormats.FullDefinition_NameOnly;

                    format = Format.Update(format);

                    Interlocked.CompareExchange(ref _enumMemberFormat, CreateEnumMemberFormat(format), null);
                }

                return _enumMemberFormat;
            }
        }

        protected void IncreaseDepth()
        {
            Depth++;
        }

        protected void DecreaseDepth()
        {
            if (Depth == 0)
                throw new InvalidOperationException("Cannot decrease depth.");

            Depth--;
        }

        public abstract void WriteStartDocument();

        public abstract void WriteEndDocument();

        public abstract void WriteStartAssemblies();

        public abstract void WriteEndAssemblies();

        public abstract void WriteStartAssembly(IAssemblySymbol assemblySymbol);

        public abstract void WriteAssembly(IAssemblySymbol assemblySymbol);

        public abstract void WriteEndAssembly(IAssemblySymbol assemblySymbol);

        public abstract void WriteAssemblySeparator();

        public abstract void WriteStartNamespaces();

        public abstract void WriteEndNamespaces();

        public abstract void WriteStartNamespace(INamespaceSymbol namespaceSymbol);

        public abstract void WriteNamespace(INamespaceSymbol namespaceSymbol, SymbolDisplayFormat format = null);

        public abstract void WriteEndNamespace(INamespaceSymbol namespaceSymbol);

        public abstract void WriteNamespaceSeparator();

        public abstract void WriteStartTypes();

        public abstract void WriteEndTypes();

        public abstract void WriteStartType(INamedTypeSymbol typeSymbol);

        public abstract void WriteType(INamedTypeSymbol typeSymbol, SymbolDisplayFormat format = null, SymbolDisplayTypeDeclarationOptions? typeDeclarationOptions = null);

        public abstract void WriteEndType(INamedTypeSymbol typeSymbol);

        public abstract void WriteTypeSeparator();

        public abstract void WriteStartMembers();

        public abstract void WriteEndMembers();

        public abstract void WriteStartMember(ISymbol symbol);

        public abstract void WriteMember(ISymbol symbol, SymbolDisplayFormat format = null);

        public abstract void WriteEndMember(ISymbol symbol);

        public abstract void WriteMemberSeparator();

        public abstract void WriteStartEnumMembers();

        public abstract void WriteEndEnumMembers();

        public abstract void WriteStartEnumMember(ISymbol symbol);

        public abstract void WriteEnumMember(ISymbol symbol, SymbolDisplayFormat format = null);

        public abstract void WriteEndEnumMember(ISymbol symbol);

        public abstract void WriteEnumMemberSeparator();

        public abstract void WriteStartAttributes(bool assemblyAttribute);

        public abstract void WriteEndAttributes(bool assemblyAttribute);

        public abstract void WriteStartAttribute(AttributeData attribute, bool assemblyAttribute);

        public abstract void WriteEndAttribute(AttributeData attribute, bool assemblyAttribute);

        public abstract void WriteAttributeSeparator(bool assemblyAttribute);

        public abstract void WriteDocumentationComment(ISymbol symbol);

        public SymbolDisplayTypeDeclarationOptions GetTypeDeclarationOptions()
        {
            var options = SymbolDisplayTypeDeclarationOptions.None;

            if (Format.Includes(SymbolDefinitionPartFilter.Accessibility))
                options |= SymbolDisplayTypeDeclarationOptions.IncludeAccessibility;

            if (Format.Includes(SymbolDefinitionPartFilter.Modifiers))
                options |= SymbolDisplayTypeDeclarationOptions.IncludeModifiers;

            if (Format.Includes(SymbolDefinitionPartFilter.BaseType))
                options |= SymbolDisplayTypeDeclarationOptions.BaseType;

            if (Format.Includes(SymbolDefinitionPartFilter.BaseInterfaces))
                options |= SymbolDisplayTypeDeclarationOptions.Interfaces;

            return options;
        }

        private SymbolDisplayAdditionalOptions GetAdditionalOptions()
        {
            var options = SymbolDisplayAdditionalOptions.None;

            if (!Format.Includes(SymbolDefinitionPartFilter.ContainingNamespace))
                options |= SymbolDisplayAdditionalOptions.OmitContainingNamespace;

            if (Format.Includes(SymbolDefinitionPartFilter.Attributes))
                options |= SymbolDisplayAdditionalOptions.IncludeParameterAttributes | SymbolDisplayAdditionalOptions.IncludeAccessorAttributes;

            if (Format.Includes(SymbolDefinitionPartFilter.AttributeArguments))
                options |= SymbolDisplayAdditionalOptions.IncludeAttributeArguments;

            if (SupportsMultilineDefinitions)
            {
                if (Format.Includes(SymbolDefinitionFormatOptions.BaseList))
                    options |= SymbolDisplayAdditionalOptions.FormatBaseList;

                if (Format.Includes(SymbolDefinitionFormatOptions.Constraints))
                    options |= SymbolDisplayAdditionalOptions.FormatConstraints;

                if (Format.Includes(SymbolDefinitionFormatOptions.Parameters))
                    options |= SymbolDisplayAdditionalOptions.FormatParameters;

                if (Format.Includes(SymbolDefinitionFormatOptions.Attributes))
                    options |= SymbolDisplayAdditionalOptions.FormatAttributes;
            }

            if (Format.OmitIEnumerable)
                options |= SymbolDisplayAdditionalOptions.OmitIEnumerable;

            if (Format.PreferDefaultLiteral)
                options |= SymbolDisplayAdditionalOptions.PreferDefaultLiteral;

            return options;
        }

        protected virtual SymbolDisplayFormat CreateNamespaceFormat(SymbolDisplayFormat format)
        {
            return format;
        }

        protected virtual SymbolDisplayFormat CreateTypeFormat(SymbolDisplayFormat format)
        {
            return format;
        }

        protected virtual SymbolDisplayFormat CreateMemberFormat(SymbolDisplayFormat format)
        {
            return format;
        }

        protected virtual SymbolDisplayFormat CreateEnumMemberFormat(SymbolDisplayFormat format)
        {
            return format;
        }

        public void WriteDocument(IEnumerable<IAssemblySymbol> assemblies, CancellationToken cancellationToken = default)
        {
            WriteStartDocument();
            WriteAssemblies(assemblies, cancellationToken);

            if (!Format.GroupByAssembly)
            {
                if (Layout == SymbolDefinitionListLayout.TypeHierarchy)
                {
                    WriteTypeHierarchy(assemblies.SelectMany(a => a.GetTypes(Filter.IsVisibleType)), cancellationToken);
                }
                else
                {
                    WriteNamespaces(assemblies.SelectMany(a => a.GetTypes(t => t.ContainingType == null && Filter.IsVisibleType(t))), cancellationToken);
                }
            }

            WriteEndDocument();
        }

        public void WriteAssemblies(IEnumerable<IAssemblySymbol> assemblies, CancellationToken cancellationToken = default)
        {
            WriteStartAssemblies();

            using (IEnumerator<IAssemblySymbol> en = assemblies
                .OrderBy(f => f.Name)
                .ThenBy(f => f.Identity.Version)
                .GetEnumerator())
            {
                if (en.MoveNext())
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        IAssemblySymbol assembly = en.Current;

                        WriteStartAssembly(assembly);
                        WriteAssembly(assembly);

                        if (Format.Includes(SymbolDefinitionPartFilter.AssemblyAttributes))
                            WriteAttributes(assembly);

                        if (Format.GroupByAssembly)
                        {
                            if (Layout == SymbolDefinitionListLayout.TypeHierarchy)
                            {
                                WriteTypeHierarchy(assembly.GetTypes(Filter.IsVisibleType), cancellationToken);
                            }
                            else
                            {
                                WriteNamespaces(assembly.GetTypes(t => t.ContainingType == null && Filter.IsVisibleType(t)), cancellationToken);
                            }
                        }

                        WriteEndAssembly(assembly);

                        if (en.MoveNext())
                        {
                            WriteAssemblySeparator();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            WriteEndAssemblies();
        }

        private void WriteNamespaces(IEnumerable<INamedTypeSymbol> types, CancellationToken cancellationToken = default)
        {
            IEnumerable<IGrouping<INamespaceSymbol, INamedTypeSymbol>> typesByNamespace = types
                .GroupBy(t => t.ContainingNamespace, MetadataNameEqualityComparer<INamespaceSymbol>.Instance)
                .Where(g => Filter.IsVisibleNamespace(g.Key))
                .OrderBy(g => g.Key, Comparer);

            WriteStartNamespaces();

            if (Layout == SymbolDefinitionListLayout.NamespaceHierarchy)
            {
                WriteNamespaceHierarchy(typesByNamespace.ToDictionary(f => f.Key, f => f.AsEnumerable()), cancellationToken);
            }
            else
            {
                Debug.Assert(Layout == SymbolDefinitionListLayout.NamespaceList, Layout.ToString());

                WriteNamespaces(typesByNamespace, cancellationToken);
            }

            WriteEndNamespaces();
        }

        private void WriteTypeHierarchy(IEnumerable<INamedTypeSymbol> types, CancellationToken cancellationToken = default)
        {
            TypeHierarchy hierarchy = TypeHierarchy.Create(types, Comparer);

            WriteTypeHierarchy(hierarchy, cancellationToken);
        }

        private void WriteTypeHierarchy(TypeHierarchy hierarchy, CancellationToken cancellationToken = default)
        {
            WriteStartTypes();
            WriteTypeHierarchy(hierarchy.Root);

            if (hierarchy.InterfaceRoot.HasChildren)
                WriteTypeHierarchy(hierarchy.InterfaceRoot);

            WriteEndTypes();

            void WriteTypeHierarchy(TypeHierarchyItem item)
            {
                cancellationToken.ThrowIfCancellationRequested();

                INamedTypeSymbol symbol = item.Symbol;

                WriteStartType(symbol);

                WriteType(symbol);

                if (symbol != null
                    && !item.IsExternal)
                {
                    WriteMembers(symbol);
                }

                foreach (TypeHierarchyItem derivedItem in item.Children())
                {
                    WriteTypeSeparator();
                    WriteTypeHierarchy(derivedItem);
                }

                WriteEndType(symbol);
            }
        }

        private void WriteNamespaces(IEnumerable<IGrouping<INamespaceSymbol, INamedTypeSymbol>> typesByNamespace, CancellationToken cancellationToken = default)
        {
            using (IEnumerator<IGrouping<INamespaceSymbol, INamedTypeSymbol>> en = typesByNamespace.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        INamespaceSymbol namespaceSymbol = en.Current.Key;

                        WriteStartNamespace(namespaceSymbol);
                        WriteNamespace(namespaceSymbol);

                        if ((Filter.SymbolGroups & SymbolGroupFilter.Type) != 0)
                            WriteTypes(en.Current, cancellationToken);

                        WriteEndNamespace(namespaceSymbol);

                        if (en.MoveNext())
                        {
                            WriteNamespaceSeparator();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void WriteNamespaceHierarchy(Dictionary<INamespaceSymbol, IEnumerable<INamedTypeSymbol>> typesByNamespace, CancellationToken cancellationToken = default)
        {
            var rootNamespaces = new HashSet<INamespaceSymbol>(MetadataNameEqualityComparer<INamespaceSymbol>.Instance);

            var nestedNamespaces = new HashSet<INamespaceSymbol>(MetadataNameEqualityComparer<INamespaceSymbol>.Instance);

            foreach (INamespaceSymbol namespaceSymbol in typesByNamespace.Select(f => f.Key))
            {
                if (namespaceSymbol.IsGlobalNamespace)
                {
                    rootNamespaces.Add(namespaceSymbol);
                }
                else
                {
                    INamespaceSymbol n = namespaceSymbol;

                    while (true)
                    {
                        INamespaceSymbol containingNamespace = n.ContainingNamespace;

                        if (containingNamespace.IsGlobalNamespace)
                        {
                            rootNamespaces.Add(n);
                            break;
                        }

                        nestedNamespaces.Add(n);

                        n = containingNamespace;
                    }
                }
            }

            using (IEnumerator<INamespaceSymbol> en = rootNamespaces
                .OrderBy(f => f, Comparer)
                .GetEnumerator())
            {
                if (en.MoveNext())
                {
                    while (true)
                    {
                        WriteNamespaceWithHierarchy(en.Current);

                        if (en.MoveNext())
                        {
                            WriteNamespaceSeparator();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            void WriteNamespaceWithHierarchy(INamespaceSymbol namespaceSymbol)
            {
                cancellationToken.ThrowIfCancellationRequested();

                WriteNamespace(namespaceSymbol);

                if ((Filter.SymbolGroups & SymbolGroupFilter.Type) != 0)
                    WriteTypes(typesByNamespace[namespaceSymbol], cancellationToken);

                using (List<INamespaceSymbol>.Enumerator en = nestedNamespaces
                    .Where(f => MetadataNameEqualityComparer<INamespaceSymbol>.Instance.Equals(f.ContainingNamespace, namespaceSymbol))
                    .Distinct(MetadataNameEqualityComparer<INamespaceSymbol>.Instance)
                    .OrderBy(f => f, Comparer)
                    .ToList()
                    .GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        while (true)
                        {
                            nestedNamespaces.Remove(en.Current);

                            WriteNamespaceWithHierarchy(en.Current);

                            if (en.MoveNext())
                            {
                                WriteNamespaceSeparator();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                WriteEndNamespace(namespaceSymbol);
            }
        }

        private void WriteTypes(IEnumerable<INamedTypeSymbol> types, CancellationToken cancellationToken = default)
        {
            using (IEnumerator<INamedTypeSymbol> en = types
                .OrderBy(f => f, Comparer)
                .GetEnumerator())
            {
                if (en.MoveNext())
                {
                    WriteStartTypes();

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        INamedTypeSymbol type = en.Current;

                        WriteStartType(type);
                        WriteType(type);

                        WriteMembers(type);

                        WriteEndType(type);

                        if (en.MoveNext())
                        {
                            WriteTypeSeparator();
                        }
                        else
                        {
                            break;
                        }
                    }

                    WriteEndTypes();
                }
            }
        }

        private void WriteMembers(INamedTypeSymbol type)
        {
            switch (type.TypeKind)
            {
                case TypeKind.Class:
                case TypeKind.Interface:
                case TypeKind.Struct:
                    {
                        if ((Filter.SymbolGroups & SymbolGroupFilter.Member) != 0)
                            WriteMembers();

                        if (Layout != SymbolDefinitionListLayout.TypeHierarchy
                            && (Filter.SymbolGroups & SymbolGroupFilter.Type) != 0)
                        {
                            WriteTypes(type.GetTypeMembers().Where(f => Filter.IsVisibleType(f)));
                        }

                        break;
                    }
                case TypeKind.Enum:
                    {
                        if ((Filter.SymbolGroups & SymbolGroupFilter.Member) != 0)
                            WriteEnumMembers();

                        break;
                    }
                default:
                    {
                        Debug.Assert(type.TypeKind == TypeKind.Delegate, type.TypeKind.ToString());
                        break;
                    }
            }

            void WriteMembers()
            {
                using (IEnumerator<ISymbol> en = type
                    .GetMembers()
                    .Where(f => Filter.IsVisibleMember(f))
                    .OrderBy(f => f, Comparer)
                    .GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        WriteStartMembers();

                        ISymbol symbol = en.Current;

                        while (true)
                        {
                            WriteStartMember(symbol);
                            WriteMember(symbol);
                            WriteEndMember(symbol);

                            if (en.MoveNext())
                            {
                                ISymbol next = en.Current;

                                if (Format.EmptyLineBetweenMembers
                                    || (Format.EmptyLineBetweenMemberGroups && symbol.GetMemberDeclarationKind() != next.GetMemberDeclarationKind()))
                                {
                                    WriteMemberSeparator();
                                }

                                symbol = next;
                            }
                            else
                            {
                                break;
                            }
                        }

                        WriteEndMembers();
                    }
                }
            }

            void WriteEnumMembers()
            {
                using (IEnumerator<ISymbol> en = type
                    .GetMembers()
                    .Where(m => m.Kind == SymbolKind.Field
                        && m.DeclaredAccessibility == Accessibility.Public)
                    .GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        WriteStartEnumMembers();

                        while (true)
                        {
                            WriteStartEnumMember(en.Current);
                            WriteEnumMember(en.Current);
                            WriteEndEnumMember(en.Current);

                            if (en.MoveNext())
                            {
                                WriteEnumMemberSeparator();
                            }
                            else
                            {
                                break;
                            }
                        }

                        WriteEndEnumMembers();
                    }
                }
            }
        }

        public void WriteAttributes(ISymbol symbol)
        {
            using (IEnumerator<AttributeData> en = symbol
                .GetAttributes()
                .Where(f => Filter.IsVisibleAttribute(f.AttributeClass))
                .OrderBy(f => f.AttributeClass, Comparer).GetEnumerator())
            {
                if (en.MoveNext())
                {
                    bool assemblyAttribute = symbol.Kind == SymbolKind.Assembly;

                    WriteStartAttributes(assemblyAttribute);

                    while (true)
                    {
                        WriteStartAttribute(en.Current, assemblyAttribute);
                        WriteAttribute(en.Current);
                        WriteEndAttribute(en.Current, assemblyAttribute);

                        if (en.MoveNext())
                        {
                            WriteAttributeSeparator(assemblyAttribute);
                        }
                        else
                        {
                            break;
                        }
                    }

                    WriteEndAttributes(assemblyAttribute);
                }
            }
        }

        public void WriteAttribute(AttributeData attribute)
        {
            SymbolDisplayFormat format = (!Format.Includes(SymbolDefinitionPartFilter.ContainingNamespace))
                ? SymbolDefinitionDisplayFormats.TypeNameAndContainingTypesAndTypeParameters
                : SymbolDefinitionDisplayFormats.TypeNameAndContainingTypesAndNamespacesAndTypeParameters;

            ImmutableArray<SymbolDisplayPart> parts = attribute.AttributeClass.ToDisplayParts(format);

            SymbolDisplayPart part = parts.FirstOrDefault(f => f.Kind == SymbolDisplayPartKind.ClassName);

            Debug.Assert(part.Kind == SymbolDisplayPartKind.ClassName, part.Kind.ToString());

            if (part.Kind == SymbolDisplayPartKind.ClassName)
            {
                const string attributeSuffix = "Attribute";

                string text = part.ToString();

                if (text.EndsWith(attributeSuffix, StringComparison.Ordinal))
                {
                    parts = parts.Replace(part, part.WithText(text.Remove(text.Length - attributeSuffix.Length)));
                }
            }

            Write(parts);

            if (!Format.Includes(SymbolDefinitionPartFilter.AttributeArguments))
                return;

            bool hasConstructorArgument = false;
            bool hasNamedArgument = false;

            AppendConstructorArguments();
            AppendNamedArguments();

            if (hasConstructorArgument || hasNamedArgument)
            {
                Write(")");
            }

            void AppendConstructorArguments()
            {
                ImmutableArray<TypedConstant>.Enumerator en = attribute.ConstructorArguments.GetEnumerator();

                if (en.MoveNext())
                {
                    hasConstructorArgument = true;
                    Write("(");

                    while (true)
                    {
                        AddConstantValue(en.Current);

                        if (en.MoveNext())
                        {
                            Write(", ");
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            void AppendNamedArguments()
            {
                ImmutableArray<KeyValuePair<string, TypedConstant>>.Enumerator en = attribute.NamedArguments.GetEnumerator();

                if (en.MoveNext())
                {
                    hasNamedArgument = true;

                    if (hasConstructorArgument)
                    {
                        Write(", ");
                    }
                    else
                    {
                        Write("(");
                    }

                    while (true)
                    {
                        Write(en.Current.Key);
                        Write(" = ");
                        AddConstantValue(en.Current.Value);

                        if (en.MoveNext())
                        {
                            Write(", ");
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            void AddConstantValue(TypedConstant typedConstant)
            {
                switch (typedConstant.Kind)
                {
                    case TypedConstantKind.Primitive:
                        {
                            Write(SymbolDisplay.FormatPrimitive(typedConstant.Value, quoteStrings: true, useHexadecimalNumbers: false));
                            break;
                        }
                    case TypedConstantKind.Enum:
                        {
                            OneOrMany<EnumFieldSymbolInfo> oneOrMany = EnumUtility.GetConstituentFields(typedConstant.Value, (INamedTypeSymbol)typedConstant.Type);

                            OneOrMany<EnumFieldSymbolInfo>.Enumerator en = oneOrMany.GetEnumerator();

                            if (en.MoveNext())
                            {
                                while (true)
                                {
                                    WriteSymbol(en.Current.Symbol);

                                    if (en.MoveNext())
                                    {
                                        Write(" | ");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                Write("(");
                                WriteSymbol((INamedTypeSymbol)typedConstant.Type);
                                Write(")");
                                Write(typedConstant.Value.ToString());
                            }

                            break;
                        }
                    case TypedConstantKind.Type:
                        {
                            Write("typeof");
                            Write("(");
                            WriteSymbol((ISymbol)typedConstant.Value);
                            Write(")");

                            break;
                        }
                    case TypedConstantKind.Array:
                        {
                            var arrayType = (IArrayTypeSymbol)typedConstant.Type;

                            Write("new ");
                            WriteSymbol(arrayType.ElementType);

                            Write("[] { ");

                            ImmutableArray<TypedConstant>.Enumerator en = typedConstant.Values.GetEnumerator();

                            if (en.MoveNext())
                            {
                                while (true)
                                {
                                    AddConstantValue(en.Current);

                                    if (en.MoveNext())
                                    {
                                        Write(", ");
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            Write(" }");
                            break;
                        }
                    default:
                        {
                            throw new InvalidOperationException();
                        }
                }
            }

            void WriteSymbol(ISymbol symbol)
            {
                SymbolDisplayFormat format2 = (!Format.Includes(SymbolDefinitionPartFilter.ContainingNamespace))
                    ? SymbolDefinitionDisplayFormats.TypeNameAndContainingTypesAndTypeParameters
                    : SymbolDefinitionDisplayFormats.TypeNameAndContainingTypesAndNamespacesAndTypeParameters;

                Write(symbol.ToDisplayParts(format2));
            }
        }

        public virtual void Write(ISymbol symbol, SymbolDisplayFormat format, SymbolDisplayTypeDeclarationOptions? typeDeclarationOptions = null, SymbolDisplayAdditionalOptions? additionalOptions = null)
        {
            ImmutableArray<SymbolDisplayPart> parts = SymbolDefinitionDisplay.GetDisplayParts(
                symbol,
                format,
                typeDeclarationOptions: typeDeclarationOptions ?? GetTypeDeclarationOptions(),
                additionalOptions: additionalOptions ?? GetAdditionalOptions(),
                shouldDisplayAttribute: Filter.IsVisibleAttribute);

            Write(parts);
        }

        public virtual void Write(IEnumerable<SymbolDisplayPart> parts)
        {
            foreach (SymbolDisplayPart part in parts)
                Write(part);
        }

        public virtual void Write(SymbolDisplayPart part)
        {
            Write(part.ToString());
        }

        public abstract void Write(string value);

        public abstract void WriteLine();

        public virtual void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }
    }
}
