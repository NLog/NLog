# NLog

NLog is a logging library for .NET with support for structured logging, flexible
configuration, and routing log messages to multiple destinations. This package
includes file and console logging; additional destinations are available through
[extension packages](https://nlog-project.org/config/?tab=targets).

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

NLog can also load logging rules, targets, and layouts from an XML configuration
file. See the [configuration tutorial](https://github.com/NLog/NLog/wiki/Tutorial)
for details.

## Integrations

- For `Microsoft.Extensions.Logging`, use
  [NLog.Extensions.Logging](https://www.nuget.org/packages/NLog.Extensions.Logging).
- For ASP.NET Core, use
  [NLog.Web.AspNetCore](https://www.nuget.org/packages/NLog.Web.AspNetCore).

## Documentation and support

- [Structured logging](https://github.com/NLog/NLog/wiki/How-to-use-structured-logging)
- [Configuration options](https://nlog-project.org/config/)
- [API reference](https://nlog-project.org/documentation/)
- [Troubleshooting](https://github.com/NLog/NLog/wiki/Logging-troubleshooting)
- [Release notes](https://github.com/NLog/NLog/releases)
- [Report an issue](https://github.com/NLog/NLog/issues)

NLog is licensed under the
[BSD 3-Clause license](https://github.com/NLog/NLog/blob/main/LICENSE.txt).
