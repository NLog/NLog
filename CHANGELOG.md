See also [releases](https://github.com/NLog/NLog/releases) and [milestones](https://github.com/NLog/NLog/milestones).

This file is new since 4.3.4. If requested, we will try to add the older releases in this file.


### V4.4  (2016/12/13)

- [#1583](https://github.com/nlog/nlog/pull/1583) Don't stop logging when there is an invalid layoutrenderer in the layout. (@304NotModified)
- [#1740](https://github.com/nlog/nlog/pull/1740) WebServiceTarget support for JSON & Injecting JSON serializer into NLog (#1740) (@tetrodoxin)
- [#1754](https://github.com/nlog/nlog/pull/1754) JsonLayout: JsonLayout: add includeAllProperties & excludeProperties  (@aireq)
- [#1439](https://github.com/nlog/nlog/pull/1439) Allow comma separated values (List) for Layout Renderers in nlog.config (@304NotModified)
- [#1782](https://github.com/nlog/nlog/pull/1782) Improvement on #1439: Support Generic (I)List and (I)Set for Target/Layout/Layout renderers properties in nlog.config (@304NotModified)
- [#1769](https://github.com/nlog/nlog/pull/1769) Optionally keeping variables during configuration reload (@nazim9214)
- [#1514](https://github.com/nlog/nlog/pull/1514) Add LimitingTargetWrapper (#1514) (@Jeinhaus)
- [#1581](https://github.com/nlog/nlog/pull/1581) Registering Layout renderers with func (one line needed), easier registering layout/layoutrender/targets (@304NotModified)
- [#1735](https://github.com/nlog/nlog/pull/1735) UrlHelper - Added standard support for UTF8 encoding, added support for RFC2396  &  RFC3986 (#1735) (@snakefoot)
- [#1768](https://github.com/nlog/nlog/pull/1768) ExceptionLayoutRenderer - Added support for AggregateException (@snakefoot)
- [#1752](https://github.com/nlog/nlog/pull/1752) Layout processinfo with support for custom Format-string (@snakefoot)
- [#1836](https://github.com/nlog/nlog/pull/1836) Callsite: add includeNamespace option (@304NotModified)
- [#1817](https://github.com/nlog/nlog/pull/1817) Added condition to AutoFlushWrappper (@nazim9214)

#### Improvements

- [#1732](https://github.com/nlog/nlog/pull/1732) Handle duplicate attributes (error or using first occurence) in nlog.config (@nazim9214)
- [#1778](https://github.com/nlog/nlog/pull/1778) ConsoleTarget - DetectConsoleAvailable - Disabled by default (@snakefoot)
- [#1585](https://github.com/nlog/nlog/pull/1585) More clear internallog when reading XML config (@304NotModified)
- [#1784](https://github.com/nlog/nlog/pull/1784) ProcessInfoLayoutRenderer - Applied usage of LateBoundMethod (@snakefoot)
- [#1771](https://github.com/nlog/nlog/pull/1771) FileTarget - Added extra archive check is needed, after closing stale file handles (@snakefoot)
- [#1779](https://github.com/nlog/nlog/pull/1779) Improve performance of filters (2-3 x faster) (@snakefoot)
- [#1780](https://github.com/nlog/nlog/pull/1780) PropertiesLayoutRenderer - small performance improvement (@snakefoot)
- [#1776](https://github.com/nlog/nlog/pull/1776) Don't crash on an invalid (xml) app.config by default (@304NotModified)
- [#1763](https://github.com/nlog/nlog/pull/1763) JsonLayout - Performance improvements (@snakefoot)
- [#1755](https://github.com/nlog/nlog/pull/1755) General performance improvement (@snakefoot)
- [#1756](https://github.com/nlog/nlog/pull/1755) WindowsMultiProcessFileAppender (@snakefoot, @AndreGleichner)

### v4.3.11 (2016/11/07)

#### Improvements

- [#1700](https://github.com/nlog/nlog/pull/1700) Improved concurrency when multiple Logger threads are writing to async Target (@snakefoot)
- [#1750](https://github.com/nlog/nlog/pull/1750) Log payload for NLogViewerTarget/NetworkTarget to Internal Logger (@304NotModified)
- [#1745](https://github.com/nlog/nlog/pull/1745) FilePathLayout - Reduce memory-allocation for cleanup of filename (@snakefoot)
- [#1746](https://github.com/nlog/nlog/pull/1746) DateLayout - Reduce memory allocation when low time resolution (@snakefoot)
- [#1719](https://github.com/nlog/nlog/pull/1719) Avoid (Internal)Logger-boxing and params-array-allocation on Exception (@snakefoot)
- [#1683](https://github.com/nlog/nlog/pull/1683) FileTarget - Faster async processing of LogEvents for the same file (@snakefoot)
- [#1730](https://github.com/nlog/nlog/pull/1730) Conditions: Try interpreting first as non-string value (@304NotModified)
- [#1814](https://github.com/nlog/nlog/pull/1814) Improve [Obsolete] warnings - include the Nlog version when it became obsolete (#1814) (@ie-zero)
- [#1809](https://github.com/nlog/nlog/pull/1809) FileTarget - Close stale file handles outside archive mutex lock (@snakefoot)

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
