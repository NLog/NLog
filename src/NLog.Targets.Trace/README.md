# NLog Trace Target

NLog Trace Target for writing to System.Diagnostics.Trace for each logevent.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki - Trace Target](https://github.com/NLog/NLog/wiki/Trace-target) for available options and examples.

# NLog TraceListener

NLog TraceListener for redirecting System.Diagnostics.Trace into NLog Logger output.

See the [NLog Wiki - NLogTraceListener](https://github.com/NLog/NLog/wiki/NLog-Trace-Listener-for-System-Diagnostics-Trace) for available options and examples.

# NLog ActivityId LayoutRenderer

NLog LayoutRenderer `${activityid}` renders Guid from System.Diagnostics.Trace.CorrelationManager.ActivityId

See the [NLog Wiki - ActivityId](https://github.com/NLog/NLog/wiki/Trace-Activity-Id-Layout-Renderer) for available options and examples.

## Register Extension

NLog will only recognize type-alias `Trace` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.Targets.Trace"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterTarget<NLog.Targets.TraceTarget>();
});
```
