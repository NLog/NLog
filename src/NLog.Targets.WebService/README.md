# NLog WebService Target

NLog WebService Target calls WebService method on each logevent, with support for different protocols: JsonPost, XmlPost, HttpGet, HttpPost, Soap11, Soap12

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/WebService-target) for available options and examples.

## Register Extension

NLog will only recognize type-alias `WebService` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.Targets.WebService"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterTarget<NLog.Targets.WebServiceTarget>();
});
```
