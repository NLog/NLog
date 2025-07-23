# NLog AtomFile Target

NLog File Target writing to file using operating system API for atomic file appending (O_APPEND), so multiple processes can write concurrently to the same file.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Atomic-File-target) for available options and examples.

## Linux Support

Linux requires platform specific publish for [Mono.Posix.NETStandard](https://www.nuget.org/packages/Mono.Posix.NETStandard) nuget-package:
```
dotnet publish with --framework net8.0 --configuration release --runtime linux-x64
```

## Register Extension

NLog will only recognize type-alias `AtomFile` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.Targets.AtomicFile"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterTarget<NLog.Targets.AtomicFileTarget>();
});
```
