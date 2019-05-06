![NLog](https://raw.githubusercontent.com/NLog/NLog.github.io/master/images/NLog-logo-only_small.png)

[![NuGet](https://img.shields.io/nuget/v/nlog.svg)](https://www.nuget.org/packages/NLog)
[![Semantic Versioning](https://img.shields.io/badge/semver-2.0.0-3D9FE0.svg)](https://semver.org/)
[![Twitter Follow](https://img.shields.io/twitter/follow/NLogOfficial.svg?style=social?maxAge=2592000)](https://twitter.com/NLogOfficial)
[![StackOverflow](https://img.shields.io/stackexchange/stackoverflow/t/nlog.svg?maxAge=2592000&label=stackoverflow)](https://stackoverflow.com/questions/tagged/nlog) 
[![license](https://img.shields.io/github/license/mashape/apistatus.svg)]()


[![](https://sonarcloud.io/api/project_badges/measure?project=nlog&metric=ncloc&branch=dev)](https://sonarcloud.io/dashboard/?id=nlog&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog&metric=bugs&branch=dev)](https://sonarcloud.io/dashboard/?id=nlog&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog&metric=vulnerabilities&branch=dev)](https://sonarcloud.io/dashboard/?id=nlog&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog&metric=code_smells&branch=dev)](https://sonarcloud.io/project/issues?id=nlog&resolved=false&types=CODE_SMELL&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog&metric=duplicated_lines_density&branch=dev)](https://sonarcloud.io/component_measures/domain/Duplications?id=nlog&branch=dev) 
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog&metric=sqale_debt_ratio&branch=dev)](https://sonarcloud.io/dashboard/?id=nlog&branch=dev) 
[![codecov.io](https://codecov.io/github/NLog/NLog/coverage.svg?branch=dev)](https://codecov.io/github/NLog/NLog?branch=dev)

<!--
[![NuGet downloads](https://img.shields.io/nuget/dt/NLog.svg)](https://www.nuget.org/packages/NLog)

[![Pre-release version](https://img.shields.io/nuget/vpre/NLog.svg)](https://www.nuget.org/packages/NLog)-->



<!--[NLog is Looking for Developers!](https://nlog-project.org/2015/08/05/NLog-is-looking-for-developers.html)-->



NLog is a free logging platform for .NET with rich log routing and management
capabilities. It makes it easy to produce and manage high-quality logs for
your application regardless of its size or complexity.

It can process diagnostic messages emitted from any .NET language, augment
them with contextual information, format them according to your preference
and send them to one or more targets such as file or database.

For more information, see the website [nlog-project.org](https://nlog-project.org)
or just get started with the tutorials on [the NLog wiki](https://github.com/NLog/NLog/wiki).

Major releases will be posted on [project news](https://nlog-project.org/archives/). For smaller updates, follow us on [Twitter](https://twitter.com/NLogOfficial)


Getting started
---

- For ASP.NET Core 1 users, see [installation instructions](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-(csproj---vs2017))
- For ASP.NET Core 2 users, see [installation instructions](https://github.com/NLog/NLog.Web/wiki/Getting-started-with-ASP.NET-Core-2)
- ASP.NET (classic) users: 
  1. Install [NLog.Config NuGet package](https://www.nuget.org/packages/NLog.config)
  2. recommended, install also install [NLog.Web NuGet package](https://www.nuget.org/packages/NLog.Web)

- Others:
  1. Install [NLog.Config NuGet package](https://www.nuget.org/packages/NLog.config)


More detailed tutorial can be found [here](https://github.com/NLog/NLog/wiki/Tutorial).

For the possible options in the config, check the [Options list](https://nlog-project.org/config/)

Having troubles? Check the [troubleshooting guide](https://github.com/NLog/NLog/wiki/Logging-troubleshooting)



-----

 ℹ️ Looking for NLog 5? Just install NLog 4.5!

NLog 4.5 implements the platforms added in NLog 5 (.NET Standard 1, .NET Standard 2, UWP, etc) and added structural logging, *without breaking changes*!

----

Packages & Status
---
NLog consists of multiple packages. Most of the functionality is inside the NLog (core) package. What's inside the packages? See [targets](https://github.com/NLog/NLog/wiki/Targets) and [layout renderers](https://github.com/NLog/NLog/wiki/Layout-Renderers) overview!

Package  | Build status | NuGet 
-------- | :------------ | :------------ 
NLog                                    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)   [![Build Status](https://travis-ci.org/NLog/NLog.svg?branch=dev)](https://travis-ci.org/NLog/NLog)                    | [![NuGet](https://img.shields.io/nuget/v/nlog.svg)](https://www.nuget.org/packages/NLog)
[NLog.Extensions.Logging](https://github.com/NLog/NLog.Extensions.Logging)  | [![Build status](https://img.shields.io/appveyor/ci/nlog/nlog-framework-logging/master.svg)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master) | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging)
NLog.Config                                                         | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Config.svg)](https://www.nuget.org/packages/NLog.Config)
[NLog.Contrib.ActiveMQ](https://github.com/NLog/NLog.Contrib.ActiveMQ)                                              | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-contrib-activemq/master.svg)](https://ci.appveyor.com/project/nlog/nlog-contrib-activemq/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Contrib.ActiveMQ.svg)](https://www.nuget.org/packages/NLog.Contrib.ActiveMQ)
[NLog.Elmah](https://github.com/NLog/NLog.Elmah)                    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-Elmah/master.svg)](https://ci.appveyor.com/project/nlog/nlog-Elmah/branch/master)               | [![NuGet package](https://img.shields.io/nuget/v/NLog.Elmah.svg)](https://www.nuget.org/packages/NLog.Elmah)
[NLog.Etw](https://github.com/NLog/NLog.Etw)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-etw/master.svg)](https://ci.appveyor.com/project/nlog/nlog-etw/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Etw.svg)](https://www.nuget.org/packages/NLog.Etw)
[NLog.InstallNLogConfig](https://github.com/NLog/NLog.InstallNLogConfig)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-InstallNLogConfig/master.svg)](https://ci.appveyor.com/project/nlog/nlog-InstallNLogConfig/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.InstallNLogConfig.svg)](https://www.nuget.org/packages/NLog.InstallNLogConfig)
[NLog.MailKit](https://github.com/NLog/NLog.MailKit)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-mailkit/master.svg)](https://ci.appveyor.com/project/nlog/nlog-mailkit/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.MailKit.svg)](https://www.nuget.org/packages/NLog.MailKit)
[NLog.ManualFlush](https://github.com/NLog/NLog.ManualFlush)        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-ManualFlush/master.svg)](https://ci.appveyor.com/project/nlog/nlog-ManualFlush/branch/master)   | [![NuGet package](https://img.shields.io/nuget/v/NLog.ManualFlush.svg)](https://www.nuget.org/packages/NLog.ManualFlush)
NLog.Schema                                                         | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Schema.svg)](https://www.nuget.org/packages/NLog.Schema)
[NLog.Web](https://github.com/NLog/NLog.Web)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-web/master.svg)](https://ci.appveyor.com/project/nlog/nlog-web/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Web.svg)](https://www.nuget.org/packages/NLog.Web)
[NLog.Web for ASP.NET Core](https://github.com/NLog/NLog.Web)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-web/master.svg)](https://ci.appveyor.com/project/nlog/nlog-web/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Web.AspNetCore.svg)](https://www.nuget.org/packages/NLog.Web.AspNetCore)
[NLog.Windows.Forms](https://github.com/NLog/NLog.Windows.Forms)    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-windows-forms/master.svg)](https://ci.appveyor.com/project/nlog/nlog-windows-forms/branch/master)           | [![NuGet package](https://img.shields.io/nuget/v/NLog.Windows.Forms.svg)](https://www.nuget.org/packages/NLog.Windows.Forms)
[NLog.Owin.Logging](https://github.com/NLog/NLog.Owin.Logging)      | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-owin-logging/master.svg)](https://ci.appveyor.com/project/nlog/nlog-owin-logging/branch/master)          | [![NuGet package](https://img.shields.io/nuget/v/NLog.Owin.Logging.svg)](https://www.nuget.org/packages/NLog.Owin.Logging)





Questions, bug reports or feature requests?
---
Do you have feature request or would you like to report a bug? Please post them on the [issue list](https://github.com/NLog/NLog/issues) and follow [these guidelines](.github/CONTRIBUTING.md).
Please post questions on [StackOverflow](https://stackoverflow.com/).




Frequently Asked Questions (FAQ)
---
See [FAQ on the Wiki](https://github.com/NLog/NLog/wiki/faq)


Contributing
---
As the current NLog team is a small team, we cannot fix every bug or implement every feature on our own. So contributions are really appreciated!

If you like to start with a small task, then
[up-for-grabs](https://github.com/NLog/NLog/issues?utf8=%E2%9C%93&q=is%3Aopen+is%3Aissue+label%3Aup-for-grabs+-label%3A%22almost+ready%22+)  are nice to start with.


A good way to get started (flow)


1. Fork the NLog repos.
1. Create a new branch in you current repos from the 'dev' branch. (critical bugfixes from 'master')
1. 'Check out' the code with Git or [GitHub Desktop](https://desktop.github.com/)
1. Check [contributing.md](.github/CONTRIBUTING.md#sync-projects)
1. Push commits and create a Pull Request (PR) to NLog


License
---
NLog is open source software, licensed under the terms of BSD license.
See [LICENSE.txt](LICENSE.txt) for details.


How to build
---
Use Visual Studio 2017 and open the solution 'NLog.sln' - C# 7.0 support is required.

For building in the cloud we use:
- AppVeyor for Windows builds, including Silverlight and Xamarin.
- Travis for Mono builds.
- CodeCov for code coverage

Trying to build your fork in the cloud? Check [this how-to](howto-build-your-fork.md)

