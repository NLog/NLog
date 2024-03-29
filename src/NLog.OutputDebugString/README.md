# NLog OutputDebugString Target

NLog OutputDebugString Target writes to the [OutputDebugString](https://msdn.microsoft.com/da-dk/library/windows/desktop/aa363362.aspx) Win32 API, which can be monitored using debugger tools like DebugView.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/OutputDebugString-target) for available options and examples.

## Register Extension

NLog will only recognize type-alias `OutputDebugString` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.OutputDebugString"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterTarget<NLog.Targets.OutputDebugStringTarget>();
});
```
