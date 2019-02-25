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
            SymbolDocumentationProvider documentationProvider = null) : base(filter, format, documentationProvider)
        {
            _writer = writer;
        }

        public override bool SupportsMultilineDefinitions => false;

        protected override SymbolDisplayFormat CreateNamespaceFormat(SymbolDisplayFormat format)
        {
            return format.Update(kindOptions: SymbolDisplayKindOptions.None);
        }

        protected override SymbolDisplayAdditionalOptions GetAdditionalOptions()
        {
            return base.GetAdditionalOptions() & ~(SymbolDisplayAdditionalOptions.IncludeAccessorAttributes | SymbolDisplayAdditionalOptions.IncludeParameterAttributes);
        }

        public override void WriteStartDocument()
        {
            _writer.WriteStartDocument();
            WriteStartElement("root");
        }

        public override void WriteEndDocument()
        {
            WriteEndElement();
            _writer.WriteEndDocument();
        }

        public override void WriteStartAssemblies()
        {
            WriteStartElement("assemblies");
        }

        public override void WriteEndAssemblies()
        {
            WriteEndElement();
        }

        public override void WriteStartAssembly(IAssemblySymbol assemblySymbol)
        {
            WriteStartElement("assembly");
        }

        public override void WriteAssembly(IAssemblySymbol assemblySymbol)
        {
            WriteStartAttribute("name");
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
            WriteStartElement("namespaces");
        }

        public override void WriteEndNamespaces()
        {
            WriteEndElement();
        }

        public override void WriteStartNamespace(INamespaceSymbol namespaceSymbol)
        {
            WriteStartElement("namespace");
        }

        public override void WriteNamespace(INamespaceSymbol namespaceSymbol, SymbolDisplayFormat format = null)
        {
            WriteStartAttribute("name");

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
            WriteStartElement("types");
        }

        public override void WriteEndTypes()
        {
            WriteEndElement();
        }

        public override void WriteStartType(INamedTypeSymbol typeSymbol)
        {
            WriteStartElement("type");
        }

        public override void WriteType(INamedTypeSymbol typeSymbol, SymbolDisplayFormat format = null, SymbolDisplayTypeDeclarationOptions? typeDeclarationOptions = null)
        {
            if (typeSymbol != null)
            {
                WriteStartAttribute("def");
                Write(typeSymbol, format ?? TypeFormat, typeDeclarationOptions);
                WriteEndAttribute();
                WriteDocumentationComment(typeSymbol);

                if (Format.Includes(SymbolDefinitionPartFilter.Attributes))
                    WriteAttributes(typeSymbol);
            }
            else
            {
                WriteAttributeString("def", "");
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
            WriteStartElement("members");
        }

        public override void WriteEndMembers()
        {
            WriteEndElement();
        }

        public override void WriteStartMember(ISymbol symbol)
        {
            WriteStartElement("member");
        }

        public override void WriteMember(ISymbol symbol, SymbolDisplayFormat format = null)
        {
            if (format == null)
            {
                format = (symbol.GetFirstExplicitInterfaceImplementation() != null)
                    ? ExplicitInterfaceImplementationFormat
                    : MemberFormat;
            }

            WriteStartAttribute("def");
            Write(symbol, format);
            WriteEndAttribute();
            WriteDocumentationComment(symbol);

            if (Format.Includes(SymbolDefinitionPartFilter.Attributes))
            {
                WriteAttributes(symbol);

                switch (symbol.Kind)
                {
                    case SymbolKind.NamedType:
                        {
                            var typeSymbol = (INamedTypeSymbol)symbol;

                            if (typeSymbol.TypeKind == TypeKind.Delegate)
                            {
                                foreach (IParameterSymbol parameterSymbol in typeSymbol.DelegateInvokeMethod.Parameters)
                                    WriteAttributes(parameterSymbol);
                            }

                            break;
                        }
                    case SymbolKind.Event:
                        {
                            var eventSymbol = (IEventSymbol)symbol;

                            if (eventSymbol.AddMethod != null)
                                WriteAttributes(eventSymbol.AddMethod);

                            if (eventSymbol.RemoveMethod != null)
                                WriteAttributes(eventSymbol.RemoveMethod);

                            break;
                        }
                    case SymbolKind.Method:
                        {
                            var methodSymbol = (IMethodSymbol)symbol;

                            foreach (IParameterSymbol parameterSymbol in methodSymbol.Parameters)
                                WriteAttributes(parameterSymbol);

                            break;
                        }
                    case SymbolKind.Property:
                        {
                            var propertySymbol = (IPropertySymbol)symbol;

                            foreach (IParameterSymbol parameterSymbol in propertySymbol.Parameters)
                                WriteAttributes(parameterSymbol);

                            if (propertySymbol.GetMethod != null)
                                WriteAttributes(propertySymbol.GetMethod);

                            if (propertySymbol.SetMethod != null)
                                WriteAttributes(propertySymbol.SetMethod);

                            break;
                        }
                }
            }
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
            WriteStartElement("members");
        }

        public override void WriteEndEnumMembers()
        {
            WriteEndElement();
        }

        public override void WriteStartEnumMember(ISymbol symbol)
        {
            WriteStartElement("member");
        }

        public override void WriteEnumMember(ISymbol symbol, SymbolDisplayFormat format = null)
        {
            WriteStartAttribute("def");
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

        public override void WriteStartAttributes(ISymbol symbol)
        {
            WriteStartElement("attributes");

            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    {
                        var methodSymbol = (IMethodSymbol)symbol;

                        string accessorName = GetAccessorName(methodSymbol);

                        if (accessorName != null)
                            WriteAttributeString("accessor", accessorName);

                        break;
                    }
                case SymbolKind.Parameter:
                    {
                        var parameterSymbol = (IParameterSymbol)symbol;

                        WriteAttributeString("parameter", parameterSymbol.Name);
                        break;
                    }
            }

            string GetAccessorName(IMethodSymbol methodSymbol)
            {
                switch (methodSymbol.MethodKind)
                {
                    case MethodKind.EventAdd:
                        return "add";
                    case MethodKind.EventRemove:
                        return "remove";
                    case MethodKind.PropertyGet:
                        return "get";
                    case MethodKind.PropertySet:
                        return "set";
                    default:
                        return null;
                }
            }
        }

        public override void WriteEndAttributes(ISymbol symbol)
        {
            WriteEndElement();
        }

        public override void WriteStartAttribute(AttributeData attribute, ISymbol symbol)
        {
            WriteStartElement("attribute");
        }

        public override void WriteEndAttribute(AttributeData attribute, ISymbol symbol)
        {
            WriteEndElement();
        }

        public override void WriteAttributeSeparator(ISymbol symbol)
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

        private void WriteAttributeString(string name, string value)
        {
            _writer.WriteAttributeString(name, value);
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
                    WriteStartElement("doc");

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
