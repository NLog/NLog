![NLog](https://raw.githubusercontent.com/NLog/NLog.github.io/master/images/NLog-logo-only_small.png)
===
*Vote or submit ideas on [UserEcho](https://nlog.userecho.com)!*

===
[![Join the chat at https://gitter.im/NLog/NLog](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/NLog/NLog?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Twitter](https://img.shields.io/badge/Twitter-NLogOfficial-blue.svg)](https://twitter.com/nlogofficial)
[![Last News](https://img.shields.io/badge/News-21_nov_2015-orange.svg)](http://nlog-project.org/archives/)
[![NuGet downloads](https://img.shields.io/nuget/dt/NLog.svg)](https://www.nuget.org/packages/NLog)
[![codecov.io](https://codecov.io/github/NLog/NLog/coverage.svg?branch=master)](https://codecov.io/github/NLog/NLog?branch=master) 


<!--[![Pre-release version](https://img.shields.io/nuget/vpre/NLog.svg)](https://www.nuget.org/packages/NLog)--> 



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

Package  | Build status | Nuget |
-------- | :------------ | :------------ | :------------------
NLog (Windows / Xamarin)                                                      | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.svg)](https://www.nuget.org/packages/NLog)                                 |
NLog (Mono)                                                         | [![Build Status](https://travis-ci.org/NLog/NLog.svg?branch=master)](https://travis-ci.org/NLog/NLog)                                                         |                                                                                                                                  |
NLog.Config                                                         | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Config.svg)](https://www.nuget.org/packages/NLog.Config)                   |
[NLog.Contrib.ActiveMQ](https://github.com/NLog/NLog.Contrib.ActiveMQ)                                              | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-contrib-activemq/master.svg)](https://ci.appveyor.com/project/nlog/nlog-contrib-activemq/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Contrib.ActiveMQ.svg)](https://www.nuget.org/packages/NLog.Contrib.ActiveMQ)                   |
NLog.Extended                                                       | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Extended.svg)](https://www.nuget.org/packages/NLog.Extended)               |
[NLog.Elmah](https://github.com/NLog/NLog.Elmah)                    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-Elmah/master.svg)](https://ci.appveyor.com/project/nlog/nlog-Elmah/branch/master)               | [![NuGet package](https://img.shields.io/nuget/v/NLog.Elmah.svg)](https://www.nuget.org/packages/NLog.Elmah)                     |
[NLog.Etw](https://github.com/NLog/NLog.Etw)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-etw/master.svg)](https://ci.appveyor.com/project/nlog/nlog-etw/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Etw.svg)](https://www.nuget.org/packages/NLog.Etw)                         |
[NLog.InstallNLogConfig](https://github.com/NLog/NLog.InstallNLogConfig)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-InstallNLogConfig/master.svg)](https://ci.appveyor.com/project/nlog/nlog-InstallNLogConfig/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.InstallNLogConfig.svg)](https://www.nuget.org/packages/NLog.InstallNLogConfig)                         |
[NLog.ManualFlush](https://github.com/NLog/NLog.ManualFlush)        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-ManualFlush/master.svg)](https://ci.appveyor.com/project/nlog/nlog-ManualFlush/branch/master)   | [![NuGet package](https://img.shields.io/nuget/v/NLog.ManualFlush.svg)](https://www.nuget.org/packages/NLog.ManualFlush)         |
NLog.Schema                                                         | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Schema.svg)](https://www.nuget.org/packages/NLog.Schema)                   |
[NLog.Web](https://github.com/NLog/NLog.Web)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-web/master.svg)](https://ci.appveyor.com/project/nlog/nlog-web/branch/master)                   | [![NuGet package](https://img.shields.io/nuget/v/NLog.Web.svg)](https://www.nuget.org/packages/NLog.Web)                         |
[NLog.Windows.Forms](https://github.com/NLog/NLog.Windows.Forms)    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-windows-forms/master.svg)](https://ci.appveyor.com/project/nlog/nlog-windows-forms/branch/master)           | [![NuGet package](https://img.shields.io/nuget/v/NLog.Windows.Forms.svg)](https://www.nuget.org/packages/NLog.Windows.Forms)     |





Questions, bug reports or feature requests?
---
Do you have feature requests, questions or would you like to report a bug? Please post them on the [issue list](https://github.com/NLog/NLog/issues) and follow [these guidelines](CONTRIBUTING.md).
Please also post or vote features requests on [UserEcho](https://nlog.userecho.com).

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
See LICENSE.txt for details.
