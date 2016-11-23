![NLog](https://raw.githubusercontent.com/NLog/NLog.github.io/master/images/NLog-logo-only_small.png)
===
*Vote or submit ideas on [UserEcho](https://nlog.userecho.com)!*

===
[![Join the chat at https://gitter.im/NLog/NLog](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/NLog/NLog?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Twitter Follow](https://img.shields.io/twitter/follow/NLogOfficial.svg?style=social?maxAge=2592000)](https://twitter.com/NLogOfficial)
[![Last News](https://img.shields.io/badge/News-16_april_2016-orange.svg)](http://nlog-project.org/archives/)
[![codecov.io](https://codecov.io/github/NLog/NLog/coverage.svg?branch=master)](https://codecov.io/github/NLog/NLog?branch=master) 
[![Semantic Versioning](https://img.shields.io/badge/semver-2.0.0-3D9FE0.svg)](http://semver.org/)
[![Rager Releases](http://rager.io/badge.svg?url=https%3A%2F%2Fgithub.com%2FNLog%2FNLog)](http://rager.io/projects/search?badge=1&query=github.com/nlog/nlog)
[![StackOverflow](https://img.shields.io/stackexchange/stackoverflow/t/nlog.svg?maxAge=2592000)](https://stackoverflow.com/questions/tagged/nlog)

<!--
[![NuGet downloads](https://img.shields.io/nuget/dt/NLog.svg)](https://www.nuget.org/packages/NLog)

[![Pre-release version](https://img.shields.io/nuget/vpre/NLog.svg)](https://www.nuget.org/packages/NLog)--> 



<!--[NLog is Looking for Developers!](http://nlog-project.org/2015/08/05/NLog-is-looking-for-developers.html)-->


NLog is a free logging platform for .NET with rich log routing and management 
capabilities. It makes it easy to produce and manage high-quality logs for 
your application regardless of its size or complexity. 

It can process diagnostic messages emitted from any .NET language, augment 
them with contextual information, format them according to your preference 
and send them to one or more targets such as file or database. 

For more information, see the website [nlog-project.org](http://nlog-project.org)
or just get started with the tutorials on [the NLog wiki](https://github.com/NLog/NLog/wiki).

[Project news - including RSS feed](http://nlog-project.org/archives/)

Packages & Status
---
NLog consists of multiple packages. Most of the functionality is inside the NLog (core) package. What's inside the packages? See [targets](https://github.com/NLog/NLog/wiki/Targets) and [layout renderers](https://github.com/NLog/NLog/wiki/Layout-Renderers) overview!

Package  | Build status | NuGet |
-------- | :------------ | :------------ | :------------------
NLog (.Net / Silverlight 4+5 / Xamarin iOS / Xamarin Android / Windows Phone 8)                                    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.svg)](https://www.nuget.org/packages/NLog)                            |
NLog (.NET Core)   - coreCLR branch                                             | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/coreclr.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/coreclr)                   | [![NuGet package](https://img.shields.io/badge/nuget-v5.0--beta-blue.svg)](https://www.nuget.org/packages/NLog)       | 
[NLog (Microsoft Logging Platform)](https://github.com/NLog/NLog.Framework.Logging)  | [![Build status](https://img.shields.io/appveyor/ci/nlog/nlog-framework-logging/master.svg)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master) | [![NuGet Pre Release](https://badge.fury.io/nu/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging) |
NLog (Mono)                                                         | [![Build Status](https://travis-ci.org/NLog/NLog.svg?branch=master)](https://travis-ci.org/NLog/NLog)                                                         |                                                                                                                                  |
NLog.Config                                                         | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.Config.svg)](https://www.nuget.org/packages/NLog.Config)                   |
[NLog.Contrib.ActiveMQ](https://github.com/NLog/NLog.Contrib.ActiveMQ)                                              | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-contrib-activemq/master.svg)](https://ci.appveyor.com/project/nlog/nlog-contrib-activemq/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.Contrib.ActiveMQ.svg)](https://www.nuget.org/packages/NLog.Contrib.ActiveMQ)                   |
NLog.Extended                                                       | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.Extended.svg)](https://www.nuget.org/packages/NLog.Extended)               |
[NLog.Elmah](https://github.com/NLog/NLog.Elmah)                    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-Elmah/master.svg)](https://ci.appveyor.com/project/nlog/nlog-Elmah/branch/master)               | [![NuGet package](https://badge.fury.io/nu/NLog.Elmah.svg)](https://www.nuget.org/packages/NLog.Elmah)                     |
[NLog.Etw](https://github.com/NLog/NLog.Etw)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-etw/master.svg)](https://ci.appveyor.com/project/nlog/nlog-etw/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.Etw.svg)](https://www.nuget.org/packages/NLog.Etw)                         |
[NLog.InstallNLogConfig](https://github.com/NLog/NLog.InstallNLogConfig)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-InstallNLogConfig/master.svg)](https://ci.appveyor.com/project/nlog/nlog-InstallNLogConfig/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.InstallNLogConfig.svg)](https://www.nuget.org/packages/NLog.InstallNLogConfig)                         |
[NLog.ManualFlush](https://github.com/NLog/NLog.ManualFlush)        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-ManualFlush/master.svg)](https://ci.appveyor.com/project/nlog/nlog-ManualFlush/branch/master)   | [![NuGet package](https://badge.fury.io/nu/NLog.ManualFlush.svg)](https://www.nuget.org/packages/NLog.ManualFlush)         |
NLog.Schema                                                         | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.Schema.svg)](https://www.nuget.org/packages/NLog.Schema)                   |
[NLog.Web](https://github.com/NLog/NLog.Web)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-web/master.svg)](https://ci.appveyor.com/project/nlog/nlog-web/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.Web.svg)](https://www.nuget.org/packages/NLog.Web)                         |
[NLog.Web for ASP.NET Core](https://github.com/NLog/NLog.Web)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-web/master.svg)](https://ci.appveyor.com/project/nlog/nlog-web/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.Web.AspNetCore.svg)](https://www.nuget.org/packages/NLog.Web.AspNetCore)                         |
[NLog.Windows.Forms](https://github.com/NLog/NLog.Windows.Forms)    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-windows-forms/master.svg)](https://ci.appveyor.com/project/nlog/nlog-windows-forms/branch/master)           | [![NuGet package](https://badge.fury.io/nu/NLog.Windows.Forms.svg)](https://www.nuget.org/packages/NLog.Windows.Forms)     |
[NLog.Owin.Logging](https://github.com/NLog/NLog.Owin.Logging)      | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-owin-logging/master.svg)](https://ci.appveyor.com/project/nlog/nlog-owin-logging/branch/master)          | [![NuGet package](https://badge.fury.io/nu/NLog.Owin.Logging.svg)](https://www.nuget.org/packages/NLog.Owin.Logging)    |





Questions, bug reports or feature requests?
---
Do you have feature requests, questions or would you like to report a bug? Please post them on the [issue list](https://github.com/NLog/NLog/issues) and follow [these guidelines](CONTRIBUTING.md).
You could also post questions on [StackOverflow](https://stackoverflow.com/) - in general your will get faster response there. 
Please also post or vote features requests on [UserEcho](https://nlog.userecho.com).



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
1. Create a new branch in you current repos from the 'master' branch.
1. 'Check out' the code with Git or [GitHub Desktop](https://desktop.github.com/)
1. Check [contributing.md](https://github.com/NLog/NLog/blob/master/CONTRIBUTING.md#sync-projects)
1. push commits and create a Pull Request (PR) to NLog


License
---
NLog is open source software, licensed under the terms of BSD license. 
See [LICENSE.txt](LICENSE.txt) for details.


How to build
---
Use Visual studio 2012/2013/2015 and open solution file in the 'src' folder, like 'NLog.netfx45.sln'

For building in the cloud we use:
- AppVeyor for Windows builds, including Silverlight and Xamarin. 
- Travis for Mono builds.
- CodeCov for code coverage

How to build your fork in the cloud
---
Steps to set up [AppVeyor](https://ci.appveyor.com)/[Travis](https://travis-ci.org/)/[CodeCov](https://codecov.io/) for your own fork.

**AppVeyor**:

1. Login with your Github account to https://ci.appveyor.com 
2. Choose "projects" 
3. Select your fork and press "+" button
4. Done. All config is in appveyor.yml already

**Travis**:

1. Login with your Github account to https://travis-ci.org/
2. Select your fork
3. Push and wait

**CodeCov**: (AppVeyor needed)

1. Login with your Github account to https://codecov.io/
2. Press "+  Add new repository to Codecov" button
3. Select your fork
4. Wait for a build on AppVeyor. All the config is already in appveyor.yml. The first report can take some minutes after the first build.


