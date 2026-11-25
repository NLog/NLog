# Repository tools

These tools support NLog's source checks, configuration schema and API documentation.
Use this directory when changing those build steps or the metadata they consume.

## Tools and their consumers

| Directory | Purpose and when to use it | Consumer |
| --- | --- | --- |
| [CheckSourceCode](CheckSourceCode) | Checks license headers and type/file naming conventions in `src` and `tests`. Run it when changing C# source files. It is a console application that exits with code 1 if checks fail. | The `source-checks` job in [CI](../.github/workflows/ci.yml). |
| [DumpApiXml](DumpApiXml) | Extracts configuration-related types and properties from NLog assemblies, together with XML documentation comments, into `API/NLog.api`. It also loads `NLog*.dll` extension assemblies beside the executable. | The `DumpApi` target in [src/NLog.proj](../src/NLog.proj), before schema generation. |
| [MergeApiXml](MergeApiXml) | Combines framework-specific `API/NLog.api` files into `NLogMerged.api.xml`, recording release/framework support. | The `DumpApi` target, which supplies the release output directory. |
| [MakeNLogXSD](MakeNLogXSD) | Generates configuration schemas from the API XML and the embedded [TemplateXSD.xml](MakeNLogXSD/TemplateXSD.xml). Use it when changing configuration metadata or the schema template. | The `XSD` target in `src/NLog.proj`; the combined `NLog.xsd` is then used by `NuGetSchemaPackage`. |
| [NLogNugetPackages](NLogNugetPackages) | Collects NuGet references to NLog extension packages used by the schema and documentation tools. Update this project when changing which extension packages those tools consume. It is a library project, not a command-line tool. | Project references from `DumpApiXml` and `SandcastleDocs/dll_to_doc`. |
| [SandcastleDocs](SandcastleDocs) | Builds the API reference documentation for [nlog-project.org](https://nlog-project.org/documentation/). `dll_to_doc` gathers the NuGet assemblies and XML comments; `NLog.shfbproj` defines the documentation build. | [BuildDoc.cmd](SandcastleDocs/BuildDoc.cmd); see the [documentation build instructions](SandcastleDocs/README.md). |

## Build and source checks

[NLog.Tools.slnx](NLog.Tools.slnx) contains the tool projects, including the
`dll_to_doc` dependency project. Building this solution does **not** run source
checks, generate schemas or build the Sandcastle documentation.

The current [CI `source-checks` job](../.github/workflows/ci.yml) runs on Windows
with the .NET 11 SDK. `CheckSourceCode`, `MergeApiXml` and `MakeNLogXSD` target
`net11.0`; `DumpApiXml`, `NLogNugetPackages` and `dll_to_doc` target .NET Framework
4.8.1. Use the workflow's build and run commands from the repository root to
reproduce these checks, keeping their order: build the tools before running the
source checker with `--no-build`.

## Configuration schema pipeline

The same CI job builds NLog for `net481` and invokes the `XSD` target in
[src/NLog.proj](../src/NLog.proj). That target runs this pipeline:

1. `DumpApiXml` reads each selected framework's `NLog.dll` and writes `API/NLog.api`.
2. `MergeApiXml` combines those files into `NLogMerged.api.xml` in the release output directory.
3. `MakeNLogXSD` uses the API files and its embedded template to generate both
   framework-specific schemas and the combined `NLog.xsd`.

`MergeApiXml` still contains the historical release labels `1.0` and `2.0` and
an optional `\\MASTER\lab\NLog\1.0\Release` input. Those labels are not the current
NLog package version. The tool remains called by `DumpApi`; check that caller
and schema generation before considering it obsolete.

For the separate Sandcastle workflow, start with its own README and run
`BuildDoc.cmd` from `tools/SandcastleDocs` (the favicon copy uses relative paths).
The generated documentation is placed in its `Doc` directory. Its
`dll_to_doc` project uses NuGet packages, so it does not automatically document
unpublished changes to the NLog source checkout.
