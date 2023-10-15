# NLog EventLog Target

NLog EventLog Target writes to the Windows EventLog.

See the [NLog Wiki](https://github.com/nlog/nlog/wiki/EventLog-target) for available options and examples.

## Register Extension

NLog will only recognize type-alias `eventlog` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.WindowsEventLog"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => ext.RegisterTarget<NLog.Targets.EventLogTarget>());
```
