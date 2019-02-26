
# `list-symbols` Command

List symbol definitions from the specified project or solution.

## Synopsis

```
roslynator list-symbols <PROJECT|SOLUTION>
[--depth]
[--empty-line-between-members]
[--file-log]
[--file-log-verbosity]
[--format]
[--group-by-assembly]
[--ignored-attributes]
[--ignored-projects]
[--ignored-symbols]
[--documentation]
[--indent-chars]
[--language]
[--layout]
[--msbuild-path]
[-o|--output]
[--projects]
[-p|--properties]
[--references]
[-v|--verbosity]
[--visibility]
```

## Arguments

**`PROJECT|SOLUTION`**

The project or solution to analyze.

### Optional Options

**`[--depth]`** `{member|type|namespace}`

Defines a depth of a documentation. Default value is `member`.

**`[--empty-line-between-members]`**

Indicates whether an empty line should be added between two member definitions.

**`[--format]`**

Specifies parts of a symbol definition that should be formatted. Allowed values are attributes, parameters, base-list and constraints.

**`[--group-by-assembly]`**

Indicates whether symbols should be grouped by assembly.

**`[--ignored-attributes]`** `<FULLY_QUALIFIED_METADATA_NAME>`

Defines a list of attributes that should be ignored.

**`--ignored-projects`** <PROJECT_NAME>

Defines projects that should be skipped.

**`[--ignored-symbols]`** `<FULLY_QUALIFIED_METADATA_NAME>`

Defines a list of symbols that should be ignored. Namespace of types can be specified.

**`[--documentation]`**

Indicates whether a documentation should be included. This option is available for XML and plain text.

**`[--indent-chars]`** `<INDENT_CHARS>`

Defines characters that should be used for indentation. Default value is four spaces.

**`--language`** `{cs[harp]|v[isual-]b[asic])}`

Defines project language.

**`--layout`** `{namespace-list|namespace-hierarchy|type-hierarchy}`

Defines layout of a list of symbol definitions. Allowed values are namespace-list, namespace-hierarchy or type-hierarchy. Default value is namespace-list.

**`--msbuild-path`** <MSBUILD_PATH>

Defines a path to MSBuild.

*Note: First found instance of MSBuild will be used if the path to MSBuild is not specified.*

**`[-o|--output]`** `<OUTPUT_PATH>`

Defines path to file(s) that will store a list of symbol definitions. Possible output formats are XML ('xml' extension), Markdown ('md' extension) or plaintext format."

**`--projects`** <PROJECT_NAME>

Defines projects that should be analyzed.

**`-p|--properties`** `<NAME=VALUE>`

Defines one or more MSBuild properties.

**`--references`** <ASSEMBLY_FILE>

Defines file name/path to assembly(ies) that should be included.

**`-v|--verbosity`** `{q[uiet]|m[inimal]|n[ormal]|d[etailed]|diag[nostic]}`

Defines the amount of information to display in the log.

**`[--visibility]`** `{public|internal|private}`

Defines a visibility of a type or a member. Default value is `public`.

## See Also

* [Roslynator Command-Line Interface](README.md)
