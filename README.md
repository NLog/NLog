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

 ℹ️ NLog 5.0 Released!

 NLog 5.0 is finally here. See [List of major changes in NLog 5.0](https://nlog-project.org/2021/08/25/nlog-5-0-preview1-ready.html)

NLog Packages
---
The NLog-nuget-package provides everything needed for doing file- and console-logging. But there are also multiple NLog extension packages,
that provides additional target- and layout-output. See [targets](https://nlog-project.org/config/?tab=targets) and [layout renderers](https://nlog-project.org/config/?tab=layout-renderers) overview!

See Nuget/build status of all official packages [here](https://github.com/NLog/NLog/blob/dev/packages-and-status.md)


Questions, bug reports or feature requests?
---
Issues with getting it working? 
Please check the [troubleshooting guide](https://github.com/NLog/NLog/wiki/Logging-troubleshooting)  before asking! With a clear error message, it's really easier to solve the issue! 

Unclear how to configure NLog correctly of other questions? Please post questions on [StackOverflow](https://stackoverflow.com/).

Do you have feature request or would you like to report a bug? Please post them on the [issue list](https://github.com/NLog/NLog/issues) and follow [these guidelines](.github/CONTRIBUTING.md).


Frequently Asked Questions (FAQ)
---
See [FAQ on the Wiki](https://github.com/NLog/NLog/wiki/faq)


Contributing
---
As the current NLog team is a small team, we cannot fix every bug or implement every feature on our own. So contributions are really appreciated!

If you like to start with a small task, then
[up-for-grabs](https://github.com/NLog/NLog/issues?utf8=%E2%9C%93&q=is%3Aopen+is%3Aissue+label%3Aup-for-grabs+-label%3A%22almost+ready%22+)  are nice to start with.

Please note, we have a `dev` and `master` branch

- `master` is for pure bug fixes and targets NLog 4.x
- `dev` targets NLog 5


A good way to get started (flow)


1. Fork the NLog repos.
1. Create a new branch in you current repos from the 'dev' branch. (critical bugfixes from 'master')
1. 'Check out' the code with Git or [GitHub Desktop](https://desktop.github.com/)
1. Check [contributing.md](.github/CONTRIBUTING.md#sync-projects)
1. Push commits and create a Pull Request (PR) to NLog

Please note: bugfixes should target the **master** branch, others the **dev** branch (NLog 5)


License
---
NLog is open source software, licensed under the terms of BSD license.
See [LICENSE.txt](LICENSE.txt) for details.


How to build
---
Use Visual Studio 2019 and open the solution 'NLog.sln'.

For building in the cloud we use:
- AppVeyor for Windows- and Linux-builds
- SonarQube for code coverage

Trying to build your fork in the cloud? Check [this how-to](howto-build-your-fork.md)

Note: master points to NLog 4.x and dev to NLog 5.x

