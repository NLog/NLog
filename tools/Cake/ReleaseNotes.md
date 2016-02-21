### New in 0.8.0 (Released 2015/01/18)

DNUPackSettings OutputDirectory should be a DirectoryPath
Add GitLink Alias
Make #tool and #addin package manager agnostic
XmlPeek alias
Move from WebClient to HttpClient
PlatformTarget is missing Win32
Move ToolFixture to Cake.Testing
Line number in error messages is incorrect when using directives

### New in 0.7.0 (Released 2015/12/23)

* CakeBuildLog ConsolePalette missing LogLevel.Fatal map
* StartProcess hangs sometimes with large input
* Log errors to console standard error
* Support arbitrary text when parsing AssemblyInformationalVersion.
* Run unit tests on Travis
* Use OutputDirectory property in Choco Pack for Cake
* Workarounds for incomplete tool settings
* Adding support for Atlasssian Bamboo Build Server
* Added missing CakeAliasCategory attribute
* Add code of conduct

### New in 0.6.4 (Released 2015/12/09)
* Quoted process fails on unix

### New in 0.6.3 (Released 2015/12/07)
* ProcessStartInfo filename not always Quoted
* Support spaces in MSBuild configuration
* Add support for DNU

### New in 0.6.2 (Released 2015/12/03)
* Added fix for getting current framework name on Mono.

### New in 0.6.1 (Released 2015/12/02)
* Addded NUnit 3 support.
* Added MSBuild support for ARM processor.
* Added support to deprecate aliases.
* Added new AppVeyor environment variable (job name).
* Added support for MSBuild platform architecture.
* Added output directory for ChocolateyPack.
* Corrected parameter passed to Create method of GitReleaseManager.
* Fixed misconfiguration in GitVersion Runner.
* Fixed null reference exception being thrown when analyzing ReSharper CLI reports.
* ComVisible Attribute was not being parsed correctly by AssemblyInfoParseResult.
* Fixed globber exception when path has ampersand.
* CopyFile logged incorrect target file path.
* ParseAssemblyInfo ignored commented information.
* Got support for .cake files in GitHub.
* Created a Visual Studio Code extension for Cake.
* Created a VSTS extension for Cake.
* Fixed issue with external nugets used directly via #addin directive.
* DupFinder: Added ability to fail the build on detected issues.
* InspectCode: Added ability to fail the build on detected issues.
* TextTransform now handles Regex special characters.

### New in 0.6.0 (Released 2015/11/04)
* Added Chocolatey support.
* Added GitReleaseManager support.
* Added GitReleaseNotes support.
* Added GitVersion support.
* Added MyGet build system support.
* Added OpenCover support.
* Added ReportGenerator support.
* Added ReportUnit support.
* Added Cake script analyzer support.
* Extended AssemblyInfo parser.
* Extended ProcessArgumentBuilder with switch.
* Extended TeamCity build system support.
* Improved NuGet release notes handling.
* Refactored Cake Tool handling & tests.

### New in 0.5.5 (Released 2015/10/12)
* Added alias to retrieve all environment variables.
* Added additional xUnit settings.
* Added verbose message when glob pattern did not match anything.
* Added task setup/teardown functionality.
* Fix for referencing parent directory in glob patterns.
* Added verbose logging for file and directory aliases.
* Removed quotes from MSBuild arguments.
* Added StartProcess alias overload taking process arguments as string.
* Added Cake.Testing NuGet package.
* Added support for AssemblyConfiguration when patching assembly information.
* Fixed bug with dots in glob patterns.
* Fixed bug with reference loading (affects #tool and #addin directives).

### New in 0.5.4 (Released 2015/09/12)
* Removed .nuspec requirement for NuGetPack.
* Enhanced exception message to include name of missing argument.
* Extended ProcessAliases with methods returning IProcess.
* Added string formatting for process argument builder.
* Added path to NuGet resolver for Mono on OS X 10.11.
* Added Homebrew install paths to Cake tool resolver.
* Changed NUnit argument prefix from '/' to '-'.
* Restored accidental sematic change with globber predicates.

### New in 0.5.3 (Released 2015/08/31)
* Additional NUnit switches.
* Made IProcess disposable and added Kill method.
* Fix for glob paths containing parentheses.
* Fix for MSBuild Platform target.
* xUnit: Added support for -noappdomain option.
* DupFinder support added.
* InspectCode Support added.

### New in 0.5.2 (Released 2015/08/11)
* Globber performance improvements.
* Increased visibility of skipped tasks.
* Added ILRepack support.
* Fix for PlatformTarget not used in MSBuild runner.
* Changed TeamCityOutput to a nullable boolean.
* Fix for CleanDirectory bug.
* Added support for using-alias-directives (Roslyn only).
* Added XmlPoke support.

### New in 0.5.1 (Released 2015/07/27)
* Increased stability when running on Mono.
* Added MSTest support for Visual Studio 2015 (version 14.0).
* Renamed MSOrXBuild to DotNetBuild.
* Better error reporting on Mono.
* Fixed path bug affecting non Windows systems.
* Cake now logs a warning if an assembly can't be loaded.

### New in 0.5.0 (Released 2015/07/20)
* Added Mono support.
* Added XBuild alias.
* Improved tool resolution.
* Added Fixie support.
* Added IsRunningOnWindows() alias.
* Added IsRunningOnUnix() alias.
* Added NuGet proxy support.
* Fixed MSBuild verbosity bug.
* Added shebang line support.

### New in 0.4.3 (Released 2015/07/05)
* Added TeamCity support.
* Added filter predicate to globber and clean directory methods.
* Added Unzip alias.
* Added DownloadFile alias.
* Added method to retrieve filename without it's extension.
* Added support for InternalsVisibleToAttribute when generating assembly info.
* Added extension methods to ProcessSettings.
* Fixed formatting in build report.
* Fixed problems with whitespace in arguments.

### New in 0.4.2 (Released 2015/05/27)
* Added aliases for making paths absolute.
* Added support for creating Octopus Deploy releases.

### New in 0.4.1 (Released 2015/05/18)
* Made Cake work on .NET 4.6 again without experimental flag.
* The tools directory now have higher precedence than environment paths when resolving nuget.exe.

### New in 0.4.0 (Released 2015/05/12)
* Now using RC2 of Roslyn from NuGet since MyGet distribution was no longer compatible.
* Added support for MSBuild 14.0.

### New in 0.3.2 (Released 2015/04/16)
* NuGet package issue fix.

### New in 0.3.1 (Released 2015/04/16)
* Fixed an issue where Roslyn assemblies weren't loaded properly after install.

### New in 0.3.0 (Released 2015/04/16)
* Added experimental support for nightly build of Roslyn.
* Fixed an issue where passing multiple assemblies to NUnit resulted in multiple executions of NUnit.
* Added Windows 10 OS support.

### New in 0.2.2 (Released 2015/03/31)
* Added lots of example code.
* Added target platform option to ILMerge tool.
* Added #tool line directive.
* Added support for NuGet update command.

### New in 0.2.1 (Released 2015/03/17)
* Added convertable paths and removed path add operators.

### New in 0.2.0 (Released 2015/03/15)
* Added script dry run option.
* Added MSBuild verbosity setting.
* Added convenience aliases for working with directory and file paths.
* Fixed console rendering bug.
* Fixed nuspec xpath bug.
* Fixed parsing of command line arguments.

### New in 0.1.34 (Released 2015/03/03)
* Added support for NuGet SetApiKey.
* Fixed unsafe logging.
* Made text transformation placeholders configurable.
* Added missing common special paths.
* Fixed script path bug.
* Added XML transformation support.

### New in 0.1.33 (Released 2015/02/24)
* Added Multiple Assembly Support.
* Added process output and timeout.
* Fixed code generation issue.
* Added aliases for executing cake scripts out of process.
* Added file hash calculator.
* Added aliases for checking existence of directories and files.
* Added support for NSIS.

### New in 0.1.32 (Released 2015/02/10)
* Fixed issue where script hosts had been made internal by mistake.

### New in 0.1.31 (Released 2015/02/10)
* Documentation updates only.

### New in 0.1.30 (Released 2015/02/08)
* Added support for installing NuGet packages from script.
* Added filter support to CleanDirectory.

### New in 0.1.29 (Released 2015/01/28)
* Fixed globber bug that prevented NUnit runner from running.

### New in 0.1.28 (Released 2015/01/18)
* Added support for transforming nuspec files.
* Added support for copying directories.

### New in 0.1.27 (Released 2015/01/13)
* Made build log easier to read.
* Fixed wrong namespace for CLSCompliant attribute.
* Added predictable encoding to AssemblyInfoCreator.

### New in 0.1.26 (Released 2015/01/11)
* Added AppVeyor support.
* Added #addin directive for NuGet addins.
* Added assembly company to AssemblyInfoCreator.
* Added finally handler for tasks.
* Added error reporter for tasks.

### New in 0.1.25 (Released 2015/01/01)
* Added parsing of solution version information if available.
* Fixed so logging won't throw an exception if one of the arguments is null.
* Fix for argument parsing without script.
* Added support for simple text transformations.

### New in 0.1.24 (Released 2014/12/12)
* Added support for NuGet sources.
* Added solution and project parsers.

### New in 0.1.23 (Released 2014/11/21)
* Removed silent flag from xUnit.net v2 runner since it's been deprecated.

### New in 0.1.22 (Released 2014/11/20)
* Added support for script setup/teardown.
* Added MSBuild node reuse option.
* Added xUnit.net v2 support.

### New in 0.1.21 (Released 2014/09/23)
* Added line directives to generated scripts.

### New in 0.1.20 (Released 2014/09/14)
* Fix for relative paths in Globber.
* Specifying a script now take precedence over version or help commands.
* Throws if target cannot be reached due to constraints.
* Added logging when tasks are skipped due to constraints.
* Changed location of transformed nuspec file.
* Made nuspec XML namespaces optional.

### New in 0.1.19 (Released 2014/09/03)
* Added default file convention recognizer.
* Added assembly info parser.
* Added error handling.
* Added total duration to task report.
* Added Sign extension for assembly certificate signing.
* Changed the way processes are started.
* Now outputs full stack trace in diagnostic mode.
* Fixed issue with relative paths in tools.
* Added xUnit silent flag.

### New in 0.1.18 (Released 2014/08/21)
* Added external script loading.
* IFile.OpenWrite will now truncate existing file.
* Added overloads for common script alias methods.
* Added support for running custom processes.
* MSBuild runner now uses latest MSBuild version if not explicitly specified.
* Moved Tool<T> to Cake.Core.
* Ignored errors are now logged.
* Added more NUnit settings.
* Added environment variable script aliases.

### New in 0.1.17 (Released 2014/07/29)
* Made non interactive mode mandatory for NuGet restore.
* Added missing Cake.Common.xml.
* Major refactoring of tools.
* Added attributes for documentation.

### New in 0.1.16 (Released 2014/07/23)
* Added WiX support.
* Added .nuspec metadata manipulation support to NuGet package creation.

### New in 0.1.15 (Released 2014/07/20)
* Added NuGet push support.

### New in 0.1.14 (Released 2014/07/17)
* Added Cake.Core NuGet package.
* Added support for loading external script aliases.

### New in 0.1.13 (Released 2014/07/10)
* No more logging when creating script aliases.

### New in 0.1.12 (Released 2014/07/10)
* Added file deletion.
* Added file moving.
* Added directory creation.
* Added version command.
* Major refactoring of Cake (console application).
* NuGet packer now use absolute paths.
* Minor fix for console background colors.
* Added way of retrieving environment variables.
* Added script alias property support.

### New in 0.1.11 (Released 2014/07/01)
* Critical bug fix for script host.

### New in 0.1.10 (Released 2014/07/01)
* Added parsing of FAKE's release notes format.
* Added task description support.
* Added script methods for log.

### New in 0.1.9 (Releases 2014/06/28)
* Added AssemblyInfo creator.
* Zip: Fixed bug with relative paths.
* MSBuild: Added support for max CPU count.
* Added logging of process launch parameters.
* MSBuild: Fix for multiple property values & quotation.
* Fixed issue with cleaning deep dir structures.

### New in 0.1.8 (Released 2014/06/25)
* Added NuGet restore support.
* Task names are no longer case sensitive.
* Bug fix for non quoted MSBuild solution argument.
* Added custom collections for file and directory paths.

### New in 0.1.7 (Released 2014/06/21)
* Renamed method Run to RunTarget.
* Various fixes and improvements.

### New in 0.1.6 (Released 2014/06/18)
* Added MSTest support.

### New in 0.1.5 (Released 2014/06/17)
* Added ILMerge support.

### New in 0.1.4 (Released 2014/06/15)
* Added NUnit support.

### New in 0.1.3 (Released 2014/06/14)
* Fixed compression bug where sub directories were not properly included in zip file.

### New in 0.1.2 (Released 2014/06/13)
* Fixed bug where globbing did not take OS case sensitivity into account.

### New in 0.1.1 (Released 2014/06/12)
* Added NuGet Symbol support.
* Restructured solution. Removed individual assemblies and introduced Cake.Common.dll.

### New in 0.1.0 (Released 2014/06/11)
* Added extensions methods for opening files.
* Added task report.
* Minor fix for cleaning directories.

### New in 0.0.8 (Released 2014/06/10)
* Added xUnit options.
* Copying files now overwrite the destination files.

### New in 0.0.7 (Released 2014/06/10)
* Added zip compression support.
* Added NuGet packing support.
* Added file copy convenience methods.

### New in 0.0.6 (Released 2014/06/06)
* Added basic IO functionality such as cleaning and deleting directories.
* Added script host methods for built in functionality (MSBuild, xUnit and Globbing).

### New in 0.0.5 (Released 2014/06/06)
* Added support for MSBuild tool version.
* Added support for MSBuild platform target.

### New in 0.0.4 (Released 2014/06/05)
* Added script argument support.

### New in 0.0.3 (Released 2014/06/04)
* Bug fix for when resolving working directory.

### New in 0.0.2 (Released 2014/06/04)
* Added logging support.
* Added dedicated script runner.

### New in 0.0.1 (Released 2014/05/06)
* First release of Cake.
