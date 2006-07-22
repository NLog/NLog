using NLog;

namespace NLog.Benchmark
{
    class Log4NetWithFastLoggerBenchmark : Log4NetBenchmark
    {
        public override string Header
        {
            get
            {
                return @"using System.Reflection;
using System.Globalization;
" + base.Log4NetCommonHeader + @"

public sealed class FastLogger : LoggerWrapperImpl
{
    private readonly static Type declaringType = typeof(FastLogger);
    public FastLogger(ILogger logger) : base(logger)
    {

    }

    public bool IsDebugEnabled
    {
        get { return Logger.IsEnabledFor(Level.Debug); }
    }
    
    public void Debug(string message)
    {
        Logger.Log(declaringType, Level.Debug, message, null);
    }
    public void DebugFormat(string format, params object[] args)
    {
        if (Logger.IsEnabledFor(Level.Debug))
        {
            Logger.Log(declaringType, Level.Debug, String.Format(CultureInfo.InvariantCulture, format, args), null);
        }
    }
    
    public bool IsInfoEnabled
    {
        get { return Logger.IsEnabledFor(Level.Info); }
    }
    
    public void Info(string message)
    {
        Logger.Log(declaringType, Level.Info, message, null);
    }
    public void InfoFormat(string format, params object[] args)
    {
        if (Logger.IsEnabledFor(Level.Info))
        {
            Logger.Log(declaringType, Level.Info, String.Format(CultureInfo.InvariantCulture, format, args), null);
        }
    }

    public bool IsWarnEnabled
    {
        get { return Logger.IsEnabledFor(Level.Warn); }
    }
    
    public void Warn(string message)
    {
        Logger.Log(declaringType, Level.Warn, message, null);
    }
    public void WarnFormat(string format, params object[] args)
    {
        if (Logger.IsEnabledFor(Level.Warn))
        {
            Logger.Log(declaringType, Level.Warn, String.Format(CultureInfo.InvariantCulture, format, args), null);
        }
    }
    
    public bool IsErrorEnabled
    {
        get { return Logger.IsEnabledFor(Level.Error); }
    }
    
    public void Error(string message)
    {
        Logger.Log(declaringType, Level.Error, message, null);
    }
    public void ErrorFormat(string format, params object[] args)
    {
        if (Logger.IsEnabledFor(Level.Error))
        {
            Logger.Log(declaringType, Level.Error, String.Format(CultureInfo.InvariantCulture, format, args), null);
        }
    }
    
    public bool IsFatalEnabled
    {
        get { return Logger.IsEnabledFor(Level.Fatal); }
    }
    
    public void Fatal(string message)
    {
        Logger.Log(declaringType, Level.Fatal, message, null);
    }
    public void FatalFormat(string format, params object[] args)
    {
        if (Logger.IsEnabledFor(Level.Fatal))
        {
            Logger.Log(declaringType, Level.Fatal, String.Format(CultureInfo.InvariantCulture, format, args), null);
        }
    }
    
}

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
";
            }
        }

        public override string CreateSource(string variableName, string name)
        {
            return "static FastLogger " + variableName + " = FastLoggerLogManager.GetLogger(\"" + name + "\");";
        }

        public override string Name
        {
            get { return "Log4NetWithFastLogger"; }
        }
        public override string Init
        {
            get { return "log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(\"Log4NetWithFastLogger.config\"));"; }
        }
    }
}