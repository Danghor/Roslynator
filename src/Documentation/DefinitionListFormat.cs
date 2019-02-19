﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.CodeAnalysis;
namespace Roslynator.Documentation
{
    internal class DefinitionListFormat
    {
        public DefinitionListFormat(
            SymbolDefinitionListLayout layout = DefaultValues.Layout,
            SymbolDefinitionPartFilter parts = DefaultValues.Parts,
            SymbolDefinitionFormatOptions formatOptions = DefaultValues.FormatOptions,
            string indentChars = DefaultValues.IndentChars,
            bool emptyLineBetweenMembers = DefaultValues.EmptyLineBetweenMembers,
            bool omitIEnumerable = DefaultValues.OmitIEnumerable,
            bool preferDefaultLiteral = DefaultValues.PreferDefaultLiteral)
        {
            Layout = layout;
            Parts = parts;
            FormatOptions = formatOptions;
            IndentChars = indentChars;
            EmptyLineBetweenMembers = emptyLineBetweenMembers;
            OmitIEnumerable = omitIEnumerable;
            PreferDefaultLiteral = preferDefaultLiteral;
        }

        public static DefinitionListFormat Default { get; } = new DefinitionListFormat();

        public SymbolDefinitionListLayout Layout { get; }

        public SymbolDefinitionPartFilter Parts { get; }

        public SymbolDefinitionFormatOptions FormatOptions { get; }

        public string IndentChars { get; }

        public bool EmptyLineBetweenMembers { get; }

        public bool OmitIEnumerable { get; }

        public bool PreferDefaultLiteral { get; }

        public bool Includes(SymbolDefinitionPartFilter parts)
        {
            return (Parts & parts) == parts;
        }

        public bool Includes(SymbolDefinitionFormatOptions formatOptions)
        {
            return (FormatOptions & formatOptions) == formatOptions;
        }

        internal SymbolDisplayFormat Update(SymbolDisplayFormat format)
        {
            SymbolDisplayGenericsOptions genericsOptions = SymbolDisplayGenericsOptions.IncludeTypeParameters
                | SymbolDisplayGenericsOptions.IncludeVariance;

            if (Includes(SymbolDefinitionPartFilter.Constraints))
                genericsOptions |= SymbolDisplayGenericsOptions.IncludeTypeConstraints;

            SymbolDisplayMemberOptions memberOptions = SymbolDisplayMemberOptions.IncludeType
                | SymbolDisplayMemberOptions.IncludeExplicitInterface
                | SymbolDisplayMemberOptions.IncludeParameters
                | SymbolDisplayMemberOptions.IncludeConstantValue
                | SymbolDisplayMemberOptions.IncludeRef;

            if (Includes(SymbolDefinitionPartFilter.Modifiers))
                memberOptions |= SymbolDisplayMemberOptions.IncludeModifiers;

            if (Includes(SymbolDefinitionPartFilter.Accessibility))
                memberOptions |= SymbolDisplayMemberOptions.IncludeAccessibility;

            SymbolDisplayParameterOptions parameterOptions = SymbolDisplayParameterOptions.IncludeExtensionThis
                | SymbolDisplayParameterOptions.IncludeParamsRefOut
                | SymbolDisplayParameterOptions.IncludeType;

            if (Includes(SymbolDefinitionPartFilter.ParameterName))
                parameterOptions |= SymbolDisplayParameterOptions.IncludeName;

            if (Includes(SymbolDefinitionPartFilter.ParameterDefaultValue))
                parameterOptions |= SymbolDisplayParameterOptions.IncludeDefaultValue;

            return format.Update(
                genericsOptions: genericsOptions,
                memberOptions: memberOptions,
                parameterOptions: parameterOptions);
        }

        internal static class DefaultValues
        {
            public const SymbolDefinitionListLayout Layout = SymbolDefinitionListLayout.NamespaceList;
            public const SymbolDefinitionPartFilter Parts = SymbolDefinitionPartFilter.All;
            public const SymbolDefinitionFormatOptions FormatOptions = SymbolDefinitionFormatOptions.None;
            public const Visibility Visibility = Roslynator.Visibility.Private;
            public const SymbolGroupFilter SymbolGroupFilter = Roslynator.SymbolGroupFilter.NamespaceOrTypeOrMember;
            public const string IndentChars = "  ";
            public const bool EmptyLineBetweenMembers = false;
            public const bool OmitIEnumerable = true;
            public const bool PreferDefaultLiteral = true;
        }
    }
}
