# NLog Registry LayoutRenderer

NLog Registry LayoutRenderer to output value from Windows Registry.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Registry-Layout-Renderer) for available options and examples.

## Register Extension

NLog will only recognize type-alias `registry` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.WindowsRegistry"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterLayoutRenderer<NLog.LayoutRenderers.RegistryLayoutRenderer>();
});
```
