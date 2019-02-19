﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using CommandLine;
using Roslynator.Documentation;

namespace Roslynator.CommandLine
{
#if DEBUG
    [Verb("list-symbols", HelpText = "Lists symbols in the specified project or solution.")]
#endif
    public class ListSymbolsCommandLineOptions : MSBuildCommandLineOptions
    {
        [Option(longName: ParameterNames.Depth,
            HelpText = "Defines a depth of a list. Allowed values are member, type or namespace. Default value is member.",
            MetaValue = "<DEPTH>")]
        public string Depth { get; set; }

        [Option(longName: "empty-line-between-members",
            HelpText = "Indicates whether an empty line should be added between two member definitions.")]
        public bool EmptyLineBetweenMembers { get; set; }

        [Option(longName: ParameterNames.Format,
            HelpText = "Specifies parts of a symbol definition that should be formatted.")]
        public IEnumerable<string> Format { get; set; }

        [Option(longName: "ignored-attribute-names",
            HelpText = "Defines a list of attributes' names that should be ignored, i.e. if the symbol has an attribute with the specified name it will be ignored.",
            MetaValue = "<FULLY_QUALIFIED_METADATA_NAME>")]
        public IEnumerable<string> IgnoredAttributeNames { get; set; }

        [Option(longName: "ignored-names",
            HelpText = "Defines a list of metadata names that should be excluded from a documentation. Namespace of type names can be specified.",
            MetaValue = "<FULLY_QUALIFIED_METADATA_NAME>")]
        public IEnumerable<string> IgnoredNames { get; set; }

        [Option(longName: ParameterNames.IgnoredParts,
            HelpText = "Defines parts of a symbol definition that should be excluded.",
            MetaValue = "<IGNORED_PARTS>")]
        public IEnumerable<string> IgnoredParts { get; set; }

        [Option(longName: "include-documentation",
            HelpText = "Indicates whether a documentation should be included.")]
        public bool IncludeDocumentation { get; set; }

        [Option(longName: "indent-chars",
            Default = DefinitionListFormat.DefaultValues.IndentChars,
            HelpText = "Defines characters that should be used for indentation. Default value is two spaces.",
            MetaValue = "<INDENT_CHARS>")]
        public string IndentChars { get; set; }

        [Option(longName: ParameterNames.Layout,
            HelpText = "Defines layout of a list of symbol definitions.")]
        public string Layout { get; set; }

        [Option(longName: "output",
            HelpText = "Defines path to file(s) that will store a list of symbol definitions.",
            MetaValue = "<OUTPUT_FILE>")]
        public IEnumerable<string> Output { get; set; }

        [Option(longName: "references")]
        public IEnumerable<string> References { get; set; }

        [Option(longName: ParameterNames.RootDirectoryUrl,
            HelpText = "Defines a relative url to the documentation root directory. This option is relevant only for markdown output.",
            MetaValue = "<ROOT_DIRECTORY_URL>")]
        public string RootDirectoryUrl { get; set; }

        [Option(longName: ParameterNames.Visibility,
            Default = nameof(Roslynator.Visibility.Private),
            HelpText = "Defines one or more visibility of a type or a member. Allowed values are public, internal or private.",
            MetaValue = "<VISIBILITY>")]
        public IEnumerable<string> Visibility { get; set; }

        //[Option(longName: "include-ienumerable",
        //    HelpText = "Indicates whether interface System.Collections.IEnumerable should be included in a documentation if a type also implements interface System.Collections.Generic.IEnumerable<T>.")]
        //public bool IncludeIEnumerable { get; set; }

        //[Option(longName: "no-precedence-for-system",
        //    HelpText = "Indicates whether symbols contained in 'System' namespace should be ordered as any other symbols and not before other symbols.")]
        //public bool NoPrecedenceForSystem { get; set; }
    }
}
