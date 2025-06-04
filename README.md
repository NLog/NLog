![NLog](https://raw.githubusercontent.com/NLog/NLog.github.io/master/images/NLog-logo-only_small.png)

<!--[![Pre-release version](https://img.shields.io/nuget/vpre/NLog.svg)](https://www.nuget.org/packages/NLog)-->
[![NuGet](https://img.shields.io/nuget/v/nlog.svg)](https://www.nuget.org/packages/NLog)
[![Semantic Versioning](https://img.shields.io/badge/semver-2.0.0-3D9FE0.svg)](https://semver.org/)
[![NuGet downloads](https://img.shields.io/nuget/dt/NLog.svg)](https://www.nuget.org/packages/NLog)
<!--[![StackOverflow](https://img.shields.io/stackexchange/stackoverflow/t/nlog.svg?maxAge=2592000&label=stackoverflow)](https://stackoverflow.com/questions/tagged/nlog) -->


[![](https://sonarcloud.io/api/project_badges/measure?project=nlog2&metric=ncloc&branch=dev)](https://sonarcloud.io/dashboard/?id=nlog2&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog2&metric=bugs&branch=dev)](https://sonarcloud.io/dashboard/?id=nlog2&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog2&metric=vulnerabilities&branch=dev)](https://sonarcloud.io/dashboard/?id=nlog2&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog2&metric=code_smells&branch=dev)](https://sonarcloud.io/project/issues?id=nlog2&resolved=false&types=CODE_SMELL&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog2&metric=duplicated_lines_density&branch=dev)](https://sonarcloud.io/component_measures/domain/Duplications?id=nlog2&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog2&metric=sqale_debt_ratio&branch=dev)](https://sonarcloud.io/dashboard/?id=nlog2&branch=dev) 
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=nlog2&metric=coverage&branch=dev)](https://sonarcloud.io/dashboard?id=nlog2&branch=dev)


[![](https://img.shields.io/badge/Docs-GitHub%20wiki-brightgreen)](https://github.com/NLog/NLog/wiki)
[![](https://img.shields.io/badge/Troubleshoot-Guide-orange)](https://github.com/nlog/nlog/wiki/Logging-troubleshooting)


NLog is a free logging platform for .NET with rich log routing and management
capabilities. It makes it easy to produce and manage high-quality logs for
your application regardless of its size or complexity.

It can process diagnostic messages emitted from any .NET language, augment
them with contextual information, format them according to your preference
and send them to one or more targets such as file or database.

Major and minor releases will be posted on [project news](https://nlog-project.org/archives/). 

Getting started
---

  * [.NET Framework](https://github.com/NLog/NLog/wiki/Tutorial)
  * [ASP.NET Core](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-6)
  * [.NET Core Console](https://github.com/NLog/NLog/wiki/Getting-started-with-.NET-Core-2---Console-application)

For the possible options in the config, check the [Options list](https://nlog-project.org/config/) and [API Reference](https://nlog-project.org/documentation/)

Having troubles? Check the [troubleshooting guide](https://github.com/NLog/NLog/wiki/Logging-troubleshooting)

-----


 ℹ️ NLog 6.0 will support AOT

NLog 6.0 is now being prepared. See [List of goals for NLog 6.0](https://nlog-project.org/2024/10/01/nlog-6-0-goals.html)


NLog Extensions
---
The NLog-nuget-package provides everything needed for doing file- and console-logging. If you need other output options, then there are many NLog extension packages available (such as databases, email, cloud services, etc.).
See [targets](https://nlog-project.org/config/?tab=targets) and [layout renderers](https://nlog-project.org/config/?tab=layout-renderers) overview!

The NLog extension packages maintained by the NLog-project are [listed here](https://github.com/NLog/NLog/blob/dev/packages-and-status.md) with Nuget/build status.

It is also possible to [Create your own custom NLog extensions](https://github.com/NLog/NLog/wiki/Extending-NLog).


Questions, bug reports or feature requests?
---
If having issues with getting NLog working? Then please check the [troubleshooting guide](https://github.com/NLog/NLog/wiki/Logging-troubleshooting) before asking! This will often provide you with clear error message when asking, so it is easier to solve the issue!

If having questions about how to configure NLog correctly? Then please post questions on [StackOverflow](https://stackoverflow.com/questions/tagged/nlog) (using the `nlog` tag)

Have you found a bug or issue with NLog functionality? Please post them on the [issue list](https://github.com/NLog/NLog/issues) and follow [these guidelines](/CONTRIBUTING.md).


Frequently Asked Questions (FAQ)
---
See [FAQ on the Wiki](https://github.com/NLog/NLog/wiki/faq)


License
---
NLog is open source software, licensed under the terms of BSD license.
See [LICENSE.txt](LICENSE.txt) for details.
