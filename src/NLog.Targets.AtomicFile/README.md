# NLog AtomFile Target

NLog File Target extension that uses operating-system append semantics, allowing multiple processes to write concurrently to the same file.

`AtomicFileTarget` extends NLog's standard `FileTarget` and uses native operating-system file APIs to provide atomic append semantics:

- **Windows**: uses `FILE_APPEND_DATA` together with `SYNCHRONIZE`.
- **Linux** and **macOS**: uses `open()` with `O_APPEND` and `O_CLOEXEC`.
  - `O_APPEND` ensures that each write is positioned at the end of the file by the operating system.
  - `O_CLOEXEC` prevents the file descriptor from being unintentionally inherited across `exec()`.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Atomic-File-target) for available options and examples.

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

## Example Configuration:

```xml
<nlog>
<extensions>
    <add assembly="NLog.Targets.AtomicFile"/>
</extensions>
<targets>
  <target xsi:type="AtomFile"
          name="logfile"
          fileName="${basedir}/logs/application.log"
          layout="${longdate}|${level:uppercase=true}|${message} ${exception:format=tostring}" />
</targets>
</nlog>
```