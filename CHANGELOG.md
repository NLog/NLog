See also [releases](https://github.com/NLog/NLog/releases) and [milestones](https://github.com/NLog/NLog/milestones).

This file is new since 4.3.4. If requested, we will try to add the older releases in this file.

### V4.3.4 (2016/05/16)

#### Features
- [#1423](https://github.com/nlog/nlog/pull/1423) Injection of zip-compressor for fileTarget (@AndreGleichner)
- [#1434](https://github.com/nlog/nlog/pull/1434) Added constructors with name argument to the target types (@304NotModified, @flyingcroissant)
- [#1400](https://github.com/nlog/nlog/pull/1400) Added WrapLineLayoutRendererWrapper (@mathieubrun)

#### Improvements
- [#1456](https://github.com/nlog/nlog/pull/1456) FileTarget: Improvements in FileTarget archive cleanup. (@bhaeussermann)
- [#1417](https://github.com/nlog/nlog/pull/1417) FileTarget prevent stackoverflow after setting FileName property on init (@304NotModified)

#### Fixes
- [#1454](https://github.com/nlog/nlog/pull/1454) Fix LoggingRule.ToString (@304NotModified)
- [#1453](https://github.com/nlog/nlog/pull/1453) Fix potential nullref exception in LogManager.Shutdown() (@304NotModified)
- [#1450](https://github.com/nlog/nlog/pull/1450) Fix duplicate Target after config Initialize (@304NotModified)
- [#1446](https://github.com/nlog/nlog/pull/1446) FileTarget: create dir if CreateDirs=true and replacing file content (@304NotModified)
- [#1432](https://github.com/nlog/nlog/pull/1432) Check if directory NLog.dll is detected in actually exists (@gregmac)

#### Other
- [#1440](https://github.com/nlog/nlog/pull/1440) Added extra unit tests for context classes (@304NotModified)
