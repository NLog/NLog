# NLog AutoReload Config

NLog AutoReload Config Monitor for activating AutoReload support for `NLog.config` XML-files.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

## Register Extension

AutoReload Config Monitor must be enabled from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupMonitorForAutoReload().LoadConfigurationFromFile();
```

Notice if using `appsettings.json` for NLog configuration, then this extension is not required for supporting `AutoReload`.