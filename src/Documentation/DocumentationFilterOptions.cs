// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator.Documentation
{
    internal class DocumentationFilterOptions : SymbolFilterOptions
    {
        public static ImmutableArray<MetadataName> IgnoredAttributes { get; } = GetIgnoredAttributes().Select(MetadataName.Parse).ToImmutableArray();

        public static DocumentationFilterOptions Instance { get; } = new DocumentationFilterOptions(
            visibility: VisibilityFilter.Public,
            symbolGroups: SymbolGroupFilter.TypeOrMember,
            rules: null,
            attributeRules: ImmutableArray.Create<AttributeFilterRule>(new IgnoredAttributeNameFilterRule(IgnoredAttributes)));

        private static string[] GetIgnoredAttributes()
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
        internal DocumentationFilterOptions(
            VisibilityFilter visibility = VisibilityFilter.All,
            SymbolGroupFilter symbolGroups = SymbolGroupFilter.TypeOrMember,
            IEnumerable<SymbolFilterRule> rules = null,
            IEnumerable<AttributeFilterRule> attributeRules = null) : base(visibility, symbolGroups, rules, attributeRules)
        {
        }

        public override SymbolFilterResult GetResult(AttributeData attribute)
        {
            SymbolFilterResult result = base.GetResult(attribute);

            if (result != SymbolFilterResult.Success)
                return result;

#if DEBUG
            switch (attribute.AttributeClass.MetadataName)
            {
                case "FooAttribute":
                case "BarAttribute":
                    return SymbolFilterResult.Success;
            }

            if (_knownVisibleAttributes.Contains(attribute.AttributeClass))
                return SymbolFilterResult.Success;

            Debug.Fail(attribute.AttributeClass.ToDisplayString());
#endif
            return result;
        }
    }
}
