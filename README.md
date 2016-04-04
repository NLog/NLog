![NLog](https://raw.githubusercontent.com/NLog/NLog.github.io/master/images/NLog-logo-only_small.png)
===
*Vote or submit ideas on [UserEcho](https://nlog.userecho.com)!*

===
[![Join the chat at https://gitter.im/NLog/NLog](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/NLog/NLog?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Twitter](https://img.shields.io/badge/Twitter-NLogOfficial-blue.svg)](https://twitter.com/nlogofficial)
[![Last News](https://img.shields.io/badge/News-24_mar_2016-orange.svg)](http://nlog-project.org/archives/)
[![codecov.io](https://codecov.io/github/NLog/NLog/coverage.svg?branch=master)](https://codecov.io/github/NLog/NLog?branch=master) 
[![Semantic Versioning](https://img.shields.io/badge/semver-2.0.0-3D9FE0.svg)](http://semver.org/)


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
NLog (Windows / Silverlight 4+5)                                    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.svg)](https://www.nuget.org/packages/NLog)                            |
NLog (Xamarin iOS / Xamarin Android / Windows Phone 8)              | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/master.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/master)                   | [![NuGet package](https://img.shields.io/badge/nuget-v4.3.0--rc2-blue.svg)](https://www.nuget.org/packages/NLog)         |
NLog (CoreCLR)                                                | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog/coreclr.svg)](https://ci.appveyor.com/project/nlog/nlog/branch/coreclr)                   | [![NuGet package](https://img.shields.io/badge/nuget-v4.4.0--beta3-blue.svg)](https://www.nuget.org/packages/NLog)       | 
[NLog (ASP.NET 5)](https://github.com/NLog/NLog.Framework.Logging)  | [![Build status](https://img.shields.io/appveyor/ci/nlog/nlog-framework-logging/master.svg)](https://ci.appveyor.com/project/nlog/nlog-framework-logging/branch/master) | [![NuGet Pre Release](https://badge.fury.io/nu/NLog.Extensions.Logging.svg)](https://www.nuget.org/packages/NLog.Extensions.Logging) |
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
[NLog.Web for ASP.NET 5](https://github.com/NLog/NLog.Web)                        | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-web/master.svg)](https://ci.appveyor.com/project/nlog/nlog-web/branch/master)                   | [![NuGet package](https://badge.fury.io/nu/NLog.Web.ASPNET5.svg)](https://www.nuget.org/packages/NLog.Web.ASPNET5)                         |
[NLog.Windows.Forms](https://github.com/NLog/NLog.Windows.Forms)    | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-windows-forms/master.svg)](https://ci.appveyor.com/project/nlog/nlog-windows-forms/branch/master)           | [![NuGet package](https://badge.fury.io/nu/NLog.Windows.Forms.svg)](https://www.nuget.org/packages/NLog.Windows.Forms)     |
[NLog.Owin.Logging](https://github.com/NLog/NLog.Owin.Logging)      | [![AppVeyor](https://img.shields.io/appveyor/ci/nlog/nlog-owin-logging/master.svg)](https://ci.appveyor.com/project/nlog/nlog-owin-logging/branch/master)          | [![NuGet package](https://badge.fury.io/nu/NLog.Owin.Logging.svg)](https://www.nuget.org/packages/NLog.Owin.Logging)    |





Questions, bug reports or feature requests?
---
Do you have feature requests, questions or would you like to report a bug? Please post them on the [issue list](https://github.com/NLog/NLog/issues) and follow [these guidelines](CONTRIBUTING.md).
Please also post or vote features requests on [UserEcho](https://nlog.userecho.com).


Frequently Asked Questions (FAQ)
---
* **What is NLog?**
 - NLog is a free and open source library which helps to write log messages. 

* **Why should I use a log libary? I can just do `file.WriteLine()`**
  - Beside writing to files, you can write to many different targets, like databases, event viewer, trace etc. 
  - The output is templatable with many predefined template values. 
  - With a simple configuration file you can decide runtime (after deployment), what to log and where! No need to restart the program or recyle the app pool!

* **Why should I use NLog?**
  - NLog is fully written in C#, has many years of experience and is easy to extend!

* **Is it free?**
  - It's licensed under the BSD license, so you can use it in commercial (closed sourse) programs without problems. 
  
* **Show me the magic!**
  - Check the [tutorial](https://github.com/NLog/NLog/wiki/Tutorial) to get started!

* **I can't see anything?!**
  - NLog not working as expected? Check the [troubleshooting guide](https://github.com/NLog/NLog/wiki/Logging-troubleshooting). If you think it's a bug, please check [contributing.md](https://github.com/NLog/NLog/blob/master/CONTRIBUTING.md#bug-reports]) and [create a GitHub issue](https://github.com/NLog/NLog/issues/new)!

* **I'm missing important stuff!**
  - You can send a feature request, but do you know you can [extend NLog with a few lines of code](http://nlog-project.org/2015/06/30/extending-nlog-is-easy.html)?

* **How do I upgrade to NLog 4.x?** 
  - Check the [4.0 release post](http://nlog-project.org/2015/06/09/nlog-4-has-been-released.html), there are some breaking changes.
  - Update all the NLog packages. The latest stable version is recommend. 
  - When upgrading from NLog 4.1.0, please the next question.

* **I have trouble updating NLog from 4.1.0**
  - We take [semver](https://semver.org) very serious! Because NLog is strong named, it's important to keep the assembly version of all major versions the same, otherwise every library build on 4.0.0 should be reompiled for every other 4.x release (4.1, 4.2 etc)  - which is unwanted because of semver. <br>
   In NLog 4.1.0 there was a mistake in the assembly version, which has been fixed in 4.1.1. Upgrading from NLog 4.1.0 to another version can give issues when using NuGet. This will result in the following error:
   
  > Could not load file or assembly 'NLog' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. (Exception from HRESULT: 0x80131040)

  If you upgrade, remove or alter the `<assemblybinding>`, as explained at the [4.1.1 news post](http://nlog-project.org/2015/09/12/nlog-4-1-1-has-been-released.html).    
  
* **Should I use Common Logging?**
   - That's up to you. It has it pros and cons. The greatest advantage is that you can easily switch between logging implementations (NLog, Log4Net, EntLib). This can be very important if you’re writing a library yourself, then the user who's using your library can choose which implementation to use.

  - There are some downsides: 

     - You are limited in some features, or some features aren't available at all (like context classes or event properties)
     - The performance is a bit lower.
     - The platform support is lower. For example, there is no Xamarin support or a specialized .Net 4.5 build
     - The progress is limited by NLog and Common logging. 
  
* **Which Common Logging version should I use?**
   - As you may have noticed the latest version of Common Logging doesn't match the latest version of NLog -  the latest Common Logging is build to NLog 4.1. But that is not a problem! Since NLog 4.0 the assembly version is fixed to `4.0.0.0` and because follow [semver](https://semver.org), you can use the latest version of NLog with [Common.Logging.NLog41](https://www.nuget.org/packages/Common.Logging.NLog41/). 
    
* **I'm writing a library who's using NLog. Should I update when NLog has an update?**
   - If you don't use the latest additions, then you should only update every NLog major version. As mentioned at the Common Logging version, we will keep the assembly version fixed. The end-user don't need `<assemblybinding>`-magic! So in short: your library should target NLog 4.0 and in the future NLog 5.0.


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


