# NLog Network Target

NLog Network Target for sending mesages using TCP / UDP sockets with support for SSL / TSL.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Network-target) for available options and examples.

## NLog SysLog Target

NLog Syslog Target combines the NLog NetworkTarget with NLog SyslogLayout

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Syslog-target) for available options and examples.

## NLog GELF Target

NLog Gelf Target combines the NLog NetworkTarget with NLog GelfLayout for Graylog Extended Logging Format (GELF)

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Gelf-target) for available options and examples.

## NLog Log4JXml Target

NLog Log4JXml Target combines the NLog NetworkTarget with NLog Log4JXmlEventLayout for NLogViewer / Chainsaw.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Chainsaw-target) for available options and examples.

## Register Extension

NLog will only recognize type-alias `Network` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.Targets.Network"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterTarget<NLog.Targets.NetworkTarget>();
   ext.RegisterTarget<NLog.Targets.Log4JXmlTarget>();
   ext.RegisterTarget<NLog.Targets.SyslogTarget>();
   ext.RegisterTarget<NLog.Targets.Log4JXmlTarget>();
   ext.RegisterTarget<NLog.Targets.GelfTarget>();
   ext.RegisterLayout<NLog.Layouts.Log4JXmlEventLayout>();
   ext.RegisterLayout<NLog.Layouts.GelfLayout>();
   ext.RegisterLayout<NLog.Layouts.SyslogLayout>();
});
```
