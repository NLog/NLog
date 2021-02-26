See also [releases](https://github.com/NLog/NLog/releases) and [milestones](https://github.com/NLog/NLog/milestones).

Date format: (year/month/day)

## Change Log

### V4.7.8 (2021/02/25)

#### Bugfixes
- [#4316](https://github.com/NLog/NLog/pull/4316) Fix TrimDirectorySeparators to handle root-path on Windows and Linux to load Nlog.config from root-path (#4316) (@snakefoot)

#### Improvements
- [#4273](https://github.com/NLog/NLog/pull/4273) Handle Breaking change with string.IndexOf(string) in .NET 5 (#4273) (@snakefoot)
- [#4301](https://github.com/NLog/NLog/pull/4301) Update docs, remove ArrayList in docs (#4301) (@304NotModified)

### V4.7.7 (2021/01/20)

#### Bugfixes
- [#4229](https://github.com/NLog/NLog/pull/4229) Skip lookup MainModule.FileName on Android platform to avoid crash  (#4229) (@snakefoot)
- [#4202](https://github.com/NLog/NLog/pull/4202) JsonLayout - Generate correct json for keys that contain quote (#4202) (@virgilp)
- [#4245](https://github.com/NLog/NLog/pull/4245) JsonLayout - Unwind after invalid property value to avoid invalid Json (#4245) (@snakefoot)

#### Improvements
- [#4222](https://github.com/NLog/NLog/pull/4222) Better handling of low memory (#4222) (@snakefoot)
- [#4221](https://github.com/NLog/NLog/pull/4221) JsonLayout - Added new ExcludeEmptyProperties to skip GDC/MDC/MLDC properties with null or empty values (#4221) (@pruiz)

#### Performance
- [#4207](https://github.com/NLog/NLog/pull/4207) Skip allocation of SingleCallContinuation when ThrowExceptions = false (#4207) (@snakefoot)

### V4.7.6 (2020/12/06)

#### Bugfixes
- [#4142](https://github.com/NLog/NLog/pull/4142) JsonSerializer - Ensure invariant formatting of DateTimeOffset (#4142) (@snakefoot)
- [#4172](https://github.com/NLog/NLog/pull/4172) AsyncTaskTarget - Flush when buffer is full should not block forever (#4172) (@snakefoot)
- [#4182](https://github.com/NLog/NLog/pull/4182) Failing to lookup ProcessName because of Access Denied should fallback to Win32-API (#4182) (@snakefoot)

#### Features
- [#4153](https://github.com/NLog/NLog/pull/4153) ExceptionLayoutRenderer - Added FlattenException option (#4153) (@snakefoot)

#### Improvements
- [#4141](https://github.com/NLog/NLog/pull/4141) NetworkTarget - Improve handling of synchronous exceptions from UdpClient.SendAsync (#4141) (@snakefoot)
- [#4176](https://github.com/NLog/NLog/pull/4176) AsyncTaskTarget - Include TaskScheduler in ContinueWith (#4176) (@snakefoot)
- [#4190](https://github.com/NLog/NLog/pull/4190) Improving debugger-display for Logger.Properties and LogEventInfo.Properties (@snakefoot)

#### Performance
- [#4132](https://github.com/NLog/NLog/pull/4132) Improve thread concurrency when using wrapper cached=true (#4132) (@snakefoot)
- [#4171](https://github.com/NLog/NLog/pull/4171) ConditionLayoutExpression - Skip allocating StringBuilder for every condition check (#4171) (@snakefoot)

### V4.7.5 (2020/09/27)

#### Bugfixes
- [#4106](https://github.com/NLog/NLog/pull/4106) FileTarget - New current file should write start header, when archive operation is performed on previous file (#4106) (@snakefoot)

#### Improvements
- [#4102](https://github.com/NLog/NLog/pull/4102) LoggingConfiguration - ValidateConfig should only throw when enabled (#4102) (@snakefoot)
- [#4114](https://github.com/NLog/NLog/pull/4114) Fix VerificationException Operation could destabilize the runtime (#4114) (@snakefoot)

#### Performance
- [#4115](https://github.com/NLog/NLog/pull/4115) Removed EmptyDefaultDictionary from MappedDiagnosticsContext (#4115) (@snakefoot)

#### Other
- [#4109](https://github.com/NLog/NLog/pull/4109) Fix root .editorconfig to use end_of_line = CRLF. Remove local .editorconfig (#4109) (@snakefoot)
- [#4097](https://github.com/NLog/NLog/pull/4097) Improve docs (#4097) (@304NotModified)

### V4.7.4 (2020/08/22)

#### Features
- [#4076](https://github.com/NLog/NLog/pull/4076) DatabaseTarget - Added AllowDbNull for easier support for nullable parameters (#4076) (@snakefoot)

#### Bugfixes
- [#4069](https://github.com/NLog/NLog/pull/4069) Fluent LogBuilder should suppress exception on invalid callerFilePath (#4069) (@snakefoot)

#### Improvements
- [#4073](https://github.com/NLog/NLog/pull/4073) FileTarget - Extra validation of the LogEvent-timestamp before checking time to archive (#4073) (@snakefoot)
- [#4068](https://github.com/NLog/NLog/pull/4068) FileTarget - Improve diagnostic logging to see reason for archiving (@snakefoot)

### V4.7.3 (2020/07/31)

#### Features
- [#4017](https://github.com/NLog/NLog/pull/4017) Allow to change the RuleName of a LoggingRule (#4017) (@304NotModified)
- [#3974](https://github.com/NLog/NLog/pull/3974) logging of AggregrateException.Data to prevent it from losing it after Flatten call (#3974) (@chaos0307)

#### Bugfixes
- [#4011](https://github.com/NLog/NLog/pull/4011) LocalIpAddressLayoutRenderer - IsDnsEligible and PrefixOrigin throws PlatformNotSupportedException on Linux (#4011) (@snakefoot)

#### Improvements
- [#4057](https://github.com/NLog/NLog/pull/4057) ObjectReflectionCache - Reduce noise from properties that throws exceptions like Stream.ReadTimeout (#4057) (@snakefoot)
- [#4053](https://github.com/NLog/NLog/pull/4053) MessageTemplates - Changed Literal.Skip to be Int32 to support message templates longer than short.MaxValue (#4053) (@snakefoot)
- [#4043](https://github.com/NLog/NLog/pull/4043) ObjectReflectionCache - Skip reflection for Stream objects (#4043) (@snakefoot)
- [#3999](https://github.com/NLog/NLog/pull/3999) LogFactory Shutdown is public so it can be used from NLogLoggerProvider (#3999) (@snakefoot)
- [#3972](https://github.com/NLog/NLog/pull/3972) Editor config with File header template (#3972) (@304NotModified)

#### Performance
- [#4058](https://github.com/NLog/NLog/pull/4058) FileTarget - Skip delegate capture in GetFileCreationTimeSource. Fallback only necessary when appender has been closed. (#4058) (@snakefoot)
- [#4021](https://github.com/NLog/NLog/pull/4021) ObjectReflectionCache - Reduce initial memory allocation until needed (#4021) (@snakefoot)
- [#3977](https://github.com/NLog/NLog/pull/3977) FilteringTargetWrapper - Remove delegate allocation (#3977) (@snakefoot)

### V4.7.2 (2020/05/18)

#### Bugfixes
- [#3969](https://github.com/NLog/NLog/pull/3969) FileTarget - ArchiveOldFileOnStartup not working together with ArchiveAboveSize (@snakefoot)

#### Improvements
- [#3962](https://github.com/NLog/NLog/pull/3962) XSD: Added Enabled attribute for <logger> (@304NotModified)

### V4.7.1 (2020/05/15)

#### Features
- [#3871](https://github.com/NLog/NLog/pull/3871) LogManager.Setup().LoadConfigurationFromFile("NLog.config") added to fluent setup (@snakefoot + @304NotModified)
- [#3909](https://github.com/NLog/NLog/pull/3909) LogManager.Setup().GetCurrentClassLogger() added to fluent setup (@snakefoot + @304NotModified)
- [#3861](https://github.com/NLog/NLog/pull/3861) LogManager.Setup().SetupInternalLogger(s => s.AddLogSubscription()) added to fluent setup (@snakefoot + @304NotModified)
- [#3891](https://github.com/NLog/NLog/pull/3891) ColoredConsoleTarget - Added Condition option to control when to do word-highlight (@snakefoot)
- [#3915](https://github.com/NLog/NLog/pull/3915) Added new option CaptureStackTrace to ${stacktrace} and ${callsite} to skip implicit capture by Logger (@snakefoot)
- [#3940](https://github.com/NLog/NLog/pull/3940) Exception-LayoutRenderer with new option BaseException for rendering innermost exception (@snakefoot)

#### Improvements
- [#3857](https://github.com/NLog/NLog/pull/3857) FileTarget - Batch write to filestream in max chunksize of 100 times BufferSize (@snakefoot)
- [#3867](https://github.com/NLog/NLog/pull/3867) InternalLogger include whether NLog comes from GlobalAssemblyCache when logging NLog version (@snakefoot)
- [#3862](https://github.com/NLog/NLog/pull/3862) InternalLogger LogToFile now support ${processdir} to improve support for single-file-publish (@snakefoot)
- [#3877](https://github.com/NLog/NLog/pull/3877) Added ${processdir} that matches ${basedir:processDir=true} for consistency with InternalLogger (@snakefoot)
- [#3881](https://github.com/NLog/NLog/pull/3881) Object property reflection now support IReadOnlyDictionary as expando object (@snakefoot)
- [#3884](https://github.com/NLog/NLog/pull/3884) InternalLogger include more details like FilePath when loading NLog.config file (@snakefoot)
- [#3895](https://github.com/NLog/NLog/pull/3895) Added support for Nullable when doing conversion of property types (@304NotModified)
- [#3896](https://github.com/NLog/NLog/pull/3896) InternalLogger Warnings when skipping unrecognized LayoutRenderer options (@snakefoot)
- [#3901](https://github.com/NLog/NLog/pull/3901) MappedDiagnosticsLogicalContext - SetScoped with IReadOnlyList also for Xamarin (@snakefoot)
- [#3904](https://github.com/NLog/NLog/pull/3904) Object property reflection automatically performs ToString for System.Reflection.Module (@snakefoot)
- [#3921](https://github.com/NLog/NLog/pull/3921) Improve ${basedir:fixtempdir=true} to work better on Linux when TMPDIR not set (@snakefoot)
- [#3928](https://github.com/NLog/NLog/pull/3928) NetworkTarget respect MaxQueueSize for http- / https-protocol (@snakefoot)
- [#3930](https://github.com/NLog/NLog/pull/3930) LogFactory.LoadConfiguration() reports searched paths when throwing FileNotFoundException (@304NotModified)
- [#3949](https://github.com/NLog/NLog/pull/3949) ReplaceNewLines-LayoutRenderer will also remove windows newlines on the Linux platform (@Silvenga)

#### Bugfixes
- [#3868](https://github.com/NLog/NLog/pull/3868) SplitGroup Target Wrapper should not skip remaining targets when single target fails (@snakefoot)
- [#3918](https://github.com/NLog/NLog/pull/3918) ColoredConsoleTarget - Fix bug in handling of newlines without word-highlight (@snakefoot)
- [#3941](https://github.com/NLog/NLog/pull/3941) ${processid} will no longer fail because unable to lookup ${processdir} on Mono Android (@snakefoot)

#### Performance
- [#3855](https://github.com/NLog/NLog/pull/3855) FileTarget - Skip checking file-length when only using ArchiveEvery (@snakefoot)
- [#3898](https://github.com/NLog/NLog/pull/3898) ObjectGraphScanner performs caching of property reflection for NLog config items (@snakefoot)
- [#3894](https://github.com/NLog/NLog/pull/3894) Condition expressions now handles operator like '==' without memory boxing (@snakefoot)
- [#3903](https://github.com/NLog/NLog/pull/3903) ObjectGraphScanner should only check for layout-attributes on Layouts and LayoutRenderers (@snakefoot)
- [#3902](https://github.com/NLog/NLog/pull/3902) MessageTemplate parsing of properties skips unnecessary array allocation (@snakefoot)


### V4.7 (2020/03/20)

#### Features
- [#3686](https://github.com/NLog/NLog/pull/3686) + [#3740](https://github.com/NLog/NLog/pull/3740) LogManager.Setup() allows fluent configuration of LogFactory options (@snakefoot + @304NotModified)
- [#3610](https://github.com/NLog/NLog/pull/3610) LogManager.Setup().SetupSerialization(s => s.RegisterObjectTransformation(...)) for overriding default property reflection (@snakefoot + @304NotModified + @Giorgi + @mmurrell)
- [#3787](https://github.com/NLog/NLog/pull/3787) LogManager.Setup().SetupExtensions(s => s.RegisterConditionMethod(...)) can use lambda methods and not just static methods (@snakefoot)
- [#3713](https://github.com/NLog/NLog/pull/3713) ${level:format=FullName} will expand Info + Warn to their full name (@snakefoot)
- [#3714](https://github.com/NLog/NLog/pull/3714) + [#3734](https://github.com/NLog/NLog/pull/3734) FileTarget - Supports MaxArchiveDays for cleanup of old files based on their age (@snakefoot)
- [#3737](https://github.com/NLog/NLog/pull/3737) + [#3769](https://github.com/NLog/NLog/pull/3769) Layout.FromMethod to create Layout directly from a lambda method (@snakefoot)
- [#3771](https://github.com/NLog/NLog/pull/3771) Layout.FromString to create Layout directly from string along with optional parser validation (@snakefoot)
- [#3793](https://github.com/NLog/NLog/pull/3793) ${dir-separator} for rendering platform specific directory path separator (@304NotModified)
- [#3755](https://github.com/NLog/NLog/pull/3755) FileTarget - Supports ArchiveOldFileOnStartupAboveSize for cleanup of existing file when above size (@Sam13)
- [#3796](https://github.com/NLog/NLog/pull/3796) + [#3823](https://github.com/NLog/NLog/pull/3823) InternalLogger - Added LogMessageReceived event (@304NotModified + @snakefoot)
- [#3829](https://github.com/NLog/NLog/pull/3829) DatabaseTarget - Assign connection properties like SqlConnection.AccessToken (@304NotModified + @snakefoot)
- [#3839](https://github.com/NLog/NLog/pull/3839) DatabaseTarget - Assign command properties like SqlCommand.CommandTimeout (@snakefoot)
- [#3833](https://github.com/NLog/NLog/pull/3833) ${onHasProperties} for only rendering when logevent includes properties from structured logging (@snakefoot)

#### Improvements
- [#3521](https://github.com/NLog/NLog/pull/3521) XmlLoggingConfiguration - Marked legacy constructors with ignoreErrors parameter as obsolete (@snakefoot)
- [#3689](https://github.com/NLog/NLog/pull/3689) LoggingConfiguration - Perform checking of unused targets during initialization for better validation (@snakefoot)
- [#3704](https://github.com/NLog/NLog/pull/3704) EventLogTarget - Improve diagnostics logging when using dynamic EventLog source (@snakefoot)
- [#3706](https://github.com/NLog/NLog/pull/3706) ${longdate} now also supports raw value for use as DatabaseTarget parameter with DbType (@snakefoot)
- [#3728](https://github.com/NLog/NLog/pull/3728) SourceLink for GitHub for easy debugging into the NLog source code (@304NotModified)
- [#3743](https://github.com/NLog/NLog/pull/3743) JsonLayout - EscapeForwardSlash now automatically applies to sub-attributes (@snakefoot)
- [#3742](https://github.com/NLog/NLog/pull/3742) TraceTarget - Introduced EnableTraceFail=false to avoid Environment.FailFast (@snakefoot)
- [#3750](https://github.com/NLog/NLog/pull/3750) ExceptionLayoutRenderer - Improved error message when Format-token parsing fails (@snakefoot)
- [#3747](https://github.com/NLog/NLog/pull/3747) AutoFlushWrapper - Set AutoFlush=false for AsyncTaskTarget by default (@snakefoot)
- [#3754](https://github.com/NLog/NLog/pull/3754) LocalIpAddressLayoutRenderer - Higher priority to network-addresses that has valid gateway adddress (@snakefoot)
- [#3762](https://github.com/NLog/NLog/pull/3762) LogFactory - Flush reports to InternalLogger what targets produces timeouts (@snakefoot)

#### Bugfixes
- [#3758](https://github.com/NLog/NLog/pull/3758) LogFactory - Fix deadlock issue with AutoReload (@snakefoot)
- [#3766](https://github.com/NLog/NLog/pull/3766) JsonLayout - Fixed ThreadAgnostic to correctly capture context when using nested JsonLayout (@snakefoot)
- [#3700](https://github.com/NLog/NLog/pull/3700) ExceptionLayoutRenderer - Fixed so Format option HResult also works for NetCore (@snakefoot)
- [#3761](https://github.com/NLog/NLog/pull/3761) + [#3784](https://github.com/NLog/NLog/pull/3784) Log4JXml Layout will render NDLC + NDC scopes in correct order (@adanek + @304NotModified)
- [#3821](https://github.com/NLog/NLog/pull/3821) Logger - Added exception handler for CallSite capture for platform that fails to capture StackTrace (@snakefoot)
- [#3835](https://github.com/NLog/NLog/pull/3835) StringSplitter - Fixed quote handling when reading elements for config list-properties (@snakefoot)
- [#3828](https://github.com/NLog/NLog/pull/3828) Utilities: fix ConversionHelpers.TryParseEnum for white space (@304NotModified)

#### Performance
- [#3683](https://github.com/NLog/NLog/pull/3683) ObjectGraphScanner - Avoid holding list.SyncRoot lock while scanning (@snakefoot)
- [#3691](https://github.com/NLog/NLog/pull/3691) FileTarget - ConcurrentWrites=true on NetCore now much faster when archive enabled (@snakefoot)
- [#3694](https://github.com/NLog/NLog/pull/3694) + [#3705](https://github.com/NLog/NLog/pull/3705) JsonConverter - Write DateTime directly without string allocation (@snakefoot)
- [#3692](https://github.com/NLog/NLog/pull/3692) XmlLayout - Removed unnecessary double conversion to string (@snakefoot)
- [#3735](https://github.com/NLog/NLog/pull/3735) WebServiceTarget - Reduced memory allocations by removing unnecessary delegate capture (@snakefoot)
- [#3739](https://github.com/NLog/NLog/pull/3739) NetworkTarget  - Reduced memory allocation for encoding into bytes without string allocation (@snakefoot)
- [#3748](https://github.com/NLog/NLog/pull/3748) AsyncTaskTarget - Skip default AsyncWrapper since already having internal queue (@snakefoot)
- [#3767](https://github.com/NLog/NLog/pull/3767) Mark Condition Expressions as ThreadSafe to improve concurrency in Layouts (@snakefoot)
- [#3764](https://github.com/NLog/NLog/pull/3764) DatabaseTarget - Added IsolationLevel option that activates transactions for better batching performance (@snakefoot)
- [#3779](https://github.com/NLog/NLog/pull/3779) SimpleLayout - Assignment of string-reference with null-value will translate into FixedText (@304NotModified)
- [#3790](https://github.com/NLog/NLog/pull/3790) AsyncWrapper - Less aggressive with scheduling timer events for background writing (@snakefoot)
- [#3830](https://github.com/NLog/NLog/pull/3830) Faster assignment of properties accessed through reflection (@304NotModified)
- [#3832](https://github.com/NLog/NLog/pull/3832) ${replace} - Faster search and replace when not explicitly have requested regex support (@snakefoot)
- [#3828](https://github.com/NLog/NLog/pull/3828) Skip need for Activator.CreateInstance in DbTypeSetter (@304NotModified)

### V4.6.8 (2019/11/04)

#### Bugfixes
- [#3566](https://github.com/NLog/NLog/pull/3566) DatabaseTarget - Auto escape special chars in password, and improve handling of empty username/password (@304NotModified)
- [#3584](https://github.com/NLog/NLog/pull/3584) LoggingRule - Fixed IndexOutOfRangeException for SetLoggingLevels with LogLevel.Off (@snakefoot)
- [#3609](https://github.com/NLog/NLog/pull/3609) FileTarget - Improved handling of relative path in ArchiveFileName (@snakefoot)
- [#3631](https://github.com/NLog/NLog/pull/3631) ExceptionLayoutRenderer - Fixed missing separator when Format-value gives empty result (@brinko99)
- [#3647](https://github.com/NLog/NLog/pull/3647) ${substring} - Length should not be mandatory (@304NotModified)
- [#3653](https://github.com/NLog/NLog/pull/3653) SimpleLayout - Fixed NullReferenceException in PreCalculate during TryGetRawValue optimization (@snakefoot)

#### Features
- [#3578](https://github.com/NLog/NLog/pull/3578) LogFactory - AutoShutdown can be configured to unhook from AppDomain-Unload, and avoid premature shutdown with IHostBuilder (@snakefoot)
- [#3579](https://github.com/NLog/NLog/pull/3579) PerformanceCounterLayoutRenderer - Added Layout-support for Instance-property (@snakefoot)
- [#3583](https://github.com/NLog/NLog/pull/3583) ${local-ip} Layout Renderer for local machine ip-address (@snakefoot + @304NotModified)
- [#3583](https://github.com/NLog/NLog/pull/3583) CachedLayoutRendererWrapper - Added CachedSeconds as ambient property. Ex. ${local-ip:cachedSeconds=60} (@snakefoot)
- [#3586](https://github.com/NLog/NLog/pull/3586) JsonLayout - Added EscapeForwardSlash-option to skip Json-escape of forward slash (@304NotModified)
- [#3593](https://github.com/NLog/NLog/pull/3593) AllEventPropertiesLayoutRenderer - Added Exclude-option that specifies property-keys to skip (@snakefoot)
- [#3611](https://github.com/NLog/NLog/pull/3611) ${Exception} - Added new Format-option values HResult and Properties (@snakefoot)

#### Improvements
- [#3622](https://github.com/NLog/NLog/pull/3622) + [#3651](https://github.com/NLog/NLog/pull/3651) ConcurrentRequestQueue refactoring to reduce code complexity (@snakefoot)
- [#3636](https://github.com/NLog/NLog/pull/3636) AsyncTargetWrapper now fallback to clearing internal queue if flush fails to release blocked writer threads (@snakefoot)
- [#3641](https://github.com/NLog/NLog/pull/3641) ${CallSite} - Small improvements for recognizing async callsite cases (@snakefoot)
- [#3642](https://github.com/NLog/NLog/pull/3642) LogManager.GetCurrentClassLogger - Improved capture of Logger name when called within lambda_method (@snakefoot)
- [#3649](https://github.com/NLog/NLog/pull/3649) ${BaseDir:FixTempDir=true} fallback to process directory for .NET Core 3 Single File Publish (@snakefoot)
- [#3649](https://github.com/NLog/NLog/pull/3649) Auto-loading NLog configuration from process.exe.nlog will priotize process directory for .NET Core 3 Single File Publish (@snakefoot)
- [#3654](https://github.com/NLog/NLog/pull/3654) ObjectPathRendererWrapper minor refactorings (@snakefoot)
- [#3660](https://github.com/NLog/NLog/pull/3660) ObjectHandleSerializer.GetObjectData includes SerializationFormatter=true for use in MDLC + NDLC (@snakefoot)
- [#3662](https://github.com/NLog/NLog/pull/3662) FileTarget - Extra logging when FileName Layout renders empty string (@snakefoot)

#### Performance
- [#3618](https://github.com/NLog/NLog/pull/3618) LogFactory - Faster initial assembly reflection and config loading (@snakefoot)
- [#3635](https://github.com/NLog/NLog/pull/3635) ConsoleTarget - Added WriteBuffer option that allows batch writing to console-stream with reduced allocations (@snakefoot)
- [#3635](https://github.com/NLog/NLog/pull/3635) ConsoleTarget - Added global lock to prevent any threadsafety issue from unsafe console (@snakefoot)


### V4.6.7 (2019/08/25)

#### Features
- [#3531](https://github.com/NLog/NLog/pull/3531) Added ${object-path} / ${exception:objectpath=PropertyName}, for rendering a property of an object (e.g. an exception) (#3531) (@304NotModified)
- [#3560](https://github.com/NLog/NLog/pull/3560) WhenMethodFilter - Support dynamic filtering using lambda (#3560) (@snakefoot)
- [#3184](https://github.com/NLog/NLog/pull/3184) Added support for dynamic layout renderer in log level filters (e.g. minLevel, maxLevel)  (#3184) (@snakefoot)
- [#3558](https://github.com/NLog/NLog/pull/3558) ExceptionLayoutRenderer - Added Source as new format parameter. (@snakefoot)
- [#3523](https://github.com/NLog/NLog/pull/3523) ColoredConsoleTarget - Added DetectOutputRedirected to skip coloring on redirect (@snakefoot)

#### Improvements

- [#3541](https://github.com/NLog/NLog/pull/3541) MessageTemplateParameters - Improve validation of parameters when isPositional (#3541) (@snakefoot)
- [#3546](https://github.com/NLog/NLog/pull/3546) NetworkTarget - HttpNetworkSender no longer sends out-of-order (@snakefoot)
- [#3522](https://github.com/NLog/NLog/pull/3522) NetworkTarget - Fix InternalLogger.Trace to include Target name (#3522) (@snakefoot)
- [#3562](https://github.com/NLog/NLog/pull/3562) XML config - Support ThrowConfigExceptions=true even when xml is invalid (@snakefoot)
- [#3532](https://github.com/NLog/NLog/pull/3532) Fix summary of NoRawValueLayoutRendererWrapper class (#3532) (@304NotModified)

#### Performance

- [#3540](https://github.com/NLog/NLog/pull/3540) MessageTemplateParameters - Skip object allocation when no parameters (@snakefoot)
- [#3527](https://github.com/NLog/NLog/pull/3527) XmlLayout - Defer allocation of ObjectReflectionCache until needed (#3527) (@snakefoot)


### V4.6.6 (2019/07/14)

#### Features

- [#3514](https://github.com/NLog/NLog/pull/3514) Added XmlLoggingConfiguration(XmlReader reader) ctor, improved docs and annotations (@dmitrychilli, @304NotModified)
- [#3513](https://github.com/NLog/NLog/pull/3513) AutoFlushTargetWrapper - Added FlushOnConditionOnly property (@snakefoot)

#### Performance

- [#3492](https://github.com/NLog/NLog/pull/3492) FileTarget - improvements when ConcurrentWrites=false (@snakefoot)

### V4.6.5 (2019/06/13)

#### Bugfixes

- [#3476](https://github.com/NLog/NLog/pull/3476) Fix broken XSD schema - NLog.Schema package (@snakefoot, @304NotModified)

#### Features

- [#3478](https://github.com/NLog/NLog/pull/3478) XSD: Support `<value>` in `<variable>` (@304NotModified)
- [#3477](https://github.com/NLog/NLog/pull/3477) ${AppSetting} - Added support for ConnectionStrings Lookup (@snakefoot)
- [#3469](https://github.com/NLog/NLog/pull/3469) LogLevel - Added support for TypeConverter (@snakefoot)
- [#3453](https://github.com/NLog/NLog/pull/3453) Added null terminator line ending for network target (@Kahath)
- [#3442](https://github.com/NLog/NLog/pull/3442) Log4JXmlEventLayout - Added IncludeCallSite + IncludeSourceInfo (@snakefoot)

#### Improvements

- [#3482](https://github.com/NLog/NLog/pull/3482) Fix typos in docs and comments (@304NotModified)

#### Performance

- [#3444](https://github.com/NLog/NLog/pull/3444)  RetryingMultiProcessFileAppender - better init BufferSize (@snakefoot)


### V4.6.4 (2019/05/28)

#### Bugfixes

- [#3392](https://github.com/NLog/NLog/pull/3392) NLog.Schema: Added missing defaultAction attribute on filters element in XSD (@304NotModified)
- [#3415](https://github.com/NLog/NLog/pull/3415) AsyncWrapper in Blocking Mode can cause deadlock (@snakefoot)

#### Features

- [#3430](https://github.com/NLog/NLog/pull/3430) Added "Properties" property on Logger for reading and editing properties.(@snakefoot, @304NotModified)
- [#3423](https://github.com/NLog/NLog/pull/3423) ${all-event-properties}: Added IncludeEmptyValues option (@304NotModified)
- [#3394](https://github.com/NLog/NLog/pull/3394) ${when}, support for non-string values (@304NotModified)
- [#3398](https://github.com/NLog/NLog/pull/3398) ${whenEmpty} support for non-string values (@snakefoot, @304NotModified)
- [#3391](https://github.com/NLog/NLog/pull/3391) Added ${environment-user} (@snakefoot)
- [#3389](https://github.com/NLog/NLog/pull/3389) Log4JXmlEventLayout - Added support for configuration of Parameters (@snakefoot)
- [#3411](https://github.com/NLog/NLog/pull/3411) LoggingConfigurationParser - Recognize LoggingRule.RuleName property (@snakefoot)

#### Improvements

- [#3393](https://github.com/NLog/NLog/pull/3393) Update package descriptions to note the issues with `<PackageReference>` (@304NotModified)
- [#3409](https://github.com/NLog/NLog/pull/3409) Various XSD improvements (NLog.Schema package) (@304NotModified)

#### Performance

- [#3398](https://github.com/NLog/NLog/pull/3398) ${whenEmpty} faster rendering of string values (@snakefoot, @304NotModified)
- [#3405](https://github.com/NLog/NLog/pull/3405) FilteringTargetWrapper: Add support for batch writing (@snakefoot, @304NotModified)
- [#3405](https://github.com/NLog/NLog/pull/3405) PostFilteringTargetWrapper: performance optimizations (@snakefoot, @304NotModified)
- [#3435](https://github.com/NLog/NLog/pull/3435) Async / buffering wrapper: Improve performance when blocking (@snakefoot)
- [#3434](https://github.com/NLog/NLog/pull/3434) ObjectReflectionCache - Skip property-reflection for IFormattable (@snakefoot)

### V4.6.3 (2019/04/30)

#### Bugfixes

- [#3345](https://github.com/NLog/NLog/pull/3345) Fixed potential memory issue and message duplication with large strings (@snakefoot)
- [#3316](https://github.com/NLog/NLog/pull/3316) TargetWithContext - serialize MDC and MDLC values properly (@304NotModified)

#### Features

- [#3298](https://github.com/NLog/NLog/pull/3298) Added WithProperty and SetProperty on Logger (@snakefoot)
- [#3329](https://github.com/NLog/NLog/pull/3329) ${EventProperties} - Added ObjectPath for rendering nested property (@snakefoot, @304NotModified)
- [#3337](https://github.com/NLog/NLog/pull/3337) ${ShortDate} added support for IRawValue + IStringValueRenderer (@snakefoot)
- [#3328](https://github.com/NLog/NLog/pull/3328) Added truncate ambient property, e.g. ${message:truncate=80} (@snakefoot)
- [#3278](https://github.com/NLog/NLog/pull/3278) ConsoleTarget & ColoredConsoleTarget  - Added AutoFlush and improve default flush behavior (@snakefoot)

#### Improvements

- [#3322](https://github.com/NLog/NLog/pull/3322) FileTarget - Introduced EnableFileDeleteSimpleMonitor without FileSystemWatcher for non-Windows (@snakefoot)
- [#3332](https://github.com/NLog/NLog/pull/3332) LogFactory - GetLogger should validate name of logger (@snakefoot)
- [#3320](https://github.com/NLog/NLog/pull/3320) FallbackGroupTarget - Fixed potential issue with WINDOWS_PHONE (@snakefoot)
- [#3261](https://github.com/NLog/NLog/pull/3261) NLog config file loading: use process name (e.g. applicationname.exe.nlog) when app.config is not available (@snakefoot)

#### Performance

- [#3311](https://github.com/NLog/NLog/pull/3311) Split string - avoid allocation of object array. Added StringHelpers.SplitAndTrimTokens (@snakefoot)
- [#3305](https://github.com/NLog/NLog/pull/3305) AppSettingLayoutRenderer - Mark as ThreadSafe and ThreadAgnostic (@snakefoot)

#### Misc

- [#3338](https://github.com/NLog/NLog/pull/3338) Update docs of various context classes (@304NotModified)
- [#3288](https://github.com/NLog/NLog/pull/3288) Move NLogPackageLoaders for better unittest debugging experience of NLog (@304NotModified)
- [#3274](https://github.com/NLog/NLog/pull/3274) Make HttpNetworkSender mockable, add unit test, introduce NSubsitute (@304NotModified)
- 10 pull requests with refactorings (@snakefoot, @304NotModified)

### V4.6.2 (2019/04/02)

#### Bugfixes

- [#3260](https://github.com/NLog/NLog/pull/3260) Fix escaping nested close brackets when parsing layout renderers (@lobster2012-user)
- [#3271](https://github.com/NLog/NLog/pull/3271) NLog config - Fixed bug where empty xml-elements were ignored (@snakefoot, @jonreis)


### V4.6.1 (2019/03/29)

#### Bugfixes

- [#3199](https://github.com/NLog/NLog/pull/3199) LoggingConfigurationParser - Fixed bug in handling of extensions prefix (@snakefoot)
- [#3253](https://github.com/NLog/NLog/pull/3253) Fix wrong warnings on `<nlog>` element (only wrong warnings) (#3253) (@snakefoot, @304NotModified)
- [#3195](https://github.com/NLog/NLog/pull/3195) SimpleStringReader: fix DebuggerDisplay value (#3195) (@lobster2012-user)
- [#3198](https://github.com/NLog/NLog/pull/3198) XmlLayout - Fixed missing encode of xml element value (@snakefoot)
- [#3191](https://github.com/NLog/NLog/pull/3191) VariableLayoutRenderer - Fixed format-string for internal logger warning (@snakefoot, @lobster2012-user)
- [#3258](https://github.com/NLog/NLog/pull/3258) Fix error with Embedded Assembly in LogAssemblyVersion (@snakefoot)

#### Improvements

- [#3255](https://github.com/NLog/NLog/pull/3255) Auto-flush on process exit improvements (@snakefoot)
- [#3189](https://github.com/NLog/NLog/pull/3189) AsyncTaskTarget - Respect TaskDelayMilliseconds on low activity (@snakefoot)

#### Performance

- [#3256](https://github.com/NLog/NLog/pull/3256) ${NDLC} + ${NDC} - Faster checking when TopFrames = 1 (@snakefoot)
- [#3201](https://github.com/NLog/NLog/pull/3201) ${GDC} reading is now lockfree (#3201) (@snakefoot)

### V4.6

#### Features

- [#2363](https://github.com/NLog/NLog/pull/2363) + [#2899](https://github.com/NLog/NLog/pull/2899) + [#3085](https://github.com/NLog/NLog/pull/3085) + [#3091](https://github.com/NLog/NLog/pull/3091) Database target: support for DbType for parameters (including SqlDbType) -  (@hubo0831,@ObikeDev,@sorvis, @304NotModified, @snakefoot)
- [#2610](https://github.com/NLog/NLog/pull/2610) AsyncTargetWrapper with LogEventDropped- + LogEventQueueGrow-events (@Pomoinytskyi)
- [#2670](https://github.com/NLog/NLog/pull/2670) + [#3014](https://github.com/NLog/NLog/pull/3014) XmlLayout - Render LogEventInfo.Properties as XML (@snakefoot)
- [#2678](https://github.com/NLog/NLog/pull/2678) NetworkTarget - Support for SSL & TLS (@snakefoot)
- [#2709](https://github.com/NLog/NLog/pull/2709) XML Config: Support for constant variable in level attributes (level, minlevel, etc) (@304NotModified)
- [#2848](https://github.com/NLog/NLog/pull/2848) Added defaultAction for `<filter>` (@304NotModified)
- [#2849](https://github.com/NLog/NLog/pull/2849) IRawValue-interface and ${db-null} layout renderer (@304NotModified)
- [#2902](https://github.com/NLog/NLog/pull/2902) JsonLayout with support for System.Dynamic-objects (@304NotModified)
- [#2907](https://github.com/NLog/NLog/pull/2907) New Substring, Left & Right Wrappers (@304NotModified)
- [#3098](https://github.com/NLog/NLog/pull/3098) `<rule>` support for one or more '*' and '?' wildcards and in any position  (@beppemarazzi)
- [#2909](https://github.com/NLog/NLog/pull/2909) AsyncTaskTarget - BatchSize + RetryCount (@snakefoot)
- [#3018](https://github.com/NLog/NLog/pull/3018) ColoredConsoleTarget - Added EnableAnsiOutput option (VS Code support) (@jp7677 + @snakefoot)
- [#3031](https://github.com/NLog/NLog/pull/3031) + [#3092](https://github.com/NLog/NLog/pull/3092) Support ${currentdir},${basedir},${tempdir} and Environment Variables for internalLogFile when parsing nlog.config (@snakefoot)
- [#3050](https://github.com/NLog/NLog/pull/3050) Added IncludeGdc property in JsonLayout (@casperc89)
- [#3071](https://github.com/NLog/NLog/pull/3071) ${HostName} Layout Renderer for full computer DNS name (@amitsaha)
- [#3053](https://github.com/NLog/NLog/pull/3053) ${AppSetting} Layout Renderer (app.config + web.config) moved from NLog.Extended for NetFramework (@snakefoot)
- [#3060](https://github.com/NLog/NLog/pull/3060) TargetWithContext - Support for PropertyType using IRawValue-interface (@snakefoot)
- [#3124](https://github.com/NLog/NLog/pull/3124) NetworkTarget - Added support for KeepAliveTimeSeconds (@snakefoot)
- [#3129](https://github.com/NLog/NLog/pull/3129) ConfigSetting - Preregister so it can be accessed without extension registration (#3129) (@snakefoot)
* [#3165](https://github.com/NLog/NLog/pull/3165) Added noRawValue layout wrapper (@snakefoot)


#### Enhancements

- [#2989](https://github.com/NLog/NLog/pull/2989) JsonLayout includes Type-property when rendering Exception-object (@snakefoot)
- [#2891](https://github.com/NLog/NLog/pull/2891) LoggingConfigurationParser - Extracted from XmlLoggingConfiguration (Prepare for appsettings.json)  (@snakefoot)
- [#2910](https://github.com/NLog/NLog/pull/2910) Added support for complex objects in MDLC and NDLC on Net45  (@snakefoot)
- [#2918](https://github.com/NLog/NLog/pull/2918) PerformanceCounter - Improve behavior for CPU usage calculation  (@snakefoot)
- [#2941](https://github.com/NLog/NLog/pull/2941) TargetWithContext - Include all properties even when duplicate names  (@snakefoot)
- [#2974](https://github.com/NLog/NLog/pull/2974) Updated resharper annotations for better validation (@imanushin)
- [#2979](https://github.com/NLog/NLog/pull/2979) Improve default reflection support on NetCore Native (@snakefoot)
- [#3017](https://github.com/NLog/NLog/pull/3017) EventLogTarget with better support for MaximumKilobytes configuration (@Coriolanuss)
- [#3039](https://github.com/NLog/NLog/pull/3039) Added Xamarin PreserveAttribute for the entire Assembly to improve AOT-linking (@snakefoot)
- [#3045](https://github.com/NLog/NLog/pull/3045) Create snupkg packages and use portable PDB (@snakefoot)
- [#3048](https://github.com/NLog/NLog/pull/3048) KeepFileOpen + ConcurrentWrites on Xamarin + UWP - [#3079](https://github.com/NLog/NLog/pull/3079) (@304NotModified)
- [#3082](https://github.com/NLog/NLog/pull/3082) + [#3100](https://github.com/NLog/NLog/pull/3100)  WebService Target allow custom override of SoapAction-header for Soap11 (@AlexeyRokhin)
- [#3162](https://github.com/NLog/NLog/pull/3162) ContextProperty with IncludeEmptyValue means default value for ValueType (#3162) (@snakefoot)
- [#3159](https://github.com/NLog/NLog/pull/3159) AppSettingLayoutRenderer - Include Item for NLog.Extended (@snakefoot)
- [#3187](https://github.com/NLog/NLog/pull/3187) AsyncTaskTarget - Fixed unwanted delay caused by slow writer   (@snakefoot)
-  Various refactorings (19 pull requests) (@beppemarazzi, @304NotModified, @snakefoot)

#### Performance

- [#2650](https://github.com/NLog/NLog/pull/2650) AsyncTargetWrapper using ConcurrentQueue for NetCore2 for better thread-concurrency (@snakefoot)
- [#2890](https://github.com/NLog/NLog/pull/2890) AsyncTargetWrapper - TimeToSleepBetweenBatches changed default to 1ms (@snakefoot)
- [#2897](https://github.com/NLog/NLog/pull/2897) InternalLogger performance optimization when LogLevel.Off (@snakefoot)
- [#2935](https://github.com/NLog/NLog/pull/2935) InternalLogger LogLevel changes to LogLevel.Off by default unless being used. (@snakefoot)
- [#2934](https://github.com/NLog/NLog/pull/2934) CsvLayout - Allocation optimizations and optional skip quoting-check for individual columns. (@snakefoot)
- [#2949](https://github.com/NLog/NLog/pull/2949) MappedDiagnosticsLogicalContext - SetScoped with IReadOnlyList (Prepare for MEL BeginScope) (@snakefoot)
- [#2973](https://github.com/NLog/NLog/pull/2973) IRenderString-interface to improve performance for Layout with single LayoutRenderer (@snakefoot)
- [#3103](https://github.com/NLog/NLog/pull/3103) StringBuilderPool - Reduce memory overhead until required (@snakefoot)

**LibLog Breaking change**

* [damianh/LibLog#181](https://github.com/damianh/LibLog/pull/181) - Sub-components using LibLog ver. 5.0.3 (or newer) will now use MDLC + NDLC (Instead of MDC + NDC) when detecting application is using NLog ver. 4.6. Make sure to update NLog.config to match this change. Make sure that all sub-components have upgraded to LibLog ver. 5.0.3 (or newer) if they make use of `OpenNestedContext` or `OpenMappedContext`.

See also [NLog 4.6 Milestone](https://github.com/NLog/NLog/milestone/44?closed=1)

### V4.5.11 (2018/11/06)

#### Enhancements

- [#2985](https://github.com/NLog/NLog/pull/2985) LogBuilder - Support fluent assignment of message-template after properties (@snakefoot)
- [#2983](https://github.com/NLog/NLog/pull/2983) JsonSerializer - Use ReferenceEquals instead of object.Equals when checking for cyclic object loops (#2983) (@snakefoot)
- [#2988](https://github.com/NLog/NLog/pull/2988) NullAppender - Added missing SecuritySafeCritical (@snakefoot)

#### Fixes

- [#2987](https://github.com/NLog/NLog/pull/2987) JSON encoding should create valid JSON for non-string dictionary-keys (@snakefoot)

### V4.5.10 (2018/09/17)

#### Fixes
- [#2883](https://github.com/NLog/NLog/pull/2883) Fix LoadConfiguration for not found config file (@snakefoot, @304NotModified)

### v4.5.9 (2018/08/24)

#### Fixes
- [#2865](https://github.com/NLog/NLog/pull/2865) JSON encoding should create valid JSON for special double values (@snakefoot)

#### Enhancements
- [#2846](https://github.com/NLog/NLog/pull/2846) Include Entry Assembly File Location when loading candidate NLog.config (@snakefoot)

### v4.5.8 (2018/08/05)

#### Features
- [#2809](https://github.com/NLog/NLog/pull/2809) MethodCallTarget - Support for Lamba method (@snakefoot)
- [#2816](https://github.com/NLog/NLog/pull/2816) MessageTemplates - Support rendering of alignment + padding (@snakefoot)

#### Fixes  
- [#2827](https://github.com/NLog/NLog/pull/2827) FileTarget - Failing to CreateArchiveMutex should not stop logging (@snakefoot)
- [#2830](https://github.com/NLog/NLog/pull/2830) Auto loading of assemblies was broken in some cases (@snakefoot)

#### Enhancements
- [#2814](https://github.com/NLog/NLog/pull/2814) LoggingConfiguration - Improves CheckUnusedTargets to handle target wrappers (@snakefoot)

#### Performance
- [#2817](https://github.com/NLog/NLog/pull/2817) Optimize LayoutRendererWrappers to reduce string allocations (#2817) (@snakefoot)

### v4.5.7 (2018/07/19)

#### Features

- [#2792](https://github.com/nlog/nlog/pull/2792) OutputDebugStringTarget - Support Xamarin iOS and Android (@snakefoot)
- [#2776](https://github.com/nlog/nlog/pull/2776) FileTarget - Introduced OpenFileFlushTimeout to help when AutoFlush = false (@snakefoot)

#### Fixes 

- [#2761](https://github.com/nlog/nlog/pull/2761) ${Callsite} fix class naming when includeNamespace=false and cleanNamesOfAnonymousDelegates=true (@Azatey)
- [#2752](https://github.com/nlog/nlog/pull/2752) JSON: Fixes issue where char types are not properly escaped (#2752) (@smbecker)

#### Enhancements

- [#2804](https://github.com/nlog/nlog/pull/2804) FileTarget - Do not trust Last File Write TimeStamp when AutoFlush=false (@snakefoot)
- [#2763](https://github.com/nlog/nlog/pull/2763) Throw better error when target name is null (@masters3d)
- [#2788](https://github.com/nlog/nlog/pull/2788) ${Assembly-version} make GetAssembly protected and virtual (@alexangas)
- [#2756](https://github.com/nlog/nlog/pull/2756) LongDateLayoutRenderer: Improve comments (@stic)
- [#2749](https://github.com/nlog/nlog/pull/2749) NLog.WindowsEventLog: Update dependency System.Diagnostics.EventLog to RTM version (@304NotModified)

#### Performance

- [#2797](https://github.com/nlog/nlog/pull/2797) Better performance with Activator.CreateInstance (@tangdf)


### v4.5.6 (2018/05/29)

#### Fixes 

- [#2747](https://github.com/nlog/nlog/pull/2747) JsonSerializer - Generate valid Json when hitting the MaxRecursionLimit (@snakefoot)
- Fixup for [NLog.WindowsEventLog package](https://www.nuget.org/packages/NLog.WindowsEventLog)

#### Enhancements

- [#2745](https://github.com/nlog/nlog/pull/2745) FileTarget - Improve support for Linux FileSystem without BirthTime (@snakefoot)

#### Performance

- [#2744](https://github.com/nlog/nlog/pull/2744) LogEventInfo - HasProperties should allocate PropertiesDicitonary when needed (@snakefoot)
- [#2743](https://github.com/nlog/nlog/pull/2743) JsonLayout - Reduce allocations when needing to escape string (44% time improvement) (@snakefoot)

### v4.5.5 (2018/05/25)

#### Fixes

- [#2736](https://github.com/NLog/NLog/pull/2736) FileTarget - Calculate correct archive date when multiple file appenders (@snakefoot)

#### Features

- [#2726](https://github.com/nlog/nlog/pull/2726) WhenRepeated - Support logging rules with multiple targets (@snakefoot)
- [#2727](https://github.com/nlog/nlog/pull/2727) Support for custom targets that implements IUsesStackTrace (@snakefoot)
- [#2719](https://github.com/nlog/nlog/pull/2719) DatabaseTarget: use parameters on install (@Jejuni)

#### Enhancements

- [#2718](https://github.com/nlog/nlog/pull/2718) JsonLayout - Always stringify when requested (@snakefoot)
- [#2739](https://github.com/nlog/nlog/pull/2739) Target.WriteAsyncLogEvents(IList) to public 

#### Performance

- [#2704](https://github.com/nlog/nlog/pull/2704) Allocation improvement in precalculating layouts (@snakefoot)

### v4.5.4 (2018/05/05)

#### Fixes

- [#2688](https://github.com/nlog/nlog/pull/2688) Faulty invalidate of FormattedMessage when getting PropertiesDictionary (@snakefoot)
- [#2687](https://github.com/nlog/nlog/pull/2687) Fix: NLog.config build-action and copy for non-core projects, it's now "copy if newer" (@304NotModified)
- [#2698](https://github.com/nlog/nlog/pull/2698) FileTarget - Calculate correct archive date, when using Monthly archive (@snakefoot)

#### Enhancements

- [#2673](https://github.com/nlog/nlog/pull/2673) TargetWithContext - Easier to use without needing to override ContextProperties (@snakefoot)
- [#2684](https://github.com/nlog/nlog/pull/2684) DatabaseTarget - Skip static assembly lookup for .Net Standard (@snakefoot)
- [#2689](https://github.com/nlog/nlog/pull/2689) LogEventInfo - Structured logging parameters are not always immutable (@snakefoot)
- [#2679](https://github.com/nlog/nlog/pull/2679) Target.WriteAsyncThreadSafe should always have exception handler (@snakefoot)
- [#2586](https://github.com/nlog/nlog/pull/2586) Target.MergeEventProperties is now obsolete (@snakefoot)
- Sonar warning fixes:  [#2691](https://github.com/nlog/nlog/pull/2691),  [#2694](https://github.com/nlog/nlog/pull/2694), [#2693](https://github.com/nlog/nlog/pull/2693), [#2690](https://github.com/nlog/nlog/pull/2690), [#2685](https://github.com/nlog/nlog/pull/2685), [#2683](https://github.com/nlog/nlog/pull/2683), [#2696](https://github.com/NLog/NLog/pull/2696) (@snakefoot, @304NotModified)

### v4.5.3 (2018/04/16)

#### Fixes

- [#2662](https://github.com/nlog/nlog/pull/2662) FileTarget - Improve handling of archives with multiple active files (@snakefoot)

#### Enhancements

- [#2587](https://github.com/nlog/nlog/pull/2587) Internal Log - Include target type and target name in the log messages (@snakefoot)
- [#2651](https://github.com/nlog/nlog/pull/2651) Searching for NLog Extension Files should handle DirectoryNotFoundException  (@snakefoot)

#### Performance

- [#2653](https://github.com/nlog/nlog/pull/2653) LayoutRenderer ThreadSafe Attribute introduced to allow lock free Precalculate + other small performance improvements (@snakefoot)

### v4.5.2 (2018/04/06)

#### Features

- [#2648](https://github.com/nlog/nlog/pull/2648) ${processtime} and ${time} added invariant option (@snakefoot)

#### Fixes

- [#2643](https://github.com/nlog/nlog/pull/2643) UWP with NetStandard2 on Net Native does not support Assembly.CodeBase + Handle native methods in StackTrace (#2643) (@snakefoot)
- [#2644](https://github.com/nlog/nlog/pull/2644) FallbackGroupTarget: handle async state on fallback correctly (@snakefoot)

#### Performance

- [#2645](https://github.com/nlog/nlog/pull/2645) Minor performance optimization of some layoutrenderers (@snakefoot)
- [#2642](https://github.com/nlog/nlog/pull/2642) FileTarget - InitializeFile should skip dictionary lookup when same file (@snakefoot)


### v4.5.1 (2018/04/03)

#### Fixes

- [#2637](https://github.com/nlog/nlog/pull/2637) Fix IndexOutOfRangeException in NestedDiagnosticsLogicalContext (@snakefoot)
- [#2638](https://github.com/nlog/nlog/pull/2638) Handle null values correctly in LogReceiverSecureService (@304NotModified)

#### Performance

- [#2639](https://github.com/nlog/nlog/pull/2639) MessageTemplates - Optimize ParseHole for positional templates (@snakefoot)
- [#2640](https://github.com/nlog/nlog/pull/2640) FileTarget - InitializeFile no longer need justData parameter + dispose fileapenders earlier (@snakefoot)
- [#2628](https://github.com/nlog/nlog/pull/2628) RoundRobinGroupTarget - Replaced lock with Interlocked for performance (@snakefoot)




### v4.5 RTM (2018/03/25)

NLog 4.5 adds structured logging and .NET Standard support/UPW without breaking changes! Also a lot features has been added!


List of important changes in NLog 4.5

#### Features

- Support for .Net Standard 2.0 [#2263](https://github.com/nlog/nlog/pull/2263) + [#2402](https://github.com/nlog/nlog/pull/2402) (@snakefoot)
- Support for .Net Standard 1.5 [#2341](https://github.com/nlog/nlog/pull/2341) (@snakefoot)
- Support for .Net Standard 1.3 (and UWP) [#2441](https://github.com/nlog/nlog/pull/2441) + [#2597](https://github.com/nlog/nlog/pull/2597) (Remember to manually flush on app suspend). (@snakefoot)
- Introduced Structured logging [#2208](https://github.com/nlog/nlog/pull/2208) + [#2262](https://github.com/nlog/nlog/pull/2262) + [#2244](https://github.com/nlog/nlog/pull/2244) + [#2544](https://github.com/nlog/nlog/pull/2544) (@snakefoot, @304NotModified, @jods4, @nblumhardt) - see https://github.com/NLog/NLog/wiki/How-to-use-structured-logging
- Json conversion also supports object properties [#2179](https://github.com/nlog/nlog/pull/2179),  [#2555](https://github.com/nlog/nlog/pull/2555) (@snakefoot, @304NotModified)
- event-properties layout-renderer can now render objects as json [#2241](https://github.com/nlog/nlog/pull/2241) (@snakefoot, @304NotModified)
- exception layout-renderer can now render exceptions as json [#2357](https://github.com/nlog/nlog/pull/2357) (@snakefoot)
- Default file archive logic is now easier to use [#1993](https://github.com/nlog/nlog/pull/1993) (@snakefoot)
- Introduced InstallationContext.ThrowExceptions [#2214](https://github.com/nlog/nlog/pull/2214) (@rbarillec)
- WebServiceTarget - Allow configuration of proxy address [#2375](https://github.com/nlog/nlog/pull/2375) (@snakefoot)
- WebServiceTarget - JsonPost with JsonLayout without being wrapped in parameter [#2590](https://github.com/nlog/nlog/pull/2590) (@snakefoot)
- ${guid}, added GeneratedFromLogEvent [#2226](https://github.com/nlog/nlog/pull/2226) (@snakefoot)
- TraceTarget RawWrite to always perform Trace.WriteLine independent of LogLevel [#1968](https://github.com/nlog/nlog/pull/1968) (@snakefoot)
- Adding OverflowAction options to BufferingTargetWrapper [#2276](https://github.com/nlog/nlog/pull/2276) (@mikegron)
- WhenRepeatedFilter - Filtering of identical LogEvents  [#2123](https://github.com/nlog/nlog/pull/2123) + [#2297](https://github.com/nlog/nlog/pull/2297) (@snakefoot) 
- ${callsite} added CleanNamesOfAsyncContinuations option [#2292](https://github.com/nlog/nlog/pull/2292) (@tkhaugen, @304NotModified)
- ${ndlctiming} allows timing of ndlc-scopes [#2377](https://github.com/nlog/nlog/pull/2377) (@snakefoot)
- NLogViewerTarget - Enable override of the Logger-name [#2390](https://github.com/nlog/nlog/pull/2390) (@snakefoot)
- ${sequenceid} added [#2411](https://github.com/nlog/nlog/pull/2411) (@MikeFH)  
- Added "regex-matches" for filtering [#2437](https://github.com/nlog/nlog/pull/2437) (@MikeFH)
- ${gdc}, ${mdc} & {mdlc} - Support Format parameter [#2500](https://github.com/nlog/nlog/pull/2500) (@snakefoot)
- ${currentDir} added [#2491](https://github.com/nlog/nlog/pull/2491) (@UgurAldanmaz)
- ${AssemblyVersion}: add type (File, Assembly, Informational) option [#2487](https://github.com/nlog/nlog/pull/2487) (@alexangas)
- FileTarget: Support byte order mark [#2456](https://github.com/nlog/nlog/pull/2456) (@KYegres)
- TargetWithContext - Easier to create custom NLog targets with support for MDLC and NDLC [#2467](https://github.com/nlog/nlog/pull/2467) (@snakefoot)
- ${callname-filename} - Without line number [#2591](https://github.com/nlog/nlog/pull/2591) (@brunotag)
- MDC + MDLC with SetScoped property support  [#2592](https://github.com/nlog/nlog/pull/2592) (@MikeFH)
- LoggingConfiguration AddRule includes final-parameter [#2612](https://github.com/nlog/nlog/pull/2612) (@893949088)

#### Fixes 
- Improve archive stability during concurrent file access [#1889](https://github.com/nlog/nlog/pull/1889) (@snakefoot)
- FallbackGroup could lose log events [#2265](https://github.com/nlog/nlog/pull/2265) (@frabar666)
- ${exception} - only include separator when items are available [#2257](https://github.com/nlog/nlog/pull/2257) (@jojosardez)
- LogFactory - Fixes broken EventArgs for ConfigurationChanged [#1897](https://github.com/nlog/nlog/pull/1897) (@snakefoot)
- Do not report wrapped targets as unused targets [#2290](https://github.com/nlog/nlog/pull/2290) (@thesmallbang)
- Added IIncludeContext, implemented missing properties [#2117](https://github.com/nlog/nlog/pull/2117) (@304NotModified)
- Improve logging of callsite linenumber for async-methods [#2386](https://github.com/nlog/nlog/pull/2386) (@snakefoot)
- NLogTraceListener - DisableFlush is enabled by default when AutoFlush=true [#2407](https://github.com/nlog/nlog/pull/2407) (@snakefoot)
- NLogViewer - Better defaults for connection limits [#2404](https://github.com/nlog/nlog/pull/2404) (@304NotModified)
- LoggingConfiguration.LoggingRules is not thread safe [#2393](https://github.com/nlog/nlog/pull/2393), [#2418](https://github.com/nlog/nlog/pull/2418)
- Fix XmlLoggingConfiguration reloading [#2475](https://github.com/nlog/nlog/pull/2475) (@snakefoot)
- Database Target now supports EntityFramework ConnectionStrings [#2510](https://github.com/nlog/nlog/pull/2510) (@Misiu, @snakefoot)
- LoggingConfiguration.RemoveTarget now works while actively logging [#2549](https://github.com/nlog/nlog/pull/2549) (@jojosardez, @snakefoot)
- FileTarget does not fail on platforms without global mutex support [#2604](https://github.com/nlog/nlog/pull/2604) (@snakefoot)
- LoggingConfiguration does not fail when AutoReload is not possible on the platforms without FileWatcher [#2603](https://github.com/nlog/nlog/pull/2603)  (@snakefoot)

#### Performance 
- More targets has OptimizeBufferReuse enabled by default [#1913](https://github.com/nlog/nlog/pull/1913) + [#1923](https://github.com/nlog/nlog/pull/1923) + [#1912](https://github.com/nlog/nlog/pull/1912) + [#1911](https://github.com/nlog/nlog/pull/1911) + [#1910](https://github.com/nlog/nlog/pull/1910) + [#1909](https://github.com/nlog/nlog/pull/1909) + [#1908](https://github.com/nlog/nlog/pull/1908) + [#1907](https://github.com/nlog/nlog/pull/1907) + [#2560](https://github.com/nlog/nlog/pull/2560)   (@snakefoot)
- StringBuilderPool - Improved Layout Render Performance by reusing StringBuilders [#2208](https://github.com/nlog/nlog/pull/2208)   (@snakefoot)
- JsonLayout - Improved Layout Performance, by optimizing use of StringBuilder [#2208](https://github.com/nlog/nlog/pull/2208)   (@snakefoot)
- FileTarget - Faster byte-encoding of log messsages, by using crude Encoding.GetMaxByteCount() instead of exact Encoding.GetByteCount() [#2208](https://github.com/nlog/nlog/pull/2208) (@snakefoot)
- Target - Precalculate Layout should ignore sub-layouts for complex layout (Ex Json) [#2378](https://github.com/nlog/nlog/pull/2378)  (@snakefoot)
- MessageLayoutRenderer - Skip `string.Format` allocation (for caching) when writing to a single target, instead format directly into output buffer. [#2507](https://github.com/nlog/nlog/pull/2507)  (@snakefoot)




Changes since rc 07:
- [#2621](https://github.com/nlog/nlog/pull/2621) Single Target optimization logic refactored to reuse optimization approval (@snakefoot)
- [#2622](https://github.com/nlog/nlog/pull/2622) NetworkTarget - Http / Https should not throw on async error response (@snakefoot)
- [#2619](https://github.com/nlog/nlog/pull/2619) NetworkTarget - Reduce allocation when buffer is less than MaxMessageSize (@snakefoot)
- [#2616](https://github.com/nlog/nlog/pull/2616) LogManager.Shutdown - Should disable file watcher and avoid auto reload (@snakefoot)
- [#2620](https://github.com/nlog/nlog/pull/2620) Single Target optimization should only be done when parseMessageTemplate = null (@snakefoot)

### v4.5-rc07 (2018/03/07)
- [#2614](https://github.com/nlog/nlog/pull/2614) NLog 4.5 RC7 changelog & version (@304NotModified)
- [#2612](https://github.com/nlog/nlog/pull/2612) add final param to `AddRule` Methods (#2612) (@893949088)
- [#2590](https://github.com/nlog/nlog/pull/2590) WebServiceTarget - JsonPost with support for single nameless parameter (@snakefoot)
- [#2604](https://github.com/nlog/nlog/pull/2604) FileTarget - Failing to CreateArchiveMutex should not stop logging (#2604) (@snakefoot)
- [#2592](https://github.com/nlog/nlog/pull/2592) Make Set methods of MDC and MDLC return IDisposable (#2592) (@MikeFH)
- [#2591](https://github.com/nlog/nlog/pull/2591) callsite-filename renderer (#2591) (@brunotag)
- [#2597](https://github.com/nlog/nlog/pull/2597) Replace WINDOWS_UWP with NETSTANDARD1_3 to support UWP10 shared libraries (@snakefoot)
- [#2599](https://github.com/nlog/nlog/pull/2599) TryImplicitConversion should only check object types (@snakefoot)
- [#2595](https://github.com/nlog/nlog/pull/2595) IsSafeToDeferFormatting - Convert.GetTypeCode is faster and better (@snakefoot)
- [#2609](https://github.com/nlog/nlog/pull/2609) NLog - Fix Callsite when wrapping ILogger in Microsoft Extension Logging (@snakefoot)
- [#2613](https://github.com/nlog/nlog/pull/2613) Attempt to make some unit-tests more stable (@snakefoot)
- [#2603](https://github.com/nlog/nlog/pull/2603) MultiFileWatcher - Improve error handling if FileSystemWatcher fails (@snakefoot)

### v4.5-rc06 (2018/02/20)
- [#2585](https://github.com/nlog/nlog/pull/2585) NLog 4.5 rc6 version and changelog (#2585) (@304NotModified)
- [#2581](https://github.com/nlog/nlog/pull/2581) MessageTemplateParameter(s)  ctors to internal (@304NotModified)
- [#2576](https://github.com/nlog/nlog/pull/2576) Fix possible infinite loop in message template parser + better handling incorrect templates (@304NotModified)
- [#2580](https://github.com/nlog/nlog/pull/2580) ColoredConsoleTarget.cs: Fix typo (@perlun)

### v4.5-rc05 (2018/02/13)
- [#2571](https://github.com/nlog/nlog/pull/2571) 4.5 rc5 version and release notes (@304NotModified)
- [#2572](https://github.com/nlog/nlog/pull/2572) copyright 2018 (@304NotModified)
- [#2570](https://github.com/nlog/nlog/pull/2570) Update nuspec NLog.Config and NLog.Schema (@304NotModified)
- [#2542](https://github.com/nlog/nlog/pull/2542) Added TooManyStructuredParametersShouldKeepBeInParamList testcase (@304NotModified)
- [#2467](https://github.com/nlog/nlog/pull/2467) TargetWithContext - Easier to capture snapshot of MDLC and NDLC context (#2467) (@snakefoot)
- [#2555](https://github.com/nlog/nlog/pull/2555) JsonLayout - Added MaxRecursionLimit and set default to 0 (@snakefoot)
- [#2568](https://github.com/nlog/nlog/pull/2568) WebServiceTarget - Rollback added Group-Layout (@snakefoot)
- [#2544](https://github.com/nlog/nlog/pull/2544) MessageTemplate renderer with support for mixed mode templates (@snakefoot)
- [#2538](https://github.com/nlog/nlog/pull/2538) Renamed ValueSerializer to ValueFormatter (@snakefoot)
- [#2554](https://github.com/nlog/nlog/pull/2554) LogBuilder - Check level before allocation of Properties-dictionary (@snakefoot)
- [#2550](https://github.com/nlog/nlog/pull/2550) DefaultJsonSerializer - Reflection should skip index-item-properties (@snakefoot)
- [#2549](https://github.com/nlog/nlog/pull/2549) LoggingConfiguration - FindTargetByName should also find target + fix for logging on a target even after removed (@snakefoot)
- [#2548](https://github.com/nlog/nlog/pull/2548) IAppDomain.FriendlyName should also work on NetStandard15 (@snakefoot)
- [#2563](https://github.com/nlog/nlog/pull/2563) WebService-Target fails internally with PlatformNotSupportedException on NetCore (@snakefoot)
- [#2560](https://github.com/nlog/nlog/pull/2560) Network/NLogViewer/Chainsaw Target - Enabled OptimizeBufferReuse by default, but not for sub classes (@snakefoot)
- [#2551](https://github.com/nlog/nlog/pull/2551) Blackhole LoggingRule without targets (@snakefoot)
- [#2534](https://github.com/nlog/nlog/pull/2534) Docs for DefaultJsonSerializer/(i)ValueSerializer (#2534) (@304NotModified)
- [#2519](https://github.com/nlog/nlog/pull/2519) RegisterItemsFromAssembly - Include assemblies from nuget packages (Strict) (@304NotModified, @snakefoot)
- [#2524](https://github.com/nlog/nlog/pull/2524) FileTarget - Dynamic archive mode with more strict file-mask for cleanup (@snakefoot)
- [#2518](https://github.com/nlog/nlog/pull/2518) DatabaseTarget - Added DbProvider System.Data.SqlClient for NetStandard (@snakefoot)
- [#2514](https://github.com/nlog/nlog/pull/2514) Added missing docgen for different options (Less noise in appveyor) (@snakefoot)

### v4.5-rc04 (2018/01/15)
- [#2490](https://github.com/nlog/nlog/pull/2490) LogEventInfo.MessageTemplateParameters as class instead of interface (#2490) (@snakefoot)
- [#2510](https://github.com/nlog/nlog/pull/2510) Database target entity framework connection string (@snakefoot)
- [#2513](https://github.com/nlog/nlog/pull/2513) Update docs for [AppDomainFixedOutput], remove [AppDomainFixedOutput] on ${currentDir} (@NLog)
- [#2512](https://github.com/nlog/nlog/pull/2512) LogManager.LoadConfiguration - Support relative paths by default (@snakefoot)
- [#2507](https://github.com/nlog/nlog/pull/2507) Reduce string allocations for logevents with single target destination (@snakefoot)
- [#2491](https://github.com/nlog/nlog/pull/2491) Added ${currentDir} (#2491) (@UgurAldanmaz)
- [#2500](https://github.com/nlog/nlog/pull/2500) ${gdc}, ${mdc} & {mdlc} - Support Format parameter (@snakefoot)
- [#2497](https://github.com/nlog/nlog/pull/2497) RegisterItemsFromAssembly - Include assemblies from nuget packages (Fix) (@snakefoot)
- [#2472](https://github.com/nlog/nlog/pull/2472) LogManager.LoadConfiguration - Prepare for custom config file readers (@snakefoot)
- [#2486](https://github.com/nlog/nlog/pull/2486) JsonConverter- Sanitize Exception.Data dictionary keys option (on by default) (@snakefoot)
- [#2495](https://github.com/nlog/nlog/pull/2495) MultiFileWatcher - Detach from FileSystemWatcher before disposing (@snakefoot)
- [#2493](https://github.com/nlog/nlog/pull/2493) RegisterItemsFromAssembly - Include assemblies from nuget packages (@snakefoot)
- [#2487](https://github.com/nlog/nlog/pull/2487) Adds version types to AssemblyVersion layout renderer  (#2487) (@alexangas)
- [#2464](https://github.com/nlog/nlog/pull/2464) Add methods to enabling/disabling LogLevels (@MikeFH)
- [#2477](https://github.com/nlog/nlog/pull/2477) fix typo (@heldersepu)
- [#2485](https://github.com/nlog/nlog/pull/2485) FileTarget - Dispose Archive-Mutex after completing file-archive (@snakefoot)
- [#2475](https://github.com/nlog/nlog/pull/2475) Fix XmlLoggingConfiguration reloading (@MikeFH)
- [#2471](https://github.com/nlog/nlog/pull/2471) TreatWarningsAsErrors = true (@snakefoot)
- [#2462](https://github.com/nlog/nlog/pull/2462) AsyncLogEventInfo - Removed private setter (@snakefoot)

### v4.5-rc03 (2017/12/11)
- [#2460](https://github.com/nlog/nlog/pull/2460) NLog 4.5 rc3 version and changelog (@304NotModified)
- [#2459](https://github.com/nlog/nlog/pull/2459) StringBuilderExt.CopyToStream - Optimize MemoryStream allocation (@snakefoot)
- [#2456](https://github.com/nlog/nlog/pull/2456) FileTarget: Support byte order mark (optional) (#2456) (@KYegres)
- [#2453](https://github.com/nlog/nlog/pull/2453) NestedDiagnosticsContext - Only allocate Stack-object on Write (@snakefoot)
- [#2458](https://github.com/nlog/nlog/pull/2458) Document and (minor) refactor on MruCache class (@ie-zero)
- [#2452](https://github.com/nlog/nlog/pull/2452) ThreadLocalStorageHelper - NetStandard only allocate when needed (@snakefoot)

### v4.5-rc02 (2017/12/04)
- [#2444](https://github.com/nlog/nlog/pull/2444) NLog 4.5 RC2 version and changelog (@304NotModified)
- [#2450](https://github.com/nlog/nlog/pull/2450) No need to call  type(T) (@304NotModified)
- [#2451](https://github.com/nlog/nlog/pull/2451) FileTarget - Improved and less internal logging (@snakefoot)
- [#2449](https://github.com/nlog/nlog/pull/2449) Refactor: fix comment, remove unused, cleanup, String ->` string etc (#2449) (@304NotModified)
- [#2447](https://github.com/nlog/nlog/pull/2447) CallSiteInformation - Prepare for fast classname lookup from filename and linenumber (@snakefoot)
- [#2446](https://github.com/nlog/nlog/pull/2446) Refactor - split methodes, remove duplicates (#2446) (@304NotModified)
- [#2437](https://github.com/nlog/nlog/pull/2437) Create regex-matches condition method (#2437) (@MikeFH)
- [#2441](https://github.com/nlog/nlog/pull/2441) Support for UWP platform (@snakefoot)
- [#2439](https://github.com/nlog/nlog/pull/2439) Better config Better Code Hub (@304NotModified)
- [#2431](https://github.com/nlog/nlog/pull/2431) NetCoreApp - Improve auto loading of NLog extension dlls (@snakefoot)
- [#2418](https://github.com/nlog/nlog/pull/2418) LoggingConfiguration - Modify and clone LoggingRules under lock (@snakefoot)
- [#2422](https://github.com/nlog/nlog/pull/2422) Avoid unnecessary string-allocation, skip unnecessary lock (@snakefoot)
- [#2419](https://github.com/nlog/nlog/pull/2419) Logger - Added missing MessageTemplateFormatMethodAttribute + removed obsolete internal method (@snakefoot)
- [#2421](https://github.com/nlog/nlog/pull/2421) Changed IIncludeContext to internal interface until someone needs it (@snakefoot)
- [#2417](https://github.com/nlog/nlog/pull/2417) IsMono - Cache Type.GetType to avoid constant AppDomain.TypeResolve events (@snakefoot)
- [#2420](https://github.com/nlog/nlog/pull/2420) Merged CallSite Test from PR1812 (@snakefoot)

### v4.5-rc01 (2017/11/23)
- [#2414](https://github.com/nlog/nlog/pull/2414) Revert breaking change of NestedDiagnosticsLogicalContext.Pop() (@304NotModified)
- [#2415](https://github.com/nlog/nlog/pull/2415) NetStandard15 - Moved dependency System.Xml.XmlSerializer to NLog.Wcf (@snakefoot)
- [#2413](https://github.com/nlog/nlog/pull/2413) NestedDiagnosticsLogicalContext - Protect against double dispose (@snakefoot)
- [#2412](https://github.com/nlog/nlog/pull/2412) MessageTemplate - Render test of DateTime, TimeSpan, DateTimeOffset (@snakefoot)
- [#2411](https://github.com/nlog/nlog/pull/2411) Create SequenceIdLayoutRenderer (@MikeFH)
- [#2409](https://github.com/nlog/nlog/pull/2409) Revert "Avoid struct copy on readonly field access" (@snakefoot)
- [#2403](https://github.com/nlog/nlog/pull/2403) NLog 4.5 beta 8 (@304NotModified)
- [#2406](https://github.com/nlog/nlog/pull/2406) From [StringFormatMethod] to [MessageTemplateFormatMethod] (@304NotModified)
- [#2402](https://github.com/nlog/nlog/pull/2402) Introduced NLog.Wcf and Nlog.WindowsIdentity for .NET standard (@snakefoot)
- [#2404](https://github.com/nlog/nlog/pull/2404) Updated NLog viewer target defaults (@304NotModified)
- [#2405](https://github.com/nlog/nlog/pull/2405) Added unit test for JsonLayout - serialize of objects (@304NotModified)
- [#2407](https://github.com/nlog/nlog/pull/2407) NLogTraceListener - set DisableFlush true by default (@snakefoot)
- [#2401](https://github.com/nlog/nlog/pull/2401) MailTarget is supported by NetStandard2.0 (but without SmtpSection) (@snakefoot)
- [#2398](https://github.com/nlog/nlog/pull/2398) Log4JXml - Fixed initalization of XmlWriterSettings for IndentXml (@snakefoot)
- [#2386](https://github.com/nlog/nlog/pull/2386) LogEventInfo.StackTrace moved into CallSiteInformation (@snakefoot)
- [#2399](https://github.com/nlog/nlog/pull/2399) Avoid struct copy on readonly field access (@snakefoot)
- [#2389](https://github.com/nlog/nlog/pull/2389) Fixed Sonar Lint code analysis warnings (@snakefoot)
- [#2396](https://github.com/nlog/nlog/pull/2396) Update xunit and Microsoft.NET.Test.Sdk (@304NotModified)
- [#2387](https://github.com/nlog/nlog/pull/2387) JsonConverter - Do not include static properties by default (@snakefoot)
- [#2392](https://github.com/nlog/nlog/pull/2392) Removed unneeded default references like System.Drawing for NetFramework (@snakefoot)
- [#2390](https://github.com/nlog/nlog/pull/2390) NLogViewerTarget - Enable override of the Logger-name (@snakefoot)
- [#2385](https://github.com/nlog/nlog/pull/2385) FileTarget - ArchiveMutex only created when needed (@snakefoot)
- [#2378](https://github.com/nlog/nlog/pull/2378) Target - Precalculate Layout should ignore sub-layouts for complex layouts (@snakefoot)
- [#2388](https://github.com/nlog/nlog/pull/2388) PropertiesDictionary - Removed obsolete (private) method (@snakefoot)
- [#2384](https://github.com/nlog/nlog/pull/2384) InternalLogger should work when NetCore loads NetFramework DLL (@snakefoot)
- [#2377](https://github.com/nlog/nlog/pull/2377) NDLC - Perform low resolution scope timing (@snakefoot)
- [#2372](https://github.com/nlog/nlog/pull/2372) Log4JXmlEventLayoutRenderer - Minor platform fixes for IncludeNdlc (@snakefoot)
- [#2371](https://github.com/nlog/nlog/pull/2371) FileTarget - Enable archive mutex for Unix File Appender (if available) (@snakefoot)
- [#2375](https://github.com/nlog/nlog/pull/2375) WebServiceTarget - Allow configuration of proxy address (@snakefoot)
- [#2362](https://github.com/nlog/nlog/pull/2362) NLog - NETSTANDARD1_5 (Cleanup package references) (@snakefoot)
- [#2361](https://github.com/nlog/nlog/pull/2361) Use expression-bodied members (@c0shea)

### v4.5-beta07 (2017/10/15)
- [#2359](https://github.com/nlog/nlog/pull/2359) WrapperLayoutRendererBase - Transform with access to LogEventInfo (@snakefoot)
- [#2357](https://github.com/nlog/nlog/pull/2357) ExceptionLayoutRenderer - Support Serialize Format (@snakefoot)
- [#2358](https://github.com/nlog/nlog/pull/2358) WrapperLayoutRendererBuilderBase - Transform with access to LogEventInfo (@snakefoot)
- [#2314](https://github.com/nlog/nlog/pull/2314) Refactor InternalLogger class (@304NotModified, @ie-zero)
- [#2356](https://github.com/nlog/nlog/pull/2356) NLog - MessageTemplates - Renamed config to parseMessageTemplates (@snakefoot)
- [#2353](https://github.com/nlog/nlog/pull/2353) remove old package.config (@304NotModified)
- [#2354](https://github.com/nlog/nlog/pull/2354) NLog - NETSTANDARD1_5 (revert breaking change) (@snakefoot)
- [#2342](https://github.com/nlog/nlog/pull/2342) Remove redundant qualifiers (#2342) (@c0shea)
- [#2349](https://github.com/nlog/nlog/pull/2349) NLog - NETSTANDARD1_5 (Fix uppercase with culture) (@snakefoot)
- [#2341](https://github.com/nlog/nlog/pull/2341) NLog - NETSTANDARD1_5 (@snakefoot)

### v4.5-beta06 (2017/10/12)
- [#2346](https://github.com/nlog/nlog/pull/2346) Add messageTemplateParser to XSD (@304NotModified)
- [#2348](https://github.com/nlog/nlog/pull/2348) NLog MessageTemplateParameter with CaptureType (@snakefoot)

### v4.5-beta05 (2017/10/12)
- [#2340](https://github.com/nlog/nlog/pull/2340) NLog - MessageTemplateParameters - Always parse when IsPositional (@304NotModified, @snakefoot)
- [#2337](https://github.com/nlog/nlog/pull/2337) Use string interpolation (@c0shea)
- [#2327](https://github.com/nlog/nlog/pull/2327) Naming: consistent private fields (@304NotModified)

### v4.5-beta04 (2017/10/10)
- [#2318](https://github.com/nlog/nlog/pull/2318) NLog 4.5 beta 4 (@304NotModified)
- [#2326](https://github.com/nlog/nlog/pull/2326) NLog - ValueSerializer - Faster integer and enum (@snakefoot)
- [#2328](https://github.com/nlog/nlog/pull/2328) fix xunit warning (@304NotModified)
- [#2117](https://github.com/nlog/nlog/pull/2117) Added IIncludeContext, implemented missing properties, added includeNdlc, sync NDLC and NDC (@304NotModified)
- [#2317](https://github.com/nlog/nlog/pull/2317) More explicit side effect (@304NotModified)
- [#2290](https://github.com/nlog/nlog/pull/2290) Do not report wrapped targets as unused targets (#2290) (@thesmallbang)
- [#2319](https://github.com/nlog/nlog/pull/2319) NLog - MessageTemplateParameter - IsReservedFormat (@snakefoot)
- [#2316](https://github.com/nlog/nlog/pull/2316) LogManager.LogFactory public (@304NotModified)
- [#2208](https://github.com/nlog/nlog/pull/2208) Added Structured events / Message Templates (@snakefoot)
- [#2312](https://github.com/nlog/nlog/pull/2312) Improve stability of unstable test on Travis: BufferingTargetWrapperA… (@304NotModified)
- [#2309](https://github.com/nlog/nlog/pull/2309) 2017 copyright in T4 files (@304NotModified)
- [#2292](https://github.com/nlog/nlog/pull/2292) ${callsite} added CleanNamesOfAsyncContinuations  option (#2292) (@304NotModified)
- [#2310](https://github.com/nlog/nlog/pull/2310) Upgrade xUnit to 2.3.0 RTM (@304NotModified)
- [#2313](https://github.com/nlog/nlog/pull/2313) Removal of old package.config files (@304NotModified)
- [#1872](https://github.com/nlog/nlog/pull/1872) FileTarget: more internal logging (@304NotModified)
- [#1897](https://github.com/nlog/nlog/pull/1897) LogFactory - Fixed EventArgs for ConfigurationChanged (@snakefoot)
- [#2301](https://github.com/nlog/nlog/pull/2301) Docs, rename and refactor of PropertiesDictionary/MessageTemplateParameters (@304NotModified)
- [#2302](https://github.com/nlog/nlog/pull/2302) Copyright to 2017 (@304NotModified)
- [#2262](https://github.com/nlog/nlog/pull/2262) LogEventInfo.MessageTemplate - Subset of LogEventInfo.Properties (@snakefoot)
- [#2300](https://github.com/nlog/nlog/pull/2300) Fixed title (@304NotModified)

### v4.5-beta03 (2017/09/30)
- [#2298](https://github.com/nlog/nlog/pull/2298) Search also for lowercase nlog.config (@304NotModified)
- [#2297](https://github.com/nlog/nlog/pull/2297) WhenRepeatedFilter - Log after timeout (@snakefoot)


### v4.5-beta01 (2017/09/16)
- [#2263](https://github.com/nlog/nlog/pull/2263) Support .NET Standard 2.0 and move to VS 2017 (@snakefoot)


### v4.4.13 (2018/02/28)

#### Fixes

- [#2600](https://github.com/nlog/nlog/pull/2600) Fix 'System.ReadOnlySpan`1[System.Char]' cannot be converted to type 'System.String' (@snakefoot)


### v4.4.12 (2017/08/08)

#### Fixes

- [#2229](https://github.com/nlog/nlog/pull/2229) Fix: ReconfigExistingLoggers sometimes throws an Exception (@jpdillingham)

### v4.4.11 (2017/06/17)

#### Fixes

- [#2164](https://github.com/nlog/nlog/pull/2164) JsonLayout - Don't mark ThreadAgnostic when IncludeMdc or IncludeMdlc is enabled (@snakefoot)

### v4.4.10 (2017/05/31)

#### Features

- [#2110](https://github.com/nlog/nlog/pull/2110) NdlcLayoutRenderer - Nested Diagnostics Logical Context (@snakefoot)
- [#2114](https://github.com/nlog/nlog/pull/2114) EventlogTarget: Support for MaximumKilobytes (@304NotModified, @ajitpeter)
- [#2109](https://github.com/nlog/nlog/pull/2109) JsonLayout - IncludeMdc and IncludeMdlc (@snakefoot)

#### Fixes

- [#2138](https://github.com/nlog/nlog/pull/2138) ReloadConfigOnTimer - fix potential NullReferenceException (@snakefoot)
- [#2113](https://github.com/nlog/nlog/pull/2113) BugFix: `<targets>` after `<rules>` won't work (@304NotModified, @Moafak)
- [#2131](https://github.com/nlog/nlog/pull/2131) Fix : LogManager.ReconfigureExistingLoggers() could throw InvalidOperationException (@304NotModified, @jpdillingham)

#### Improvements

- [#2137](https://github.com/nlog/nlog/pull/2137) NLogTraceListener - Reduce overhead by checking LogLevel (@snakefoot)
- [#2112](https://github.com/nlog/nlog/pull/2112) LogReceiverWebServiceTarget - Ensure PrecalculateVolatileLayouts (@snakefoot)
- [#2103](https://github.com/nlog/nlog/pull/2103) Improve Install of targets / crash Install on Databasetarget. (@M4ttsson)
- [#2101](https://github.com/nlog/nlog/pull/2101) LogFactory.Shutdown - Add warning on target flush timeout (@snakefoot)

### v4.4.9
 
#### Features
 - [#2090](https://github.com/nlog/nlog/pull/2090) ${log4jxmlevent} - Added IncludeAllProperties option (@snakefoot) 
 - [#2090](https://github.com/nlog/nlog/pull/2090) Log4JXmlEvent Layout - Added IncludeAllProperties, IncludeMdlc and IncludeMdc option (@snakefoot)
 
#### Fixes
 - [#2090](https://github.com/nlog/nlog/pull/2090) Log4JXmlEvent Layout - Fixed bug with empty nlog:properties (@snakefoot)
 - [#2093](https://github.com/nlog/nlog/pull/2093) Fixed bug to logging by day of week (@RussianDragon)
 - [#2095](https://github.com/nlog/nlog/pull/2095) Fix: include ignoreErrors attribute not working for non-existent file (@304NotModified, @ghills)

### v4.4.8 (2017/04/28)

#### Features
- [#2078](https://github.com/nlog/nlog/pull/2078) Include MDLC in log4j renderer (option) (@thoemmi)

### v4.4.7 (2017/04/25)

#### Features

- [#2063](https://github.com/nlog/nlog/pull/2063) JsonLayout - Added JsonAttribute property EscapeUnicode (@snakefoot)

#### Improvements

- [#2075](https://github.com/nlog/nlog/pull/2075) StackTraceLayoutRenderer with Raw format should display source FileName (@snakefoot)
- [#2067](https://github.com/nlog/nlog/pull/2067) ${EventProperties}, ${newline}, ${basedir} & ${tempdir} as ThreadAgnostic (performance improvement) (@snakefoot)
- [#2061](https://github.com/nlog/nlog/pull/2061) MethodCallTarget - Fixed possible null-reference-exception on initialize (@snakefoot)

## v4.4.6 (2017/04/11)

#### Features

- [#2006](https://github.com/nlog/nlog/pull/2006) Added AsyncTaskTarget - Base class for using async methods (@snakefoot)
- [#2051](https://github.com/nlog/nlog/pull/2051) Added LogMessageGenerator overloads for exceptions (#2051) (@c0shea)
- [#2034](https://github.com/nlog/nlog/pull/2034) ${level} add format option (full, single char and ordinal) (#2034) (@c0shea)
- [#2042](https://github.com/nlog/nlog/pull/2042) AutoFlushTargetWrapper - Added AsyncFlush property (@snakefoot)

#### Improvements

- [#2048](https://github.com/nlog/nlog/pull/2048) Layout - Ensure StackTraceUsage works for all types of Layout (@snakefoot)
- [#2041](https://github.com/nlog/nlog/pull/2041) Reduce memory allocations (AsyncContinuation exceptionHandler) & refactor (@snakefoot)
- [#2040](https://github.com/nlog/nlog/pull/2040) WebServiceTarget - Avoid re-throwing exceptions in async completion method (@snakefoot)

### v4.4.5 (2017/03/28)

#### Fixes

- [#2010](https://github.com/nlog/nlog/pull/2010) LogFactory - Ensure to flush and close on shutdown - fixes broken logging (@snakefoot)
- [#2017](https://github.com/nlog/nlog/pull/2017) WebServiceTarget - Fix boolean parameter conversion for Xml and Json (lowercase) (@snakefoot)


#### Improvements

- [#2017](https://github.com/nlog/nlog/pull/2017) Merged the JSON serializer code into DefaultJsonSerializer (@snakefoot)

### 4.4.4 (2017/03/10)

#### Features

- [#2000](https://github.com/nlog/nlog/pull/2000) Add weekly archival option to FileTarget (@dougthor42)
- [#2009](https://github.com/nlog/nlog/pull/2009) Load assembly event (@304NotModified)
- [#1917](https://github.com/nlog/nlog/pull/1917) Call NLogPackageLoader.Preload (static) for NLog packages on load (@304NotModified)

#### Improvements

- [#2007](https://github.com/nlog/nlog/pull/2007) Target.Close() - Extra logging to investigate shutdown order (@snakefoot)
- [#2003](https://github.com/nlog/nlog/pull/2003) Update XSD for `<NLog>` options (@304NotModified)
- [#1977](https://github.com/nlog/nlog/pull/1977) update xsd template (internallogger) for 4.4.3 version (@AuthorProxy)
- [#1956](https://github.com/nlog/nlog/pull/1956) Improve docs ThreadAgnosticAttribute (#1956) (@304NotModified)
- [#1992](https://github.com/nlog/nlog/pull/1992) Fixed merge error of XML documentation for Target Write-methods (@snakefoot)

#### Fixes

- [#1995](https://github.com/nlog/nlog/pull/1995) Proper apply default-target-parameters to nested targets in WrappedTargets (@nazim9214)

### 4.4.3 (2017/02/17)

#### Fixes

- [#1966](https://github.com/nlog/nlog/pull/1966) System.UriFormatException on load (Mono) (@JustArchi)
- [#1960](https://github.com/nlog/nlog/pull/1960) EventLogTarget: Properly parse and set EventLog category (@marinsky)

### 4.4.2 (2017/02/06)
 
#### Features

- [#1799](https://github.com/nlog/nlog/pull/1799) FileTarget: performance improvement: 10-70% faster, less garbage collecting (3-4 times less) by reusing buffers  (@snakefoot, @AndreGleichner)
- [#1919](https://github.com/nlog/nlog/pull/1919) Func overloads for InternalLogger (@304NotModified)
- [#1915](https://github.com/nlog/nlog/pull/1915) allow wildcard (*) in `<include>` (@304NotModified)
- [#1914](https://github.com/nlog/nlog/pull/1914) basedir: added option processDir=true (@304NotModified)
- [#1906](https://github.com/nlog/nlog/pull/1906) Allow Injecting basedir (@304NotModified)

#### Improvements

- [#1927](https://github.com/nlog/nlog/pull/1927) InternalLogger - Better support for multiple threads when using file (@snakefoot)
- [#1871](https://github.com/nlog/nlog/pull/1871) Filetarget - Allocations optimization (#1871) (@nazim9214)
- [#1931](https://github.com/nlog/nlog/pull/1931) FileTarget - Validate File CreationTimeUtc when non-Windows (@snakefoot)
- [#1942](https://github.com/nlog/nlog/pull/1942) FileTarget - KeepFileOpen should watch for file deletion, but not every second (@snakefoot)
- [#1876](https://github.com/nlog/nlog/pull/1876) FileTarget - Faster archive check by caching the static file-create-time (60-70% improvement) (#1876) (@snakefoot)
- [#1878](https://github.com/nlog/nlog/pull/1878) FileTarget - KeepFileOpen should watch for file deletion (#1878) (@snakefoot)
- [#1932](https://github.com/nlog/nlog/pull/1932) FileTarget - Faster rendering of filepath, when not ThreadAgnostic (@snakefoot)
- [#1937](https://github.com/nlog/nlog/pull/1937) LogManager.Shutdown - Verify that active config exists (@snakefoot)
- [#1926](https://github.com/nlog/nlog/pull/1926) RetryingWrapper - Allow closing target, even when busy retrying (@snakefoot)
- [#1925](https://github.com/nlog/nlog/pull/1925) JsonLayout - Support Precalculate for async processing (@snakefoot)
- [#1816](https://github.com/nlog/nlog/pull/1816) EventLogTarget - don't crash with invalid Category / EventId (@304NotModified)
- [#1815](https://github.com/nlog/nlog/pull/1815) Better parsing for Layouts with int/bool type. (@304NotModified, @rymk)
- [#1868](https://github.com/nlog/nlog/pull/1868) WebServiceTarget - FlushAsync - Avoid premature flush (#1868) (@snakefoot)
- [#1899](https://github.com/nlog/nlog/pull/1899) LogManager.Shutdown - Use the official method for closing down (@snakefoot)


#### Fixes
                                                                          
- [#1886](https://github.com/nlog/nlog/pull/1886) FileTarget - Archive should not fail when ArchiveFileName matches FileName (@snakefoot)
- [#1893](https://github.com/nlog/nlog/pull/1893) FileTarget - MONO doesn't like using the native Win32 API (@snakefoot)
- [#1883](https://github.com/nlog/nlog/pull/1883) LogFactory.Dispose - Should always close down created targets (@snakefoot)

### v4.4.1 (2016/12/24)

Summary:

- Fixes for medium trust (@snakefoot, @304notmodified)
- Performance multiple improvements for flush events (@snakefoot)
- FileTarget: Improvements for archiving  (@snakefoot)  
- FileTarget - Reopen filehandle when file write fails  (@snakefoot)  
- ConsoleTarget: fix crash when console isn't available (@snakefoot)
- NetworkTarget - UdpNetworkSender should exercise the provided Close-callback  (@snakefoot)

Detail:

- [#1874](https://github.com/nlog/nlog/pull/1874) Fixes for medium trust (@snakefoot, @304notmodified)
- [#1873](https://github.com/nlog/nlog/pull/1873) PartialTrustDomain - Handle SecurityException to allow startup and logging (#1873) (@snakefoot)
- [#1859](https://github.com/nlog/nlog/pull/1859) FileTarget - MONO should also check SupportsSharableMutex (#1859) (@snakefoot)
- [#1853](https://github.com/nlog/nlog/pull/1853) AsyncTargetWrapper - Flush should start immediately without waiting (#1853) (@snakefoot)
- [#1858](https://github.com/nlog/nlog/pull/1858) FileTarget - Reopen filehandle when file write fails (#1858) (@snakefoot)
- [#1867](https://github.com/nlog/nlog/pull/1867) FileTarget - Failing to delete old archive files, should not stop logging (@snakefoot)
- [#1865](https://github.com/nlog/nlog/pull/1865) Compile MethodInfo into LateBoundMethod-delegate (ReflectedType is deprecated) (@snakefoot)
- [#1850](https://github.com/nlog/nlog/pull/1850) ConsoleTarget - Apply Encoding on InitializeTarget, if Console available (#1850) (@snakefoot)
- [#1862](https://github.com/nlog/nlog/pull/1862) SHFB config cleanup & simplify (@304NotModified)
- [#1863](https://github.com/nlog/nlog/pull/1863) Minor cosmetic changes on FileTarget class (@ie-zero)
- [#1861](https://github.com/nlog/nlog/pull/1861) Helper class ParameterUtils removed (@ie-zero)
- [#1847](https://github.com/nlog/nlog/pull/1847) LogFactory.Dispose() fixed race condition with reloadtimer (#1847) (@snakefoot)
- [#1849](https://github.com/nlog/nlog/pull/1849) NetworkTarget - UdpNetworkSender should exercise the provided Close-callback (@snakefoot)
- [#1857](https://github.com/nlog/nlog/pull/1857) Fix immutability of LogLevel properties (@ie-zero)
- [#1860](https://github.com/nlog/nlog/pull/1860) FileAppenderCache implements IDisposable (@ie-zero)
- [#1848](https://github.com/nlog/nlog/pull/1848) Standarise implementation of events (@ie-zero)
- [#1844](https://github.com/nlog/nlog/pull/1844) FileTarget - Mono2 runtime detection to skip using named archive-mutex (@snakefoot)


### v4.4  (2016/12/14)

#### Features

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


### v4.3.8 (2016/09/05)

#### Features
- [#1619](https://github.com/NLog/NLog/pull/1619) NetworkTarget: Added option to specify EOL (@kevindaub)

#### Improvements    
- [#1596](https://github.com/NLog/NLog/pull/1596) Performance tweak in NLog routing (@304NotModified)
- [#1593](https://github.com/NLog/NLog/pull/1593) FileTarget: large performance improvement - back to 1 million/sec (@304NotModified)
- [#1621](https://github.com/nlog/nlog/pull/1621) FileTarget: writing to non-existing drive was slowing down NLog a lot (@304NotModified)

#### Fixes
- [#1616](https://github.com/nlog/nlog/pull/1616) FileTarget: Don't throw an exception if a dir is missing when deleting old files on startup (@304NotModified)

### v4.3.7 (2016/08/06)

#### Features
- [#1469](https://github.com/nlog/nlog/pull/1469) Allow overwriting possible nlog configuration file paths (@304NotModified)
- [#1578](https://github.com/nlog/nlog/pull/1578) Add support for name parameter on ${Assembly-version} (@304NotModified)
- [#1580](https://github.com/nlog/nlog/pull/1580) Added option to not render empty literals on nested json objects (@johnkors)

#### Improvements
- [#1558](https://github.com/nlog/nlog/pull/1558) Callsite layout renderer: improve string comparison test (performance) (@304NotModified)
- [#1582](https://github.com/nlog/nlog/pull/1582) FileTarget: Performance improvement for CleanupInvalidFileNameChars  (@304NotModified)

#### Fixes
- [#1556](https://github.com/nlog/nlog/pull/1556) Bugfix: Use the culture when rendering the layout (@304NotModified)
 

### v4.3.6 (2016/07/24)

#### Features
- [#1531](https://github.com/nlog/nlog/pull/1531) Support Android 4.4 (@304NotModified)
- [#1551](https://github.com/nlog/nlog/pull/1551) Addded CompoundLayout (@luigiberrettini)

#### Fixes
- [#1548](https://github.com/nlog/nlog/pull/1548) Bugfix: Can't update EventLog's Source property (@304NotModified, @Page-Not-Found)
- [#1553](https://github.com/nlog/nlog/pull/1553) Bugfix: Throw configException when registering invalid extension assembly/type. (@304NotModified, @Jeinhaus)
- [#1547](https://github.com/nlog/nlog/pull/1547) LogReceiverWebServiceTarget is leaking communication channels (@MartinTherriault)


### v4.3.5 (2016/06/13)

#### Features
- [#1471](https://github.com/nlog/nlog/pull/1471) Add else option to ${when} (@304NotModified)
- [#1481](https://github.com/nlog/nlog/pull/1481) get items for diagnostic contexts (DiagnosticsContextes, GetNames() method) (@tiljanssen)

#### Fixes

- [#1504](https://github.com/nlog/nlog/pull/1504) Fix ${callsite} with async method with return value (@PELNZ)

### v4.3.4 (2016/05/16)

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

### v4.3.3 (2016/04/28)
- [#1411](https://github.com/nlog/nlog/pull/1411) MailTarget: fix "From" errors (bug introduced in NLog 4.3.2) (@304NotModified)

### v4.3.2 (2016/04/26)
- [#1404](https://github.com/nlog/nlog/pull/1404) FileTarget cleanup: move to background thread. (@304NotModified)
- [#1403](https://github.com/nlog/nlog/pull/1403) Fix filetarget: Thread was being aborted (#2) (@304NotModified)
- [#1402](https://github.com/nlog/nlog/pull/1402) Getting the 'From' when UseSystemNetMailSettings is true (@MoaidHathot)
- [#1401](https://github.com/nlog/nlog/pull/1401) Allow target configuration to support a hierachy of XML nodes (#1401) (@304NotModified)
- [#2](https://github.com/nlog/nlog/pull/2) Fix filetarget: Thread was being aborted (#2) (@304NotModified)
- [#1394](https://github.com/nlog/nlog/pull/1394) Make test methods public (#1394) (@luigiberrettini)
- [#1393](https://github.com/nlog/nlog/pull/1393) Remove test dependency on locale (@luigiberrettini)

### v4.3.1 (2016/04/20)
- [#1386](https://github.com/nlog/nlog/pull/1386) Fix "allLayouts is null" exception (@304NotModified)
- [#1387](https://github.com/nlog/nlog/pull/1387) Fix filetarget: Thread was being aborted (@304NotModified)
- [#1383](https://github.com/nlog/nlog/pull/1383) Fix configuration usage in `${var}` renderer (@bhaeussermann, @304NotModified)

### v4.3.0 (2016/04/16)
- [#1211](https://github.com/nlog/nlog/pull/1211) Update nlog.config for 4.3 (@304NotModified)
- [#1368](https://github.com/nlog/nlog/pull/1368) Update license (@304NotModified)

### v4.3.0-rc3 (2016/04/09)
- [#1348](https://github.com/nlog/nlog/pull/1348) Fix nullref + fix relative path for file archive (@304NotModified)
- [#1352](https://github.com/nlog/nlog/pull/1352) Fix for writing log file to root path (@304NotModified)
- [#1357](https://github.com/nlog/nlog/pull/1357) autoload NLog.config in assets folder (Xamarin Android) (@304NotModified)
- [#1358](https://github.com/nlog/nlog/pull/1358) no-recusive logging in internallogger. (@304NotModified)
- [#1364](https://github.com/nlog/nlog/pull/1364) Fix stacktraceusage with more than 1 rule (@304NotModified)

### v4.3.0-rc2 (2016/03/26)
- [#1335](https://github.com/nlog/nlog/pull/1335) Fix all build warnings (@304NotModified)
- [#1336](https://github.com/nlog/nlog/pull/1336) Throw NLogConfigurationException if TimeToSleepBetweenBatches `<= 0` (@vincechan)
- [#1333](https://github.com/nlog/nlog/pull/1333) Fix ${callsite} when loggerType can't be found due to inlining (@304NotModified)
- [#1329](https://github.com/nlog/nlog/pull/1329) Update SHFB (@304NotModified)

### v4.3.0-rc1 (2016/03/22)
- [#1323](https://github.com/nlog/nlog/pull/1323) Add TimeStamp options to XML, Appsetting and environment var (@304NotModified)
- [#1286](https://github.com/nlog/nlog/pull/1286) Easier api: AddRule methods, fix AllTargets crash, fix IsLevelEnabled(off) crash, refactor internal (@304NotModified)
- [#1317](https://github.com/nlog/nlog/pull/1317) don't require ProviderName attribute when using `<connectionStrings>` (app.config etc) (@304NotModified)
- [#1316](https://github.com/nlog/nlog/pull/1316) Fix scan for stacktrace usage (bug never released) (@304NotModified)
- [#1299](https://github.com/nlog/nlog/pull/1299) Also use logFactory for ThrowConfigExceptions (@304NotModified)
- [#1309](https://github.com/nlog/nlog/pull/1309) Added nested json from xml unit test (@pysco68, @304NotModified)
- [#1310](https://github.com/nlog/nlog/pull/1310) Fix threadsafe issue of GetLogger / GetCurrentClassLogger (+improve performance) (@304NotModified)
- [#1313](https://github.com/nlog/nlog/pull/1313) Added the NLog.Owin.Logging badges to README packages list (@pysco68)
- [#1222](https://github.com/nlog/nlog/pull/1222) internalLogger, write to System.Diagnostics.Debug / System.Diagnostics.Trace #1217 (@bryjamus)
- [#1303](https://github.com/nlog/nlog/pull/1303) Fix threadsafe issue of ScanProperties3 (@304NotModified)
- [#1273](https://github.com/nlog/nlog/pull/1273) Added the ability to allow virtual paths for SMTP pickup directory (@michaeljbaird)
- [#1298](https://github.com/nlog/nlog/pull/1298) NullReferenceException fix for VariableLayoutRenderer (@neris)
- [#1295](https://github.com/nlog/nlog/pull/1295) Fix Callsite render bug introducted in 4.3 beta (@304NotModified)
- [#1285](https://github.com/nlog/nlog/pull/1285) Fix: {$processtime} has incorrect milliseconds formatting (@304NotModified)
- [#1296](https://github.com/nlog/nlog/pull/1296) CachedLayoutRender: allow ClearCache as (ambient) property (@304NotModified)
- [#1294](https://github.com/nlog/nlog/pull/1294) Fix thread-safe issue ScanProperties (@304NotModified)
- [#1281](https://github.com/nlog/nlog/pull/1281) FileTargetTests: Fix runtime overflow-of-minute issue in DateArchive_SkipPeriod. (@bhaeussermann)
- [#1274](https://github.com/nlog/nlog/pull/1274) FileTarget: Fix archive does not work when date in file name. (@bhaeussermann)
- [#1275](https://github.com/nlog/nlog/pull/1275) Less logging for unstable unit tests (and also probably too much) (@304NotModified)
- [#1270](https://github.com/nlog/nlog/pull/1270) Added testcase (NestedJsonAttrTest) (@304NotModified)
- [#1279](https://github.com/nlog/nlog/pull/1279) Fix tests to ensure all AsyncTargetWrapper's are closed. (@bhaeussermann)
- [#1238](https://github.com/nlog/nlog/pull/1238) Control throwing of NLogConfigurationExceptions (LogManager.ThrowConfigExceptions) (@304NotModified)
- [#1265](https://github.com/nlog/nlog/pull/1265) More thread-safe method (@304NotModified)
- [#1260](https://github.com/nlog/nlog/pull/1260) try read nlog.config in ios/android (@304NotModified)
- [#1253](https://github.com/nlog/nlog/pull/1253) Added docs for UrlEncode (@304NotModified)
- [#1252](https://github.com/nlog/nlog/pull/1252) improve InternalLoggerTests unit test (@304NotModified)
- [#1259](https://github.com/nlog/nlog/pull/1259) Internallogger improvements (@304NotModified)
- [#1258](https://github.com/nlog/nlog/pull/1258) fixed typo in NLog.config (@icnocop)
- [#1256](https://github.com/nlog/nlog/pull/1256) Badges Shields.io ->` Badge.fury.io (@304NotModified)
- [#1225](https://github.com/nlog/nlog/pull/1225) XmlLoggingConfiguration: Set config values on correct LogFactory object (@bhaeussermann, @304NotModified)
- [#1](https://github.com/nlog/nlog/pull/1) Fix ambiguity in `cref` in comments. (@304NotModified)
- [#1254](https://github.com/nlog/nlog/pull/1254) Remove SubversionScc / AnkhSVN info from solutions (@304NotModified)
- [#1247](https://github.com/nlog/nlog/pull/1247) Init version issue template (@304NotModified)
- [#1245](https://github.com/nlog/nlog/pull/1245) Add Logger.Swallow(Task task) (@breyed)
- [#1246](https://github.com/nlog/nlog/pull/1246) added badges UWP / web.ASPNET5 (@304NotModified)
- [#1227](https://github.com/nlog/nlog/pull/1227) LogFactory: Add generic-type versions of GetLogger() and GetCurrentClassLogger() (@bhaeussermann)
- [#1242](https://github.com/nlog/nlog/pull/1242) Improve unit test (@304NotModified)
- [#1213](https://github.com/nlog/nlog/pull/1213) Log more to InternalLogger (@304NotModified)
- [#1240](https://github.com/nlog/nlog/pull/1240) Added StringHelpers + StringHelpers.IsNullOrWhiteSpace (@304NotModified)
- [#1239](https://github.com/nlog/nlog/pull/1239) Fix unstable MDLC Unit test  + MDLC free dataslot (@304NotModified, @MikeFH)
- [#1236](https://github.com/nlog/nlog/pull/1236) Bugfix: Internallogger creates folder, even when turned off. (@eduardorascon)
- [#1232](https://github.com/nlog/nlog/pull/1232) Fix HttpGet protocol for WebService (@MikeFH)
- [#1223](https://github.com/nlog/nlog/pull/1223) Fix deadlock on Factory (@304NotModified)

### v4.3.0-beta2 (2016/02/04)
- [#1220](https://github.com/nlog/nlog/pull/1220) FileTarget: Add internal logging for archive date. (@bhaeussermann)
- [#1214](https://github.com/nlog/nlog/pull/1214) Better unit test cleanup between tests + fix threadsafe issue ScanProperties (@304NotModified)
- [#1212](https://github.com/nlog/nlog/pull/1212) Support reading nlog.config from Android assets folder (@304NotModified)
- [#1215](https://github.com/nlog/nlog/pull/1215) FileTarget: Archiving not working properly with AsyncWrapper (@bhaeussermann)
- [#1216](https://github.com/nlog/nlog/pull/1216) Added more docs to InternalLogger (@304NotModified)
- [#1207](https://github.com/nlog/nlog/pull/1207) FileTarget: Fix Footer for archiving. (@bhaeussermann)
- [#1210](https://github.com/nlog/nlog/pull/1210) Added extra unit test (@304NotModified)
- [#1191](https://github.com/nlog/nlog/pull/1191) Throw exception when base.InitializeTarget() is not called + inline GetAllLayouts() (@304NotModified)
- [#1208](https://github.com/nlog/nlog/pull/1208) FileTargetTests: Supplemented ReplaceFileContentsOnEachWriteTest() to test with and without header and footer (@bhaeussermann)
- [#1197](https://github.com/nlog/nlog/pull/1197) Improve XML Docs (@304NotModified)
- [#1200](https://github.com/nlog/nlog/pull/1200) Added unit test for K datetime format (@304NotModified)

### 4.3.0-beta1 (2016/01/27)
- [#1143](https://github.com/nlog/nlog/pull/1143) Consistent Exception handling v3 (@304NotModified)
- [#1195](https://github.com/nlog/nlog/pull/1195) FileTarget: added ReplaceFileContentsOnEachWriteTest (@304NotModified)
- [#925](https://github.com/nlog/nlog/pull/925) RegistryLayoutRenderer: Support for layouts, RegistryView (32, 64 bit) and all root key names (HKCU/HKLM etc) (@304NotModified, @Niklas-Peter)
- [#1157](https://github.com/nlog/nlog/pull/1157) FIx (xml-) config classes for thread-safe issues (@304NotModified)
- [#1183](https://github.com/nlog/nlog/pull/1183) FileTarget: Fix compress archive file not working when using concurrentWrites="True" and keepFileOpen="True" (@bhaeussermann)
- [#1187](https://github.com/nlog/nlog/pull/1187) MethodCallTarget: allow optional parameters, no nullref exceptions. +unit tests (@304NotModified)
- [#1171](https://github.com/nlog/nlog/pull/1171) Coloredconsole not compiled regex by default (@304NotModified)
- [#1173](https://github.com/nlog/nlog/pull/1173) Unit test added for Variable node (@UgurAldanmaz)
- [#1138](https://github.com/nlog/nlog/pull/1138) Callsite fix for async methods (@304NotModified)
- [#1126](https://github.com/nlog/nlog/pull/1126) Fix and test archiving when writing to same file from different processes (@bhaeussermann)
- [#1170](https://github.com/nlog/nlog/pull/1170) LogBuilder: add StringFormatMethod Annotations (@304NotModified)
- [#1127](https://github.com/nlog/nlog/pull/1127) Max message length option for Eventlog target (@UgurAldanmaz)
- [#1149](https://github.com/nlog/nlog/pull/1149) Fix crash during delete of old archives & archive delete optimization (@brutaldev)
- [#1154](https://github.com/nlog/nlog/pull/1154) Fix nuget for Xamarin.iOs (@304NotModified)
- [#1159](https://github.com/nlog/nlog/pull/1159) README-developers.md: Added pull request checklist. (@bhaeussermann)
- [#1131](https://github.com/nlog/nlog/pull/1131) Reducing memory allocations in ShortDateLayoutRenderer by caching the formatted date. (@epignosisx)
- [#1141](https://github.com/nlog/nlog/pull/1141) Remove code dup of InternalLogger (T4) (@304NotModified)
- [#1144](https://github.com/nlog/nlog/pull/1144) add doc (@304NotModified)
- [#1142](https://github.com/nlog/nlog/pull/1142) PropertyHelper: rename to readable names (@304NotModified)
- [#1139](https://github.com/nlog/nlog/pull/1139) Reduce Memory Allocations in LongDateLayoutRenderer (@epignosisx)
- [#1112](https://github.com/nlog/nlog/pull/1112) ColoredConsoleTarget performance improvements. (@bhaeussermann)
- [#1135](https://github.com/nlog/nlog/pull/1135) FileTargetTests: Fix DateArchive_SkipPeriod test. (@bhaeussermann)
- [#1119](https://github.com/nlog/nlog/pull/1119) FileTarget: Use last-write-time for archive file name (@bhaeussermann)
- [#1089](https://github.com/nlog/nlog/pull/1089) Support For Relative Paths in the File Targets (@Page-Not-Found)
- [#1068](https://github.com/nlog/nlog/pull/1068) Overhaul ExceptionLayoutRenderer (@Page-Not-Found)
- [#1125](https://github.com/nlog/nlog/pull/1125) FileTarget: Fix continuous archiving bug. (@bhaeussermann)
- [#1113](https://github.com/nlog/nlog/pull/1113) Bugfix: EventLogTarget OnOverflow=Split writes always to Info level (@UgurAldanmaz)
- [#1116](https://github.com/nlog/nlog/pull/1116) Config: Implemented inheritance policy for autoReload in included config files (@bhaeussermann)
- [#1100](https://github.com/nlog/nlog/pull/1100) FileTarget: Fix archive based on time does not always archive. (@bhaeussermann)
- [#1110](https://github.com/nlog/nlog/pull/1110) Fix: Deadlock in NetworkTarget (@kt1996)
- [#1109](https://github.com/nlog/nlog/pull/1109) FileTarget: Fix archiving for ArchiveFileName without a pattern. (@bhaeussermann)
- [#1104](https://github.com/nlog/nlog/pull/1104) Merge from 4.2.3 (Improve performance of FileTarget, performance GDC) (@304NotModified, @epignosisx)
- [#1095](https://github.com/nlog/nlog/pull/1095) Fix find calling method on stack trace (@304NotModified)
- [#1099](https://github.com/nlog/nlog/pull/1099) Added extra callsite unit tests (@304NotModified)
- [#1084](https://github.com/nlog/nlog/pull/1084) Log unused targets to internal logger (@UgurAldanmaz)

### 4.2.3 (2015/12/12)
- [#1083](https://github.com/nlog/nlog/pull/1083) Changed the heading in Readme file (@Page-Not-Found)
- [#1081](https://github.com/nlog/nlog/pull/1081) Update README.md (@UgurAldanmaz)
- [#4](https://github.com/nlog/nlog/pull/4) Update from base repository (@304NotModified, @bhaeussermann, @ie-zero, @epignosisx, @stefandevo, @nathan-schubkegel)
- [#1066](https://github.com/nlog/nlog/pull/1066) Add AllLevels and AllLoggingLevels to LogLevel.cs. (@rellis-of-rhindleton)
- [#1062](https://github.com/nlog/nlog/pull/1062) Fix Xamarin Build in PR (and don't fail in fork) (@304NotModified)
- [#1061](https://github.com/nlog/nlog/pull/1061) skip xamarin-dependent steps in appveyor PR builds (@nathan-schubkegel)
- [#1040](https://github.com/nlog/nlog/pull/1040) Xamarin (iOS, Android) and Windows Phone 8 (@304NotModified, @stefandevo)
- [#1041](https://github.com/nlog/nlog/pull/1041) EventLogTarget: Add overflow action for too large messages (@epignosisx)

### 4.2.2 (2015/11/21)
- [#1054](https://github.com/nlog/nlog/pull/1054) LogReceiverWebServiceTarget.CreateLogReceiver()  should be virtual (@304NotModified)
- [#1048](https://github.com/nlog/nlog/pull/1048) Var layout renderer improvements (@304NotModified)
- [#1043](https://github.com/nlog/nlog/pull/1043) Moved sourcecode tests to separate tool (@304NotModified)

### 4.2.1-RC1 (2015/11/13)
- [#1031](https://github.com/nlog/nlog/pull/1031) NetworkTarget: linkedlist + configure max connections (@304NotModified)
- [#1037](https://github.com/nlog/nlog/pull/1037) Added FilterResult tests (@304NotModified)
- [#1036](https://github.com/nlog/nlog/pull/1036) Logbuilder tests + fix passing Off (@304NotModified)
- [#1035](https://github.com/nlog/nlog/pull/1035) Added tests for Conditional logger (@304NotModified)
- [#1033](https://github.com/nlog/nlog/pull/1033) Databasetarget: restored 'UseTransactions' and print warning if used (@304NotModified)
- [#1032](https://github.com/nlog/nlog/pull/1032) Filetarget: Added tests for the 2 kind of slashes (@304NotModified)
- [#1027](https://github.com/nlog/nlog/pull/1027) Reduce memory allocations in Logger.Log when using CsvLayout and JsonLayout (@epignosisx)
- [#1020](https://github.com/nlog/nlog/pull/1020) Reduce memory allocations in Logger.Log by avoiding GetEnumerator. (@epignosisx)
- [#1019](https://github.com/nlog/nlog/pull/1019) Issue #987: Filetarget: Max archives settings sometimes removes to many files (@bhaeussermann)
- [#1021](https://github.com/nlog/nlog/pull/1021) Fix #2 ObjectGraphScanner.ScanProperties: Collection was modified (@304NotModified)
- [#994](https://github.com/nlog/nlog/pull/994) Introduce FileAppenderCache class (@ie-zero)
- [#968](https://github.com/nlog/nlog/pull/968) Fix: LogFactoryTests remove Windows specific values (@ie-zero)
- [#999](https://github.com/nlog/nlog/pull/999) Unit Tests added to LogFactory class (@ie-zero)
- [#1000](https://github.com/nlog/nlog/pull/1000) Fix methods' indentation in LogManager class (@ie-zero)
- [#1001](https://github.com/nlog/nlog/pull/1001) Dump() now uses InternalLogger.Debug() consistent (@ie-zero)

### 4.2.0 (2015/10/24)
- [#996](https://github.com/nlog/nlog/pull/996) ColoredConsoleTarget: Fixed broken WholeWords option of highlight-word. (@bhaeussermann)
- [#995](https://github.com/nlog/nlog/pull/995) ArchiveFileCompression: auto add `.zip` to compressed filename when archiveName isn't specified (@bhaeussermann)
- [#993](https://github.com/nlog/nlog/pull/993) changed to nuget appveyor account (@304NotModified)
- [#992](https://github.com/nlog/nlog/pull/992) added logo (@304NotModified)
- [#988](https://github.com/nlog/nlog/pull/988) Unit test for proving max-archive bug of #987 (@304NotModified)
- [#991](https://github.com/nlog/nlog/pull/991) Document FileTarget inner properties/methods (@ie-zero)
- [#986](https://github.com/nlog/nlog/pull/986) Added more unit tests for max archive with dates in files. (@304NotModified)
- [#984](https://github.com/nlog/nlog/pull/984) Fixes #941. Add annotations for custom string formatting methods (@bhaeussermann)
- [#985](https://github.com/nlog/nlog/pull/985) Fix: file archiving DateAndSequence & FileArchivePeriod.Day won't work always (wrong switching day detected) (@304NotModified)
- [#982](https://github.com/nlog/nlog/pull/982) Document FileTarget inner properties/methods (@ie-zero)
- [#983](https://github.com/nlog/nlog/pull/983) Remove obsolete code from FileTarget (@ie-zero)
- [#981](https://github.com/nlog/nlog/pull/981) More tests inner parse + docs (@304NotModified)
- [#952](https://github.com/nlog/nlog/pull/952) Fixes #931. FileTarget: Log info concerning archiving to internal logger (@bhaeussermann)
- [#976](https://github.com/nlog/nlog/pull/976) Fix SL4/SL5 warnings by adding/editing XML docs (@304NotModified)
- [#973](https://github.com/nlog/nlog/pull/973) More fluent unit tests (@304NotModified)
- [#975](https://github.com/nlog/nlog/pull/975) Fix parse of inner layout (@304NotModified)
- [#3](https://github.com/nlog/nlog/pull/3) Update (@304NotModified, @UgurAldanmaz, @vbfox, @kevindaub, @Niklas-Peter, @bhaeussermann, @breyed, @wrangellboy)
- [#974](https://github.com/nlog/nlog/pull/974) Small Codecoverage improvement (@304NotModified)
- [#966](https://github.com/nlog/nlog/pull/966) Fix: Exception is thrown when archiving is enabled (@304NotModified)
- [#939](https://github.com/nlog/nlog/pull/939) Bugfix: useSystemNetMailSettings=false still uses .config settings + feature: PickupDirectoryLocation from nlog.config (@dnlgmzddr)
- [#972](https://github.com/nlog/nlog/pull/972) Added Codecov.io (@304NotModified)
- [#971](https://github.com/nlog/nlog/pull/971) Removed unneeded System.Drawing references (@304NotModified)
- [#967](https://github.com/nlog/nlog/pull/967) Getcurrentclasslogger documentation / error messages improvements (@304NotModified)
- [#963](https://github.com/nlog/nlog/pull/963) FIx: Collection was modified - GetTargetsByLevelForLogger (@304NotModified)
- [#954](https://github.com/nlog/nlog/pull/954) Issue 941: Add annotations for customer string formatting messages (@wrangellboy)
- [#940](https://github.com/nlog/nlog/pull/940) Documented default fallback value and RanToCompletion (@breyed)
- [#947](https://github.com/nlog/nlog/pull/947) Fixes #319. Added IncrementValue property. (@bhaeussermann)
- [#945](https://github.com/nlog/nlog/pull/945) Added Travis Badge (@304NotModified)
- [#944](https://github.com/nlog/nlog/pull/944) Skipped some unit tests for Mono (@304NotModified)
- [#938](https://github.com/nlog/nlog/pull/938) Issue #913: Log NLog version to internal log. (@bhaeussermann)
- [#937](https://github.com/nlog/nlog/pull/937) Added more registry unit tests (@304NotModified)
- [#933](https://github.com/nlog/nlog/pull/933) Issue #612: Cached Layout Renderer is reevaluated when LoggingConfiguration is changed (@bhaeussermann)
- [#2](https://github.com/nlog/nlog/pull/2) Support object vals for mdlc (@UgurAldanmaz)
- [#927](https://github.com/nlog/nlog/pull/927) Comments change in LogFactory (@Niklas-Peter)
- [#926](https://github.com/nlog/nlog/pull/926) Assure automatic re-configuration after configuration change (@Niklas-Peter)

### 4.1.2 (2015/09/20)
- [#920](https://github.com/nlog/nlog/pull/920) Added AssemblyFileVersion as property to build script (@304NotModified)
- [#912](https://github.com/nlog/nlog/pull/912) added fluent .properties, fix/add fluent unit tests (@304NotModified)
- [#909](https://github.com/nlog/nlog/pull/909) added ThreadAgnostic on AllEventPropertiesLayoutRenderer (@304NotModified)
- [#910](https://github.com/nlog/nlog/pull/910) Fixes "Collection was modified" crash with ReconfigExistingLoggers (@304NotModified)
- [#906](https://github.com/nlog/nlog/pull/906) added some extra tests (@304NotModified)

### 4.1.1 (2015/09/12)
- [#900](https://github.com/nlog/nlog/pull/900) fix generated code after change .tt (#894) (@304NotModified)
- [#901](https://github.com/nlog/nlog/pull/901) Safe autoload (@304NotModified)
- [#894](https://github.com/nlog/nlog/pull/894) fix generated code after change .tt (#894) (@304NotModified)
- [#896](https://github.com/nlog/nlog/pull/896) Support object vals for mdlc (@UgurAldanmaz)
- [#898](https://github.com/nlog/nlog/pull/898) Resolves Internal Logging With Just Filename (@kevindaub)
- [#1](https://github.com/nlog/nlog/pull/1) Update from base repository (@304NotModified, @UgurAldanmaz, @vbfox)
- [#892](https://github.com/nlog/nlog/pull/892) Remove unused windows.forms stuff (@304NotModified)
- [#894](https://github.com/nlog/nlog/pull/894) Obsolete attribute doesn't specify the correct replacement (@vbfox)

### 4.1.0 (2015/08/30)
- [#884](https://github.com/nlog/nlog/pull/884) Changes at MDLC to support .Net 4.0 and .Net 4.5 (@UgurAldanmaz)
- [#881](https://github.com/nlog/nlog/pull/881) Change GitHub for Windows to GitHub Desktop (@campbeb)
- [#874](https://github.com/nlog/nlog/pull/874) Wcf receiver client (@kevindaub, @304NotModified)
- [#871](https://github.com/nlog/nlog/pull/871) ${event-properties} - Added culture and format properties (@304NotModified)
- [#861](https://github.com/nlog/nlog/pull/861) LogReceiverServiceTests: Added one-way unit test (retry) (@304NotModified)
- [#866](https://github.com/nlog/nlog/pull/866) FileTarget.DeleteOldDateArchive minor fix (@remye06)
- [#872](https://github.com/nlog/nlog/pull/872) Updated appveyor.yml (unit test CMD) (@304NotModified)
- [#743](https://github.com/nlog/nlog/pull/743) Support object values for GDC, MDC and NDC contexts. (@williamb1024)
- [#773](https://github.com/nlog/nlog/pull/773) Fixed DateAndSequence archive numbering mode + bugfix no max archives (@remye06)
- [#858](https://github.com/nlog/nlog/pull/858) Fixed travis build with unit tests (@kevindaub, @304NotModified)
- [#856](https://github.com/nlog/nlog/pull/856) Revert "LogReceiverServiceTests: Added one-way unit test" (@304NotModified)
- [#854](https://github.com/nlog/nlog/pull/854) LogReceiverServiceTests: Added one-way unit test (@304NotModified)
- [#855](https://github.com/nlog/nlog/pull/855) Update appveyor.yml (@304NotModified)
- [#853](https://github.com/nlog/nlog/pull/853) Update appveyor config (@304NotModified)
- [#850](https://github.com/nlog/nlog/pull/850) Archive files delete right order (@304NotModified)
- [#848](https://github.com/nlog/nlog/pull/848) Refactor file archive unittest (@304NotModified)
- [#820](https://github.com/nlog/nlog/pull/820) fix unloaded appdomain with xml auto reload (@304NotModified)
- [#789](https://github.com/nlog/nlog/pull/789) added config option for breaking change (Exceptions logging) in NLog 4.0 [WIP] (@304NotModified)
- [#833](https://github.com/nlog/nlog/pull/833) Move MDLC and Traceactivity from Contrib + handle missing dir in filewachter (@304NotModified, @kichristensen)
- [#818](https://github.com/nlog/nlog/pull/818) Updated InternalLogger to Create Directories If Needed (@kevindaub)
- [#844](https://github.com/nlog/nlog/pull/844) Fix ThreadAgnosticAttributeTest unit test (@304NotModified)
- [#834](https://github.com/nlog/nlog/pull/834) Fix SL5 (@304NotModified)
- [#827](https://github.com/nlog/nlog/pull/827) added test: Combine archive every day and archive above size (@304NotModified)
- [#811](https://github.com/nlog/nlog/pull/811) Easier API (@304NotModified)
- [#816](https://github.com/nlog/nlog/pull/816) Overhaul NLog variables (@304NotModified)
- [#788](https://github.com/nlog/nlog/pull/788) Fix:  exception is not correctly logged when calling without message [WIP] (@304NotModified)
- [#814](https://github.com/nlog/nlog/pull/814) Bugfix: `<extensions>` needs to be the first element in the config (@304NotModified)
- [#813](https://github.com/nlog/nlog/pull/813) Added unit test: reload after replace (@304NotModified)
- [#812](https://github.com/nlog/nlog/pull/812) Unit tests: added some extra time for completion (@304NotModified)
- [#800](https://github.com/nlog/nlog/pull/800) Replace NewLines Layout Renderer Wrapper (@flower189)
- [#805](https://github.com/nlog/nlog/pull/805) Fix issue #804: Logging to same file from multiple processes misses messages (@bhaeussermann)
- [#797](https://github.com/nlog/nlog/pull/797) added switch to JsonLayout to suppress the extra spaces (@tmusico)
- [#809](https://github.com/nlog/nlog/pull/809) Improve docs `ICreateFileParameters` (@304NotModified)
- [#808](https://github.com/nlog/nlog/pull/808) Added logrecievertest with ServiceHost (@304NotModified)
- [#780](https://github.com/nlog/nlog/pull/780) Call site line number layout renderer - fix (@304NotModified)
- [#776](https://github.com/nlog/nlog/pull/776) added SwallowAsync(Task) (@breyed)
- [#774](https://github.com/nlog/nlog/pull/774) FIxed ArchiveOldFileOnStartup with layout renderer used in ArchiveFileName (@remye06)
- [#750](https://github.com/nlog/nlog/pull/750) Optional encoding for JsonAttribute (@grbinho)
- [#742](https://github.com/nlog/nlog/pull/742) Fix monodevelop build. (@txdv)
- [#781](https://github.com/nlog/nlog/pull/781) All events layout renderer: added `IncludeCallerInformation` option. (@304NotModified)
- [#794](https://github.com/nlog/nlog/pull/794) Support for auto loading UNC paths (@mikeobrien)
- [#786](https://github.com/nlog/nlog/pull/786) added unit test for forwardscomp (@304NotModified)

### 4.0.1 (2015/06/18)
- [#762](https://github.com/nlog/nlog/pull/762) Improved config example (@304NotModified)
- [#760](https://github.com/nlog/nlog/pull/760) Autoload fix for ASP.net + better autoloading logging (@304NotModified)
- [#763](https://github.com/nlog/nlog/pull/763) Fixed reference for Siverlight (broken and fixed in 4.0.1) (@304NotModified)
- [#759](https://github.com/nlog/nlog/pull/759) Check if directory watched exists (@kichristensen)
- [#755](https://github.com/nlog/nlog/pull/755) Fix unneeded breaking change with requirement of MailTarget.SmtpServer (@304NotModified)
- [#758](https://github.com/nlog/nlog/pull/758) Correct obsolete text (@kichristensen)
- [#754](https://github.com/nlog/nlog/pull/754) Optimized references (@304NotModified)
- [#753](https://github.com/nlog/nlog/pull/753) Fix autoflush (@304NotModified)
- [#744](https://github.com/nlog/nlog/pull/744) Alternate fix for #730 (@williamb1024)
- [#751](https://github.com/nlog/nlog/pull/751) Fix incorrect loglevel obsolete message (@SimonCropp)
- [#747](https://github.com/nlog/nlog/pull/747) Correct race condition in AsyncTargetWrapperExceptionTest (@williamb1024)
- [#746](https://github.com/nlog/nlog/pull/746) Fix for #736 (@akamyshanov)
- [#736](https://github.com/nlog/nlog/pull/736) fixes issue (#736) when the NLog assembly is loaded from memory (@akamyshanov)
- [#715](https://github.com/nlog/nlog/pull/715) Message queue target test check if queue exists (@304NotModified)

### 4.0.0 (2015/05/26)
- [#583](https://github.com/nlog/nlog/pull/583) .gitattributes specifies which files should be considered as text (@ilya-g)

### 4.0-RC (2015/05/26)
- [#717](https://github.com/nlog/nlog/pull/717) Improved description and warning. (@304NotModified)
- [#718](https://github.com/nlog/nlog/pull/718) GOTO considered harmful (@304NotModified)
- [#689](https://github.com/nlog/nlog/pull/689) Make alignment stay consistent when fixed-length truncation occurs.(AlignmentOnTruncation property) (@logiclrd)
- [#716](https://github.com/nlog/nlog/pull/716) Flush always explicit (@304NotModified)
- [#714](https://github.com/nlog/nlog/pull/714) added some docs for the ConditionalXXX methods (@304NotModified)
- [#712](https://github.com/nlog/nlog/pull/712) nuspec: added author + added NLog tag (@304NotModified)
- [#707](https://github.com/nlog/nlog/pull/707) Introduce auto flush behaviour again (@kichristensen)
- [#705](https://github.com/nlog/nlog/pull/705) EventLogTarget.Source layoutable & code improvements to EventLogTarget (@304NotModified)
- [#704](https://github.com/nlog/nlog/pull/704) Thread safe: GetCurrentClassLogger test + fix (@304NotModified)
- [#703](https://github.com/nlog/nlog/pull/703) Added 'lost messages' Webservice unittest (@304NotModified)
- [#692](https://github.com/nlog/nlog/pull/692) added Encoding property for consoleTarget + ColorConsoleTarget (@304NotModified)
- [#699](https://github.com/nlog/nlog/pull/699) Added Webservice tests with REST api. (@304NotModified)
- [#654](https://github.com/nlog/nlog/pull/654) Added unit test to validate the [DefaultValue] attribute values + update DefaultAttributes (@304NotModified)
- [#671](https://github.com/nlog/nlog/pull/671) Bugfix: Broken xml stops logging (@304NotModified)
- [#697](https://github.com/nlog/nlog/pull/697) V3.2.1 manual merge (@304NotModified, @kichristensen)
- [#698](https://github.com/nlog/nlog/pull/698) Fixed where log files couldn't use the same name as archive file (@BrandonLegault)
- [#691](https://github.com/nlog/nlog/pull/691) Right way to log exceptions (@304NotModified)
- [#670](https://github.com/nlog/nlog/pull/670) added unit test: string with variable get expanded (@304NotModified)
- [#547](https://github.com/nlog/nlog/pull/547) Fix use of single archive in file target (@kichristensen)
- [#674](https://github.com/nlog/nlog/pull/674) Add a Gitter chat badge to README.md (@gitter-badger)
- [#629](https://github.com/nlog/nlog/pull/629) BOM option/fix for WebserviceTarget + code improvements (@304NotModified)
- [#650](https://github.com/nlog/nlog/pull/650) fix default value of Commandtype (@304NotModified)
- [#651](https://github.com/nlog/nlog/pull/651) init `TimeStamp` and `SequenceID` in all ctors (@304NotModified)
- [#657](https://github.com/nlog/nlog/pull/657) Fixed quite a few typos (@sean-gilliam)

### v3.2.1 (2015/03/26)
- [#600](https://github.com/nlog/nlog/pull/600) Looks good (@kichristensen)
- [#645](https://github.com/nlog/nlog/pull/645) Stacktrace broken fix 321 (@304NotModified)
- [#606](https://github.com/nlog/nlog/pull/606) LineEndingMode type in xml configuration and xsd schema (@ilya-g)
- [#608](https://github.com/nlog/nlog/pull/608) Archiving system runs when new log file is created #390 (@awardle)
- [#584](https://github.com/nlog/nlog/pull/584) Stacktrace broken fix (@304NotModified, @ilya-g)
- [#601](https://github.com/nlog/nlog/pull/601) Mailtarget allow empty 'To' and various code improvements (@304NotModified)
- [#618](https://github.com/nlog/nlog/pull/618) Handle .tt in .csproj better (@304NotModified)
- [#619](https://github.com/nlog/nlog/pull/619) Improved badges (@304NotModified)
- [#616](https://github.com/nlog/nlog/pull/616) Added DEBUG-Conditional trace and debug methods #2 (@304NotModified)
- [#10](https://github.com/nlog/nlog/pull/10) Manual merge with master (@304NotModified, @kichristensen, @YuLad, @ilya-g, @MartinTherriault, @aelij)
- [#602](https://github.com/nlog/nlog/pull/602) Logger overloads generated by T4 (@304NotModified)
- [#613](https://github.com/nlog/nlog/pull/613) Treat warnings as errors (@304NotModified)
- [#9](https://github.com/nlog/nlog/pull/9) 304 not modified stacktrace broken fix (@304NotModified, @kichristensen, @YuLad, @ilya-g, @MartinTherriault, @aelij)
- [#610](https://github.com/nlog/nlog/pull/610) Fixed NLog/NLog#609 (@dodexahedron)
- [#8](https://github.com/nlog/nlog/pull/8) Refactoring + comments (@ilya-g)
- [#4](https://github.com/nlog/nlog/pull/4) HiddenAssemblies list is treated like immutable. (@ilya-g)
- [#6](https://github.com/nlog/nlog/pull/6) Sync back (@304NotModified, @kichristensen, @YuLad, @ilya-g, @MartinTherriault, @aelij)
- [#512](https://github.com/nlog/nlog/pull/512) FileTarget uses time from the current TimeSource for date-based archiving (@ilya-g)
- [#560](https://github.com/nlog/nlog/pull/560) Archive file zip compression (@aelij)
- [#576](https://github.com/nlog/nlog/pull/576) Instance property XmlLoggingConfiguration.DefaultCultureInfo should not change global state (@ilya-g)
- [#585](https://github.com/nlog/nlog/pull/585) improved Cyclomatic complexity of ConditionTokenizer (@304NotModified)
- [#598](https://github.com/nlog/nlog/pull/598) Added nullref checks for MailTarget.To (@304NotModified)
- [#582](https://github.com/nlog/nlog/pull/582) Fix NLog.proj build properties (@ilya-g)
- [#5](https://github.com/nlog/nlog/pull/5) Extend stack trace frame skip condition to types derived from the loggerType (@ilya-g)
- [#575](https://github.com/nlog/nlog/pull/575) Event Log Target unit tests improvement (@ilya-g)
- [#556](https://github.com/nlog/nlog/pull/556) Enable the counter sequence parameter to take layouts (@304NotModified)
- [#559](https://github.com/nlog/nlog/pull/559) Eventlog audit events (@304NotModified)
- [#565](https://github.com/nlog/nlog/pull/565) Set the service contract for LogReceiverTarget as one way (@MartinTherriault)
- [#563](https://github.com/nlog/nlog/pull/563) Added info sync projects + multiple .Net versions (@304NotModified)
- [#542](https://github.com/nlog/nlog/pull/542) Delete stuff moved to NLog.Web (@kichristensen)
- [#543](https://github.com/nlog/nlog/pull/543) Auto load extensions to allow easier integration with extensions (@kichristensen)
- [#555](https://github.com/nlog/nlog/pull/555) SMTP Closing connections fix (@304NotModified)
- [#3](https://github.com/nlog/nlog/pull/3) Sync back (@kichristensen, @304NotModified, @YuLad, @ilya-g)
- [#544](https://github.com/nlog/nlog/pull/544) Escape closing bracket in AppDomainLayoutRenderer test (@kichristensen)
- [#546](https://github.com/nlog/nlog/pull/546) Update nuget packages project url (@kichristensen)
- [#545](https://github.com/nlog/nlog/pull/545) Merge exception tests (@kichristensen)
- [#540](https://github.com/nlog/nlog/pull/540) Added CONTRIBUTING.md and schields (@304NotModified)
- [#2](https://github.com/nlog/nlog/pull/2) sync back (@kichristensen, @304NotModified, @YuLad, @ilya-g)
- [#535](https://github.com/nlog/nlog/pull/535) App domain layout renderer (@304NotModified)
- [#519](https://github.com/nlog/nlog/pull/519) Fluent API available for ILogger interface (@ilya-g)
- [#523](https://github.com/nlog/nlog/pull/523) Fix for issue #507: NLog optional or empty mail recipient (@YuLad)
- [#497](https://github.com/nlog/nlog/pull/497) Remove Windows Forms targets (@kichristensen)
- [#530](https://github.com/nlog/nlog/pull/530) Added Stacktrace layout renderer SkipFrames (@304NotModified)
- [#490](https://github.com/nlog/nlog/pull/490) AllEventProperties Layout Renderer (@vladikk)
- [#517](https://github.com/nlog/nlog/pull/517) Fluent API uses the same time source for timestamping as the Logger. (@ilya-g)
- [#503](https://github.com/nlog/nlog/pull/503) Add missing tags to Nuget packages (@kichristensen)
- [#496](https://github.com/nlog/nlog/pull/496) Fix monodevelop build (@dmitry-shechtman)
- [#489](https://github.com/nlog/nlog/pull/489) Add .editorconfig (@damageboy)
- [#491](https://github.com/nlog/nlog/pull/491) LogFactory Class Refactored (@ie-zero)
- [#422](https://github.com/nlog/nlog/pull/422) Run logging code outside of transaction (@Giorgi)
- [#474](https://github.com/nlog/nlog/pull/474) [Fix] ArchiveFileOnStartTest was failing (@ie-zero)
- [#479](https://github.com/nlog/nlog/pull/479) LogManager class refactored (@ie-zero)
- [#478](https://github.com/nlog/nlog/pull/478) Get[*]Logger() return Logger instead of ILogger (@ie-zero)
- [#481](https://github.com/nlog/nlog/pull/481) JsonLayout (@vladikk)
- [#473](https://github.com/nlog/nlog/pull/473) LineEndingMode Changed to Immutable Class (@ie-zero)
- [#469](https://github.com/nlog/nlog/pull/469) Corrects a copy-pasted code comment. (@JoshuaRogers)
- [#467](https://github.com/nlog/nlog/pull/467) LoggingRule.Final only suppresses matching levels. (@ilya-g)
- [#465](https://github.com/nlog/nlog/pull/465) Fix #283: throwExceptions ="false" but Is still an error (@YuLad)
- [#464](https://github.com/nlog/nlog/pull/464) Added 'enabled' attribute to the logging rule element. (@ilya-g)

### v3.2.0.0 (2014/12/21)
- [#463](https://github.com/nlog/nlog/pull/463) Pluggable time sources support in NLog.xsd generator utility (@ilya-g)
- [#460](https://github.com/nlog/nlog/pull/460) Add exception to NLogEvent (@kichristensen)
- [#457](https://github.com/nlog/nlog/pull/457) Unobsolete XXXExceptions methods (@kichristensen)
- [#449](https://github.com/nlog/nlog/pull/449) Added new archive numbering mode (@1and1-webhosting-infrastructure)
- [#450](https://github.com/nlog/nlog/pull/450) Added support for hidden/blacklisted assemblies (@1and1-webhosting-infrastructure)
- [#454](https://github.com/nlog/nlog/pull/454) DateRenderer now includes milliseconds (@ilivewithian)
- [#448](https://github.com/nlog/nlog/pull/448) Added unit test to identify work around when using colons within when layout renderers (@reedyrm)
- [#447](https://github.com/nlog/nlog/pull/447) Change GetCandidateFileNames() to also yield appname.exe.nlog when confi... (@jltrem)
- [#443](https://github.com/nlog/nlog/pull/443) Implement Flush in LogReceiverWebServiceTarget (@kichristensen)
- [#430](https://github.com/nlog/nlog/pull/430) Make ExceptionLayoutRenderer more extensible (@SurajGupta)
- [#442](https://github.com/nlog/nlog/pull/442) BUG FIX: Modification to LogEventInfo.Properties While Iterating (@tsconn23)
- [#439](https://github.com/nlog/nlog/pull/439) Fix for UDP broadcast (@dmitriyett)
- [#415](https://github.com/nlog/nlog/pull/415) Fixed issue (#414) with AutoFlush on FileTarget. (@richol)
- [#409](https://github.com/nlog/nlog/pull/409) Fix loss of exception info when reading Exception.Message property throw... (@wilbit)
- [#407](https://github.com/nlog/nlog/pull/407) Added some missing [StringFormatMethod]s (@roji)
- [#405](https://github.com/nlog/nlog/pull/405) Close channel (@kichristensen)
- [#404](https://github.com/nlog/nlog/pull/404) Correctly delete first line i RichTextBox (@kichristensen)
- [#402](https://github.com/nlog/nlog/pull/402) Add property to stop scanning properties (@kichristensen)
- [#401](https://github.com/nlog/nlog/pull/401) Pass correct parameters into ConfigurationReloaded (@kichristensen)
- [#397](https://github.com/nlog/nlog/pull/397) Improve test run time (@kichristensen)
- [#398](https://github.com/nlog/nlog/pull/398) Remove obsolete attribute from ErrorException (@kichristensen)
- [#395](https://github.com/nlog/nlog/pull/395) Speed up network target tests (@kichristensen)
- [#394](https://github.com/nlog/nlog/pull/394) Always return exit code 0 from test scripts (@kichristensen)
- [#393](https://github.com/nlog/nlog/pull/393) Avoid uneccassary reflection (@kichristensen)
- [#392](https://github.com/nlog/nlog/pull/392) Remove EnumerableHelpers (@kichristensen)
- [#369](https://github.com/nlog/nlog/pull/369) Add of archiveOldFileOnStartup parameter in FileTarget (@cvanbergen)
- [#377](https://github.com/nlog/nlog/pull/377) Apply small performance patch (@pgatilov)
- [#382](https://github.com/nlog/nlog/pull/382) contribute fluent log builder (@pwelter34)

### v3.1.0 (2014/06/23)
- [#371](https://github.com/nlog/nlog/pull/371) Use merging of event properties in async target wrapper to fix empty collection issue (@tuukkapuranen)
- [#357](https://github.com/nlog/nlog/pull/357) Extended ReplaceLayoutRendererWrapper and LayoutParser to support more advanced Regex replacements and more escape codes (@DannyVarod)
- [#359](https://github.com/nlog/nlog/pull/359) Fix #71 : Removing invalid filename characters from created file (@cvanbergen)
- [#366](https://github.com/nlog/nlog/pull/366) Fix for #365: Behaviour when logging null arguments (@cvanbergen)
- [#372](https://github.com/nlog/nlog/pull/372) Fix #370:  EventLogTarget source and log name case insensitive comparison (@cvanbergen)
- [#358](https://github.com/nlog/nlog/pull/358) Made EndpointAddress virtual (@MikeChristensen)
- [#353](https://github.com/nlog/nlog/pull/353) Configuration to disable expensive flushing in NLogTraceListener (@robertvazan)
- [#351](https://github.com/nlog/nlog/pull/351) Obsolete added to LogException() method in Logger class. (@ie-zero)
- [#352](https://github.com/nlog/nlog/pull/352) Remove public constructors from LogLevel (@ie-zero)
- [#349](https://github.com/nlog/nlog/pull/349) Changed all ReSharper annotations to internal (issue 292) (@MichaelLogutov)

### v3.0 (2014/06/02)
- [#346](https://github.com/nlog/nlog/pull/346) Fix: #333 Delete archived files in correct order (@cvanbergen)
- [#347](https://github.com/nlog/nlog/pull/347) Fixed #281: Don't create empty batches when event list is empty (@robertvazan)
- [#246](https://github.com/nlog/nlog/pull/246) Additional Layout Renderer "Assembly-Name" (@Slowpython)
- [#344](https://github.com/nlog/nlog/pull/344) Replacement for [LogLevel]Exception methods (@ie-zero)
- [#337](https://github.com/nlog/nlog/pull/337) Fixes an exception that occurs on startup in apps using NLog. AFAIK shou... (@activescott)
- [#338](https://github.com/nlog/nlog/pull/338) Fix: File target doesn't duplicate header in archived files #245 (@cvanbergen)
- [#341](https://github.com/nlog/nlog/pull/341) SpecialFolderLayoutRenderer honor file and dir (@arjoe)
- [#345](https://github.com/nlog/nlog/pull/345) Default value added in EnviromentLayoutRender (@ie-zero)
- [#335](https://github.com/nlog/nlog/pull/335) Fix/callsite incorrect (@JvanderStad)
- [#334](https://github.com/nlog/nlog/pull/334) Fixes empty "properties" collection. (@erwinwolff)
- [#336](https://github.com/nlog/nlog/pull/336) Fix for invalid XML characters in Log4JXmlEventLayoutRenderer (@JvanderStad)
- [#329](https://github.com/nlog/nlog/pull/329) ExceptionLayoutRenderer extension (@tjandras)
- [#323](https://github.com/nlog/nlog/pull/323) Update DatabaseTarget.cs (@GunsAkimbo)
- [#315](https://github.com/nlog/nlog/pull/315) Dispose of dequeued SocketAsyncEventArgs (@gcschorer)
- [#300](https://github.com/nlog/nlog/pull/300) Avoid NullReferenceException when environment variable not set. (@bkryl)
- [#305](https://github.com/nlog/nlog/pull/305) Redirects Logger.Log(a, b, ex) to Logger.LogException(a, b, ex) (@arangas)
- [#321](https://github.com/nlog/nlog/pull/321) Avoid NullArgumentException when running in a Unity3D application (@mattyway)
- [#285](https://github.com/nlog/nlog/pull/285) Changed modifier of ProcessLogEventInfo (@cincuranet)
- [#270](https://github.com/nlog/nlog/pull/270) Integrate JetBrains Annotations (@damageboy)

### 2.1.0 (2013/10/07)
- [#257](https://github.com/nlog/nlog/pull/257) Fixed SL5 compilation error (@emazv72)
- [#241](https://github.com/nlog/nlog/pull/241) Date Based File Archiving (@mkaltner)
- [#239](https://github.com/nlog/nlog/pull/239) Add layout renderer for retrieving values from AppSettings. (@mpareja)
- [#227](https://github.com/nlog/nlog/pull/227) Pluggable time sources (@robertvazan)
- [#226](https://github.com/nlog/nlog/pull/226) Shared Mutex Improvement (@cjberg)
- [#216](https://github.com/nlog/nlog/pull/216) Optional ConditionMethod arguments, ignoreCase argument for standard condition methods, EventLogTarget enhancements (@tg73)
- [#219](https://github.com/nlog/nlog/pull/219) Avoid Win32-specific file functions in Mono where parts not implemented. (@KeithLRobertson)
- [#215](https://github.com/nlog/nlog/pull/215) Revert "Fix writing NLog properties in Log4JXmlEvent" (@kichristensen)
- [#206](https://github.com/nlog/nlog/pull/206) Correctly use comments in NLog.Config package (@kichristensen)

### v2.0.1 (2013/04/08)
- [#197](https://github.com/nlog/nlog/pull/197) Better request queue logging (@kichristensen)
- [#192](https://github.com/nlog/nlog/pull/192) Allow Form Control Target to specify append direction (@simongh)
- [#182](https://github.com/nlog/nlog/pull/182) Fix locks around layoutCache (@brutaldev)
- [#178](https://github.com/nlog/nlog/pull/178) Anonymous delegate class and method name cleanup (@aalex675)
- [#168](https://github.com/nlog/nlog/pull/168) Deadlock in NLog library using Control-Target (WinForms) (@falstaff84)
- [#176](https://github.com/nlog/nlog/pull/176) Fix for #175 NLogTraceListener not using LogFactory (@HakanL)
- [#163](https://github.com/nlog/nlog/pull/163) #110 Exceptions swallowed in custom target (@johnrey1)
- [#12](https://github.com/nlog/nlog/pull/12) AppDomain testability (@kichristensen)
- [#11](https://github.com/nlog/nlog/pull/11) Updated code to not log exception double times (@ParthDesai)
- [#10](https://github.com/nlog/nlog/pull/10) Improved Fix Code for issue 6575 (@ParthDesai)
- [#7](https://github.com/nlog/nlog/pull/7) Fixed Issue in Code For Invalid XML (@ParthDesai)
- [#6](https://github.com/nlog/nlog/pull/6) Fix For Issue #7031 (@ParthDesai)
- [#5](https://github.com/nlog/nlog/pull/5) Codeplex BUG 6227 - LogManager.Flush throws... (@kichristensen)
- [#4](https://github.com/nlog/nlog/pull/4) Adding a test for pull request #1 which fixes bug 6370 from Codeplex (@sebfischer83)
- [#3](https://github.com/nlog/nlog/pull/3) TraceTarget no longer blocks on error messages. Fixes Codeplex bug 2599 (@kichristensen)
- [#1](https://github.com/nlog/nlog/pull/1) Codeplex Bug 6370 (@sebfischer83)

### NLog-1.0-RC1 (2006/07/10)
- [#27](https://github.com/nlog/nlog/pull/27) added Debugger target (#27), fixed Database ${callsite} (#26) (@jkowalski)
- [#27](https://github.com/nlog/nlog/pull/27) added Debugger target (#27), fixed Database ${callsite} (#26) (@jkowalski)
