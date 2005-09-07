#if LOG4NET_WITH_FASTLOGGER

using log4net.Core;
using System.Reflection;

public sealed class FastLoggerLogManager
{
    private static readonly WrapperMap s_wrapperMap = new WrapperMap(
            new WrapperCreationHandler(WrapperCreationHandler));
    private FastLoggerLogManager()
    {

    }
    public static FastLogger GetLogger(string name)
    {
        return (FastLogger)s_wrapperMap.GetWrapper(
                LoggerManager.GetLogger(Assembly.GetCallingAssembly(), name));
    }
    private static ILoggerWrapper WrapperCreationHandler(ILogger logger)
    {
        return new FastLogger(logger);
    }
}

#endif
