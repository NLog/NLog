# NLog RegEx Replace LayoutRenderer

NLog RegEx Replace LayoutRenderer for complex search and replace operations

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Replace-Layout-Renderer) for available options and examples.

## Register Extension

NLog will only recognize type-alias `regex-replace` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.RegEx"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterLayoutRenderer<NLog.LayoutRenderers.Wrappers.RegexReplaceLayoutRendererWrapper>();
});
```
