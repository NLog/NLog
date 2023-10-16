# NLog Database Target

NLog Database Target can write to any database that has ADO.NET Database Provider. Ex. MS-SQL, MySQL, SqLite, Oracle, etc.

Make sure to also install the nuget-package for the wanted Database Provider, and configure the [DbProvider](https://github.com/NLog/NLog/wiki/Database-target#dbprovider-examples) for the Database Target.

If having trouble with output, then check [NLog InternalLogger](https://github.com/NLog/NLog/wiki/Internal-Logging) for clues. See also [Troubleshooting NLog](https://github.com/NLog/NLog/wiki/Logging-Troubleshooting)

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Database-target) for available options and examples.

## Register Extension

NLog will only recognize type-alias `database` when loading from `NLog.config`-file, if having added extension to `NLog.config`-file:

```xml
<extensions>
    <add assembly="NLog.Database"/>
</extensions>
```

Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

```csharp
LogManager.Setup().SetupExtensions(ext => {
   ext.RegisterTarget<NLog.Targets.DatabaseTarget>();
});
```
