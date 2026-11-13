# NLog

NLog is a logging platform for .NET with support for structured logging, flexible
configuration, and routing log messages to multiple destinations. The core NLog
package works independently of `Microsoft.Extensions.Logging` and includes file
and console logging; additional destinations are available through
[extension packages](https://nlog-project.org/config/?tab=targets).

## Capabilities

- Capture message-template properties, event properties, and contextual information
  with structured logging.
- Route events to multiple targets using logging rules and filters.
- Format and enrich output using layouts and layout renderers, including JSON layouts.
- Use asynchronous and buffered target wrappers to control how log events are delivered.
- Extend targets, layouts, and filters with first-party and community packages.

## Getting started

Install the package in your application:

```sh
dotnet add package NLog
```

For example, configure console logging in a .NET console application's `Program.cs`:

```csharp
using NLog;

var logger = LogManager.Setup()
    .LoadConfiguration(builder => builder.ForLogger().WriteToConsole())
    .GetCurrentClassLogger();

try
{
    logger.Info("Hello, {Name}!", "World");
}
finally
{
    LogManager.Shutdown();
}
```

The `Name` message-template property is captured as a structured event property,
so layouts can include it separately from the formatted message.

## Configuration

- **Fluent API:** Configure targets, rules, and layouts in C#, as shown above.
  See the [Fluent Configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API).
- **XML:** Define targets, rules, and layouts in an `NLog.config` file.
  See the [configuration tutorial](https://github.com/NLog/NLog/wiki/Tutorial).
- **JSON (`appsettings.json`):** Use
  [NLog.Extensions.Logging](https://www.nuget.org/packages/NLog.Extensions.Logging)
  to load the `NLog` configuration section through the .NET configuration system.
  When integrated with the application's configuration, `AddNLog()` / `UseNLog()`
  can load this section automatically. The integration also provides the
  `${configsetting}` layout renderer for accessing application settings.
  See [NLog configuration with appsettings.json](https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-configuration-with-appsettings.json)
  for setup examples and environment-specific configuration.

## Integrations

- [NLog.Extensions.Logging](https://www.nuget.org/packages/NLog.Extensions.Logging)
  provides an NLog provider for Microsoft's `ILogger` abstraction, including scopes
  and `appsettings.json` configuration support.
- [NLog.Extensions.Hosting](https://www.nuget.org/packages/NLog.Extensions.Hosting)
  integrates NLog with the .NET Generic Host.
- [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.Web.AspNetCore)
  integrates NLog with ASP.NET Core, including web-specific logging functionality.

## Documentation and support

- [Structured logging](https://github.com/NLog/NLog/wiki/How-to-use-structured-logging)
- [Configuration options](https://nlog-project.org/config/)
- [API reference](https://nlog-project.org/documentation/)
- [Troubleshooting](https://github.com/NLog/NLog/wiki/Logging-troubleshooting)
- [Release notes](https://github.com/NLog/NLog/releases)
- [Report an issue](https://github.com/NLog/NLog/issues)

NLog is licensed under the
[BSD 3-Clause license](https://github.com/NLog/NLog/blob/main/LICENSE.txt).
