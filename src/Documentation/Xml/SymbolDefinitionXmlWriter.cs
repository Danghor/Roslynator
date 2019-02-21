// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.CodeAnalysis;

namespace Roslynator.Documentation.Xml
{
    internal class SymbolDefinitionXmlWriter : SymbolDefinitionWriter
    {
        private readonly XmlWriter _writer;

        public SymbolDefinitionXmlWriter(
            XmlWriter writer,
            SymbolFilterOptions filter = null,
            DefinitionListFormat format = null,
            SymbolDocumentationProvider documentationProvider = null,
            IComparer<ISymbol> comparer = null) : base(filter, format, documentationProvider, comparer)
        {
            _writer = writer;
        }

        public override bool SupportsMultilineDefinitions => false;

        protected override SymbolDisplayFormat CreateNamespaceFormat(SymbolDisplayFormat format)
        {
            return format.Update(kindOptions: SymbolDisplayKindOptions.None);
        }

        public override void WriteStartDocument()
        {
            _writer.WriteStartDocument();
            WriteStartElement("Root");
            _writer.WriteAttributeString("Layout", Layout.ToString());

            if (Format.GroupByAssembly)
                _writer.WriteAttributeString("IsGroupedByAssembly", Format.GroupByAssembly.ToString());
        }

        public override void WriteEndDocument()
        {
            WriteEndElement();
            _writer.WriteEndDocument();
        }

        public override void WriteStartAssemblies()
        {
            WriteStartElement("Assemblies");
        }

        public override void WriteEndAssemblies()
        {
            WriteEndElement();
        }

        public override void WriteStartAssembly(IAssemblySymbol assemblySymbol)
        {
            WriteStartElement("Assembly");
        }

        public override void WriteAssembly(IAssemblySymbol assemblySymbol)
        {
            WriteStartAttribute("Name");
            Write(assemblySymbol.Identity.ToString());
            WriteEndAttribute();
        }

        public override void WriteEndAssembly(IAssemblySymbol assemblySymbol)
        {
            WriteEndElement();
        }

        public override void WriteAssemblySeparator()
        {
        }

        public override void WriteStartNamespaces()
        {
            WriteStartElement("Namespaces");
        }

        public override void WriteEndNamespaces()
        {
            WriteEndElement();
        }

        public override void WriteStartNamespace(INamespaceSymbol namespaceSymbol)
        {
            WriteStartElement("Namespace");
        }

        public override void WriteNamespace(INamespaceSymbol namespaceSymbol, SymbolDisplayFormat format = null)
        {
            WriteStartAttribute("Name");

            if (!namespaceSymbol.IsGlobalNamespace)
                Write(namespaceSymbol, format ?? NamespaceFormat);

            WriteEndAttribute();
            WriteDocumentationComment(namespaceSymbol);
        }

        public override void WriteEndNamespace(INamespaceSymbol namespaceSymbol)
        {
            WriteEndElement();
        }

        public override void WriteNamespaceSeparator()
        {
        }

        public override void WriteStartTypes()
        {
            WriteStartElement("Types");
        }

        public override void WriteEndTypes()
        {
            WriteEndElement();
        }

        public override void WriteStartType(INamedTypeSymbol typeSymbol)
        {
            WriteStartElement("Type");
        }

        public override void WriteType(INamedTypeSymbol typeSymbol, SymbolDisplayFormat format = null, SymbolDisplayTypeDeclarationOptions? typeDeclarationOptions = null)
        {
            if (typeSymbol != null)
            {
                WriteStartAttribute("Def");
                Write(typeSymbol, format ?? TypeFormat, typeDeclarationOptions);
                WriteEndAttribute();
                WriteDocumentationComment(typeSymbol);

                if (Format.Includes(SymbolDefinitionPartFilter.Attributes))
                    WriteAttributes(typeSymbol);
            }
            else
            {
                _writer.WriteAttributeString("Def", "");
            }
        }

        public override void WriteEndType(INamedTypeSymbol typeSymbol)
        {
            WriteEndElement();
        }

        public override void WriteTypeSeparator()
        {
        }

        public override void WriteStartMembers()
        {
            WriteStartElement("Members");
        }

        public override void WriteEndMembers()
        {
            WriteEndElement();
        }

        public override void WriteStartMember(ISymbol symbol)
        {
            WriteStartElement("Member");
        }

        public override void WriteMember(ISymbol symbol, SymbolDisplayFormat format = null)
        {
            WriteStartAttribute("Def");
            Write(symbol, format ?? MemberFormat);
            WriteEndAttribute();
            WriteDocumentationComment(symbol);

            if (Format.Includes(SymbolDefinitionPartFilter.Attributes))
                WriteAttributes(symbol);
        }

        public override void WriteEndMember(ISymbol symbol)
        {
            WriteEndElement();
        }

        public override void WriteMemberSeparator()
        {
        }

        public override void WriteStartEnumMembers()
        {
            WriteStartElement("Members");
        }

        public override void WriteEndEnumMembers()
        {
            WriteEndElement();
        }

        public override void WriteStartEnumMember(ISymbol symbol)
        {
            WriteStartElement("Member");
        }

        public override void WriteEnumMember(ISymbol symbol, SymbolDisplayFormat format = null)
        {
            WriteStartAttribute("Def");
            Write(symbol, format ?? EnumMemberFormat);
            WriteEndAttribute();
            WriteDocumentationComment(symbol);

            if (Format.Includes(SymbolDefinitionPartFilter.Attributes))
                WriteAttributes(symbol);
        }

        public override void WriteEndEnumMember(ISymbol symbol)
        {
            WriteEndElement();
        }

        public override void WriteEnumMemberSeparator()
        {
        }

        public override void WriteStartAttributes(bool assemblyAttribute)
        {
            WriteStartElement("Attributes");
        }

        public override void WriteEndAttributes(bool assemblyAttribute)
        {
            WriteEndElement();
        }

        public override void WriteStartAttribute(AttributeData attribute, bool assemblyAttribute)
        {
            WriteStartElement("Attribute");
        }

        public override void WriteEndAttribute(AttributeData attribute, bool assemblyAttribute)
        {
            WriteEndElement();
        }

        public override void WriteAttributeSeparator(bool assemblyAttribute)
        {
        }

        public override void Write(string value)
        {
            Debug.Assert(value?.Contains("\n") != true, @"\n");
            Debug.Assert(value?.Contains("\r") != true, @"\r");

            _writer.WriteString(value);
        }

        public override void WriteLine()
        {
            throw new InvalidOperationException();
        }

        public override void WriteLine(string value)
        {
            throw new InvalidOperationException();
        }

        private void WriteStartElement(string localName)
        {
            _writer.WriteStartElement(localName);
            IncreaseDepth();
        }

        private void WriteStartAttribute(string localName)
        {
            _writer.WriteStartAttribute(localName);
        }

        private void WriteEndElement()
        {
            _writer.WriteEndElement();
            DecreaseDepth();
        }

        private void WriteEndAttribute()
        {
            _writer.WriteEndAttribute();
        }

        public override void WriteDocumentationComment(ISymbol symbol)
        {
            IEnumerable<string> elements = DocumentationProvider?.GetXmlDocumentation(symbol)?.GetElementsAsText(skipEmptyElement: true, makeSingleLine: true);

            if (elements == null)
                return;

            using (IEnumerator<string> en = elements.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    WriteStartElement("Doc");

                    do
                    {
                        WriteDocumentation(en.Current);
                    }
                    while (en.MoveNext());

                    _writer.WriteWhitespace(_writer.Settings.NewLineChars);

                    for (int i = 1; i < Depth; i++)
                        _writer.WriteWhitespace(_writer.Settings.IndentChars);

                    WriteEndElement();
                }
            }

            void WriteDocumentation(string element)
            {
                using (var sr = new StringReader(element))
                {
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        _writer.WriteWhitespace(_writer.Settings.NewLineChars);

                        for (int i = 0; i < Depth; i++)
                            _writer.WriteWhitespace(_writer.Settings.IndentChars);

                        _writer.WriteRaw(line);
                    }
                }
            }
        }
    }
}
