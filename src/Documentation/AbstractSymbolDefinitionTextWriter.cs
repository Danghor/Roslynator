﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Roslynator.Documentation
{
    internal abstract class AbstractSymbolDefinitionTextWriter : SymbolDefinitionWriter
    {
        protected AbstractSymbolDefinitionTextWriter(
            SymbolFilterOptions filter,
            DefinitionListFormat format = null,
            SymbolDocumentationProvider documentationProvider = null,
            IComparer<ISymbol> comparer = null) : base(filter, format, documentationProvider, comparer)
        {
        }

        protected abstract void WriteIndentation();

        public override void WriteStartDocument()
        {
        }

        public override void WriteEndDocument()
        {
        }

        public override void WriteStartAssemblies()
        {
        }

        public override void WriteEndAssemblies()
        {
        }

        public override void WriteStartAssembly(IAssemblySymbol assemblySymbol)
        {
            WriteIndentation();
        }

        public override void WriteAssembly(IAssemblySymbol assemblySymbol)
        {
            Write("assembly ");
            WriteLine(assemblySymbol.Identity.ToString());
            IncreaseDepth();
        }

        public override void WriteEndAssembly(IAssemblySymbol assemblySymbol)
        {
            DecreaseDepth();
        }

        public override void WriteAssemblySeparator()
        {
            if (Format.IncludeAssemblyAttributes)
                WriteLine();
        }

        public override void WriteEndNamespaces()
        {
        }

        public override void WriteStartNamespace(INamespaceSymbol namespaceSymbol)
        {
            WriteIndentation();
        }

        public override void WriteNamespace(INamespaceSymbol namespaceSymbol, SymbolDisplayFormat format = null)
        {
            if (namespaceSymbol.IsGlobalNamespace)
                return;

            WriteDocumentationComment(namespaceSymbol);
            Write(namespaceSymbol, format ?? GetNamespaceFormat(namespaceSymbol));
            WriteLine();
            IncreaseDepth();
        }

        public override void WriteEndNamespace(INamespaceSymbol namespaceSymbol)
        {
            if (namespaceSymbol.IsGlobalNamespace)
                return;

            DecreaseDepth();
        }

        public override void WriteEndTypes()
        {
        }

        public override void WriteStartType(INamedTypeSymbol typeSymbol)
        {
            WriteIndentation();
        }

        public override void WriteType(INamedTypeSymbol typeSymbol, SymbolDisplayFormat format = null, SymbolDisplayTypeDeclarationOptions? typeDeclarationOptions = null)
        {
            WriteDocumentationComment(typeSymbol);
            Write(typeSymbol, format ?? GetTypeFormat(typeSymbol), typeDeclarationOptions);
            WriteLine();

            IncreaseDepth();
        }

        public override void WriteEndType(INamedTypeSymbol typeSymbol)
        {
            DecreaseDepth();
        }

        public override void WriteEndMembers()
        {
        }

        public override void WriteStartMember(ISymbol symbol)
        {
            WriteIndentation();
        }

        public override void WriteMember(ISymbol symbol, SymbolDisplayFormat format = null)
        {
            WriteDocumentationComment(symbol);
            Write(symbol, format ?? GetMemberFormat(symbol));
            WriteLine();
            IncreaseDepth();
        }

        public override void WriteEndMember(ISymbol symbol)
        {
            DecreaseDepth();
        }

        public override void WriteEndEnumMembers()
        {
        }

        public override void WriteStartEnumMember(ISymbol symbol)
        {
            WriteIndentation();
        }

        public override void WriteEnumMember(ISymbol symbol, SymbolDisplayFormat format = null)
        {
            WriteDocumentationComment(symbol);
            Write(symbol, format ?? GetEnumMemberFormat(symbol));
            WriteLine();
            IncreaseDepth();
        }

        public override void WriteEndEnumMember(ISymbol symbol)
        {
            DecreaseDepth();
        }

        public override void WriteEnumMemberSeparator()
        {
        }

        public override void WriteStartAttributes(bool assemblyAttribute)
        {
            Write("[");
        }

        public override void WriteEndAttributes(bool assemblyAttribute)
        {
            Write("]");

            if (assemblyAttribute || SupportsMultilineDefinitions)
            {
                WriteLine();
            }
            else
            {
                Write(" ");
            }
        }

        public override void WriteStartAttribute(AttributeData attribute, bool assemblyAttribute)
        {
        }

        public override void WriteEndAttribute(AttributeData attribute, bool assemblyAttribute)
        {
        }

        public override void WriteAttributeSeparator(bool assemblyAttribute)
        {
            if (assemblyAttribute
                || (Format.FormatAttributes && SupportsMultilineDefinitions))
            {
                Write("]");
                WriteLine();
                Write("[");
            }
            else
            {
                Write(", ");
            }
        }

        public override void Write(ISymbol symbol, SymbolDisplayFormat format, SymbolDisplayTypeDeclarationOptions? typeDeclarationOptions = null, SymbolDisplayAdditionalOptions? additionalOptions = null)
        {
            if (Format.IncludeAttributes)
                WriteAttributes(symbol);

            base.Write(symbol, format, typeDeclarationOptions, additionalOptions);
        }
    }
}
