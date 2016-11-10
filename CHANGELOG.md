See also [releases](https://github.com/NLog/NLog/releases) and [milestones](https://github.com/NLog/NLog/milestones).

This file is new since 4.3.4. If requested, we will try to add the older releases in this file.


### v4.3.11 (2016/11/07)

#### Improvements

- [#1700](https://github.com/nlog/nlog/pull/1700) Improved concurrency when multiple Logger threads are writing to async Target (@snakefoot)
- [#1750](https://github.com/nlog/nlog/pull/1750) Log payload for NLogViewerTarget/NetworkTarget to Internal Logger (@304NotModified)
- [#1745](https://github.com/nlog/nlog/pull/1745) FilePathLayout - Reduce memory-allocation for cleanup of filename (@snakefoot)
- [#1746](https://github.com/nlog/nlog/pull/1746) DateLayout - Reduce memory allocation when low time resolution (@snakefoot)
- [#1719](https://github.com/nlog/nlog/pull/1719) Avoid (Internal)Logger-boxing and params-array-allocation on Exception (@snakefoot)
- [#1683](https://github.com/nlog/nlog/pull/1683) FileTarget - Faster async processing of LogEvents for the same file (@snakefoot)
- [#1730](https://github.com/nlog/nlog/pull/1730) Conditions: Try interpreting first as non-string value (@304NotModified)

#### Fixes

- [#1749](https://github.com/nlog/nlog/pull/1749) Try-catch for permission when autoloading - fixing Android permission issue (@304NotModified)
- [#1751](https://github.com/nlog/nlog/pull/1751) ExceptionLayoutRenderer: prevent nullrefexception when exception is null (@304NotModified)
- [#1706](https://github.com/nlog/nlog/pull/1706) Console Target Automatic Detect if console is available on Mono (@snakefoot)



### v4.3.10 (2016/10/11)

#### Features
- [#1680](https://github.com/nlog/nlog/pull/1680) Append to existing archive file (@304NotModified)     
- [#1669](https://github.com/nlog/nlog/pull/1669) AsyncTargetWrapper - Allow TimeToSleepBetweenBatches = 0 (@snakefoot)
- [#1668](https://github.com/nlog/nlog/pull/1668) Console Target Automatic Detect if console is available (@snakefoot)


#### Improvements

- [#1697](https://github.com/nlog/nlog/pull/1697) Archiving should never fail writing (@304NotModified)
- [#1695](https://github.com/nlog/nlog/pull/1695) Performance: Counter/ProcessId/ThreadId-LayoutRenderer allocations less memory (@snakefoot)
- [#1693](https://github.com/nlog/nlog/pull/1693) Performance (allocation) improvement in Aysnc handling (@snakefoot)
- [#1694](https://github.com/nlog/nlog/pull/1694) FilePathLayout - CleanupInvalidFilePath - Happy path should not allocate (@snakefoot)
- [#1675](https://github.com/nlog/nlog/pull/1675) unseal databasetarget and make BuildConnectionString protected (@304NotModified)
- [#1690](https://github.com/nlog/nlog/pull/1690) Fix memory leak in AppDomainWrapper (@snakefoot)
- [#1702](https://github.com/nlog/nlog/pull/1702) Performance: InternalLogger should only allocate params-array when needed (@snakefoot)


#### Fixes
- [#1676](https://github.com/nlog/nlog/pull/1676) Fix FileTarget on Xamarin: Remove mutex usage for Xamarin 'cause of runtime exceptions (@304NotModified)
- [#1591](https://github.com/nlog/nlog/pull/1591) Count operation on AsyncRequestQueue is not thread-safe (@snakefoot)

### v4.3.9 (2016/09/18)

#### Features

- [#1641](https://github.com/nlog/nlog/pull/1641) FileTarget: Add WriteFooterOnArchivingOnly parameter. (@bhaeussermann)  
- [#1628](https://github.com/nlog/nlog/pull/1628) Add ExceptionDataSeparator option for ${exception} (@FroggieFrog)
- [#1626](https://github.com/nlog/nlog/pull/1626) cachekey option for cache layout wrapper (@304NotModified) 

#### Improvements 

- [#1643](https://github.com/nlog/nlog/pull/1643) Pause logging when the race condition occurs in (Colored)Console Target (@304NotModified)
- [#1632](https://github.com/nlog/nlog/pull/1632) Prevent possible crash when archiving in folder with non-archived files (@304NotModified)

#### Fixes

- [#1646](https://github.com/nlog/nlog/pull/1646) FileTarget: Fix file archive race-condition. (@bhaeussermann)
- [#1642](https://github.com/nlog/nlog/pull/1642) MDLC: fixing mutable dictionary issue (improvement) (@vlardn)
- [#1635](https://github.com/nlog/nlog/pull/1635) Fix ${tempdir} and ${nlogdir} if both have dir and file. (@304NotModified)


### V4.3.8 (2016/09/05)

#### Features
- [#1619](https://github.com/NLog/NLog/pull/1619) NetworkTarget: Added option to specify EOL (@kevindaub)

#### Improvements    
- [#1596](https://github.com/NLog/NLog/pull/1596) Performance tweak in NLog routing (@304NotModified)
- [#1593](https://github.com/NLog/NLog/pull/1593) FileTarget: large performance improvement - back to 1 million/sec (@304NotModified)
- [#1621](https://github.com/nlog/nlog/pull/1621) FileTarget: writing to non-existing drive was slowing down NLog a lot (@304NotModified)

#### Fixes
- [#1616](https://github.com/nlog/nlog/pull/1616) FileTarget: Don't throw an exception if a dir is missing when deleting old files on startup (@304NotModified)

### V4.3.7 (2016/08/06)

#### Features
- [#1469](https://github.com/nlog/nlog/pull/1469) Allow overwriting possible nlog configuration file paths (@304NotModified)
- [#1578](https://github.com/nlog/nlog/pull/1578) Add support for name parameter on ${Assembly-version} (@304NotModified)
- [#1580](https://github.com/nlog/nlog/pull/1580) Added option to not render empty literals on nested json objects (@johnkors)

#### Improvements
- [#1558](https://github.com/nlog/nlog/pull/1558) Callsite layout renderer: improve string comparison test (performance) (@304NotModified)
- [#1582](https://github.com/nlog/nlog/pull/1582) FileTarget: Performance improvement for CleanupInvalidFileNameChars  (@304NotModified)

#### Fixes
- [#1556](https://github.com/nlog/nlog/pull/1556) Bugfix: Use the culture when rendering the layout (@304NotModified)
 

### V4.3.6 (2016/07/24)

#### Features
- [#1531](https://github.com/nlog/nlog/pull/1531) Support Android 4.4 (@304NotModified)
- [#1551](https://github.com/nlog/nlog/pull/1551) Addded CompoundLayout (@luigiberrettini)

#### Fixes
- [#1548](https://github.com/nlog/nlog/pull/1548) Bugfix: Can't update EventLog's Source property (@304NotModified, @Page-Not-Found)
- [#1553](https://github.com/nlog/nlog/pull/1553) Bugfix: Throw configException when registering invalid extension assembly/type. (@304NotModified, @Jeinhaus)
- [#1547](https://github.com/nlog/nlog/pull/1547) LogReceiverWebServiceTarget is leaking communication channels (@MartinTherriault)


### V4.3.5 (2016/06/13)

#### Features
- [#1471](https://github.com/nlog/nlog/pull/1471) Add else option to ${when} (@304NotModified)
- [#1481](https://github.com/nlog/nlog/pull/1481) get items for diagnostic contexts (DiagnosticsContextes, GetNames() method) (@tiljanssen)

#### Fixes

- [#1504](https://github.com/nlog/nlog/pull/1504) Fix ${callsite} with async method with return value (@PELNZ)

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
