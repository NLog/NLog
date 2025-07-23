# NLog Concurrent File Target

NLog File Target with support for ConcurrentWrites-option where multiple processes can write to the same file.

This is the legacy FileTarget from NLog v5, if unable to use the new optimized FileTarget included with NLog v6.

Notice one must explicit configure `ConcurrentWrites="true"` to support concurrent writing to same file.
Alternative consider using the new [NLog.Targets.AtomicFile](https://www.nuget.org/packages/NLog.Targets.AtomicFile) nuget-package.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/File-target) for available options and examples.

## Register Extension

NLog will only recognize type-alias `File` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.Targets.ConcurrentFile"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterTarget<NLog.Targets.ConcurrentFileTarget>();
});
```
